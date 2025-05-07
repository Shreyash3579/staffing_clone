using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRHub.Contracts.RepositoryInterfaces;
using SignalRHub.Contracts.Services;
using SignalRHub.Core.Repository;
using SignalRHub.HubConfig;
using SignalRHub.Models;
using Staffing.SignalRHub.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRHub.Core.Services
{
    public class SignalRHubService : ISignalRHubService
    {
        private readonly IHubContext<StaffingHub> _hub;
        private readonly ISignalRHubRepository _signalRHubRepository;

        public SignalRHubService(IHubContext<StaffingHub> hubContext, ISignalRHubRepository signalRHubRepository)
        {
            _hub = hubContext;
            _signalRHubRepository = signalRHubRepository;
        }

        public async Task<string> OnPegDataReceivedFromServiceBus(PegOpportunity data)
        {
            var hub = new StaffingHub(_hub, _signalRHubRepository); // Create an instance of your hub
            var userConnectionMapping = await _signalRHubRepository.GetSignalRConnectionStringForUsers(data.LastUpdatedBy);
            await hub.UpdatesForPegOpportunity(userConnectionMapping.First().ConnectionString, data);
            return "Mesaage sent to hub";
        }


        public async Task<string> getUpdateOnSharedNotes(string sharedEmployeeCodes)
        {
            var employeeCodes = sharedEmployeeCodes.Split(',');
            var hub = new StaffingHub(_hub, _signalRHubRepository); // Create an instance of your hub
            var userConnectionMapping = await _signalRHubRepository.GetSignalRConnectionStringForUsers(sharedEmployeeCodes);
            await hub.UpdatesForSharedNotes(userConnectionMapping);
            return "Mesaage sent to hub";
        }

        public async Task<string> getUpdateOnCaseIntakeChanges(string sharedEmployeeCodes)
        {
            var employeeCodes = sharedEmployeeCodes.Split(',');
            var hub = new StaffingHub(_hub, _signalRHubRepository); // Create an instance of your hub
            var userConnectionMapping = await _signalRHubRepository.GetSignalRConnectionStringForUsers(sharedEmployeeCodes);
            await hub.UpdateOnCaseIntakeChanges(userConnectionMapping);
            return "Mesaage sent to hub";
        }


        public async Task<string> getUpdateOnRingfenceCommitment(string employeeCodes)
        {
            var employeeCodesList = employeeCodes.Split(',');
            var hub = new StaffingHub(_hub, _signalRHubRepository); // Create an instance of your hub
            var userConnectionMapping = await _signalRHubRepository.GetSignalRConnectionStringForUsers(employeeCodes);
            await hub.UpdateOnRingfenceCommitment(userConnectionMapping);
            return "Mesaage sent to hub";
        }


    }
}
