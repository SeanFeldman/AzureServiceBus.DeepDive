using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Topologies;
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

        var serviceBusClient = new ServiceBusClient(connectionString);

        var sender = serviceBusClient.CreateSender(topicName);

        var message = new ServiceBusMessage("Message from service A");
        message.Subject = "rush";
        await sender.SendMessageAsync(message);

        message = new ServiceBusMessage("Message from service B");
        message.ApplicationProperties.Add("priority", "high");
        await sender.SendMessageAsync(message);

        await sender.CloseAsync();

        var receiver = serviceBusClient.CreateReceiver(inputQueue);
        try
        {
            var receivedMessages = await receiver.ReceiveMessagesAsync(2);
            foreach (var receivedMessage in receivedMessages)
            {
                var body = receivedMessage.Body.ToString();
                var label = receivedMessage.Subject;
                var priority = receivedMessage.Subject ?? receivedMessage.ApplicationProperties["priority"];

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