using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace Topologies
{
    public static class Prepare
    {
        public static async Task Infrastructure(string connectionString, string inputQueue, string topicName, string serviceASubscription, string serviceBSubscription)
        {
            var client = await Cleanup(connectionString, inputQueue, topicName);

            var subscriptionDescription = new SubscriptionDescription(topicName, serviceASubscription)
            {
                ForwardTo = inputQueue
            };
            await client.CreateSubscriptionAsync(subscriptionDescription);
            
            subscriptionDescription = new SubscriptionDescription(topicName, serviceBSubscription)
            {
                ForwardTo = inputQueue
            };
            await client.CreateSubscriptionAsync(subscriptionDescription);

            await client.DeleteRuleAsync(topicName, serviceASubscription, RuleDescription.DefaultRuleName);
            await client.DeleteRuleAsync(topicName, serviceBSubscription, RuleDescription.DefaultRuleName);

            var ruleDescription = new RuleDescription
            {
                Name = "MessagesFromServiceA",
                Filter = new CorrelationFilter
                {
                    Label = "rush"
                }
            };
            await client.CreateRuleAsync(topicName, serviceASubscription, ruleDescription);

            ruleDescription = new RuleDescription
            {
                Name = "MessagesFromServiceB",
                Filter = new SqlFilter("user.priority in ('high', 'normal', 'low')"),
                Action = new SqlRuleAction("SET sys.Label = user.priority")
            };
            await client.CreateRuleAsync(topicName, serviceBSubscription, ruleDescription);

            await client.CloseAsync();
        }

        private static async Task<ManagementClient> Cleanup(string connectionString, string inputQueue, string topicName)
        {
            var client = new ManagementClient(connectionString);

            if (await client.TopicExistsAsync(topicName))
            {
                await client.DeleteTopicAsync(topicName);
            }

            var topicDescription = new TopicDescription(topicName);
            await client.CreateTopicAsync(topicDescription);

            if (await client.QueueExistsAsync(inputQueue))
            {
                await client.DeleteQueueAsync(inputQueue);
            }

            var queueDescription = new QueueDescription(inputQueue);
            await client.CreateQueueAsync(queueDescription);
            return client;
        }
    }
}