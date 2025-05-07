using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace SharePointOnline.API.Core.Helpers
{
    public class SharepointGraphClientHelper
    {
        private static GraphServiceClient _graphClientInstance = null!;

        private static GraphServiceClient InitializeGraphClient()
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            string clientId = ConfigurationUtility.GetValue("SharepointBossConnectorAppRegistration:Clientid");
            string tenantId = ConfigurationUtility.GetValue("SharepointBossConnectorAppRegistration:TenantId");
            string clientSecret = ConfigurationUtility.GetValue("SharepointBossConnectorAppRegistration:ClientSecret");

            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };
            
            var authProvider = new ClientSecretCredential(tenantId, clientId, clientSecret, options);
            
            return  new GraphServiceClient(authProvider, scopes);
        }

        public static async Task<IEnumerable<ListItem>> GetSharePointListData(string sharepointSiteId, string sharepointListId)
        {
            if (_graphClientInstance == null)
            {
                _graphClientInstance = InitializeGraphClient();
            }

            try
            {
                var listItems = await
                   _graphClientInstance.Sites[sharepointSiteId].Lists[sharepointListId]
                   .Items
                   .GetAsync((requestConfiguration) =>
                   {
                       requestConfiguration.QueryParameters.Top = 1000; //TODO: change this to either get recursively or using Odata.nextlink
                       requestConfiguration.QueryParameters.Expand = new string[] { "fields" };
                   });

                if (listItems == null || listItems.Value == null)
                {
                    return Enumerable.Empty<ListItem>();
                }

                return listItems.Value;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error fetching SharePoint list data: {ex.Message}");
            }
        }

    }
}
