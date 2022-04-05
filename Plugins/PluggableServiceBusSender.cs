using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Plugins;

public class PluggableServiceBusSender : ServiceBusSender
{
    private readonly IEnumerable<Func<ServiceBusMessage, Task>> _plugins;

    internal PluggableServiceBusSender(ServiceBusClient client, string queueOrTopicName, IEnumerable<Func<ServiceBusMessage, Task>> plugins)
        : base(client, queueOrTopicName)
    {
        _plugins = plugins;
    }

    public override async Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken = default)
    {
        foreach (var plugin in _plugins)
        {
            await plugin.Invoke(message);
        }
        await base.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
    }
}