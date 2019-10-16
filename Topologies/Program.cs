using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Topologies
{
    internal class Program
    {
        private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

        private static readonly string topicName = "topic";
        private static readonly string rushSubscription = "ServiceASubscription";
        private static readonly string currencySubscription = "ServiceBSubscription";

        private static readonly string inputQueue = "queue";

        private static async Task Main(string[] args)
        {
            await Prepare.Infrastructure(connectionString, inputQueue, topicName, rushSubscription, currencySubscription);

            var client = new MessageSender(connectionString, topicName);

            var message = new Message("Message from service A".AsByteArray());
            message.Label = "rush";
            await client.SendAsync(message);

            message = new Message("Message from service B".AsByteArray());
            message.UserProperties.Add("priority", "high");
            await client.SendAsync(message);

            await client.CloseAsync();

            var receiver = new MessageReceiver(connectionString, inputQueue);
            try
            {
                var receivedMessages = await receiver.ReceiveAsync(2);
                foreach (var receivedMessage in receivedMessages)
                {
                    var body = receivedMessage.Body.AsString();
                    var label = receivedMessage.Label;
                    var priority = receivedMessage.Label ?? receivedMessage.UserProperties["priority"];
                    
                    Console.WriteLine($"Body = '{body}' / Label = '{label}' / Priority = '{priority}'");
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            finally
            {
                await receiver.CloseAsync();
            }
        }
    }
}