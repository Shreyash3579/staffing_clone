using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Repository
{
    public interface IBasisPollingRepository
    {
        Task UpsertPracticeAffiliations(DataTable dataTable);
        Task InsertPracticeAreaLookUpData(DataTable practiceAreaDataTable);
        Task InsertMonthlySnapshotForPracticeAffiliations();
    }
}
