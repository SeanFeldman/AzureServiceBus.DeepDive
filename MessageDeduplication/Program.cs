using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace MessageDeduplication;

internal class Program
{
    private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

    private static readonly string destination = "queue";

    private static async Task Main(string[] args)
    {
        await Prepare.Stage(connectionString, destination);

        var serviceBusClient = new ServiceBusClient(connectionString);

        var sender = serviceBusClient.CreateSender(destination);
        try
        {
            var content = "Deep Dive de-duplication";
            var messageId = new Guid(content.Take(16).Select(x => (byte)x).ToArray()).ToString();

            var messages = new List<ServiceBusMessage>
            {
                new(content) { MessageId = messageId },
                new(content) { MessageId = messageId },
                new(content) { MessageId = messageId }
            };

            await sender.SendMessagesAsync(messages);

            Console.WriteLine("Messages sent");

            var processor = serviceBusClient.CreateProcessor(destination);

            processor.ProcessMessageAsync += args =>
            {
                Console.WriteLine($"Received message with '{args.Message.MessageId}' and content '{Encoding.UTF8.GetString(args.Message.Body)}'");

                return Task.CompletedTask;
            };

            processor.ProcessErrorAsync += args =>
            {
                Console.WriteLine($"Exception: {args.Exception}");
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync();

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
        finally
        {
            await sender.CloseAsync();
        }
    }
}