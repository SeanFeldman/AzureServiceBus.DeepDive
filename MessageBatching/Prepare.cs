using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;

namespace MessageBatching;

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

        NamespaceProperties namespaceProperties = await client.GetNamespacePropertiesAsync();
        Console.WriteLine($"Namespace '{namespaceProperties.Name}' info");
        Console.WriteLine($"SKU: {namespaceProperties.MessagingSku}");
        Console.WriteLine($"Messaging units: {namespaceProperties.MessagingUnits}\n");
    }
}