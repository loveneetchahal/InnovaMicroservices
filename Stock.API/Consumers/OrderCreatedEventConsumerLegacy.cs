using System.Linq.Expressions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServiceBus;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumerLegacy(IConfiguration configuration) : BackgroundService
    {
        private const string QueueName = "stock.api.order.created.event.direct.queue";

        private IModel? _channel;

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connectionFactory = new ConnectionFactory()
            {
                Uri = new Uri(configuration.GetConnectionString("RabbitMQ")!)
            };


            var connection = connectionFactory.CreateConnection();

            _channel = connection.CreateModel();

            _channel.BasicQos(0, 10, false);

            _channel.QueueDeclare(QueueName, true, false, false, null);
            _channel.QueueBind(QueueName, BusConst.OrderCreatedEventExchangeName, BusConst.OrderCreatedEventRouteKey,
                null);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += Consumer_Received;


            _channel.BasicConsume(QueueName, false, consumer);

            return Task.CompletedTask;
        }

        private void Consumer_Received(object? sender, BasicDeliverEventArgs e)
        {
            try
            {
                var messageAsByte = e.Body.ToArray();

                var messageAsJson = Encoding.UTF8.GetString(messageAsByte);

                var orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(messageAsJson)!;


                //if (orderCreatedEvent.OrderId % 10 == 0)
                //{
                //    Console.WriteLine($"Hata var : orderId: {orderCreatedEvent.OrderId}");
                //    throw new Exception("hata var");
                //}

                // db operations (update)

                Console.WriteLine(
                    $"orderId:{orderCreatedEvent.OrderId} totalPrice: {orderCreatedEvent.TotalPrice} stockCount: {orderCreatedEvent.StockCount}");
                _channel!.BasicAck(e.DeliveryTag, false);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}