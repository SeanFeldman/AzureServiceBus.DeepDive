using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;

namespace Topologies;
public static class Prepare
{
    public static async Task Infrastructure(string connectionString, string inputQueue, string topicName, string serviceASubscription, string serviceBSubscription)
    {
        var client = await Cleanup(connectionString, inputQueue, topicName);

        var subscriptionOptions = new CreateSubscriptionOptions(topicName, serviceASubscription)
        {
            ForwardTo = inputQueue
        };
        await client.CreateSubscriptionAsync(subscriptionOptions);

        subscriptionOptions = new CreateSubscriptionOptions(topicName, serviceBSubscription)
        {
            ForwardTo = inputQueue
        };
        await client.CreateSubscriptionAsync(subscriptionOptions);

        await client.DeleteRuleAsync(topicName, serviceASubscription, "$Default");
        await client.DeleteRuleAsync(topicName, serviceBSubscription, "$Default");

        var ruleOptions = new CreateRuleOptions
        {
            Name = "MessagesFromServiceA",
            Filter = new CorrelationRuleFilter
            {
                Subject = "rush"
            }
        };
        await client.CreateRuleAsync(topicName, serviceASubscription, ruleOptions);

        ruleOptions = new CreateRuleOptions
        {
            Name = "MessagesFromServiceB",
            Filter = new SqlRuleFilter("user.priority in ('high', 'normal', 'low')"),
            Action = new SqlRuleAction("SET sys.Label = user.priority")
        };
        await client.CreateRuleAsync(topicName, serviceBSubscription, ruleOptions);
    }

    private static async Task<ServiceBusAdministrationClient> Cleanup(string connectionString, string inputQueue, string topicName)
    {
        var client = new ServiceBusAdministrationClient(connectionString);

        if (await client.TopicExistsAsync(topicName))
        {
            await client.DeleteTopicAsync(topicName);
        }

        var topicOptions = new CreateTopicOptions(topicName);
        await client.CreateTopicAsync(topicOptions);

        if (await client.QueueExistsAsync(inputQueue))
        {
            await client.DeleteQueueAsync(inputQueue);
        }

        var queueOptions = new CreateQueueOptions(inputQueue);
        await client.CreateQueueAsync(queueOptions);
        return client;
    }
}