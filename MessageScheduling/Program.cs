using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace MessageScheduling;

internal class Program
{
    private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

    private static readonly string destination = "queue";

    private static async Task Main(string[] args)
    {
        await Prepare.Infrastructure(connectionString, destination);

        var serviceBusClient = new ServiceBusClient(connectionString);

        var sender = serviceBusClient.CreateSender(destination);
        var due = DateTimeOffset.UtcNow.AddSeconds(10);

        await sender.ScheduleMessageAsync(new ServiceBusMessage($"Deep Dive 1 + {due}"), due);
        Console.WriteLine($"{DateTimeOffset.UtcNow}: Message scheduled first");

        var sequenceId = await sender.ScheduleMessageAsync(new ServiceBusMessage($"Deep Dive 2 + {due}"), due);
        Console.WriteLine($"{DateTimeOffset.UtcNow}: Message scheduled second");

        await sender.CancelScheduledMessageAsync(sequenceId);
        Console.WriteLine($"{DateTimeOffset.UtcNow}: Canceled second");

        var receiver = serviceBusClient.CreateReceiver(destination);
        var message = await receiver.ReceiveMessageAsync();
        Console.WriteLine($"{DateTimeOffset.UtcNow}: Received message with body: '{message.Body}'");

        await sender.CloseAsync();
        await receiver.CloseAsync();
    }
}