using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using TaskManager.Common.Interfaces;
using TaskManager.Sprints.Interfaces;
using System.Text;
using System.Text.Json;
using TaskManager.Sprints.Payloads;

namespace TaskManager.Sprints.EventListeners
{
    public class FeedbackEventListener : BackgroundService
    {
        private readonly IModel _channel;
        private readonly ILogger<FeedbackEventListener> _logger;
        private readonly IFeedbackRepository _repo;
        public FeedbackEventListener(IRabbitMqConnection rmqc, ILogger<FeedbackEventListener> logger, IFeedbackRepository repo)
        {
            _channel = rmqc.GetChannel();
            _logger = logger;
            _repo = repo;

            DeclareQueues();

        }

        private void DeclareQueues()
        {
            _channel.QueueDeclare(
                 queue: "delete_feedbacks",
                 durable: true,
                 exclusive: false,
                 autoDelete: false
            );
        }
        public void DeleteFeedbacksEvent()
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                    var obj = JsonSerializer.Deserialize<DeleteFeedbacks>(json)!;

                    await _repo.DeleteFeedbackToUsersAsync(obj.GroupName, obj.SprintId);

                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);

                }
                catch (Exception e)
                {
                    LogError(e, eventArgs, DeleteFeedbacksEvent);
                }

            };

            _channel.BasicConsume(queue: "delete_feedbacks", autoAck: false, consumer: consumer);
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
            DeleteFeedbacksEvent();
            return Task.CompletedTask;
        }

    }
}
