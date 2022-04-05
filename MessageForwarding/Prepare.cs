using System.Threading.Tasks;
using Azure;
using Azure.Messaging.ServiceBus.Administration;

namespace MessageForwarding;

public static class Prepare
{
    public static async Task Infrastructure(string connectionString)
    {
        var client = new ServiceBusAdministrationClient(connectionString);

        await Task.WhenAll(
            DeleteIfExists("queue"),
            DeleteIfExists("queue0"),
            DeleteIfExists("queue1"),
            DeleteIfExists("queue2"),
            DeleteIfExists("queue3"),
            DeleteIfExists("queue4"),
            DeleteIfExists("queue5")
        );

        var options = new CreateQueueOptions("queue5");
        await client.CreateQueueAsync(options);

        options = new CreateQueueOptions("queue4");
        await client.CreateQueueAsync(options);

        options = new CreateQueueOptions("queue3")
        {
            ForwardTo = "queue4"
        };
        await client.CreateQueueAsync(options);

        options = new CreateQueueOptions("queue2")
        {
            ForwardTo = "queue3"
        };
        await client.CreateQueueAsync(options);

        options = new CreateQueueOptions("queue1")
        {
            ForwardTo = "queue2"
        };
        await client.CreateQueueAsync(options);

        options = new CreateQueueOptions("queue0")
        {
            ForwardTo = "queue1"
        };
        await client.CreateQueueAsync(options);

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
        var client = new ServiceBusAdministrationClient(connectionString);

        QueueProperties properties = await client.GetQueueAsync("queue4");
        properties.ForwardTo = "queue5";

        await client.UpdateQueueAsync(properties);
    }
}