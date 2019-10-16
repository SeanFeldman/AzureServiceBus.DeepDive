namespace PubSub
{
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Management;

    public static class Prepare
    {
        public static async Task Infrastructure(string connectionString, string topicName, string rushSubscription,
            string currencySubscription)
        {
            var client = await Cleanup(connectionString, topicName, rushSubscription, currencySubscription);

            var subscriptionDescription = new SubscriptionDescription(topicName, rushSubscription);
            await client.CreateSubscriptionAsync(subscriptionDescription);
            
            subscriptionDescription = new SubscriptionDescription(topicName, currencySubscription);
            await client.CreateSubscriptionAsync(subscriptionDescription);

            await client.DeleteRuleAsync(topicName, rushSubscription, RuleDescription.DefaultRuleName);
            await client.DeleteRuleAsync(topicName, currencySubscription, RuleDescription.DefaultRuleName);

            var ruleDescription = new RuleDescription
            {
                Name = "MessagesWithRushlabel",
                Filter = new CorrelationFilter
                {
                    Label = "rush"
                },
                Action = null
            };
            await client.CreateRuleAsync(topicName, rushSubscription, ruleDescription);

            ruleDescription = new RuleDescription
            {
                Name = "MessagesWithCurrencyCHF",
                Filter = new SqlFilter("currency = 'CHF'"),
                Action = new SqlRuleAction("SET currency = 'Złoty'")
            };
            await client.CreateRuleAsync(topicName, currencySubscription, ruleDescription);

            await client.CloseAsync();
        }

        private static async Task<ManagementClient> Cleanup(string connectionString, string topicName,
            string rushSubscription, string currencySubscription)
        {
            var client = new ManagementClient(connectionString);

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

            var topicDescription = new TopicDescription(topicName);
            await client.CreateTopicAsync(topicDescription);

            return client;
        }
    }
}