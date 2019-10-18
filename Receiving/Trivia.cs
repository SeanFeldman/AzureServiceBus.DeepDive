namespace Receiving
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;

    public static class Trivia
    {
        public static async Task Hu(MessageReceiver receiver)
        {
            Message message;

            message = await receiver.ReceiveAsync();
            // or
            var operationTimeout = TimeSpan.FromSeconds(4);
            message = await receiver.ReceiveAsync(operationTimeout);

            IList<Message> messages;

            var messageCount = 5;
            messages = await receiver.ReceiveAsync(messageCount);
            // or
            messages = await receiver.ReceiveAsync(messageCount, operationTimeout);
        }

        // If there are 5 messages in the queue and 10 are requested, what will be the outcome?
        // If there are 20 messages in the queue and 10 are requested, what will be the outcome?
    }
}