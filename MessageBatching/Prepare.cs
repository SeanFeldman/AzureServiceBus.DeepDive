namespace MessageBatching
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus.Management;

    public static class Prepare
    {
        public static async Task Infrastructure(string connectionString, string destination)
        {
            var client = new ManagementClient(connectionString);
            if (await client.QueueExistsAsync(destination))
            {
                await client.DeleteQueueAsync(destination);
            }
            await client.CreateQueueAsync(destination);

            var namespaceInfo = await client.GetNamespaceInfoAsync();
            Console.WriteLine($"Namespace '{namespaceInfo.Name}' info");
            Console.WriteLine($"SKU: {namespaceInfo.MessagingSku}");
            Console.WriteLine($"Messaging units: {namespaceInfo.MessagingUnits}\n");

            await client.CloseAsync();
        }
    }
}