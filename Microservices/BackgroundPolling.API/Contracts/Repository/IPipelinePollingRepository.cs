using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Repository
{
    public interface IPipelinePollingRepository
    {
        Task UpsertOpportunitiesFlatData(DataTable opportunityFlatDataTable, bool isFullLoad);
        Task UpsertOpportunitiesFlatDataInPipeline(DataTable opportunityFlatDataTable, bool isFullLoad);
    }
}
