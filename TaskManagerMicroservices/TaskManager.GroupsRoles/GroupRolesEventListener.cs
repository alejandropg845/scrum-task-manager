using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.GroupsRoles.Interfaces;

namespace TaskManager.GroupsRoles
{
    public class GroupRolesEventListener : BackgroundService
    {
        private readonly IGroupRolesWriteRepository _repo;
        private readonly IModel _channel;
        private readonly ILogger<GroupRolesEventListener> _logger;
        public GroupRolesEventListener(ILogger<GroupRolesEventListener> l, IRabbitMqConnection rmqc, IGroupRolesWriteRepository repo)
        {
            _logger = l;
            _channel = rmqc.GetChannel();
            _repo = repo;

            DeclareQueues();
            
        }

        private void DeclareQueues()
        {
            _channel.QueueDeclare(
                queue: "create_group_role",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            _channel.QueueDeclare(
                queue: "delete_group_roles",
                durable: true,
                exclusive: false,
                autoDelete: false
            );
        }

        public void OnSetUserRoleEvent()
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                try
                {
                    //Getting serialized json from bytes
                    var serializedMessage = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                    var obj = JsonSerializer.Deserialize<Common.Documents.GroupsRoles>(serializedMessage)!;


                    var group = await _repo.CreateGroupRoleAsync(obj.UserName, obj.RoleName, obj.GroupName, null);

                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);

                }
                catch (Exception e)
                {
                    LogError(e, eventArgs, OnSetUserRoleEvent);
                }

            };

            _channel.BasicConsume(queue: "create_group_role", autoAck: false, consumer: consumer);
        }

        public void OnDeleteGroupRoles()
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                try
                {
                    var serializedMessage = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                    string groupName = JsonSerializer.Deserialize<string>(serializedMessage)!;

                    await _repo.RemoveGroupsRolesAsync(groupName);

                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);

                }
                catch (Exception e)
                {
                    LogError(e, eventArgs, OnDeleteGroupRoles);
                }

            };

            _channel.BasicConsume(queue: "delete_group_roles", autoAck: false, consumer: consumer);
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
            OnSetUserRoleEvent();
            OnDeleteGroupRoles();
            return Task.CompletedTask;
        }
    }
}
