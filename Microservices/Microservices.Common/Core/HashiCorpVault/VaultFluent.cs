using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static Microservices.Common.Core.HashiCorpVault.VaultFluent;

namespace Microservices.Common.Core.HashiCorpVault
{

    public static class VaultFluentExtension
    {
        public static async Task<IConfigurationBuilder> AddVaultConfigurationsAsync(this IConfigurationBuilder config, VaultObject vaulObject)
        {
            if (config != null)
            {
                var httpClient = new HttpClient();
                var vaultConfigs = await (await new VaultFluent()
                                    .SetVaultObjects(httpClient, vaulObject)
                                    .GenerateAzureAdToken()
                                    .GenerateVaultTokenAsync())
                                    .GetVaultSecretsAsync();

                config.AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(vaultConfigs)));
            }
            return config;
        }
    }

    public class VaultFluent
    {
        public class VaultObject
        {
            /// <summary>
            /// Example: https://azure-amer.vault.tools.bain.com string value without '/' at the end.
            /// </summary>
            public Uri VaultUrl { get; set; }
            /// <summary>
            /// Eg: tsg/software
            /// </summary>
            public string VaultNamespace { get; set; }

            /// <summary>
            /// Eg: revenuesystems/dev/clientcaseapi
            /// </summary>
            public string VaultSecretPath { get; set; }

            /// <summary>
            /// Eg: v1/auth/azure/login
            /// </summary>
            public string VaultIdentityProviderPath { get; set; }

            /// <summary>
            ///Eg:  v1/kv/data/
            /// </summary>
            public string MountPath { get; set; }

            /// <summary>
            /// Eg: "https://management.azure.com//.default"
            /// </summary>
            public Uri AdLoginUrl { get; set; }
        }

        private VaultObject vaultObject;
        private HttpClient httpClient;
        private string clientToken;
        private string azureAdToken;


        public VaultFluent SetVaultObjects(HttpClient httpClient, VaultObject vaulObject)
        {
            this.httpClient = httpClient;
            vaultObject = vaulObject;
            return this;
        }

        public VaultFluent GenerateAzureAdToken()
        {
            var credential = new DefaultAzureCredential();
            azureAdToken = credential.GetToken(new Azure.Core.TokenRequestContext(new string[] { vaultObject.AdLoginUrl.AbsoluteUri })).Token;
            return this;
        }


        public async Task<VaultFluent> GenerateVaultTokenAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{vaultObject.VaultUrl.AbsoluteUri}{vaultObject.VaultIdentityProviderPath}");
            request.Headers.Add("X-Vault-Namespace", vaultObject.VaultNamespace);
            var authTokenReqObj = new
            {
                role = Environment.GetEnvironmentVariable("VAULT_APPROLENAME"),
                jwt = azureAdToken
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(authTokenReqObj), null, "application/json");
            var response = await httpClient.SendAsync(request);
            var authObjStr = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                await Console.Out.WriteLineAsync(authObjStr);
            }
            response.EnsureSuccessStatusCode();
            var jobj = JObject.Parse(authObjStr);
            clientToken = jobj["auth"]["client_token"].Value<string>();
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Example: revenuesystems/dev/finapi</param>
        /// <returns></returns>
        public async Task<string> GetVaultSecretsAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{vaultObject.VaultUrl.AbsoluteUri}{vaultObject.MountPath}{vaultObject.VaultSecretPath}");
            request.Headers.Add("X-Vault-Namespace", vaultObject.VaultNamespace);
            request.Headers.Add("X-Vault-Token", clientToken);
            var response = await httpClient.SendAsync(request);
            var contentStr = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                await Console.Out.WriteLineAsync(contentStr);
            }
            var jobj = JObject.Parse(contentStr);
            var jsonSecretsStr = jobj["data"]["data"].ToString();
            return jsonSecretsStr;
        }


    }
}

