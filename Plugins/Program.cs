namespace Plugins
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;

    internal class Program
    {
        private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

        private static readonly string destination = "queue";

        private static readonly TaskCompletionSource<bool> syncEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        private static async Task Main(string[] args)
        {
            await Prepare.Infrastructure(connectionString, destination);

            var client = new QueueClient(connectionString, destination);
            client.RegisterPlugin(new PrefixSuffixPlugin());

            await client.SendAsync(new Message("Deep Dive".AsByteArray()));
            Console.WriteLine("Message sent");

            client.RegisterMessageHandler(
                (message, token) =>
                {
                    Console.WriteLine($"Received message ID:{message.MessageId} and content:\n{message.Body.AsString()}");
                    syncEvent.TrySetResult(true);
                    return Task.CompletedTask;
                },
                Prepare.Options
            );

            await syncEvent.Task;
            await client.CloseAsync();
        }
    }
}