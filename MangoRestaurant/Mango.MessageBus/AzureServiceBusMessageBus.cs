
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.MessageBus
{
    public class AzureServiceBusMessageBus : IMessageBus
    {
        private string _connectionString = "Endpoint=sb://mangocaffe.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=b6D7sdrbtZ7NWYGbRwRd3LtWSsip0O9NrusuwoC6Qkw=";
        public async Task PublishMessage(BaseMessage message, string topicName)
        {
            await using var client = new ServiceBusClient(this._connectionString);

            ServiceBusSender sender = client.CreateSender(topicName);

            var jsonMsg = JsonConvert.SerializeObject(message);
            var msgToSend = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMsg))
            {
                CorrelationId = Guid.NewGuid().ToString()
            };
            await sender.SendMessageAsync(msgToSend);
            await sender.DisposeAsync();
        }
    }
}
