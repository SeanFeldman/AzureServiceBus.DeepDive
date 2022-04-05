using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Azure.Messaging.ServiceBus;

namespace AtomicSends;

internal class Program
{
    private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

    private static readonly string destination = "queue";

    private static async Task Main(string[] args)
    {
        await Prepare.Infrastructure(connectionString, destination);

        var serviceBusClient = new ServiceBusClient(connectionString);

        var sender = serviceBusClient.CreateSender(destination);

        // using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        // {
        //     var message = new ServiceBusMessage("Deep Dive 1");
        //     await sender.SendMessageAsync(message);
        //     Console.WriteLine($"Sent message 1 in transaction '{Transaction.Current.TransactionInformation.LocalIdentifier}'");
        //
        //     await Prepare.ReportNumberOfMessages(connectionString, destination);
        //
        //     message = new ServiceBusMessage("Deep Dive 2");
        //     await sender.SendMessageAsync(message);
        //     Console.WriteLine($"Sent message 2 in transaction '{Transaction.Current.TransactionInformation.LocalIdentifier}'");
        //
        //     Console.WriteLine("About to complete transaction scope.");
        //     await Prepare.ReportNumberOfMessages(connectionString, destination);
        //
        //     scope.Complete();
        //     Console.WriteLine("Completed transaction scope.");
        // }
        //
        // await Prepare.ReportNumberOfMessages(connectionString, destination);
        //
        var messages = new List<ServiceBusMessage>();
        // messages.Clear();
        // Console.WriteLine();

        for (var i = 0; i < 101; i++)
        {
            messages.Add(new ServiceBusMessage($"Deep Dive {i}"));
        }

        try
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            Console.WriteLine($"Sending {messages.Count} messages in a batch with in transaction '{Transaction.Current.TransactionInformation.LocalIdentifier}'.");
            await sender.SendMessagesAsync(messages);
            scope.Complete();
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.QuotaExceeded)
        {
            Console.Error.WriteLine(ex.Message);
        }
    }
}