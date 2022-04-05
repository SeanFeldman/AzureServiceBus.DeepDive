using Azure.Messaging.ServiceBus.Administration;

namespace AtomicSends;

using System;
using System.Threading.Tasks;

public static class Prepare
{
    public static async Task Infrastructure(string connectionString, string destination)
    {
        var client = new ServiceBusAdministrationClient(connectionString);
        if (await client.QueueExistsAsync(destination))
        {
            await client.DeleteQueueAsync(destination);
        }
        await client.CreateQueueAsync(destination);
    }

    public static async Task ReportNumberOfMessages(string connectionString, string destination)
    {
        var client = new ServiceBusAdministrationClient(connectionString);

        QueueRuntimeProperties queueProperties = await client.GetQueueRuntimePropertiesAsync(destination);
        Console.WriteLine($"{queueProperties.ActiveMessageCount} messages in '{destination}'");
    }
}