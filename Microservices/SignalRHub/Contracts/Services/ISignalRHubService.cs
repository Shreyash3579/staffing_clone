using Staffing.SignalRHub.Models;
using System.Threading.Tasks;

namespace SignalRHub.Contracts.Services
{
    public interface ISignalRHubService
    {
        Task<string> OnPegDataReceivedFromServiceBus(PegOpportunity data);

        Task<string> getUpdateOnSharedNotes(string sharedEmployeeCodes);

        Task<string> getUpdateOnCaseIntakeChanges(string sharedEmployeeCodes);


        Task<string> getUpdateOnRingfenceCommitment(string sharedEmployeeCodes);

    }
}
