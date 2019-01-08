using Microsoft.Azure.Services.AppAuthentication;
using System;
using Microsoft.Azure.KeyVault;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.Models;

namespace AzureClients
{
    public class KeyVaultHandler
    {

        public static async Task<string> GetVaultKeyValueAsync()
        {
            var appKeyValue = string.Empty;

            var Message = "";
            int retries = 0;
            bool retry = false;
            try
            {
                /* The below 4 lines of code shows you how to use AppAuthentication library to fetch secrets from your Key Vault*/
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var secret = await keyVaultClient.GetSecretAsync("https://keyvault1000.vault.azure.net/secrets/AppKey")
                        .ConfigureAwait(false);
                appKeyValue = secret.Value;

                
                /* The below do while logic is to handle throttling errors thrown by Azure Key Vault. It shows how to do exponential backoff which is the recommended client side throttling*/
                do
                {
                    long waitTime = Math.Min(getWaitTime(retries), 2000000);
                    secret = await keyVaultClient.GetSecretAsync("https://keyvault1000.vault.azure.net/secrets/AppKey")
                        .ConfigureAwait(false);
                    retry = false;
                }
                while (retry && (retries++ < 10));
            }
            /// <exception cref="KeyVaultErrorException">
            /// Thrown when the operation returned an invalid status code
            /// </exception>
            catch (KeyVaultErrorException keyVaultException)
            {
                Message = keyVaultException.Message;
                if ((int)keyVaultException.Response.StatusCode == 429)
                    retry = true;
            }

            return appKeyValue;

            #region short code

            //try
            //{


            ///* The below 4 lines of code shows you how to use AppAuthentication library to fetch secrets from your Key Vault*/
            //AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            //KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            //var secret = await keyVaultClient.GetSecretAsync("https://keyvault1000.vault.azure.net/secrets/AppKey")
            ////var secret = await keyVaultClient.GetSecretAsync("https://keyvault1000.vault.azure.net/secrets/AppKey/XXXXX")
            ////var secret = await keyVaultClient.GetSecretAsync("https://<YourKeyVaultName>.vault.azure.net/secrets/AppSecret")
            //        .ConfigureAwait(true);
            //        //.ConfigureAwait(false);

            //return secret.Value;
            //    //Message = secret.Value;
            //}
            //catch (Exception ex)
            //{
            //    var msg = ex.Message;
            //    throw;
            //}
            #endregion


        }

        // This method implements exponential backoff incase of 429 errors from Azure Key Vault
        private static long getWaitTime(int retryCount)
        {
            long waitTime = ((long)Math.Pow(2, retryCount) * 100L);
            return waitTime;
        }

        // This method fetches a token from Azure Active Directory which can then be provided to Azure Key Vault to authenticate
        public async Task<string> GetAccessTokenAsync()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://vault.azure.net");
            return accessToken;
        }



    }
}
