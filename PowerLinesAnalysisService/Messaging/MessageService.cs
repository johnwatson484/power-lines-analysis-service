using PowerLinesAnalysisService.Data;
using PowerLinesAnalysisService.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using PowerLinesAnalysisService.Analysis;
using PowerLinesMessaging;
using Microsoft.Extensions.Options;

namespace PowerLinesAnalysisService.Messaging;

public class MessageService(IOptions<MessageOptions> messageOptions, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly MessageOptions messageOptions = messageOptions.Value;
    private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory;
    private Connection connection;
    private Consumer analysisConsumer;
    private Consumer resultConsumer;
    private Sender sender;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        CreateConnection();
        CreateOddsSender();
        CreateResultConsumer();
        CreateAnalysisConsumer();

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
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
            Host = messageOptions.Host,
            Port = messageOptions.Port,
            Username = messageOptions.Username,
            Password = messageOptions.Password
        };
        connection = new Connection(options);
    }

    protected void CreateResultConsumer()
    {
        var options = new ConsumerOptions
        {
            Name = messageOptions.ResultQueue,
            QueueName = messageOptions.ResultQueue,
            SubscriptionQueueName = messageOptions.ResultSubscription,
            QueueType = QueueType.ExchangeFanout
        };

        resultConsumer = connection.CreateConsumerChannel(options);
    }

    protected void CreateAnalysisConsumer()
    {
        var options = new ConsumerOptions
        {
            Name = messageOptions.AnalysisQueue,
            QueueName = messageOptions.AnalysisQueue,
            SubscriptionQueueName = messageOptions.AnalysisSubscription,
            QueueType = QueueType.ExchangeFanout
        };

        analysisConsumer = connection.CreateConsumerChannel(options);
    }

    public void CreateOddsSender()
    {
        var options = new SenderOptions
        {
            Name = messageOptions.OddsQueue,
            QueueName = messageOptions.OddsQueue,
            QueueType = QueueType.ExchangeDirect
        };

        sender = connection.CreateSenderChannel(options);
    }

    private void ReceiveResultMessage(string message)
    {
        var result = JsonConvert.DeserializeObject<Result>(message);
        using var scope = serviceScopeFactory.CreateScope();
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

    private void ReceiveAnalysisMessage(string message)
    {
        var analysisMessage = JsonConvert.DeserializeObject<AnalysisMessage>(message);
        using var scope = serviceScopeFactory.CreateScope();
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
