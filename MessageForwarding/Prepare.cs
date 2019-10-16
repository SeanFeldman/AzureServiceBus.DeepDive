namespace MessageForwarding
{
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus.Management;

    public static class Prepare
    {
        public static async Task Infrastructure(string connectionString)
        {
            var client = new ManagementClient(connectionString);

            await Task.WhenAll(
                DeleteIfExists("queue"),
                DeleteIfExists("queue0"),
                DeleteIfExists("queue1"),
                DeleteIfExists("queue2"),
                DeleteIfExists("queue3"),
                DeleteIfExists("queue4"),
                DeleteIfExists("queue5")
            );

            var description = new QueueDescription("queue5");
            await client.CreateQueueAsync(description);

            description = new QueueDescription("queue4");
            await client.CreateQueueAsync(description);

            description = new QueueDescription("queue3")
            {
                ForwardTo = "queue4"
            };
            await client.CreateQueueAsync(description);

            description = new QueueDescription("queue2")
            {
                ForwardTo = "queue3"
            };
            await client.CreateQueueAsync(description);

            description = new QueueDescription("queue1")
            {
                ForwardTo = "queue2"
            };
            await client.CreateQueueAsync(description);

            description = new QueueDescription("queue0")
            {
                ForwardTo = "queue1"
            };
            await client.CreateQueueAsync(description);

            await client.CloseAsync();

            async Task DeleteIfExists(string queueName)
            {
                if (await client.QueueExistsAsync(queueName))
                {
                    await client.DeleteQueueAsync(queueName);
                }
            }
        }

        public static async Task AddExtraHop(string connectionString)
        {
            var client = new ManagementClient(connectionString);

            var description = new QueueDescription("queue4")
            {
                ForwardTo = "queue5"
            };

            await client.UpdateQueueAsync(description);

            await client.CloseAsync();
        }
    }
}