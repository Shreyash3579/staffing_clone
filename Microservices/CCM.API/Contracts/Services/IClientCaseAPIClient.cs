using CCM.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCM.API.Contracts.Services
{
    public interface IClientCaseAPIClient
    {
        Task<IEnumerable<CaseDataFromClientCaseAPI>> GetModifiedCasesAfterLastPolledTime(DateTime? lastPolledTime);
        Task<IEnumerable<ClientDataFromClientCaseAPI>> GetClientsForTypeahead(string searchtext);
    }
}
