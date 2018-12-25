using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureClients.Cache
{
    public class RedisCacheTests
    {
        public static void Tests()
        {
            #region note

            /*
             to set or get a string value use StringSet or StringGet methods

             use cache.StringGet("Message").ToString(); 
             * */

            #endregion
            // Connection refers to a property that returns a ConnectionMultiplexer
            // as shown in the previous example.
            IDatabase cache = RedisClientManager.lazyConnection.Value.GetDatabase();

            // Perform cache operations using the cache object...

            // Simple PING command
            string cacheCommand = "PING";
            Console.WriteLine("\nCache command  : " + cacheCommand);
            Console.WriteLine("Cache response : " + cache.Execute(cacheCommand).ToString());

            // Simple get and put of integral data types into the cache
            cacheCommand = "GET Message";
            Console.WriteLine("\nCache command  : " + cacheCommand + " or StringGet()");
            Console.WriteLine("Cache response : " + cache.StringGet("Message").ToString());

            var temp1 = cache.StringGet("Message").ToString();


            cacheCommand = "SET Message \"Hello! The cache is working from a .NET Core console app!\"";
            Console.WriteLine("\nCache command  : " + cacheCommand + " or StringSet()");
            Console.WriteLine("Cache response : " + cache.StringSet("Message", "Hello!!!!... The cache is working from a .NET Core console app!").ToString());

            var temp2 = cache.StringSet("Message", "Hello!!!!... The cache is working from a .NET Core console app!").ToString();

            // Demonstrate "SET Message" executed as expected...
            cacheCommand = "GET Message";
            Console.WriteLine("\nCache command  : " + cacheCommand + " or StringGet()");
            Console.WriteLine("Cache response : " + cache.StringGet("Message").ToString());

            var temp3 = cache.StringGet("Message").ToString();

            // Get the client list, useful to see if connection list is growing...
            cacheCommand = "CLIENT LIST";
            Console.WriteLine("\nCache command  : " + cacheCommand);
            Console.WriteLine("Cache response : \n" + cache.Execute("CLIENT", "LIST").ToString().Replace("id=", "id="));

            RedisClientManager.lazyConnection.Value.Dispose();

        }
    }
}
