using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace SendVia
{
    public static class Prepare
    {
        public static async Task Infrastructure(string connectionString, string inputQueue, string destinationQueue)
        {
            var client = new ManagementClient(connectionString);
            if (await client.QueueExistsAsync(inputQueue))
            {
                await client.DeleteQueueAsync(inputQueue);
            }
            await client.CreateQueueAsync(new QueueDescription(inputQueue) { MaxDeliveryCount = 2 });

            if (await client.QueueExistsAsync(destinationQueue))
            {
                await client.DeleteQueueAsync(destinationQueue);
            }
            await client.CreateQueueAsync(destinationQueue);

            await client.CloseAsync();
        }

        public static async Task ReportNumberOfMessages(string connectionString, string input, string destination)
        {
            var client = new ManagementClient(connectionString);

            var inputInfo = await client.GetQueueRuntimeInfoAsync(input);
            var destinationInfo = await client.GetQueueRuntimeInfoAsync(destination);
            Console.WriteLine($"{inputInfo.MessageCount} messages in '{input}'");
            Console.WriteLine($"{destinationInfo.MessageCount} messages in '{destination}'");
            Console.WriteLine();

            await client.CloseAsync();
        }
    }
}