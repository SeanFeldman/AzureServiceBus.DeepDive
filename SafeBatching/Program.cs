using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace MessageBatching;

internal class Program
{
    private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

    private static readonly string destination = "queue";

    private static async Task Main(string[] args)
    {
        await Prepare.Infrastructure(connectionString, destination);

        var serviceBusClient = new ServiceBusClient(connectionString);
        var sender = serviceBusClient.CreateSender(destination);

        var batchOptions = new CreateMessageBatchOptions
        {
            MaxSizeInBytes = 150
        };
        var batch = await sender.CreateMessageBatchAsync(batchOptions);

        var message = new ServiceBusMessage("hello");
        for (var i = 0; i < 5; i++)
        {
            Console.WriteLine(batch.TryAddMessage(message)
                ? $"Message added to the batch (size: {batch.SizeInBytes})"
                : $"Message cannot fit the batch size {batch.MaxSizeInBytes}");
        }

        await sender.SendMessagesAsync(batch);

        var receiver = serviceBusClient.CreateReceiver(destination,
            new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete});
        var messages = await receiver.ReceiveMessagesAsync(10);

        foreach (var msg in messages)
        {
            Console.WriteLine($"Message with ID: {msg.MessageId}, body: {msg.Body}");
        }
    }
}