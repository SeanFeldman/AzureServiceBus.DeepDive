using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Receiving;


    internal class Program
    {
        private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

        private static readonly string destination = "queue";

        private static readonly TaskCompletionSource<bool> syncEvent = new(TaskCreationOptions.RunContinuationsAsynchronously);

        private static async Task Main(string[] args)
        {
            await Prepare.Infrastructure(connectionString, destination);

            var serviceBusClient = new ServiceBusClient(connectionString);

            var sender = serviceBusClient.CreateSender(destination);
            try
            {
                await sender.SendMessageAsync(new ServiceBusMessage("Deep Dive"));
                Console.WriteLine("Message sent");

                var options = new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false,
                    MaxConcurrentCalls = 1,
                    MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10),
                    // PrefetchCount = 10,
                    // ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                    // SubQueue = SubQueue.DeadLetter
                };

                var processor = serviceBusClient.CreateProcessor(destination, options);

                processor.ProcessMessageAsync += async (ProcessMessageEventArgs x) =>
                {
                    Console.WriteLine($"Received message with '{x.Message.MessageId}' and content '{x.Message.Body}'");
                    //throw new InvalidOperationException();
                    await x.CompleteMessageAsync(x.Message);
                    syncEvent.TrySetResult(true);
                };

                processor.ProcessErrorAsync += (ProcessErrorEventArgs x) =>
                {
                    Console.WriteLine($"EntityPath: {x.EntityPath}");
                    Console.WriteLine($"FullyQualifiedNamespace: {x.FullyQualifiedNamespace}");
                    Console.WriteLine($"ErrorSource: {x.ErrorSource}");
                    Console.WriteLine($"Exception: {x.Exception}");

                    return Task.CompletedTask;
                };

                await processor.StartProcessingAsync();
                //await processor.StopProcessingAsync();

                await syncEvent.Task;
            }
            finally
            {
                await sender.CloseAsync();
            }
        }
    }
