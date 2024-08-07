namespace ServiceBus
{
    public interface IBus
    {
        void Publish<T>(T message, string queueName);
        void Publish<T>(T message, string exchangeName, string exchangeType);

        void PublishAsDirect<T>(T message, string exchangeName, string exchangeType, string routeKey);
    }
}