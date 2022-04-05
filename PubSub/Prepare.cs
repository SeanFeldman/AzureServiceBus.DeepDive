using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;

namespace PubSub;

public static class Prepare
{
    public static async Task Infrastructure(string connectionString, string topicName, string rushSubscription, string currencySubscription)
    {
        var client = await Cleanup(connectionString, topicName, rushSubscription, currencySubscription);

        var subscriptionOptions = new CreateSubscriptionOptions(topicName, rushSubscription);
        await client.CreateSubscriptionAsync(subscriptionOptions);

        subscriptionOptions = new CreateSubscriptionOptions(topicName, currencySubscription);
        await client.CreateSubscriptionAsync(subscriptionOptions);

        await client.DeleteRuleAsync(topicName, rushSubscription,  "$Default");
        await client.DeleteRuleAsync(topicName, currencySubscription, "$Default");

        var ruleOptions = new CreateRuleOptions
        {
            Name = "MessagesWithRushlabel",
            Filter = new CorrelationRuleFilter
            {
                Subject = "rush"
            },
            Action = null
        };
        await client.CreateRuleAsync(topicName, rushSubscription, ruleOptions);

        ruleOptions = new CreateRuleOptions
        {
            Name = "MessagesWithCurrencyGBP",
            Filter = new SqlRuleFilter("currency = 'GBP' OR currency = '£'"),
            Action = new SqlRuleAction("SET status = 'Keep calm and carry on'")
        };
        await client.CreateRuleAsync(topicName, currencySubscription, ruleOptions);
    }

    private static async Task<ServiceBusAdministrationClient> Cleanup(string connectionString, string topicName, string rushSubscription, string currencySubscription)
    {
        var client = new ServiceBusAdministrationClient(connectionString);

        if (await client.SubscriptionExistsAsync(topicName, rushSubscription))
        {
            await client.DeleteSubscriptionAsync(topicName, rushSubscription);
        }

        if (await client.SubscriptionExistsAsync(topicName, currencySubscription))
        {
            await client.DeleteSubscriptionAsync(topicName, currencySubscription);
        }

        if (await client.TopicExistsAsync(topicName))
        {
            await client.DeleteTopicAsync(topicName);
        }

        var topicDescription = new CreateTopicOptions(topicName);
        await client.CreateTopicAsync(topicDescription);

        return client;
    }
}