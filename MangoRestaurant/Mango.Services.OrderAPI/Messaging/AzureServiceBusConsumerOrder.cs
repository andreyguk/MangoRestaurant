using AutoMapper;
using Azure.Messaging.ServiceBus;
using Mango.MessageBus;
using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Repository;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.OrderAPI.Messaging
{
    public class AzureServiceBusConsumerOrder : IAzureServiceBusConsumerOrder
    {

        private readonly string _serviceBusConnectionString;
        private readonly string _checkoutMessageTopic;
        private readonly string _subscriptionCheckOut;
        private readonly string _orderPaymentProcessTopic;
        private readonly string _orderUpdatePaymentResultTopic;
        private readonly OrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IMessageBus _messageBus;

        private readonly IConfiguration _configuration;
        

        private readonly ServiceBusProcessor _checkOutProcessor;
        private readonly ServiceBusProcessor _orderUpdatePaymentStatusProcessor;

        public AzureServiceBusConsumerOrder(OrderRepository orderRepository, IConfiguration configuration, IMapper mapper, IMessageBus messageBus)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _messageBus= messageBus;
            _configuration = configuration;

            _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            _checkoutMessageTopic = _configuration.GetValue<string>("CheckoutMessageTopic");
            _subscriptionCheckOut = _configuration.GetValue<string>("SubscriptionCheckOut");
            _orderPaymentProcessTopic = _configuration.GetValue<string>("OrderPaymentProcessTopic");
            _orderUpdatePaymentResultTopic = _configuration.GetValue<string>("OrderUpdatePaymentResultTopic");            

            var client = new ServiceBusClient(_serviceBusConnectionString);
            _checkOutProcessor = client.CreateProcessor(_checkoutMessageTopic);
            _orderUpdatePaymentStatusProcessor = client.CreateProcessor(_orderUpdatePaymentResultTopic, _subscriptionCheckOut);
        }

        public async Task Start()
        {
            _checkOutProcessor.ProcessMessageAsync += OnCheckOutMessageReceived;
            _checkOutProcessor.ProcessErrorAsync += ErrorHandler;
            await _checkOutProcessor.StartProcessingAsync();

            _orderUpdatePaymentStatusProcessor.ProcessMessageAsync += OnOrderPaymentUpdateReceived;
            _orderUpdatePaymentStatusProcessor.ProcessErrorAsync += ErrorHandler;
            await _orderUpdatePaymentStatusProcessor.StartProcessingAsync();
        }

        private async Task OnOrderPaymentUpdateReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            UpdatePaymentResultMessage paymentResultMessage = JsonConvert.DeserializeObject<UpdatePaymentResultMessage>(body);

            await _orderRepository.UpdateOrderPaymentStatus(paymentResultMessage.OrderId, paymentResultMessage.Status);
            await args.CompleteMessageAsync(args.Message);
        }

        public async Task Stop()
        {
            await _checkOutProcessor.StopProcessingAsync();
            await _checkOutProcessor.DisposeAsync();

            await _orderUpdatePaymentStatusProcessor.StopProcessingAsync();
            await _orderUpdatePaymentStatusProcessor.DisposeAsync();
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
                await _messageBus.PublishMessage(paymentRequestMessage, _orderPaymentProcessTopic);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
