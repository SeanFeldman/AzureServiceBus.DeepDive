namespace MessageScheduling
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;

    internal class Program
    {
        private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

        private static readonly string destination = "queue";

        private static readonly TaskCompletionSource<bool> syncEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        
        private static async Task Main(string[] args)
        {
            await Prepare.Infrastructure(connectionString, destination);

            var sender = new MessageSender(connectionString, destination);
            var due = DateTimeOffset.UtcNow.AddSeconds(10);

            await sender.ScheduleMessageAsync(new Message($"Deep Dive + {due}".AsByteArray()), due);
            Console.WriteLine($"{DateTimeOffset.UtcNow}: Message scheduled first");

            var sequenceId = await sender.ScheduleMessageAsync(new Message($"Deep Dive + {due}".AsByteArray()), due);
            Console.WriteLine($"{DateTimeOffset.UtcNow}: Message scheduled second");

            await sender.CancelScheduledMessageAsync(sequenceId);
            Console.WriteLine($"{DateTimeOffset.UtcNow}: Canceled second");

            var receiver = new MessageReceiver(connectionString, destination);
            receiver.RegisterMessageHandler((message, token) =>
            {
                Console.WriteLine($"{DateTimeOffset.UtcNow}: Received message with '{message.MessageId}' and content '{message.Body.AsString()}'");
                syncEvent.TrySetResult(true);
                return Task.CompletedTask;
            }, ex => Task.CompletedTask);


            await syncEvent.Task;
            await sender.CloseAsync();
            await receiver.CloseAsync();
        }
    }
}