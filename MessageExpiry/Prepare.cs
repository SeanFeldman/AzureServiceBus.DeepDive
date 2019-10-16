namespace MessageExpiry
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Management;

    public static class Prepare
    {
        public static async Task Stage(string connectionString, string destination)
        {
            var client = new ManagementClient(connectionString);
            if (await client.QueueExistsAsync(destination)) await client.DeleteQueueAsync(destination);

            var description = new QueueDescription(destination)
            {
                MaxDeliveryCount = int.MaxValue
            };
            await client.CreateQueueAsync(description);

            await client.CloseAsync();
        }

        public static async Task SimulateActiveReceiver(QueueClient client)
        {
            client.RegisterMessageHandler(
                async (message, token) =>
                {
                    await client.AbandonAsync(message.SystemProperties.LockToken);
                    await Task.Delay(2000, token);
                },
                new MessageHandlerOptions(exception => Task.CompletedTask)
                {
                    AutoComplete = false
                });

            await Task.Delay(TimeSpan.FromSeconds(15));
        }
    }
}