// Source: https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/src/EntityNameFormatter.cs

using Azure.Messaging.ServiceBus;

namespace TransferDLQ;

internal static class EntityNameFormatter
{
    private const string PathDelimiter = @"/";
    private const string SubscriptionsSubPath = "Subscriptions";
    private const string RulesSubPath = "Rules";
    private const string SubQueuePrefix = "$";
    private const string DeadLetterQueueSuffix = "DeadLetterQueue";
    private const string DeadLetterQueueName = SubQueuePrefix + DeadLetterQueueSuffix;
    private const string Transfer = "Transfer";
    private const string TransferDeadLetterQueueName = SubQueuePrefix + Transfer + PathDelimiter + DeadLetterQueueName;

    /// <summary>
    /// Formats the entity path for a receiver or processor taking into account whether using a SubQueue.
    /// </summary>
    public static string FormatEntityPath(string entityPath, SubQueue subQueue)
    {
        return subQueue switch
        {
            SubQueue.None => entityPath,
            SubQueue.DeadLetter => FormatDeadLetterPath(entityPath),
            SubQueue.TransferDeadLetter => FormatTransferDeadLetterPath(entityPath),
            _ => null
        };
    }

    /// <summary>
    /// Formats the dead letter path for either a queue, or a subscription.
    /// </summary>
    /// <param name="entityPath">The name of the queue, or path of the subscription.</param>
    /// <returns>The path as a string of the dead letter entity.</returns>
    public static string FormatDeadLetterPath(string entityPath)
    {
        return EntityNameFormatter.FormatSubQueuePath(entityPath, EntityNameFormatter.DeadLetterQueueName);
    }

    /// <summary>
    /// Formats the subqueue path for either a queue, or a subscription.
    /// </summary>
    /// <param name="entityPath">The name of the queue, or path of the subscription.</param>
    /// <param name="subQueueName"></param>
    /// <returns>The path as a string of the subqueue entity.</returns>
    public static string FormatSubQueuePath(string entityPath, string subQueueName)
    {
        return string.Concat(entityPath, EntityNameFormatter.PathDelimiter, subQueueName);
    }

    /// <summary>
    /// Formats the subscription path, based on the topic path and subscription name.
    /// </summary>
    /// <param name="topicPath">The name of the topic, including slashes.</param>
    /// <param name="subscriptionName">The name of the subscription.</param>
    public static string FormatSubscriptionPath(string topicPath, string subscriptionName)
    {
        return string.Concat(topicPath, PathDelimiter, SubscriptionsSubPath, PathDelimiter, subscriptionName);
    }

    /// <summary>
    /// Formats the rule path, based on the topic path, subscription name and rule name.
    /// </summary>
    /// <param name="topicPath">The name of the topic, including slashes.</param>
    /// <param name="subscriptionName">The name of the subscription.</param>
    /// <param name="ruleName">The name of the rule</param>
    public static string FormatRulePath(string topicPath, string subscriptionName, string ruleName)
    {
        return string.Concat(
            topicPath, PathDelimiter,
            SubscriptionsSubPath, PathDelimiter,
            subscriptionName, PathDelimiter,
            RulesSubPath, PathDelimiter, ruleName);
    }

    /// <summary>
    /// Utility method that creates the name for the transfer dead letter receiver, specified by <paramref name="entityPath"/>
    /// </summary>
    public static string FormatTransferDeadLetterPath(string entityPath)
    {
        return string.Concat(entityPath, PathDelimiter, TransferDeadLetterQueueName);
    }
}
