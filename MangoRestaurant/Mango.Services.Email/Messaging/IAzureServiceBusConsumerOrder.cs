namespace Mango.Services.Email.Messaging
{
    public interface IAzureServiceBusConsumerOrder
    {
        Task Start();
        Task Stop();
    }
}
