using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace MessageBatching;

internal class Program
{
    private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

    private static readonly string destination = "queue";

    private static async Task Main(string[] args)
    {
        await Prepare.Infrastructure(connectionString, destination);

        var serviceBusClient = new ServiceBusClient(connectionString);

        var sender = serviceBusClient.CreateSender(destination);

        var messages = new List<ServiceBusMessage>();
        for (var i = 0; i < 10; i++)
        {
            var message = new ServiceBusMessage($"Deep Dive{i}");
            messages.Add(message);
        }

        Console.WriteLine($"Sending {messages.Count} messages in a batch.");
        await sender.SendMessagesAsync(messages);
        messages.Clear();
        Console.WriteLine();

        for (var i = 0; i < 6500; i++)
        {
            var message = new ServiceBusMessage($"Deep Dive{i}");
            messages.Add(message);
        }

        await sender.SendMessagesAsync(messages);

        try
        {
            Console.WriteLine($"Sending {messages.Count} messages in a batch.");
            await sender.SendMessagesAsync(messages);
        }
        catch (ServiceBusException ex)
        {
            Console.Error.WriteLine($"{ex.Reason}: {ex.Message}");
        }
    }
}