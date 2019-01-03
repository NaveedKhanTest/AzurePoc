using System;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Queue; // Namespace for Queue storage types
using System.Threading.Tasks;
using AzureClientsDotNetStandard.Models;
using Newtonsoft.Json;

/*
 Notes:
 make sure the queues we want to interact all exist when the application starts up. To do this, 
 BootstrapAzureQueues class, that loops through all of the known queues for my application and ensures they exists. 
 If they do not exist, create them. Make sure you call this from your Startup.cs!

    Passing object messages in Azure Queue Storage: 
    https://stackoverflow.com/questions/8550702/passing-object-messages-in-azure-queue-storage

 */

namespace AzureClientsDotNetStandard
{
    public static class QueueClientManager
    {
        public static void TestQueueClient()
        {
            // https://docs.microsoft.com/en-us/azure/storage/queues/storage-dotnet-how-to-use-queues
            // https://www.dotnetcurry.com/visualstudio/1328/visual-studio-connected-services-aspnet-core-azure-storage
            // https://stackoverflow.com/questions/8550702/passing-object-messages-in-azure-queue-storage

            BootstrapAzureQueues.CreateKnownAzureQueues(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            //AddMessage();
            //PeekMessage();
            //UpdateMessage();
            //Process20Messages();
            // Get the message from the queue and update the message contents.

        }

        //QueueHelper

        public static async Task AddObjectMessageAsync<T>(T objectToAdd)
        //public static async System.Threading.Tasks.Task AddObjectMessageAsync<T>(T objectToAdd)
        {
            CloudQueue queue = GetQueue();
            await QueueHelper.AddMessageAsJsonAsync(queue, objectToAdd);
        }

        public static void PeekMessageObject<T>()
        {
            CloudQueue queue = GetQueue();

            // Peek at the next message
            CloudQueueMessage peekedMessage = queue.PeekMessage();

            //JsonConvert.DeserializeObject<T>(result)
            var msg = JsonConvert.DeserializeObject<T>(peekedMessage.AsString);
            //var msg = JsonConvert.DeserializeObject<EmployeeModel>(peekedMessage.AsString);
            

            // Display message.
            Console.WriteLine(peekedMessage.AsString);
        }


        public static void AddMessage()
        {
            CloudQueue queue = GetQueue();

            // Create a message and add it to the queue.
            CloudQueueMessage message = new CloudQueueMessage("Hello, World " + DateTime.Now.ToLongTimeString());
            queue.AddMessage(message);
        }

        /// <summary>
        /// read message without changing its visibility
        /// </summary>
        public static void PeekMessage()
        {
            CloudQueue queue = GetQueue();

            // Peek at the next message
            CloudQueueMessage peekedMessage = queue.PeekMessage();

            var temp = peekedMessage.AsBytes;

            // Display message.
            Console.WriteLine(peekedMessage.AsString);
        }

        /// <summary>
        /// update the contents and increase invisible time (default invisible time is 30 second after GetMessage)
        /// </summary>
        public static void UpdateMessage()
        {
            CloudQueue queue = GetQueue();

            CloudQueueMessage messageX = queue.GetMessage();
            messageX.SetMessageContent("Updated contents." + DateTime.Now.ToLongTimeString());
            queue.UpdateMessage(messageX,
                TimeSpan.FromMinutes(1.0),  // Make it invisible for another 60 seconds.
                MessageUpdateFields.Content | MessageUpdateFields.Visibility);
        }

        /// <summary>
        /// Your code de-queues a message from a queue in two steps. When you call GetMessage, 
        /// you get the next message in a queue. A message returned from GetMessage becomes invisible 
        /// to any other code reading messages from this queue. By default, 
        /// this message stays invisible for 30 seconds. To finish removing the message from the queue, 
        /// you must also call DeleteMessage. This two-step process of removing a message assures that 
        /// if your code fails to process a message due to hardware or software failure, another instance 
        /// of your code can get the same message and try again. Your code calls DeleteMessage right after 
        /// the message has been processed.
        /// </summary>
        public static void DeQueueMessage()
        {
            CloudQueue queue = GetQueue();

            // Get the next message
            CloudQueueMessage retrievedMessage = queue.GetMessage();
            //CloudQueueMessage retrievedMessage = queue.GetMessage();

            //Todo: process message

            //Process the message in less than 30 seconds, and then delete the message
            // you can update message contents or increase invisible time before deleting
            queue.DeleteMessage(retrievedMessage);
        }

        private static CloudQueue GetQueue()
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the queue client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue.
            CloudQueue queue = queueClient.GetQueueReference("myqueue");

            // Create the queue if it doesn't already exist.
            queue.CreateIfNotExists();
            return queue;
        }

        /// <summary>
        /// There are two ways you can customize message retrieval from a queue. 
        /// First, you can get a batch of messages (up to 32). 
        /// Second, you can set a longer or shorter invisibility timeout, allowing your code more 
        /// or less time to fully process each message. The following code example uses the 
        /// GetMessages method to get 20 messages in one call. Then it processes each message using a 
        /// foreach loop. It also sets the invisibility timeout to five minutes for each message. 
        /// Note that the 5 minutes starts for all messages at the same time, so after 5 minutes 
        /// have passed since the call to GetMessages, any messages which have not been deleted will 
        /// become visible again.
        /// </summary>
        public static void Process20Messages()
        {
            CloudQueue queue = GetQueue();

            // Fetch the queue attributes.
            queue.FetchAttributes();

            // Retrieve the cached approximate message count.
            int? cachedMessageCount = queue.ApproximateMessageCount;

            // Display number of messages.
            Console.WriteLine("Number of messages in queue: " + cachedMessageCount);


            foreach (CloudQueueMessage message in queue.GetMessages(20, TimeSpan.FromMinutes(5)))
            {
                //Todo: process message

                // Process all messages in less than 5 minutes, deleting each message after processing.
                queue.DeleteMessage(message);
            }

            // Delete a queue and all the messages contained in it
            //queue.Delete();

        }


     
    }
}
