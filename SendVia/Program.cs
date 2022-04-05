using System;
using System.Threading.Tasks;
using System.Transactions;
using Azure.Messaging.ServiceBus;

namespace CrossEntityTransaction;

internal class Program
{
    private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

    private static readonly string inputQueue = "queue";
    private static readonly string destinationQueue = "destination";

    private static async Task Main(string[] args)
    {
        await Prepare.Infrastructure(connectionString, inputQueue, destinationQueue);

        var options = new ServiceBusClientOptions { EnableCrossEntityTransactions = true };
        var client = new ServiceBusClient(connectionString, options);

        var initiator = client.CreateSender(inputQueue);
        await initiator.SendMessageAsync(new ServiceBusMessage("Deep Dive"));

        var receiver = client.CreateReceiver(inputQueue);
        var sender = client.CreateSender(destinationQueue);

        var receivedMessage = await receiver.ReceiveMessageAsync();
        Console.WriteLine($"Received message from '{inputQueue}");
        await Prepare.ReportNumberOfMessages(connectionString, inputQueue, destinationQueue);

        using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await sender.SendMessageAsync(new ServiceBusMessage("Message for destination"));

            Console.WriteLine($"Sent message to '{destinationQueue}'");
            await Prepare.ReportNumberOfMessages(connectionString, inputQueue, destinationQueue);

            await receiver.CompleteMessageAsync(receivedMessage);

            Console.WriteLine("Completed incoming message");
            await Prepare.ReportNumberOfMessages(connectionString, inputQueue, destinationQueue);

            ts.Complete();
        }

        Console.WriteLine("Completed scope");
        await Prepare.ReportNumberOfMessages(connectionString, inputQueue, destinationQueue);

        await receiver.CloseAsync();
        await sender.CloseAsync();
    }
}