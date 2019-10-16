using System;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace SendVia
{
    internal class Program
    {
        private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

        private static readonly string inputQueue = "queue";
        private static readonly string destinationQueue = "destination";

        private static TaskCompletionSource<bool> syncEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        private static async Task Main(string[] args)
        {
            await Prepare.Infrastructure(connectionString, inputQueue, destinationQueue);

            var client = new QueueClient(connectionString, inputQueue);
            await client.SendAsync(new Message("Deep Dive".AsByteArray()));
            await client.CloseAsync();

            // connection has to be shared
            var connection = new ServiceBusConnection(connectionString);
            var receiver = new MessageReceiver(connection, inputQueue);
            var sender = new MessageSender(connection, destinationQueue, inputQueue);

            var incoming = await receiver.ReceiveAsync();
            Console.WriteLine($"Received message from '{inputQueue}");
            await Prepare.ReportNumberOfMessages(connectionString, inputQueue, destinationQueue);

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await sender.SendAsync(new Message("Message for destination".AsByteArray()));
                
                Console.WriteLine($"Sent message to '{destinationQueue}'");
                await Prepare.ReportNumberOfMessages(connectionString, inputQueue, destinationQueue);

                await receiver.CompleteAsync(incoming.SystemProperties.LockToken);
                Console.WriteLine("Completed incoming message");
                await Prepare.ReportNumberOfMessages(connectionString, inputQueue, destinationQueue);

                scope.Complete();
            }
            Console.WriteLine("Completed scope");
            await Prepare.ReportNumberOfMessages(connectionString, inputQueue, destinationQueue);

            await receiver.CloseAsync();
            await sender.CloseAsync();
            await connection.CloseAsync();
        }
    }
}