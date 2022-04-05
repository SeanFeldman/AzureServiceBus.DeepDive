using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;

namespace MessageSessions;

public static class Prepare
{
    public static async Task Infrastructure(string connectionString, string destination)
    {
        var client = new ServiceBusAdministrationClient(connectionString);
        if (await client.QueueExistsAsync(destination))
        {
            await client.DeleteQueueAsync(destination);
        }

        var queueDescription = new CreateQueueOptions(destination)
        {
            RequiresSession = true
        };
        await client.CreateQueueAsync(queueDescription);
    }
}