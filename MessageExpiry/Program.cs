using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace MessageExpiry;
internal class Program
{
    private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

    private static readonly string destination = "queue";

    private static async Task Main(string[] args)
    {
        await Prepare.Stage(connectionString, destination);

        var serviceBusClient = new ServiceBusClient(connectionString);

        var sender = serviceBusClient.CreateSender(destination);

        var message = new ServiceBusMessage("Deep Dive")
        {
            // if not set the default time to live on the queue counts
            TimeToLive = TimeSpan.FromSeconds(3)
        };

        await sender.SendMessageAsync(message);
        Console.WriteLine("Message sent");

        // Note that expired messages are only purged and moved to the DLQ when there is at least one
        // active receiver pulling from the main queue or subscription; that behavior is by design.
        await Prepare.SimulateActiveReceiver(serviceBusClient, destination);

        Console.WriteLine("Message must have expired by now");

        await sender.CloseAsync();
    }
}