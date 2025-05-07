using Azure.Identity;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Microservices.Common.Core.Helpers
{
    public class AzureADHelper
    {
        private readonly string _clientId;
        private readonly string _tenantId;
        private readonly string _clientSecret;
        public GraphServiceClient _graphServiceClient;

        public AzureADHelper(string clientId, string tenantId, string clientSecret)
        {
            _clientId = clientId;
            _tenantId = tenantId;
            _clientSecret = clientSecret;
            InitializeGraphClient();
        }

        private void InitializeGraphClient()
        {
            _graphServiceClient = GetGraphClient(_tenantId, _clientId, _clientSecret);
        }

        public async Task<List<string>> GetGroupNamesBySearchString(string searchString)
        {
            try
            {
                var request = await _graphServiceClient.Groups.GetAsync((requestConfiguration) =>
                {
                    requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                    requestConfiguration.QueryParameters.Select = new string[] { "id", "displayName" };
                    requestConfiguration.QueryParameters.Search = $"\"displayName:{searchString}\"";
                    requestConfiguration.QueryParameters.Orderby = new string[] { "displayName" };
                });

                return request.Value.Select(x => x.DisplayName).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<string>> GetGroupMembersEmployeeCodes(string groupId)
        {
            try
            {
                if (string.IsNullOrEmpty(groupId))
                {
                    return new List<string>();
                }

                List<string> groupMembers = new List<string>();
                var response = await _graphServiceClient.Groups[groupId].Members.GraphUser.GetAsync((requestConfiguration) =>
                {
                    requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                    requestConfiguration.QueryParameters.Select = new string[] { "employeeId", "displayName" };
                    requestConfiguration.QueryParameters.Top = 500;
                });

                while (response?.Value != null)
                {
                    groupMembers.AddRange(response.Value?.Where(x => !string.IsNullOrEmpty(x.EmployeeId)).Select(x => x.EmployeeId).ToList());

                    // If OdataNextLink has a value, there is another page
                    if (!string.IsNullOrEmpty(response.OdataNextLink))
                    {
                        // Pass the OdataNextLink to the WithUrl method to request the next page
                        response = await _graphServiceClient.Groups[groupId].Members.GraphUser
                            .WithUrl(response.OdataNextLink)
                            .GetAsync();
                    }
                    else
                    {
                        break;
                    }
                }

                return groupMembers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        public async Task<Dictionary<string, string>> GetGroupByName(IEnumerable<string> groupNames)
        {
            try
            {
                var groupNamesWithIdList = new Dictionary<string, string>();
                if (!groupNames.Any())
                {
                    return groupNamesWithIdList;
                }

                // Process in batches of 15 as grpah API for filter has a max length of 15 OR conditions
                const int batchSize = 15;
                var groupNamesList = groupNames.ToList();

                for (int i = 0; i < groupNamesList.Count; i += batchSize)
                {
                    var batch = groupNamesList.Skip(i).Take(batchSize);
                    var filterGroupNamesList = batch.Select(name => $"'{name}'");
                    var filter = string.Join(",", filterGroupNamesList);
                    var filterQuery = $"displayName in ({filter})";

                    var result = await _graphServiceClient.Groups.GetAsync((requestConfiguration) =>
                    {
                        requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                        requestConfiguration.QueryParameters.Filter = filterQuery;
                        requestConfiguration.QueryParameters.Select = new[] { "id", "displayName" };
                    });

                    foreach (var group in result.Value)
                    {
                        groupNamesWithIdList[group.DisplayName] = group.Id;
                    }
                }

                return groupNamesWithIdList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
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
    }
}
