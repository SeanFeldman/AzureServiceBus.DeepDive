namespace MessageExpiry;

using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System;
using System.Threading.Tasks;

public static class Prepare
{
    public static async Task Stage(string connectionString, string destination)
    {
        var client = new ServiceBusAdministrationClient(connectionString);
        if (await client.QueueExistsAsync(destination)) await client.DeleteQueueAsync(destination);

        var description = new CreateQueueOptions(destination)
        {
            MaxDeliveryCount = int.MaxValue
        };
        await client.CreateQueueAsync(description);
    }

    public static async Task SimulateActiveReceiver(ServiceBusClient client, string entity)
    {
        var options = new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        };
        var processor = client.CreateProcessor(entity, options);

        processor.ProcessMessageAsync += async args =>
        {
            await args.AbandonMessageAsync(args.Message);
            Console.WriteLine("(Emulating active receiver w/o receiving messages)");
            await Task.Delay(TimeSpan.FromSeconds(5));
        };

        processor.ProcessErrorAsync += async args =>
        {
            await Task.CompletedTask;
        };

        await processor.StartProcessingAsync();
        await Task.Delay(TimeSpan.FromSeconds(15));
    }
}