using System;

namespace PowerLinesAnalysisService.Messaging
{
    public interface IMessageService
    {
        void Listen();
        void CreateConnectionToQueue();
    }
}
