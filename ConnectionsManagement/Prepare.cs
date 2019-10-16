namespace ConnectionsManagement
{
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus.Management;

    public static class Prepare
    {
        public static async Task Infrastructure(string connectionString, string destination)
        {
            var client = new ManagementClient(connectionString);
            {
                await client.DeleteQueueAsync(destination);
            }
            await client.CreateQueueAsync(destination);
            await client.CloseAsync();
        }
    }
}