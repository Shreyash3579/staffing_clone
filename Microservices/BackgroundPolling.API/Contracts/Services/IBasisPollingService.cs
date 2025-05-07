using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IBasisPollingService
    {
        Task UpsertPracticeAffiliations();
        Task InsertPracticeAreaLookUpData();
        Task InsertMonthlySnapshotForPracticeAffiliations();
    }
}
