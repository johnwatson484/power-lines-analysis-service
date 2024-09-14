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
    public class MessageService : BackgroundService
    {
        private readonly MessageConfig messageConfig;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private IConnection connection;
        private IConsumer analysisConsumer;
        private IConsumer resultConsumer;
        private ISender sender;

        public MessageService(MessageConfig messageConfig, IServiceScopeFactory serviceScopeFactory)
        {
            this.messageConfig = messageConfig;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public override Task StartAsync(CancellationToken stoppingToken)
        {
            CreateConnection();
            CreateOddsSender();
            CreateResultConsumer();
            CreateAnalysisConsumer();

            return base.StartAsync(stoppingToken);
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            resultConsumer.Listen(new Action<string>(ReceiveResultMessage));
            analysisConsumer.Listen(new Action<string>(ReceiveAnalysisMessage));
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            connection.CloseConnection();
        }

        protected void CreateConnection()
        {
            var options = new ConnectionOptions
            {
                Host = messageConfig.Host,
                Port = messageConfig.Port,
                Username = messageConfig.Username,
                Password = messageConfig.Password
            };
            connection = new Connection(options);
        }

        protected void CreateResultConsumer()
        {
            var options = new ConsumerOptions
            {
                Name = messageConfig.ResultQueue,
                QueueName = messageConfig.ResultQueue,
                SubscriptionQueueName = messageConfig.ResultSubscription,
                QueueType = QueueType.ExchangeFanout
            };

            resultConsumer = connection.CreateConsumerChannel(options);
        }

        protected void CreateAnalysisConsumer()
        {
            var options = new ConsumerOptions
            {
                Name = messageConfig.AnalysisQueue,
                QueueName = messageConfig.AnalysisQueue,
                SubscriptionQueueName = messageConfig.AnalysisSubscription,
                QueueType = QueueType.ExchangeFanout
            };

            analysisConsumer = connection.CreateConsumerChannel(options);
        }

        public void CreateOddsSender()
        {
            var options = new SenderOptions
            {
                Name = messageConfig.OddsQueue,
                QueueName = messageConfig.OddsQueue,
                QueueType = QueueType.ExchangeDirect
            };

            sender = connection.CreateSenderChannel(options);
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
