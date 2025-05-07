using BackgroundPolling.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface ISharepointApiClient
    {
        Task<IEnumerable<SMAPMission>> GetSMAPMissions();
        Task<IEnumerable<StaffingPreference>> GetStaffingPreferences();
    }
}
