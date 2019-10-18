using System.Collections.Generic;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.VisualBasic;

namespace MessageDeadlettering
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;

    internal class Program
    {
        private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

        private static readonly string destination = "queue";

        private static async Task Main(string[] args)
        {
            await Prepare.Infrastructure(connectionString, destination);

            var client = new QueueClient(connectionString, destination);
            
            var message1 = new Message("Deep Dive 1".AsByteArray());
            message1.Label = "first";
            message1.TimeToLive = TimeSpan.FromSeconds(1);
            await client.SendAsync(message1);
            Console.WriteLine("Sent first message");

            var message2 = new Message("Deep Dive 2".AsByteArray());
            message2.Label = "second";
            await client.SendAsync(message2);
            Console.WriteLine("Sent second message");

            var message3 = new Message("Deep Dive 3".AsByteArray());
            message3.Label = "third";
            await client.SendAsync(message3);
            Console.WriteLine("Sent third message");

            await Task.Delay(2_000);

            try
            {
                client.RegisterMessageHandler(
                    async (msg, token) =>
                    {
                        switch (msg.Label)
                        {
                            case "first":
                                throw new InvalidOperationException("Should never received the first message.");

                            case "second":
                                await client.AbandonAsync(msg.SystemProperties.LockToken);
                                Console.WriteLine("Abandon the second message --> delivery count will exceed the maximum");
                                break;
                                
                            case "third":
                                Console.WriteLine("Dead-letter the third message explicitly");
                                await client.DeadLetterAsync(msg.SystemProperties.LockToken, new Dictionary<string, object>
                                {
                                    { "Reason", "Because we can!" },
                                    { "When", DateTime.UtcNow }
                                });
                                break;
                        }
                    },
                    new MessageHandlerOptions(exception => Task.CompletedTask)
                    {
                        AutoComplete = false,
                        MaxConcurrentCalls = 3
                    });

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            finally
            {
                await client.CloseAsync();
            }
        }
    }
}