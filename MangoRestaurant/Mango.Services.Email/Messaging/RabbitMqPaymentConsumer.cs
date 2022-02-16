﻿
using Mango.Services.Email.Messages;
using Mango.Services.Email.Repository;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.Email.Messaging
{
    public class RabbitMqPaymentConsumer : BackgroundService
    {       
        private IConnection _connection;
        private IModel _channel;
        private readonly IEmailRepository _emailRepository;
        private const string ExchangeName = "DirectPaymentUpdate_Exchange";
        private const string PaymentEmailUpdateQueueName = "PaymentEmailUpdateQueueName";
        private readonly string _checkoutQueueName;

        private readonly string _hostName;
        private readonly string _password;
        private readonly string _userName;
        string _queueName = "";

        public RabbitMqPaymentConsumer(EmailRepository emailRepository)
        {
           
            _hostName = "127.0.0.1";
            _password = "guest";
            _userName = "guest";

         

            _emailRepository = emailRepository;

            var fctory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password,
            };
            _connection = fctory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct);
            _channel.QueueDeclare(PaymentEmailUpdateQueueName, false, false, false, null);          
            _channel.QueueBind(PaymentEmailUpdateQueueName, ExchangeName, "PaymentEmail");

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
            _channel.BasicConsume(PaymentEmailUpdateQueueName, false, consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessage(UpdatePaymentResultMessage updatePaymentResultMessage)
        {           

            try
            {
                await _emailRepository.SendAndLogEmail(updatePaymentResultMessage);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
