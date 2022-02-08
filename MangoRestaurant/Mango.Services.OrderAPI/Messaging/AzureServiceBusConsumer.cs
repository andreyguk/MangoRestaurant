using AutoMapper;
using Azure.Messaging.ServiceBus;
using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Repository;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.OrderAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {

        private readonly string _serviceBusConnectionString;
        private readonly string _checkoutMessageTopic;
        private readonly string _subscriptionCheckOut;
        private readonly OrderRepository _orderRepository;
        private readonly IMapper _mapper;

        private readonly IConfiguration _configuration;

        private readonly ServiceBusProcessor _checkOutProcessor;

        public AzureServiceBusConsumer(OrderRepository orderRepository, IConfiguration configuration, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _configuration = configuration;

            _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            _checkoutMessageTopic = _configuration.GetValue<string>("CheckoutMessageTopic");
            _subscriptionCheckOut = _configuration.GetValue<string>("SubscriptionCheckOut");

            var client = new ServiceBusClient(_serviceBusConnectionString);
            _checkOutProcessor = client.CreateProcessor(_checkoutMessageTopic, _subscriptionCheckOut);
        }

        public async Task Start()
        {
            _checkOutProcessor.ProcessMessageAsync += OnCheckOutMessageReceived;
            _checkOutProcessor.ProcessErrorAsync += ErrorHandler;
            await _checkOutProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {
            await _checkOutProcessor.StopProcessingAsync();
            await _checkOutProcessor.DisposeAsync();
        }

        private Task ErrorHandler(ProcessErrorEventArgs arg)
        {
            Console.WriteLine(arg.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task OnCheckOutMessageReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            CheckoutHeaderDto checkoutHeader = JsonConvert.DeserializeObject<CheckoutHeaderDto>(body);

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
        }
    }
}
