namespace MessageForwarding
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;

    internal class Program
    {
        private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

        private static async Task Main(string[] args)
        {
            await Prepare.Infrastructure(connectionString);

            var client = new QueueClient(connectionString, "queue0");
            var message = new Message();
            message.Body = "Deep Dive".AsByteArray();
            await client.SendAsync(message);
            await client.CloseAsync();

            Console.WriteLine("Message sent to 'queue0''");

            var queue = "queue4";
            var receiver = new MessageReceiver(connectionString, queue);

            var receivedMessage = await receiver.ReceiveAsync();
            await receiver.CompleteAsync(receivedMessage.SystemProperties.LockToken);

            Console.WriteLine($"Got '{receivedMessage.Body.AsString()}' on queue '{queue}'");
            await receiver.CloseAsync();
            Console.ReadLine();
            Console.WriteLine("Add forwarding from 'queue4' to 'queue5''");

            await Prepare.AddExtraHop(connectionString);

            client = new QueueClient(connectionString, "queue0");
            message = new Message();
            message.Body = Encoding.UTF8.GetBytes("Forwarding");
            await client.SendAsync(message);

            queue = "queue5";
            receiver = new MessageReceiver(connectionString, queue);
            receivedMessage = await receiver.ReceiveAsync(TimeSpan.FromSeconds(5));

            var what = receivedMessage == null ? "nothing" : $"{receivedMessage.Body.AsString()}"; 
            Console.WriteLine($"Got {what} on queue '{queue}'");

            await receiver.CloseAsync();
        }
    }
}