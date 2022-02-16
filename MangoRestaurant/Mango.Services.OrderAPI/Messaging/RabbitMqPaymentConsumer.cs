using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Repository;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.OrderAPI.Messaging
{
    public class RabbitMqPaymentConsumer : BackgroundService
    {

        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IModel _channel;
        private readonly IOrderRepository _orderRepository;
        private const string ExchangeName = "DirectPaymentUpdate_Exchange";
        private const string PaymentOrderUpdateQueueName = "PaymentOrderUpdateQueueName";
        private readonly string _checkoutQueueName;

        private readonly string _hostName;
        private readonly string _password;
        private readonly string _userName;
        string _queueName = "";

        public RabbitMqPaymentConsumer(OrderRepository  orderRepository, IConfiguration configuration)
        {
            _configuration = configuration; 
            _hostName = "127.0.0.1";
            _password = "guest";
            _userName = "guest";

            _checkoutQueueName = _configuration.GetValue<string>("CheckoutQueueName");

            _orderRepository =  orderRepository;

            var fctory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password,
            };
            _connection = fctory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct);
            _channel.QueueDeclare(PaymentOrderUpdateQueueName, false, false, false, null);
            _channel.QueueBind(PaymentOrderUpdateQueueName, ExchangeName, "PaymentOrder");

        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
             {
                 var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                 UpdatePaymentResultMessage updatePaymentResultMessage = JsonConvert.DeserializeObject<UpdatePaymentResultMessage>(content);
                 HandleMessage(updatePaymentResultMessage).GetAwaiter().GetResult();

                 _channel.BasicAck(ea.DeliveryTag, false);
             };
            _channel.BasicConsume(PaymentOrderUpdateQueueName, false, consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessage(UpdatePaymentResultMessage updatePaymentResultMessage)
        {           

            try
            {
                await _orderRepository.UpdateOrderPaymentStatus(updatePaymentResultMessage.OrderId, updatePaymentResultMessage.Status);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
