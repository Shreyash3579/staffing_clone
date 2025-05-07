using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface ISignalRHubClient
    {
        Task<string> GetUpdateOnSharedNotes(string sharedEmployeeCodes);

        Task<string> GetUpdateOnRingfenceCommitmentsAlert(string employeeCodes);
    }
}