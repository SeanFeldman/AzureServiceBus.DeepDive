using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace Plugins;

public static class Prepare
{
    public static readonly ServiceBusProcessorOptions Options = new()
    {
        AutoCompleteMessages = true,
        MaxConcurrentCalls = 1,
        MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10)
    };

    public static async Task Infrastructure(string connectionString, string destination)
    {
        var client = new ServiceBusAdministrationClient(connectionString);
        if (await client.QueueExistsAsync(destination))
        {
            await client.DeleteQueueAsync(destination);
        }
        await client.CreateQueueAsync(destination);
    }

}