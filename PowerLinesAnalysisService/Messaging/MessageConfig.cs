using System;
using System.Configuration;

namespace PowerLinesAnalysisService.Messaging
{
    public class MessageConfig
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Transport { get; set; }

        public string ResultQueue { get; set; }

        public string ResultUsername { get; set; }

        public string ResultPassword { get; set; }
    }
}
