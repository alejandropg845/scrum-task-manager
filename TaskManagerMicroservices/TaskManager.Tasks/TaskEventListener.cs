
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.Tasks.Interfaces;

namespace TaskManager.Tasks.Services
{
    public class TaskEventListener : BackgroundService
    {
        private readonly ITaskWriteRepository _tasksRepository;
        private readonly IModel _channel;
        private readonly ILogger<TaskEventListener> _logger;
        public TaskEventListener(ILogger<TaskEventListener> l, IRabbitMqConnection connection, ITaskWriteRepository r)
        {
            _channel = connection.GetChannel();
            _logger = l;
            _tasksRepository = r;

            DeclareQueues();
        }

        private void DeclareQueues()
        {
            _channel.QueueDeclare(
                queue: "tasks_sprint",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            _channel.QueueDeclare(
               queue: "task_completed",
               durable: true,
               exclusive: false,
               autoDelete: false
            );

            _channel.QueueDeclare(
               queue: "revert_tasks_finished_status",
               durable: true,
               exclusive: false,
               autoDelete: false
            );

            _channel.QueueDeclare(
               queue: "revert_in_progress_tasks_status",
               durable: true,
               exclusive: false,
               autoDelete: false
            );
        }
        public void OnSetSprintToTasks()
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                try
                {
                    var serializedMessage = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                    var tasksIds = JsonSerializer.Deserialize<SprintInfoForTask>(serializedMessage)!;

                    await _tasksRepository.SetSprintToTasksAsync(tasksIds);

                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);

                }
                catch (Exception e)
                {

                    LogError(e, eventArgs, OnSetSprintToTasks);

                }

            };

            _channel.BasicConsume(queue: "tasks_sprint", autoAck: false, consumer: consumer);

        }

        public void OnMarkTaskAsCompleted()
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                try
                {
                    //Getting serialized json from bytes
                    var serializedMessage = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                    var taskId = JsonSerializer.Deserialize<string>(serializedMessage)!;

                    await _tasksRepository.SetTaskAsCompletedAsync(taskId);

                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);

                }
                catch (Exception e)
                {

                    LogError(e, eventArgs, OnMarkTaskAsCompleted);

                }

            };

            _channel.BasicConsume(queue: "task_completed", autoAck: false, consumer: consumer);

        }

        public void RevertTasksFinishedStatus()
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                try
                {
                    //Getting serialized json from bytes
                    var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                    var sprintId = JsonSerializer.Deserialize<string>(json)!;

                    await _tasksRepository.RevertSprintTasksSetAsFinishedAsync(sprintId);

                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);

                }
                catch (Exception e)
                {
                    LogError(e, eventArgs, RevertTasksFinishedStatus);
                    
                }

            };

            _channel.BasicConsume(queue: "revert_tasks_finished_status", autoAck: false, consumer: consumer);

        }

        public void RevertInProgressTasksStatus()
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                    var sprintId = JsonSerializer.Deserialize<string>(json)!;

                    await _tasksRepository.RevertInProgressStatusAsync(sprintId);

                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);

                }
                catch (Exception e)
                {
                    LogError(e, eventArgs, RevertInProgressTasksStatus);

                }

            };

            _channel.BasicConsume(queue: "revert_in_progress_tasks_status", autoAck: false, consumer: consumer);

        }

        private void LogError<T>(Exception e, BasicDeliverEventArgs eventArgs, T _)
        {
            _logger.LogError(
                "Error en listener: {MethodName}\n" +
                "Excepción: {Msg}\n" +
                "StackTrace: {StackTrace}",
                nameof(T),
                e.Message, e.StackTrace
            );

            _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            OnSetSprintToTasks();
            OnMarkTaskAsCompleted();
            RevertTasksFinishedStatus();
            RevertInProgressTasksStatus();
            return Task.CompletedTask;
        }
    }
}
