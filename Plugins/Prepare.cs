namespace Plugins
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Management;

    public static class Prepare
    {
        public static MessageHandlerOptions Options = new MessageHandlerOptions(exception => Task.CompletedTask)
        {
            AutoComplete = true,
            MaxConcurrentCalls = 1,
            MaxAutoRenewDuration = TimeSpan.FromMinutes(10)
        };

        public static async Task Infrastructure(string connectionString, string destination)
        {
            var client = new ManagementClient(connectionString);
            if (await client.QueueExistsAsync(destination))
            {
                await client.DeleteQueueAsync(destination);
            }

            await client.CreateQueueAsync(destination);
            await client.CloseAsync();
        }
    }
}