using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace PubSub;

internal class Program
{
    private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

    private static readonly string topicName = "topic";
    private static readonly string rushSubscription = "alwaysInRush";
    private static readonly string currencySubscription = "maybeRich";

    private static async Task Main(string[] args)
    {
        await Prepare.Infrastructure(connectionString, topicName, rushSubscription, currencySubscription);

        var serviceBusClient = new ServiceBusClient(connectionString);

        var sender = serviceBusClient.CreateSender(topicName);

        var message = new ServiceBusMessage("No time, gotta rush!") { Subject = "rush" };
        await sender.SendMessageAsync(message);

        message = new ServiceBusMessage("I'm rich! I have 1,000!") { Subject = "rush" };
        message.ApplicationProperties.Add("currency", "GBP");
        await sender.SendMessageAsync(message);

        await sender.CloseAsync();

        Console.WriteLine("Messages sent");
    }
}