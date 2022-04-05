using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Receiving;

public static class Trivia
{
    public static async Task Ask(ServiceBusClient client, string source)
    {
        ServiceBusReceivedMessage message;

        var receiver = client.CreateReceiver(source);

        message = await receiver.ReceiveMessageAsync();
        // or
        var operationTimeout = TimeSpan.FromSeconds(4);
        message = await receiver.ReceiveMessageAsync(operationTimeout);

        IReadOnlyList<ServiceBusReceivedMessage> messages;

        var messageCount = 5;
        messages = await receiver.ReceiveMessagesAsync(messageCount);
        // or
        messages = await receiver.ReceiveMessagesAsync(messageCount, operationTimeout);
    }

    // If there are 5 messages in the queue and 10 are requested, what will be the outcome?
    // If there are 20 messages in the queue and 10 are requested, what will be the outcome?
}