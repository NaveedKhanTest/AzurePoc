using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureClientsDotNetStandard
{
    public class BootstrapAzureQueues
    {
        /// <summary>
        /// make sure the all the queues exist
        /// </summary>
        /// <param name="azureConnectionString"></param>
        public static void CreateKnownAzureQueues(string azureConnectionString)
        {
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(azureConnectionString);
            //CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            //foreach (var queueName in AzureQueues.KnownQueues)
            //{
            //    queueClient.GetQueueReference(queueName).CreateIfNotExists();
            //}
        }
    }
}
