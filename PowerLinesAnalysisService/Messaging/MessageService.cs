using System;
using PowerLinesAnalysisService.Data;
using PowerLinesAnalysisService.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PowerLinesAnalysisService.Analysis;
using PowerLinesMessaging;

namespace PowerLinesAnalysisService.Messaging
{
    public class MessageService : BackgroundService, IMessageService
    {
        private IConsumer resultsConsumer;
        private IConsumer analysisConsumer;
        private MessageConfig messageConfig;
        private IServiceScopeFactory serviceScopeFactory;
        private ISender sender;

        public MessageService(IConsumer resultsConsumer, IConsumer analysisConsumer, ISender sender, MessageConfig messageConfig, IServiceScopeFactory serviceScopeFactory)
        {
            this.resultsConsumer = resultsConsumer;
            this.analysisConsumer = analysisConsumer;
            this.messageConfig = messageConfig;
            this.serviceScopeFactory = serviceScopeFactory;
            this.sender = sender;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {            
            Listen();
            return Task.CompletedTask;
        }

        public void Listen()
        {
            CreateConnectionToQueue();
            resultsConsumer.Listen(new Action<string>(ReceiveResultMessage));
            analysisConsumer.Listen(new Action<string>(ReceiveAnalysisMessage));
        }

        public void CreateConnectionToQueue()
        {
            var oddsOptions = new SenderOptions
            {
                Host = messageConfig.Host,
                Port = messageConfig.Port,
                Username = messageConfig.OddsUsername,
                Password = messageConfig.OddsPassword,
                QueueName = messageConfig.OddsQueue,
                QueueType = QueueType.ExchangeDirect
            };

            sender.CreateConnectionToQueue(oddsOptions);

            var resultOptions = new ConsumerOptions
            {
                Host = messageConfig.Host,
                Port = messageConfig.Port,
                Username = messageConfig.ResultUsername,
                Password = messageConfig.ResultPassword,
                QueueName = messageConfig.ResultQueue,
                SubscriptionQueueName = "power-lines-results-analysis",
                QueueType = QueueType.ExchangeFanout,
            
            };

            resultsConsumer.CreateConnectionToQueue(resultOptions);

            var analysisOptions = new ConsumerOptions
            {
                Host = messageConfig.Host,
                Port = messageConfig.Port,
                Username = messageConfig.AnalysisUsername,
                Password = messageConfig.AnalysisPassword,
                QueueName = messageConfig.AnalysisQueue,
                SubscriptionQueueName = "power-lines-analysis-analysis",
                QueueType = QueueType.ExchangeFanout,
            
            };

            analysisConsumer.CreateConnectionToQueue(analysisOptions);
        }

        private void ReceiveResultMessage(string message)
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

        private void ReceiveAnalysisMessage(string message)
        {
            var analysisMessage = JsonConvert.DeserializeObject<AnalysisMessage>(message);
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var analysisService = scope.ServiceProvider.GetRequiredService<IAnalysisService>();
                try
                {
                    var matchOdds = analysisService.GetMatchOdds(analysisMessage.Fixture);
                    sender.SendMessage(matchOdds, analysisMessage.Sender);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to calculate match odds for {0} v {1}: {2}", analysisMessage.Fixture.HomeTeam, analysisMessage.Fixture.AwayTeam, ex);
                }
            }
        }
    }
}
