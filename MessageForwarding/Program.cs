using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace MessageForwarding;

internal class Program
{
    private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

    private static async Task Main(string[] args)
    {
        await Prepare.Infrastructure(connectionString);

        var serviceBusClient = new ServiceBusClient(connectionString);

        var sender = serviceBusClient.CreateSender("queue0");
        var message = new ServiceBusMessage("Deep Dive");
        await sender.SendMessageAsync(message);
        await sender.CloseAsync();

        Console.WriteLine("Message sent to 'queue0''");

        var queue = "queue4";
        var receiver = serviceBusClient.CreateReceiver(queue);

        var receivedMessage = await receiver.ReceiveMessageAsync();
        await receiver.CompleteMessageAsync(receivedMessage);

        Console.WriteLine($"Got '{receivedMessage.Body}' on queue '{queue}'");
        await receiver.CloseAsync();
        Console.ReadLine();
        Console.WriteLine("Add forwarding from 'queue4' to 'queue5''");

        await Prepare.AddExtraHop(connectionString);

        sender = serviceBusClient.CreateSender("queue0");
        message = new ServiceBusMessage("Forwarding");
        await sender.SendMessageAsync(message);

        queue = "queue5";
        receiver = serviceBusClient.CreateReceiver(queue);
        receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));

        var what = receivedMessage == null ? "nothing" : $"{receivedMessage.Body}";
        Console.WriteLine($"Got {what} on queue '{queue}'");

        await receiver.CloseAsync();
    }
}