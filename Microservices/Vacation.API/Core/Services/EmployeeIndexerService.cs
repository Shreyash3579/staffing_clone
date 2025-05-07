using Azure.Search.Documents.Indexes;
using Azure.Search.Documents;
using Azure;
using System;
using System.Threading.Tasks;
using Vacation.API.Models;
using Azure.Search.Documents.Models;
using Vacation.API.Contracts.Services;
using Vacation.API.Core.Helpers;
using System.Linq;
using System.Collections.Generic;

namespace Vacation.API.Core.Services
{
    public class EmployeeIndexerService : IEmployeeIndexerService
    {
        private static SearchClient _searchClient;
        private static SearchIndexClient _indexClient;

        public EmployeeIndexerService()
        {
        }

        
        public async Task UploadDataToEmployeeConsolidatedIndex(IEnumerable<ResourcePartial> dataToUpload)
        {
            InitSearch();

            IndexDocumentsBatch<ResourcePartial> batch = IndexDocumentsBatch.Merge(
                dataToUpload
            );

            //IndexDocumentsBatch<ResourcePartial> batch = IndexDocumentsBatch.Create(
            //        IndexDocumentsAction.Merge(dataToUpload.First())
            //);


            try
            {
                IndexDocumentsResult result = await _searchClient.IndexDocumentsAsync(batch);
                //IndexDocumentsResult result = _searchClient.IndexDocuments(batch);
                if (result.Results != null && result.Results.All(r => r.Succeeded))
                {
                    Console.WriteLine("Documents updated successfully!");
                }
                else
                {
                    Console.WriteLine("Some documents failed to update.");
                }
            }
            catch (Exception ex)
            {

            }
           
        }

        private void InitSearch()
        {
            // Read the values from appsettings.json
            string searchServiceUri = ConfigurationUtility.GetValue("Search:SearchServiceUri");
            string adminApiKey = ConfigurationUtility.GetValue("Search:SearchServiceAdminApiKey");
            string searchIndex = ConfigurationUtility.GetValue("Search:SearchIndexName");

            // Create a service and index client.
            _indexClient = new SearchIndexClient(new Uri(searchServiceUri), new AzureKeyCredential(adminApiKey));
            _searchClient = _indexClient.GetSearchClient(searchIndex);
        }

    }
}
