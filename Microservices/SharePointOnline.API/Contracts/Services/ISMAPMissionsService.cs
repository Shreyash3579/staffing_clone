using SharePointOnline.API.Models;

namespace SharePointOnline.API.Contracts.Services
{
    public interface ISMAPMissionsService
    {
        Task<IEnumerable<SMAPMission>> GetSMAPMissions();
        Task<IEnumerable<StaffingPreference>> GetStaffingPreferences();
    }
}
