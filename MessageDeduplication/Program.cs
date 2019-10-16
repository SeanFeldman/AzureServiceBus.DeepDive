namespace MessageDeduplication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;

    internal class Program
    {
        private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

        private static readonly string destination = "queue";

        private static async Task Main(string[] args)
        {
            await Prepare.Stage(connectionString, destination);

            var client = new QueueClient(connectionString, destination);
            try
            {
                var content = "Deep Dive de-duplication".AsByteArray();
                var messageId = new Guid(content.Take(16).ToArray()).ToString();

                var messages = new List<Message>
                {
                    new Message(content) { MessageId = messageId },
                    new Message(content) { MessageId = messageId },
                    new Message(content) { MessageId = messageId }
                };

                await client.SendAsync(messages);

                Console.WriteLine("Messages sent");

                client.RegisterMessageHandler(
                    (message, token) =>
                    {
                        Console.WriteLine($"Received message with '{message.MessageId}' and content '{Encoding.UTF8.GetString(message.Body)}'");

                        return Task.CompletedTask;
                    },
                    new MessageHandlerOptions(
                        exception =>
                        {
                            Console.WriteLine($"Exception: {exception.Exception}");
                            return Task.CompletedTask;
                        })
                    {
                        AutoComplete = true,
                        MaxAutoRenewDuration = TimeSpan.FromSeconds(30)
                    }
                );

                await Task.Delay(TimeSpan.FromSeconds(5));

                await client.SendAsync(messages);
            }
            finally
            {
                await client.CloseAsync();
            }
        }
    }
}