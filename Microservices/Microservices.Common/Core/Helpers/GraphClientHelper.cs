using Azure.Identity;
using Microsoft.Graph;

namespace Microservices.Common.Core.Helpers
{
    public class GraphClientHelper
    {
        private GraphServiceClient _graphServiceClient;

        public GraphClientHelper()
        {
        }

        public  GraphServiceClient GraphServiceClient(string tenantId, string clientId, string clientSecret)
        {
            if (_graphServiceClient == null)
            {
                _graphServiceClient = GetGraphClient(tenantId, clientId, clientSecret);
            }
               
            return _graphServiceClient;

        }

        private GraphServiceClient GetGraphClient(string tenantId, string clientId, string clientSecret)
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };
            var authProvider = new ClientSecretCredential(tenantId, clientId, clientSecret, options);

            // Get the authentication token for testing
            // var tokenCredential = authProvider.GetToken(new TokenRequestContext(scopes));
            // string accessToken = tokenCredential.Token;

            GraphServiceClient graphClient = new GraphServiceClient(authProvider, scopes);

            return graphClient;
        }

        public void Dispose()
        {
            if (_graphServiceClient != null)
                _graphServiceClient.Dispose();
        }
    }
}
