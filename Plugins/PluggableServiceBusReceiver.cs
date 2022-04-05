using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Plugins;

public class PluggableServiceBusReceiver : ServiceBusReceiver
{
    private readonly IEnumerable<Func<ServiceBusReceivedMessage, Task>> _plugins;

    internal PluggableServiceBusReceiver(ServiceBusClient client, string queueOrSubscriptionName, IEnumerable<Func<ServiceBusReceivedMessage, Task>> plugins, ServiceBusReceiverOptions options)
        : base(client, queueOrSubscriptionName, options)
    {
        _plugins = plugins;
    }

    public override async Task<ServiceBusReceivedMessage> ReceiveMessageAsync(TimeSpan? maxWaitTime = null, CancellationToken cancellationToken = new CancellationToken())
    {
        var message = await base.ReceiveMessageAsync(maxWaitTime, cancellationToken);

        foreach (var plugin in _plugins)
        {
            await plugin.Invoke(message);
        }

        return message;
    }
}