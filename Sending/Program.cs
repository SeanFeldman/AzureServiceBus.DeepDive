using System;
using Azure.Messaging.ServiceBus;
using Sending;

var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");
var destination = "queue";

await Prepare.Infrastructure(connectionString, destination);

var serviceBusClient = new ServiceBusClient(connectionString);

var client = serviceBusClient.CreateSender(destination);
try
{
    var message = new ServiceBusMessage("Payload")
    {
        Subject = "Deep Dive" // Label
    };
    message.ApplicationProperties.Add("Machine", Environment.MachineName);

    await client.SendMessageAsync(message);

    Console.WriteLine("Message sent");
}
finally
{
    await client.CloseAsync();
}