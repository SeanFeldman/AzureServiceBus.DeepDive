using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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