using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TaskManager.Common.Interfaces;
using TaskManager.Users.Interfaces.Repository;

namespace TaskManager.Users
{
    public class UsersEventListener : BackgroundService
    {
        private readonly IModel _channel;
        private readonly ILogger<UsersEventListener> _logger;
        private readonly IUserManagementRepository _repo;
        public UsersEventListener(IRabbitMqConnection c, ILogger<UsersEventListener> logger, IUserManagementRepository repo)
        {
            _channel = c.GetChannel();
            _logger = logger;
            _repo = repo;

            DeclareQueues();
        }

        private void DeclareQueues()
        {
            _channel.QueueDeclare(
                queue: "delete_members_group",
                durable: true,
                exclusive: false,
                autoDelete: false
            );
        }
        public void RemoveGroupFromMembersEvent()
        {

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                try
                {
                    string json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                    var groupName = JsonSerializer.Deserialize<string>(json)!;

                    await _repo.RemoveGroupFromUsersAsync(groupName);

                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);

                }
                catch (Exception e)
                {
                    LogError(e, eventArgs);
                }
            };

            _channel.BasicConsume(queue: "delete_members_group", autoAck: false, consumer: consumer);

        }

        private void LogError(Exception e, BasicDeliverEventArgs eventArgs)
        {
            _logger.LogError(
                "Error en listener: {MethodName}\n" +
                "Excepción: {Msg}\n" +
                "StackTrace: {StackTrace}",
                nameof(RemoveGroupFromMembersEvent),
                e.Message, e.StackTrace
            );

            _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RemoveGroupFromMembersEvent();
            return Task.CompletedTask;
        }
    }
}
