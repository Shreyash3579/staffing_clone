using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface ISharepointPollingService
    {
        Task UpsertSignedOffSMAPMissions();
        Task UpsertStaffingPreferences();
    }
}
