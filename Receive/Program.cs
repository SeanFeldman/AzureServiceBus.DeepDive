namespace Receiving
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
            try
            {
                await client.SendAsync(new Message("Deep Dive".AsByteArray()));
                Console.WriteLine("Message sent");

                client.RegisterMessageHandler(
                    async (message, token) =>
                    {
                        Console.WriteLine($"Received message with '{message.MessageId}' and content '{message.Body.AsString()}'");
                        // throw new InvalidOperationException();
                        await client.CompleteAsync(message.SystemProperties.LockToken);
                        syncEvent.TrySetResult(true);
                    },
                    new MessageHandlerOptions(
                        exception =>
                        {
                            Console.WriteLine($"Exception: {exception.Exception}");
                            Console.WriteLine($"Action: {exception.ExceptionReceivedContext.Action}");
                            Console.WriteLine($"ClientId: {exception.ExceptionReceivedContext.ClientId}");
                            Console.WriteLine($"Endpoint: {exception.ExceptionReceivedContext.Endpoint}");
                            Console.WriteLine($"EntityPath: {exception.ExceptionReceivedContext.EntityPath}");
                            return Task.CompletedTask;
                        })
                    {
                        AutoComplete = false,
                        MaxConcurrentCalls = 1,
                        MaxAutoRenewDuration = TimeSpan.FromMinutes(10)
                    }
                );

                await syncEvent.Task;
            }
            finally
            {
                await client.CloseAsync();
            }
        }
    }
}