using CCM.API.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace CCM.API.Contracts.Services
{
    public interface IClientCaseAPIService
    {
        Task<IEnumerable<CaseViewModelBasic>> GetModifiedCasesAfterLastPolledTime(DateTime? lastPolledTime);
        Task<IEnumerable<ClientViewModel>> GetClientsForTypeahead(string searchtext);
    }
}
