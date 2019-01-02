using AzureClients.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;

namespace AzureClients
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //var key = KeyVaultHandler.GetVaultKeyValueAsync().Result;
                //ServiceBusMessageSender.SendMessages().GetAwaiter().GetResult();
                //ServiceBusMessageReceiver.ReadMessages().GetAwaiter().GetResult();

                TestQueueClient();
                //TestRedisClientManager();

            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                throw;
            }

        }

        private static void TestQueueClient()
        {
                QueueClientManager queueClient = new QueueClientManager();
                queueClient.InsertMessageAsync();
                var msg = queueClient.PeekMessage();
        }

        private static void TestRedisClientManager()
        {
            var redisCacheHandler = new RedisClientManager();
            redisCacheHandler.Set("MyKey1", "value 123", new TimeSpan(0, 0, 0, 15));
            redisCacheHandler.Set("MyKey2", "value abcd", DateTime.UtcNow.AddSeconds(15));

            var resultKey1 = redisCacheHandler.Get<string>("MyKey1");

            // Store .NET object to cache
            var employee007 = new EmployeeModel("007", "Ramis Jad", 90);
            redisCacheHandler.Set("employee007", employee007, new TimeSpan(0, 0, 0, 15));
            var employeeFromCache = redisCacheHandler.Get<EmployeeModel>("employee007");
        }
    }
}