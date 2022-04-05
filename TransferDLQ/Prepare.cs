using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;

namespace TransferDLQ;

public static class Prepare
{
    public static async Task Infrastructure(string connectionString, string inputQueue, string destinationQueue)
    {
        var client = new ServiceBusAdministrationClient(connectionString);

        if (await client.QueueExistsAsync(inputQueue))
        {
            await client.DeleteQueueAsync(inputQueue);
        }
        await client.CreateQueueAsync(new CreateQueueOptions(inputQueue) { MaxDeliveryCount = 1 });

        if (await client.QueueExistsAsync(destinationQueue))
        {
            await client.DeleteQueueAsync(destinationQueue);
        }
        await client.CreateQueueAsync(destinationQueue);
    }

    public static async Task DisableDestination(string connectionString, string destinationQueue)
    {
        var client = new ServiceBusAdministrationClient(connectionString);

        if (await client.QueueExistsAsync(destinationQueue))
        {
            QueueProperties properties = await client.GetQueueAsync(destinationQueue);
            properties.Status = EntityStatus.SendDisabled;

            await client.UpdateQueueAsync(properties);
        }
    }

    public static async Task ReportNumberOfMessages(string connectionString, string input, string destination)
    {
        var client = new ServiceBusAdministrationClient(connectionString);
        QueueRuntimeProperties inputProperties = await client.GetQueueRuntimePropertiesAsync(input);
        QueueRuntimeProperties destinationProperties = await client.GetQueueRuntimePropertiesAsync(destination);

        var activeMessageCount = inputProperties.ActiveMessageCount;
        var deadLetterMessageCount = inputProperties.DeadLetterMessageCount;
        var transferDeadLetterMessageCount = inputProperties.TransferDeadLetterMessageCount;
        var activeMessageCountDestination = destinationProperties.ActiveMessageCount;

        var inputDeadLetterPath =  EntityNameFormatter.FormatDeadLetterPath(input);
        var inputTransferDeadLetterPath = EntityNameFormatter.FormatTransferDeadLetterPath(input);

        Console.WriteLine($"{activeMessageCount} messages in '{input}'");
        Console.WriteLine($"{deadLetterMessageCount} messages in '{inputDeadLetterPath}'");
        Console.WriteLine($"{transferDeadLetterMessageCount} messages in '{inputTransferDeadLetterPath}'");
        Console.WriteLine($"{activeMessageCountDestination} messages in '{destination}'");
        Console.WriteLine();
    }
}