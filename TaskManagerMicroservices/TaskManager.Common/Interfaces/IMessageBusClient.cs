namespace TaskManager.Common.Interfaces
{
    public interface IMessageBusClient
    {
        void Publish<T>(string queueName, T message);
    }
}