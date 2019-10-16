using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace TransferDLQ
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
            await client.CreateQueueAsync(new QueueDescription(inputQueue) { MaxDeliveryCount = 1 });

            if (await client.QueueExistsAsync(destinationQueue))
            {
                await client.DeleteQueueAsync(destinationQueue);
            }
            await client.CreateQueueAsync(destinationQueue);

            await client.CloseAsync();
        }

        public static async Task DisableDestination(string connectionString, string destinationQueue)
        {
            var client = new ManagementClient(connectionString);

            if (await client.QueueExistsAsync(destinationQueue))
            {
                var description = new QueueDescription(destinationQueue)
                {
                    Status = EntityStatus.SendDisabled
                };
                await client.UpdateQueueAsync(description);
            }

            await client.CloseAsync();
        }

        public static async Task ReportNumberOfMessages(string connectionString, string input, string destination)
        {
            var client = new ManagementClient(connectionString);
            var inputInfo = await client.GetQueueRuntimeInfoAsync(input);
            var destinationInfo = await client.GetQueueRuntimeInfoAsync(destination);

            var activeMessageCount = inputInfo.MessageCountDetails.ActiveMessageCount;
            var deadLetterMessageCount = inputInfo.MessageCountDetails.DeadLetterMessageCount;
            var transferDeadLetterMessageCount = inputInfo.MessageCountDetails.TransferDeadLetterMessageCount;
            var activeMessageCountDestination = destinationInfo.MessageCountDetails.ActiveMessageCount;

            var inputDeadLetterPath = EntityNameHelper.FormatDeadLetterPath(input);
            var inputTransferDeadLetterPath = EntityNameHelper.FormatTransferDeadLetterPath(input);

            Console.WriteLine($"{activeMessageCount} messages in '{input}'");
            Console.WriteLine($"{deadLetterMessageCount} messages in '{inputDeadLetterPath}'");
            Console.WriteLine($"{transferDeadLetterMessageCount} messages in '{inputTransferDeadLetterPath}'");
            Console.WriteLine($"{activeMessageCountDestination} messages in '{destination}'");
            Console.WriteLine();

            await client.CloseAsync();
        }
    }
}