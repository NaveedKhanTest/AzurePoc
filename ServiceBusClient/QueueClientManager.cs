using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Queue; // Namespace for Queue storage types
using Microsoft.Azure; //Namespace for CloudConfigurationManager

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureClients
{
    public class QueueClientManager
    {
        public void CreateQueue()
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = GetStorageAccount();

            // Create the queue client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a container.
            CloudQueue queue = queueClient.GetQueueReference("myqueue");

            // Create the queue if it doesn't already exist
            queue.CreateIfNotExistsAsync();
        }

        public async Task InsertMessageAsync()
        {
            CloudQueue queue = await GetQueueAsync();

            // Create a message and add it to the queue.
            CloudQueueMessage message = new CloudQueueMessage("Hello, World");
            queue.AddMessageAsync(message);
        }

        /// <summary>
        /// peek at the message in the front of a queue without removing it from the queue 
        /// </summary>
        public async Task<string> PeekMessage()
        {
            CloudQueue queue = await GetQueueAsync();

            // Peek at the next message
            
            CloudQueueMessage peekedMessage = await queue.PeekMessageAsync();

            // Display message.
            //Console.WriteLine(peekedMessage.AsString);
            return peekedMessage.AsString;
        }

        private async Task<CloudQueue> GetQueueAsync()
        {
            try
            {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = GetStorageAccount();

            // Create the queue client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue.
            CloudQueue queue = queueClient.GetQueueReference("myqueue");

                // Create the queue if it doesn't already exist.
                //queue.CreateIfNotExistsAsync();
                var result = queue.CreateIfNotExistsAsync().Result;

                if (await queue.CreateIfNotExistsAsync())
                {
                    Console.WriteLine("Queue '{0}' Created", queue.Name);
                }
                else
                {
                    Console.WriteLine("Queue '{0}' Exists", queue.Name);
                }

                return queue;

            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                throw;
            }
        }

        private static CloudStorageAccount GetStorageAccount()
        {
            // Retrieve storage account from connection string
            //return CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            return CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=storage000poc;AccountKey===;EndpointSuffix=core.windows.net");
        }
    }
}
