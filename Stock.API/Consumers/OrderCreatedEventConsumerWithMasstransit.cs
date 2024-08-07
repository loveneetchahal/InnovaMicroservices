using MassTransit;
using ServiceBus;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumerWithMasstransit : IConsumer<OrderCreatedEvent>
    {
        public Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var orderCreatedEvent = context.Message;
            Console.WriteLine(
                $"orderId:{orderCreatedEvent.OrderId} totalPrice: {orderCreatedEvent.TotalPrice} stockCount: {orderCreatedEvent.StockCount}");

            return Task.CompletedTask;
        }
    }
}