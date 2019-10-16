namespace MessageDeadlettering
{
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

            var description = new QueueDescription(destination)
            {
                EnableDeadLetteringOnMessageExpiration = true, // default false
                MaxDeliveryCount = 1
            };
            await client.CreateQueueAsync(description);

            await client.CloseAsync();
        }
    }
}