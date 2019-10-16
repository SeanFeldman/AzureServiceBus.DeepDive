namespace MessageExpiry
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
            await Prepare.Stage(connectionString, destination);

            var client = new QueueClient(connectionString, destination);

            var message = new Message
            {
                Body = "Deep Dive".AsByteArray(),
                // if not set the default time to live on the queue counts
                TimeToLive = TimeSpan.FromSeconds(10)
            };
            
            await client.SendAsync(message);
            Console.WriteLine("Sent message");

            // Note that expired messages are only purged and moved to the DLQ when there is at least one
            // active receiver pulling from the main queue or subscription; that behavior is by design.
            //await Task.Delay(TimeSpan.FromSeconds(15));
            await Prepare.SimulateActiveReceiver(client);

            Console.WriteLine("Message expired");

            await client.CloseAsync();
        }
    }
}