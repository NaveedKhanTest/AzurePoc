using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureClientsDotNetStandard.Models;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Queue; // Namespace for Queue storage types

namespace AzureClientsDotNetStandard
{
    public static class QueueClientTests
    {
        public static void Tests()
        {
            //BootstrapAzureQueues.CreateKnownAzureQueues(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            //QueueClientManager.AddMessage();
            //QueueClientManager.PeekMessage();
            //QueueClientManager.UpdateMessage();
            //QueueClientManager.Process20Messages();

            var employee009 = new EmployeeModel("009", "Ramis Jad", 90, DateTime.Now.ToLongTimeString());
            QueueClientManager.AddObjectMessageAsync(employee009);

            QueueClientManager.PeekMessageObject<EmployeeModel>();
        }
    }
}
