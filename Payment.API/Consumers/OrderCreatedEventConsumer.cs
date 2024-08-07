using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServiceBus;

namespace Payment.API.Consumers
{
    public class OrderCreatedEventConsumer(IConfiguration configuration) : BackgroundService
    {
        private const string QueueName = "payment.api.order.created.event.queue";


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connectionFactory = new ConnectionFactory()
            {
                Uri = new Uri(configuration.GetConnectionString("RabbitMQ")!)
            };


            var connection = connectionFactory.CreateConnection();

            var channel = connection.CreateModel();


            channel.QueueDeclare(QueueName, true, false, false, null);
            channel.QueueBind(QueueName, BusConst.OrderCreatedEventExchangeName, string.Empty, null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += Consumer_Received;


            channel.BasicConsume(QueueName, true, consumer);

            return Task.CompletedTask;
        }

        private void Consumer_Received(object? sender, BasicDeliverEventArgs e)
        {
            var messageAsByte = e.Body.ToArray();


            var messageAsJson = Encoding.UTF8.GetString(messageAsByte);

            var orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(messageAsJson)!;

            // db operations (update)

            Console.WriteLine(
                $"payment => orderId:{orderCreatedEvent.OrderId} totalPrice: {orderCreatedEvent.TotalPrice} stockCount: {orderCreatedEvent.StockCount}");
        }
    }
}