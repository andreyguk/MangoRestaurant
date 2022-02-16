﻿using Mango.Services.Email.Messaging;

namespace Mango.Services.Email.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IAzureServiceBusConsumerOrder ServiceBusConsumer { get; set; }
        public static IApplicationBuilder UseAzureServiceBusConsumer (this IApplicationBuilder app)
        {
            ServiceBusConsumer = app.ApplicationServices.GetService<IAzureServiceBusConsumerOrder>();
            var hostApplicationLife = app.ApplicationServices.GetService<IHostApplicationLifetime>();

            hostApplicationLife.ApplicationStarted.Register(OnStart);
            hostApplicationLife.ApplicationStopped.Register(OnStop);
            return app;
        }

        private static void OnStop()
        {
            ServiceBusConsumer.Stop();
        }

        private static void OnStart()
        {
            ServiceBusConsumer.Start();
        }
    }
}
