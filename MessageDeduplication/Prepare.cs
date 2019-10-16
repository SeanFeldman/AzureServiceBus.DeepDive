namespace MessageDeduplication
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus.Management;

    public static class Prepare
    {
        public static async Task Stage(string connectionString, string destination)
        {
            var client = new ManagementClient(connectionString);
            if (await client.QueueExistsAsync(destination))
            {
                await client.DeleteQueueAsync(destination);
            }

            var queueDescription = new QueueDescription(destination)
            {
                RequiresDuplicateDetection = true,
                DuplicateDetectionHistoryTimeWindow = TimeSpan.FromSeconds(20)
            };
            await client.CreateQueueAsync(queueDescription);
            
            await client.CloseAsync();
        }
    }
}