using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;

namespace TaskManager.Common.Services
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly ILogger<MessageBusClient> _logger;
        private readonly IModel _channel;
        private readonly IBasicProperties _basicProperties;
        public MessageBusClient(ILogger<MessageBusClient> l, IRabbitMqConnection rmqc)
        {
            _logger = l;
            _channel = rmqc.GetChannel();

            _basicProperties = _channel.CreateBasicProperties();
            _basicProperties.Persistent = true;

            try { DeclareQueues(); } 
            catch(Exception ex)
            {
                _logger.LogError("Hubo un error al crear conexión de RabbitMqConnection\n" +
                    "Excepción: {Msg}\n" +
                    "StackTrace: {StackTrace}",
                    ex.Message, ex.StackTrace
                );
                throw;
            }

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
                queue: "delete_task_items",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            _channel.QueueDeclare(
                queue: "delete_user_group_role",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            _channel.QueueDeclare(
                queue: "delete_user_group",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            _channel.QueueDeclare(
                queue: "delete_group",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            _channel.QueueDeclare(
                queue: "delete_group_sprint",
                durable: true,
                exclusive: false,
                autoDelete: false
            );
        }
        public void Publish<T>(string queueName, T message)
        {
            try
            {
                
                string serializedJson = JsonSerializer.Serialize(message);

                byte[] bytedJson = Encoding.UTF8.GetBytes(serializedJson);

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: queueName,
                    basicProperties: _basicProperties,
                    body: bytedJson
                );

            }
            catch (Exception e)
            {
                _logger.LogError("Error al publicar mensaje {T}\n" +
                    "Excepción: {Msg}\n" +
                    "StackTrace: {StackTrace}",
                    nameof(T),
                    e.Message, 
                    e.StackTrace
                );
            }
        }
    }
}
