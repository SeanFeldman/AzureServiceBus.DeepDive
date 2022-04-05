using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;

namespace MessageDeduplication;

public static class Prepare
{
    public static async Task Stage(string connectionString, string destination)
    {
        var client = new ServiceBusAdministrationClient(connectionString);
        if (await client.QueueExistsAsync(destination))
        {
            await client.DeleteQueueAsync(destination);
        }

        var createQueueOptions = new CreateQueueOptions(destination)
        {
            RequiresDuplicateDetection = true,
            DuplicateDetectionHistoryTimeWindow = TimeSpan.FromSeconds(20)
        };

        await client.CreateQueueAsync(createQueueOptions);
    }
}