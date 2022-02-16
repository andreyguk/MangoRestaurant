
using Mango.Services.PaymentAPI.Messages;
using Mango.Services.PaymentAPI.RabbitMqSender;
using Newtonsoft.Json;
using PaymentProcessor;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.PaymentAPI.Messaging
{
    public class RabbitMqPaymentConsumer : BackgroundService
    {

        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IModel _channel;
        private readonly IProcessPayment _processPayment;

        private readonly string _hostName;
        private readonly string _password;
        private readonly string _userName;
        private readonly string _checkoutQueueName;

        private readonly IRabbitMqPaymentMessageSender   _rabbitMqPaymentMessageSender;
        public RabbitMqPaymentConsumer(IConfiguration configuration, IRabbitMqPaymentMessageSender rabbitMqPaymentMessageSender, IProcessPayment processPayment)
        {

            _hostName = "127.0.0.1";
            _password = "guest";
            _userName = "guest";
            _configuration = configuration;  
            _checkoutQueueName = _configuration.GetValue<string>("CheckoutQueueName");
            _rabbitMqPaymentMessageSender = rabbitMqPaymentMessageSender;
            _processPayment = processPayment;

            var fctory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password,
            };
            _connection = fctory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _checkoutQueueName, false, false, false, arguments: null);

        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
          stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
             {
                 var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                 PaymentRequestMessage paymentRequestMessage = JsonConvert.DeserializeObject<PaymentRequestMessage>(content);
                 HandleMessage(paymentRequestMessage).GetAwaiter().GetResult();

                 _channel.BasicAck(ea.DeliveryTag, false);
             };
            _channel.BasicConsume(_checkoutQueueName,false,consumer);
            return Task.CompletedTask;
        }

        private  async Task HandleMessage(PaymentRequestMessage paymentRequestMessage)
        {
            var result = _processPayment.PaymentProcessor();

            UpdatePaymentResultMessage updatePaymentResultMessage = new()
            {
                Status = result,
                OrderId = paymentRequestMessage.OrderId,
                Email = paymentRequestMessage.Email,
            };

            try
            {
                _rabbitMqPaymentMessageSender.SendMessage(updatePaymentResultMessage);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
