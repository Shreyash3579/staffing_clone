using Microsoft.AspNetCore.SignalR;
using SignalRHub.Contracts.RepositoryInterfaces;
using SignalRHub.Core.Repository;
using SignalRHub.Models;
using Staffing.SignalRHub.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SignalRHub.HubConfig
{
    public class StaffingHub : Hub
    {
        private readonly IHubContext<StaffingHub> _hubContext;
        private readonly ISignalRHubRepository _signalRHubRepository;

        public StaffingHub(IHubContext<StaffingHub> hubContext, ISignalRHubRepository signalRHubRepository)
        {
            _hubContext = hubContext;
            _signalRHubRepository = signalRHubRepository;
        }

        public async Task BroadcastDataUpdates(string data)
        {
            await Clients.All.SendAsync("broadcastDataUpdates", data);
        }

        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            string encodedEmployeecode = Context.GetHttpContext().Request.Query["employeecode"];
            string employeecode = ExtractEmployeeCode(encodedEmployeecode);

            await _signalRHubRepository.UpsertSignalRConnectionStringForUser(employeecode, connectionId);
            await base.OnConnectedAsync();
        }

        public async Task UpdatesForPegOpportunity(string employeeConnectionString, PegOpportunity data)
        {
            // Send the update to the specific client using the connectionId
            await _hubContext.Clients.Client(employeeConnectionString).SendAsync("SendPegOpportunityUpdates", data);
        }

        public async Task UpdatesForSharedNotes(IEnumerable<UserConnectionMapping> employeeConnectionStrings)
        {
            foreach (var employeeConnectionString in employeeConnectionStrings)
            {
                // Send the update to each client using their connection string
                await _hubContext.Clients.Client(employeeConnectionString.ConnectionString).SendAsync("SendUnreadSharedNotesUpdate");
            }
        }

        public async Task UpdateOnCaseIntakeChanges(IEnumerable<UserConnectionMapping> employeeConnectionStrings)
        {
            foreach (var employeeConnectionString in employeeConnectionStrings)
            {
                // Send the update to each client using their connection string
                await _hubContext.Clients.Client(employeeConnectionString.ConnectionString).SendAsync("SendUnreadCaseIntakeUpdate");
            }
        }

        public async Task UpdateOnRingfenceCommitment(IEnumerable<UserConnectionMapping> employeeConnectionStrings)
        {
            foreach (var employeeConnectionString in employeeConnectionStrings)
            {
                // Send the update to each client using their connection string
                await _hubContext.Clients.Client(employeeConnectionString.ConnectionString).SendAsync("SendUnreadRingfenceCommitmentUpdate");
            }
        }



        private string ExtractEmployeeCode(string queryParam)
        {
            // Check if the queryParam starts with "${" and ends with "}"
            if (queryParam.StartsWith("${") && queryParam.EndsWith("}"))
            {
                // Extract the code between "${" and "}"
                int startIndex = 2; // Length of "${"
                int length = queryParam.Length - 3; // Length excluding "${" and "}"
                string employeeCode = queryParam.Substring(startIndex, length);
                return employeeCode;
            }
            else
            {
                // Return the queryParam as is (no extraction)
                return queryParam;
            }
        }
    }
}
