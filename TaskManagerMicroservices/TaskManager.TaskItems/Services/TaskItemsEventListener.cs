using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using TaskManager.Common.Interfaces;
using TaskManager.TaskItems.Interfaces;

namespace TaskManager.TaskItems.Services
{
    public class TaskItemsEventListener : BackgroundService
    {
        private readonly IModel _channel;
        private readonly ILogger<TaskItemsEventListener> _logger;
        private readonly ITaskItemsWriteRepository _taskItemsRepository;
        public TaskItemsEventListener(IRabbitMqConnection connection, ILogger<TaskItemsEventListener> logger, ITaskItemsWriteRepository repo)
        {
            _channel = connection.GetChannel();
            _logger = logger;
            _taskItemsRepository = repo;

            try { DeclareQueues(); }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Error al iniciar rabbitMq listeners\n" +
                    "Excepción: {Msg}\n" +
                    "StackTrace: {StackTrace}",
                    ex.Message, ex.StackTrace
                );
            }
        }
        private void DeclareQueues()
        {
            _channel.QueueDeclare(
                queue: "delete_task_items",
                durable: true,
                exclusive: false,
                autoDelete: false
            );
        }
        public void OnDeleteTaskItems()
        {
            
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                try
                {
                    //Getting serialized json from bytes
                    string taskId = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                    await _taskItemsRepository.DeleteTaskItemsAsync(taskId);

                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                    

                }
                catch (Exception e)
                {
                    _logger.LogError(
                        "Error en listener: {MethodName}\n" +
                        "Excepción: {Msg}\n" +
                        "StackTrace: {StackTrace}",
                        nameof(OnDeleteTaskItems),
                        e.Message, e.StackTrace
                    );

                    _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(queue: "delete_task_items", autoAck: false, consumer: consumer);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            OnDeleteTaskItems();
            return Task.CompletedTask;
        }
    }
}
