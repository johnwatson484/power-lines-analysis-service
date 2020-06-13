namespace PowerLinesAnalysisService.Messaging
{
    public interface ISender
    {
        void CreateConnectionToQueue(string brokerUrl, string queue);

        void CloseConnection();

        void SendMessage(object obj, string routingKey);
    }
}
