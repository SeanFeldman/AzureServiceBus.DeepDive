using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core.Amqp;
using Azure.Messaging.ServiceBus;

namespace Plugins;

public static class Plugins
{
    public static Func<ServiceBusMessage, Task> PrefixPlugin => message =>
    {
        message.Body = new BinaryData($"---PREFIX---{Environment.NewLine}{message.Body}");

        return Task.CompletedTask;
    };

    public static Func<ServiceBusReceivedMessage, Task> PostfixPlugin => message =>
    {
        var amqpMessage = message.GetRawAmqpMessage();

        amqpMessage.Body.TryGetData(out var data);
        amqpMessage.Body.TryGetValue(out var value);

        // WARNING - more sophisticated than this
        // See https://github.com/Azure/azure-sdk-for-net/issues/12943 for discussion
        var original = Encoding.UTF8.GetString(data.First().Span);
        var modified = $"{original}{Environment.NewLine}---SUFFIX---";
        var bytes = Encoding.UTF8.GetBytes(modified);

        amqpMessage.Body = new AmqpMessageBody(new ReadOnlyMemory<byte>[] { new(bytes) });

        return Task.CompletedTask;
    };
}