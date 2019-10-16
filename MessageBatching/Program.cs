namespace MessageBatching
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using System.Transactions;
    using Microsoft.Azure.ServiceBus;

    internal class Program
    {
        private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

        private static readonly string destination = "queue";

        private static async Task Main(string[] args)
        {
            await Prepare.Infrastructure(connectionString, destination);

            var client = new QueueClient(connectionString, destination);

            var messages = new List<Message>();
            for (var i = 0; i < 10; i++)
            {
                var message = new Message();
                message.Body = $"Deep Dive{i}".AsByteArray();
                messages.Add(message);
            }

            Console.WriteLine($"Sending {messages.Count} messages in a batch.");
            await client.SendAsync(messages);
            messages.Clear();
            Console.WriteLine();

            for (var i = 0; i < 6500; i++)
            {
                var message = new Message();
                message.Body = $"Deep Dive{i}".AsByteArray();
                messages.Add(message);
            }

            try
            {
                Console.WriteLine($"Sending {messages.Count} messages in a batch.");
                await client.SendAsync(messages);
            }
            catch (MessageSizeExceededException ex)
            {
                Console.Error.WriteLine($"{nameof(MessageSizeExceededException)}: {ex.Message}");
            }
        }
    }
}