using System;
using System.Threading.Tasks;
using AtomicSends;
using Azure.Messaging.ServiceBus;

namespace ConnectionsManagement;

internal class Program
{
    private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

    private static readonly string destination = "queue";

    private static async Task Main(string[] args)
    {
        await Prepare.Infrastructure(connectionString, destination);

        Console.WriteLine("Using shared ServiceBusClient");

        {
            var serviceBusClient = new ServiceBusClient(connectionString);

            var sender = serviceBusClient.CreateSender(destination);
            await sender.SendMessageAsync(new ServiceBusMessage("Deep Dive"));
            var receiver = serviceBusClient.CreateReceiver(destination);
            await receiver.ReceiveMessageAsync();

            Console.WriteLine("Press Enter to use different ServiceBusClients");
            Console.ReadLine();

            await sender.CloseAsync();
            await receiver.CloseAsync();
            await serviceBusClient.DisposeAsync();

            GC.Collect();
        }

        {
            var serviceBusClient1 = new ServiceBusClient(connectionString);
            var serviceBusClient2 = new ServiceBusClient(connectionString);

            var sender = serviceBusClient1.CreateSender(destination);
            await sender.SendMessageAsync(new ServiceBusMessage("Deep Dive"));
            var receiver = serviceBusClient2.CreateReceiver(destination);
            await receiver.ReceiveMessageAsync();

            Console.WriteLine("Enter to stop");
            Console.ReadLine();

            await sender.CloseAsync();
            await receiver.CloseAsync();
        }
    }
}