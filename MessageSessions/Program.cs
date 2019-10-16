namespace MessageSessions
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;

    internal class Program
    {
        private static readonly string connectionString =
            Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

        private static readonly string destination = "queue";

        private static TaskCompletionSource<bool> syncEvent =
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        private static async Task Main(string[] args)
        {
            await Prepare.Infrastructure(connectionString, destination);

            var client = new QueueClient(connectionString, destination);
            try
            {
                var messages = new List<Message>
                {
                    new Message("Orange 1".AsByteArray()) {SessionId = "Orange"},
                    new Message("Green 1".AsByteArray()) {SessionId = "Green"},
                    new Message("Blue 1".AsByteArray()) {SessionId = "Blue"},
                    new Message("Green 2".AsByteArray()) {SessionId = "Green"},
                    new Message("Orange 2".AsByteArray()) {SessionId = "Orange"},
                    new Message("Blue 2".AsByteArray()) {SessionId = "Blue"},
                    new Message("Green 3".AsByteArray()) {SessionId = "Green"},
                    new Message("Orange 3".AsByteArray()) {SessionId = "Orange"},
                    new Message("Green 4".AsByteArray()) {SessionId = "Green"},
                    new Message("Purple 1".AsByteArray()) {SessionId = "Purple"},
                    new Message("Blue 3".AsByteArray()) {SessionId = "Blue"},
                    new Message("Orange 4".AsByteArray()) {SessionId = "Orange"}
                };

                await client.SendAsync(messages);

                
                Console.WriteLine("Messages sent");

                client.RegisterSessionHandler(
                    (session, message, token) =>
                    {
                        Console.WriteLine(
                            $"Received message for session '{session.SessionId}' ID:'{message.MessageId}' and content:'{message.Body.AsString()}'");
                        return Task.CompletedTask;
                    },
                    new SessionHandlerOptions(
                        exception =>
                        {
                            Console.WriteLine($"Exception: {exception.Exception}");
                            Console.WriteLine($"Action: {exception.ExceptionReceivedContext.Action}");
                            Console.WriteLine($"ClientId: {exception.ExceptionReceivedContext.ClientId}");
                            Console.WriteLine($"Endpoint: {exception.ExceptionReceivedContext.Endpoint}");
                            Console.WriteLine($"EntityPath: {exception.ExceptionReceivedContext.EntityPath}");
                            return Task.CompletedTask;
                        })
                    {
                        MaxConcurrentSessions = 1,
                        MessageWaitTimeout = TimeSpan.FromSeconds(2)
                    }
                );

                Console.ReadLine();
            }
            finally
            {
                await client.CloseAsync();
            }
        }
    }
}