namespace Plugins
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;

    class PrefixSuffixPlugin : ServiceBusPlugin
    {
        public override string Name => "PrefixSuffixPlugin";

        public override bool ShouldContinueOnException { get; } = false;

        public override Task<Message> BeforeMessageSend(Message message)
        {
            var currentBody = message.Body.AsString();
            message.Body = $"---PREFIX---{Environment.NewLine}{currentBody}".AsByteArray();
            return Task.FromResult(message);
        }

        public override Task<Message> AfterMessageReceive(Message message)
        {
            var currentBody = message.Body.AsString();
            message.Body = $"{currentBody}{Environment.NewLine}---SUFFIX---".AsByteArray();
            return Task.FromResult(message);
        }
    }
}