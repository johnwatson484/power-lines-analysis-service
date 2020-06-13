using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;

namespace PowerLinesAnalysisService.Messaging
{
    public class Consumer : IConsumer
    {
        protected ConnectionFactory connectionFactory;
        protected RabbitMQ.Client.IConnection connection;
        protected IModel channel;
        protected QueueType queueType;
        protected string queue;
        protected string tempQueue;

        public void CreateConnectionToQueue(QueueType queueType, string brokerUrl, string queue)
        {
            this.queueType = queueType;
            this.queue = queue;
            CreateConnectionFactory(brokerUrl);
            CreateConnection();
            CreateChannel();
            CreateQueue();
        }

        public void CloseConnection()
        {
            connection.Close();
        }

        public void Listen(Action<string> messageAction)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                messageAction(message);
            };
            channel.BasicConsume(queue: GetQueueName(),
                                 autoAck: true,
                                 consumer: consumer);
        }

        private void CreateConnectionFactory(string brokerUrl)
        {
            connectionFactory = new ConnectionFactory() {
                Uri = new Uri(brokerUrl)
            };
        }

        private void CreateConnection()
        {
            connection = connectionFactory.CreateConnection();
        }

        private void CreateChannel()
        {
            channel = connection.CreateModel();
        }

        private void CreateQueue()
        {
            switch (queueType)
            {
                case QueueType.Worker:
                    CreateWorkerQueue();
                    break;
                case QueueType.Exchange:
                    CreateExchange();
                    BindQueue();
                    break;
                default:
                    break;
            }
        }

        private void CreateWorkerQueue()
        {
            channel.QueueDeclare(queue: queue,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        private void CreateExchange()
        {
            channel.ExchangeDeclare(queue, ExchangeType.Fanout, true, false);
        }

        

        private void BindQueue()
        {
            tempQueue = channel.QueueDeclare().QueueName;
            channel.QueueBind(queue: tempQueue,
                              exchange: queue,
                              routingKey: "");
        }

        private string GetQueueName()
        {
            return queueType == QueueType.Exchange ? tempQueue : queue;
        }
    }
}
