using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace MessageDeadlettering;

internal class Program
{
    private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

    private static readonly string destination = "queue";

    private static async Task Main(string[] args)
    {
        await Prepare.Infrastructure(connectionString, destination);

        var serviceBusClient = new ServiceBusClient(connectionString);

        var sender = serviceBusClient.CreateSender(destination);

        var message1 = new ServiceBusMessage("Deep Dive 1");
        message1.Subject = "first";
        message1.TimeToLive = TimeSpan.FromSeconds(1);
        await sender.SendMessageAsync(message1);
        Console.WriteLine("Sent first message");

        var message2 = new ServiceBusMessage("Deep Dive 2");
        message2.Subject = "second";
        await sender.SendMessageAsync(message2);
        Console.WriteLine("Sent second message");

        var message3 = new ServiceBusMessage("Deep Dive 3");
        message3.Subject = "third";
        await sender.SendMessageAsync(message3);
        Console.WriteLine("Sent third message");

        await Task.Delay(2_000);

        var processor = serviceBusClient.CreateProcessor(destination,
            new ServiceBusProcessorOptions { AutoCompleteMessages = false, MaxConcurrentCalls = 3 });

        processor.ProcessMessageAsync += async args =>
        {
            switch (args.Message.Subject)
            {
                case "first":
                    throw new InvalidOperationException("Should never received the first message.");

                case "second":
                    await args.AbandonMessageAsync(args.Message);
                    Console.WriteLine("Abandon the second message --> delivery count will exceed the maximum");
                    break;

                case "third":
                    Console.WriteLine("Dead-letter the third message explicitly");
                    await args.DeadLetterMessageAsync(args.Message, new Dictionary<string, object>
                    {
                        { "Reason", "Because we can!" },
                        { "When", DateTime.UtcNow }
                    });
                    break;
            }
        };

        try
        {
            await processor.StartProcessingAsync();

            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            await processor.DisposeAsync();
        }
    }
}