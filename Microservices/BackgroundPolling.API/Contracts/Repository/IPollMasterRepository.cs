using System;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Repository
{
    public interface IPollMasterRepository
    {
        Task<DateTime> GetLastPolledTimeStampFromStaffingDB(string processName);
        Task UpsertPollMasterOnStaffingDB(string processName, DateTime timeStamp);
        Task<DateTime> GetLastPolledTimeStampFromAnalyticsDB(string processName);
        Task UpsertPollMasterOnAnalyticsDB(string processName, DateTime timeStamp);
    }
}
