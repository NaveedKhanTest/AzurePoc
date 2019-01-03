using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Queue; // Namespace for Queue storage types
using Newtonsoft.Json;

namespace AzureClientsDotNetStandard
{
    public static class QueueHelper
    {
        public static async Task AddMessageAsJsonAsync<T>(this CloudQueue cloudQueue, T objectToAdd)
        {
            var messageAsJson = JsonConvert.SerializeObject(objectToAdd);
            var cloudQueueMessage = new CloudQueueMessage(messageAsJson);
            await cloudQueue.AddMessageAsync(cloudQueueMessage);
        }

    }
}
