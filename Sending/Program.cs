namespace Sending
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
            try
            {
                var message = new Message
                {
                    Body = Encoding.UTF8.GetBytes("Payload"),
                    Label = "Deep Dive"
                };

                message.UserProperties.Add("Machine", Environment.MachineName);

                await client.SendAsync(message);

                Console.WriteLine("Message Send");
            }
            finally
            {
                await client.CloseAsync();
            }
        }
    }
}