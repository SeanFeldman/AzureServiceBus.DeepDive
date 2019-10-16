namespace AtomicSends
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using System.Transactions;
    using Microsoft.Azure.ServiceBus;

    internal class Program
    {
        private static readonly string connectionString =
            Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

        private static readonly string destination = "queue";

        private static async Task Main(string[] args)
        {
            await Prepare.Infrastructure(connectionString, destination);

            var client = new QueueClient(connectionString, destination);

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var message = new Message("Deep Dive 1".AsByteArray());
                await client.SendAsync(message);
                Console.WriteLine(
                    $"Sent message 1 in transaction '{Transaction.Current.TransactionInformation.LocalIdentifier}'");

                await Prepare.ReportNumberOfMessages(connectionString, destination);

                message = new Message("Deep Dive 2".AsByteArray());
                await client.SendAsync(message);
                Console.WriteLine(
                    $"Sent message 2 in transaction '{Transaction.Current.TransactionInformation.LocalIdentifier}'");

                Console.WriteLine("About to complete transaction scope.");
                await Prepare.ReportNumberOfMessages(connectionString, destination);

                scope.Complete();
                Console.WriteLine("Completed transaction scope.");
            }

            await Prepare.ReportNumberOfMessages(connectionString, destination);

            var messages = new List<Message>();
            messages.Clear();
            Console.WriteLine();

            for (var i = 0; i < 101; i++)
            {
                var message = new Message();
                message.Body = Encoding.UTF8.GetBytes($"Deep Dive {i}");
                messages.Add(message);
            }

            try
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    Console.WriteLine(
                        $"Sending {messages.Count} messages in a batch with in transaction '{Transaction.Current.TransactionInformation.LocalIdentifier}'.");
                    await client.SendAsync(messages);
                    scope.Complete();
                }
            }
            catch (QuotaExceededException ex)
            {
                Console.Error.WriteLine($"{nameof(QuotaExceededException)}: {ex.Message}");
            }

            await client.CloseAsync();
        }
    }
}