using Azure.Search.Documents.Indexes;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Staffing.AzureSearch.Models;
using Staffing.AzureSearch.Core.Helpers;
using Azure;

namespace Staffing.AzureSearch.Contracts.Services
{
    public class EmployeeIndexerService : IEmployeeIndexerService
    {
        private static SearchClient _searchClient;
        private static SearchIndexClient _indexClient;
        private readonly IStaffingApiClient _staffingApiClient;
        private static IEmbeddingGenerationService _embeddingGenerationService;

        public EmployeeIndexerService(IStaffingApiClient staffingApiClient, IEmbeddingGenerationService embeddingGenerationService)
        {
            _embeddingGenerationService = embeddingGenerationService;
            _staffingApiClient = staffingApiClient;
            InitSearch();
        }

        public async Task<string> IndexResourceNotesByLastUpdatedDate(DateTime dateTime)
        {
            var resourceNotes = await _staffingApiClient.GetResourceNotesAfterLastPolledTime(dateTime, "RP");
            var resourceNotesPartial = resourceNotes.GroupBy(x => x.EmployeeCode).Select(x => new ResourcePartial
            {
                id = x.Key,
                notes = string.Join("|", x.Select(y => y.Note))
            });

            var notesDictionary = new Dictionary<string, string>();

            foreach (var resource in resourceNotesPartial)
            {
                notesDictionary.Add(resource.id, resource.notes);
            }

            var notesDictionarywithEmbeddings = await _embeddingGenerationService.GetMultipleEmbeddingsVectorFromOpenAI(notesDictionary);

            var resourceNotesWithEmbeddings = resourceNotesPartial.Select(x => new ResourcePartial
            {
                id = x.id,
                notes = x.notes,
                notesVector = notesDictionarywithEmbeddings[x.id]
            });

            await UploadDataToEmployeeConsolidatedIndex(resourceNotesWithEmbeddings);
            return "successfull";
        }

        public async Task UploadDataToEmployeeConsolidatedIndex(IEnumerable<ResourcePartial> dataToUpload)
        {
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
            string searchServiceUri = ConfigService.GetSearchBaseUrl();
            string adminApiKey = ConfigService.GetSearchAdminApiKey();
            string searchIndex = ConfigService.GetSearchIndexName();

            // Create a service and index client.
            _indexClient = new SearchIndexClient(new Uri(searchServiceUri), new AzureKeyCredential(adminApiKey));
            _searchClient = _indexClient.GetSearchClient(searchIndex);
        }

    }
}
