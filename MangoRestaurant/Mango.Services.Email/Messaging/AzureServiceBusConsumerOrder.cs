
using Azure.Messaging.ServiceBus;
using Mango.Services.Email.Messages;
using Mango.Services.Email.Repository;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.Email.Messaging
{
    public class AzureServiceBusConsumerOrder : IAzureServiceBusConsumerOrder
    {

        private readonly string _serviceBusConnectionString;
        private readonly string _smailSubscriptionName;  
        private readonly string _orderUpdatePaymentResultTopic;
        private readonly IEmailRepository _emailRepository;


        private readonly IConfiguration _configuration;
   
        private readonly ServiceBusProcessor _orderUpdatePaymentStatusProcessor;

        public AzureServiceBusConsumerOrder(EmailRepository emailRepository, IConfiguration configuration)
        {
            _emailRepository = emailRepository;   
            _configuration = configuration;

            _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            _smailSubscriptionName = _configuration.GetValue<string>("SmailSubscriptionName");          
            _orderUpdatePaymentResultTopic = _configuration.GetValue<string>("OrderUpdatePaymentResultTopic");            

            var client = new ServiceBusClient(_serviceBusConnectionString);        
            _orderUpdatePaymentStatusProcessor = client.CreateProcessor(_orderUpdatePaymentResultTopic, _smailSubscriptionName);
        }

        public async Task Start()
        {           

            _orderUpdatePaymentStatusProcessor.ProcessMessageAsync += OnOrderPaymentUpdateReceived;
            _orderUpdatePaymentStatusProcessor.ProcessErrorAsync += ErrorHandler;
            await _orderUpdatePaymentStatusProcessor.StartProcessingAsync();
        }     

        public async Task Stop()
        {          
            await _orderUpdatePaymentStatusProcessor.StopProcessingAsync();
            await _orderUpdatePaymentStatusProcessor.DisposeAsync();
        }

        private Task ErrorHandler(ProcessErrorEventArgs arg)
        {
            Console.WriteLine(arg.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task OnOrderPaymentUpdateReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            UpdatePaymentResultMessage objMessage = JsonConvert.DeserializeObject<UpdatePaymentResultMessage>(body);

            
            try
            {
                await _emailRepository.SendAndLogEmail(objMessage);             
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
