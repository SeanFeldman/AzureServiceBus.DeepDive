using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace MessageSessions;

internal class Program
{
    private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");

    private static readonly string destination = "queue";

    private static async Task Main(string[] args)
    {
        await Prepare.Infrastructure(connectionString, destination);

        var serviceBusClient = new ServiceBusClient(connectionString);

        try
        {
            var messages = new List<ServiceBusMessage>
            {
                new("Orange 1") {SessionId = "Orange"},
                new("Green 1") {SessionId = "Green"},
                new("Blue 1") {SessionId = "Blue"},
                new("Green 2") {SessionId = "Green"},
                new("Orange 2") {SessionId = "Orange"},
                new("Blue 2") {SessionId = "Blue"},
                new("Green 3") {SessionId = "Green"},
                new("Orange 3") {SessionId = "Orange"},
                new("Green 4") {SessionId = "Green"},
                new("Purple 1") {SessionId = "Purple"},
                new("Blue 3") {SessionId = "Blue"},
                new("Orange 4") {SessionId = "Orange"}
            };

            var sender = serviceBusClient.CreateSender(destination);
            await sender.SendMessagesAsync(messages);
            await sender.DisposeAsync();

            Console.WriteLine("Messages sent");

            var options = new ServiceBusSessionProcessorOptions
            {
                MaxConcurrentSessions = 1,
                SessionIdleTimeout = TimeSpan.FromSeconds(2),
                //MaxConcurrentCallsPerSession = 10
            };
            var sessionProcessor = serviceBusClient.CreateSessionProcessor(destination, options);

            sessionProcessor.ProcessMessageAsync += args =>
            {
                Console.WriteLine($"Received message for session '{args.SessionId}' ID:'{args.Message.MessageId}' and content:'{args.Message.Body}'");
                return Task.CompletedTask;
            };

            sessionProcessor.ProcessErrorAsync += args =>
            {
                Console.WriteLine($"EntityPath: {args.EntityPath}");
                Console.WriteLine($"FullyQualifiedNamespace: {args.FullyQualifiedNamespace}");
                Console.WriteLine($"ErrorSource: {args.ErrorSource}");
                Console.WriteLine($"Exception: {args.Exception}");
                return Task.CompletedTask;
            };

            await sessionProcessor.StartProcessingAsync();

            Console.ReadLine();
        }
        finally
        {
            await serviceBusClient.DisposeAsync();
        }
    }
}