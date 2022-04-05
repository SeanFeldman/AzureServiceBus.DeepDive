using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Plugins;

public static class ServiceBusClientExtensions
{
    public static PluggableServiceBusSender CreateSender(this ServiceBusClient client, string queueOrTopicName, IEnumerable<Func<ServiceBusMessage, Task>> plugins) =>
        new PluggableServiceBusSender(client, queueOrTopicName, plugins);

    public static PluggableServiceBusReceiver CreateReceiver(this ServiceBusClient client, string queueOrSubscriptionName, IEnumerable<Func<ServiceBusReceivedMessage, Task>> plugins, ServiceBusReceiverOptions options = default) =>
        new PluggableServiceBusReceiver(client, queueOrSubscriptionName, plugins, options ?? new ServiceBusReceiverOptions());
}