using AutoMapper;
using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.RabbitMqSender;
using Mango.Services.OrderAPI.Repository;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.OrderAPI.Messaging
{
    public class RabbitMqCheckoutConsumer : BackgroundService
    {
        private readonly OrderRepository _orderRepository;
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IModel _channel;
        private readonly IMapper _mapper;
        private readonly string _hostName;
        private readonly string _password;
        private readonly string _userName;
        private readonly string _checkoutQueueName;

        private readonly IRabbitMqOrderMessageSender  _rabbitMqOrderMessageSender;
        public RabbitMqCheckoutConsumer(OrderRepository orderRepository, IConfiguration configuration, IMapper mapper, IRabbitMqOrderMessageSender rabbitMqOrderMessageSender)
        {
            _orderRepository = orderRepository;
            _hostName = "127.0.0.1";
            _password = "guest";
            _userName = "guest";
            _configuration = configuration;
            _mapper = mapper;
            _checkoutQueueName = _configuration.GetValue<string>("CheckoutQueueName");
            _rabbitMqOrderMessageSender = rabbitMqOrderMessageSender;

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
                 CheckoutHeaderDto checkoutHeader = JsonConvert.DeserializeObject<CheckoutHeaderDto>(content);
                 HandleMessage(checkoutHeader).GetAwaiter().GetResult();

                 _channel.BasicAck(ea.DeliveryTag, false);
             };
            _channel.BasicConsume(_checkoutQueueName,false,consumer);
            return Task.CompletedTask;
        }

        private  async Task HandleMessage(CheckoutHeaderDto checkoutHeader)
        {
            OrderHeader orderHeader = _mapper.Map<OrderHeader>(checkoutHeader);
            orderHeader.OrderDateTime = DateTime.Now;
            var orderDetails = new List<OrderDetail>();

            foreach (var detailList in checkoutHeader.CartDetails)
            {
                var orderDetail = new OrderDetail()
                {
                    ProductId = detailList.ProductId,
                    ProductName = detailList.Product.Name,
                    Price = detailList.Product.Price,
                    Count = detailList.Count
                };
                orderHeader.CartTotalItems += detailList.Count;
                orderDetails.Add(orderDetail);
            }

            orderHeader.OrderDetails = orderDetails;
            await _orderRepository.AddOrder(orderHeader);

            PaymentRequestMessage paymentRequestMessage = new PaymentRequestMessage
            {
                Name = orderHeader.FirstName + " " + orderHeader.LastName,
                CardNumber = orderHeader.CardNumber,
                CVV = orderHeader.CVV,
                ExpiryMonthYear = orderHeader.ExpiryMonthYear,
                OrderId = orderHeader.OrderHeaderId,
                OrderTotal = orderHeader.OrderTotal,
                Email = orderHeader.Email,
            };

            try
            {
                _rabbitMqOrderMessageSender.SendMessage(paymentRequestMessage, "orderPaymentProcessTopic");
               
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
