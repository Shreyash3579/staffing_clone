using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IStaffingPollingService
    {
        public Task<string> DeleteSecurityUsersWithExpiredEndDate();
        public Task DeleteAnalyticsLog();

        public Task UpdateSecurityUserForWFPRole();
    }
}
