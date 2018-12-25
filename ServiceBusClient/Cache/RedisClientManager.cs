using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace AzureClients
{
    /// <summary>
    /// RedisCashHandler
    /// </summary>
    public class RedisClientManager
    {
        IDatabase _cache = lazyConnection.Value.GetDatabase();

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        //make bellow method private
        public static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string cacheConnection = "RedisCache000.redis.cache.windows.net:6380,password=3iDmzMcN+LKJ+MGQTqwht4+NgkygrBc8ij24QKB0Ks0=,ssl=True,abortConnect=False";
            return ConnectionMultiplexer.Connect(cacheConnection);
        });


        /// <summary>
        /// By timespan
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeout"></param>
        public void Set(string key, object value, TimeSpan timeout)
        {
            _cache.StringSet(key, JsonConvert.SerializeObject(value), timeout);
        }

        /// <summary>
        /// By date
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expires"></param>

        public void Set(string key, object value, DateTime expires)
        {
            _cache.StringSet(key, JsonConvert.SerializeObject(value));
            _cache.KeyExpire(key, expires);
        }

        public T Get<T>(string key)
        {
            T resultValue = default(T);

            var result = _cache.StringGet(key);

            if (result.HasValue)
            {
                return JsonConvert.DeserializeObject<T>(result);
                //return JsonConvert.DeserializeObject(result.ToString());
            }

            return resultValue;
        }



    }
}
