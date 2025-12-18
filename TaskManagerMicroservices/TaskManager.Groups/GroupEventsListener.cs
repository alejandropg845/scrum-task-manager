using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using TaskManager.Common.Interfaces;
using TaskManager.Groups.Interfaces;
using TaskManager.Groups.Payloads;

namespace TaskManager.Groups
{
    public class GroupEventsListener : BackgroundService
    {
        private readonly IModel _channel;
        private readonly ILogger<GroupEventsListener> _logger;
        private readonly IGroupWriteRepository _repo;
        public GroupEventsListener(IRabbitMqConnection c, ILogger<GroupEventsListener> logger, IGroupWriteRepository repo)
        {
            _channel = c.GetChannel();
            _logger = logger;
            _repo = repo;

            DeclareQueues();
        }

        private void DeclareQueues()
        {
            _channel.QueueDeclare(
                queue: "delete_group",
                durable: true,
                exclusive: false,
                autoDelete: false
            );
        }
        public void DeleteGroupEvent()
        {

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                try
                {
                    string json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                    var obj = JsonSerializer.Deserialize<DeleteGroup>(json)!;

                    await _repo.DeleteGroupAsync(obj.GroupName, obj.Username);

                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);

                }
                catch (Exception e)
                {
                    LogError(e, eventArgs, DeleteGroupEvent);
                }
            };

            _channel.BasicConsume(queue: "delete_group", autoAck: false, consumer: consumer);

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
            DeleteGroupEvent();
            return Task.CompletedTask;
        }
    }
}
