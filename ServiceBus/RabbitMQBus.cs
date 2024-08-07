using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace ServiceBus
{
    public class RabbitMqBus : IBus
    {
        private IModel? Channel { get; set; }

        public RabbitMqBus(IConfiguration configuration)
        {
            var connectionFactory = new ConnectionFactory()
            {
                Uri = new Uri(configuration.GetConnectionString("RabbitMQ")!)
            };


            var connection = connectionFactory.CreateConnection();

            Channel = connection.CreateModel();


            Channel.BasicReturn += Channel_BasicReturn;
            Channel.CallbackException += Channel_CallbackException;
        }

        private void Channel_CallbackException(object? sender, RabbitMQ.Client.Events.CallbackExceptionEventArgs e)
        {
            Console.WriteLine("Mesaj gönderilemedi. Hata meydana geldi");
        }

        private void Channel_BasicReturn(object? sender, RabbitMQ.Client.Events.BasicReturnEventArgs e)
        {
            Console.WriteLine("Mesaj kuyruğa gönderilemedi");
        }

        public void Publish<T>(T message, string queueName)
        {
            Channel!.QueueDeclare(queueName, true, false, false, null);


            var messageAsJson = JsonSerializer.Serialize(message);
            var messageAsBytes = Encoding.UTF8.GetBytes(messageAsJson);

            Channel!.BasicPublish(string.Empty, queueName, false, null, messageAsBytes);
        }

        public void Publish<T>(T message, string exchangeName, string exchangeType)
        {
            Channel!.ExchangeDeclare(exchangeName, exchangeType, true, false, null);


            var messageAsJson = JsonSerializer.Serialize(message);
            var messageAsBytes = Encoding.UTF8.GetBytes(messageAsJson);

            //publisher ack => false
            //Channel!.BasicPublish(exchangeName, string.Empty, true, null, messageAsBytes);


            //publisher ack => true
            Channel.ConfirmSelect();
            Channel!.BasicPublish(exchangeName, string.Empty, true, null, messageAsBytes);
            Channel.WaitForConfirms(TimeSpan.FromSeconds(15));
        }

        public void PublishAsDirect<T>(T message, string exchangeName, string exchangeType, string routeKey)
        {
            Channel!.ExchangeDeclare(exchangeName, exchangeType, true, false, null);


            var messageAsJson = JsonSerializer.Serialize(message);
            var messageAsBytes = Encoding.UTF8.GetBytes(messageAsJson);

            //publisher ack => false
            //Channel!.BasicPublish(exchangeName, string.Empty, true, null, messageAsBytes);


            //publisher ack => true
            Channel.ConfirmSelect();
            Channel!.BasicPublish(exchangeName, routeKey, true, null, messageAsBytes);
            Channel.WaitForConfirms(TimeSpan.FromSeconds(15));
        }
    }
}