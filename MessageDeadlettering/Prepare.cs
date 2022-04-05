using Azure.Messaging.ServiceBus.Administration;

namespace MessageDeadlettering;

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

        var createQueueOptions = new CreateQueueOptions(destination)
        {
            DeadLetteringOnMessageExpiration = true, // default false
            MaxDeliveryCount = 1
        };
        await client.CreateQueueAsync(createQueueOptions);
    }
}