namespace AtomicSends
{
    using System;
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
            await client.CreateQueueAsync(destination);
            await client.CloseAsync();
        }

        public static async Task ReportNumberOfMessages(string connectionString, string destination)
        {
            var client = new ManagementClient(connectionString);

            var info = await client.GetQueueRuntimeInfoAsync(destination);
            Console.WriteLine($"{info.MessageCount} messages in '{destination}'");

            await client.CloseAsync();
        }
    }
}