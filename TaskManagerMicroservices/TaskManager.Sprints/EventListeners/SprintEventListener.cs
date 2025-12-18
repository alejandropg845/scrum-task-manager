using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.Sprints.Interfaces;
using TaskManager.Sprints.Payloads;

namespace TaskManager.Sprints.EventListeners
{
    public class SprintEventListener : BackgroundService
    {
        private readonly IModel _channel;
        private readonly ILogger<SprintEventListener> _logger;
        private readonly ISprintWriteRepository _repo;
        private readonly ISprintWriteService _service;
        public SprintEventListener(IRabbitMqConnection rmqc, ILogger<SprintEventListener> logger, ISprintWriteRepository repo, ISprintWriteService sprintService)
        {
            _channel = rmqc.GetChannel();
            _logger = logger;
            _repo = repo;
            _service = sprintService;

            DeclareQueues();
            

        }

        private void DeclareQueues()
        {
            _channel.QueueDeclare(
                queue: "remove_sprints",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            _channel.QueueDeclare(
                queue: "revert_sprint_status",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            _channel.QueueDeclare(
                queue: "revert_cycled_sprint",
                durable: true,
                exclusive: false,
                autoDelete: false
            );
        }
        public void OnDeleteSprints()
        {
            

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                    string groupName = JsonSerializer.Deserialize<string>(json)!;

                    int deletedCount = await _repo.DeleteSprintsAsync(groupName);

                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);

                }
                catch (Exception e)
                {
                    LogError(e, eventArgs, OnDeleteSprints);
                }

            };
            
            _channel.BasicConsume(queue: "remove_sprints", autoAck: false, consumer: consumer);
        }

        public void RevertSprintStatusEvent() // <== Esto se ejecuta cuando se comienza un sprint (si hay error)
        {

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                try
                {
                    var payload = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                    var obj = JsonSerializer.Deserialize<RevertSprintStatus>(payload)!;

                    await _repo.RevertSprintStatusAsync(obj.SprintId, obj.GroupName, obj.Status, null);

                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);

                }
                catch (Exception e)
                {
                    LogError(e, eventArgs, RevertSprintStatusEvent);
                }

            };

            _channel.BasicConsume(queue: "revert_sprint_status", autoAck: false, consumer: consumer);
        }

        public void RevertCycledSprintEvent()
        {

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                try
                {
                    var payload = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    var obj = JsonSerializer.Deserialize<RevertCycledSprint>(payload, jsonOptions)!;

                    await _service.RevertCycledSprintAsync(obj.GroupName, obj.CompletedSprintId, obj.NewSprintId);

                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);

                }
                catch (Exception e)
                {
                    LogError(e, eventArgs, RevertCycledSprintEvent);
                }

            };

            _channel.BasicConsume(queue: "revert_cycled_sprint", autoAck: false, consumer: consumer);
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
            OnDeleteSprints();
            RevertSprintStatusEvent();
            RevertCycledSprintEvent();
            return Task.CompletedTask;
        }
    }
}
