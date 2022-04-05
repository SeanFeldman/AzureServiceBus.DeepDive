using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core.Amqp;
using Azure.Messaging.ServiceBus;

namespace Plugins;

internal class Program
{
    private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");
    private static readonly string queue = "queue";

    private static async Task Main(string[] args)
    {
        await Prepare.Infrastructure(connectionString, queue);

        var serviceBusClient = new ServiceBusClient(connectionString);

        var sender = serviceBusClient.CreateSender(queue, new List<Func<ServiceBusMessage, Task>>
        {
            Plugins.PrefixPlugin
        });

        await sender.SendMessageAsync(new ServiceBusMessage("Deep Dive"));
        Console.WriteLine("Message sent");

        var receiver = serviceBusClient.CreateReceiver(queue, new List<Func<ServiceBusReceivedMessage, Task>>
        {
            Plugins.PostfixPlugin
        });

        var message = await receiver.ReceiveMessageAsync();
        Console.WriteLine($"Received message ID:{message.MessageId} and content:\n{message.Body}");

        await serviceBusClient.DisposeAsync();
    }
}

public class Plugins
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

        // WARNING - more sophisticated than this
        // See https://github.com/Azure/azure-sdk-for-net/issues/12943 for discussion
        var original = Encoding.UTF8.GetString(data.First().Span);
        var modified = $"{original}{Environment.NewLine}---SUFFIX---";
        var bytes = Encoding.UTF8.GetBytes(modified);

        amqpMessage.Body = new AmqpMessageBody(new ReadOnlyMemory<byte>[] { new(bytes) });

        return Task.CompletedTask;
    };
}