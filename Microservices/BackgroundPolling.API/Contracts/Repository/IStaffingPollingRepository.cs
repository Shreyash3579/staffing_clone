using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Repository
{
    public interface IStaffingPollingRepository
    {
        public Task<string> DeleteSecurityUsersWithExpiredEndDate();
        public Task DeleteAnalyticsLog();

        public Task UpsertSMAPMissions(DataTable dataTable);
        public Task UpsertStaffingPreferencesFromSharepoint(DataTable dataTable);
        public Task UpsertStaffingPreferencesFromSharepointToAnalyticsDB(DataTable dataTable);
    }
}
