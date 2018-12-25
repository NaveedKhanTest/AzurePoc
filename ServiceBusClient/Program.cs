namespace AzureClients
{
    class Program
    {
        static void Main(string[] args)
        {
            //var key = KeyVaultHandler.GetVaultKeyValueAsync().Result;
            ServiceBusMessageSender.SendMessages().GetAwaiter().GetResult();
            //ServiceBusMessageReceiver.ReadMessages().GetAwaiter().GetResult();
        }


    }
}