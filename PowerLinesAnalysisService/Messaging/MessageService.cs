using System;
using PowerLinesAnalysisService.Data;
using PowerLinesAnalysisService.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace PowerLinesAnalysisService.Messaging
{
    public class MessageService : BackgroundService, IMessageService
    {
        private IConsumer consumer;
        private MessageConfig messageConfig;
        private IServiceScopeFactory serviceScopeFactory;

        public MessageService(IConsumer consumer, MessageConfig messageConfig, IServiceScopeFactory serviceScopeFactory)
        {
            this.consumer = consumer;
            this.messageConfig = messageConfig;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Listen();
            return Task.CompletedTask;
        }

        public void Listen()
        {
            CreateConnectionToQueue();
            consumer.Listen(new Action<string>(ReceiveMessage));
        }

        public void CreateConnectionToQueue()
        {
            Task.Run(() =>
                consumer.CreateConnectionToQueue(new BrokerUrl(messageConfig.Host, messageConfig.Port, messageConfig.ResultUsername, messageConfig.ResultPassword).ToString(),
                messageConfig.ResultQueue))
            .Wait();
        }

        private void ReceiveMessage(string message)
        {
            var result = JsonConvert.DeserializeObject<Result>(message);
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                    dbContext.Results.Add(result);
                    dbContext.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    Console.WriteLine("{0} v {1} {2} exists, skipping", result.HomeTeam, result.AwayTeam, result.Date.Year);
                }
            }
        }
    }
}
