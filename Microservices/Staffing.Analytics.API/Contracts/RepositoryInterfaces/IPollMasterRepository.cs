using System;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.RepositoryInterfaces
{
    public interface IPollMasterRepository
    {
        Task<DateTime> GetLastPolledTimeStamp(string processName);
        Task UpsertPollMaster(string processName, DateTime timeStamp);
    }
}
