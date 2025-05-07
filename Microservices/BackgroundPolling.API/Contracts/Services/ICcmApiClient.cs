using BackgroundPolling.API.Models;
using BackgroundPolling.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface ICcmApiClient
    {

        Task<IEnumerable<CaseViewModel>> GetCasesEndingBySpecificDate(int caseEndsBeforeNumberOfDays);
        Task<IEnumerable<CaseViewModel>> GetCaseDataByCaseCodes(string oldCaseCodes);
        Task<IList<CaseOpportunityMap>> GetCasesByPipelineIds(string pipelineIds);
        Task<IList<CaseViewModel>> GetCasesWithEndDateUpdatedInCCM(DateTime? lastPolledDateTime);
        Task<CaseMasterViewModel> GetCaseMasterAndCaseMasterHistoryChangesSinceLastPolled(DateTime? lastPolledDateTime);
        Task<IList<CaseAttribute>> GetCaseAttributesByLastUpdatedDate(DateTime? lastUpdatedDate);
        Task<IEnumerable<CaseAdditionalInfo>> GetCaseAdditionalInfo(DateTime? lastPolledDateTime);

        #region FinAPI 
        Task<IEnumerable<Office>> GetOfficesFlatListByRegionOrCluster(int regionOrClusterCode);
        Task<IEnumerable<Office>> GetOfficeList();
        Task<IEnumerable<RevOffice>> GetRevOfficeList();
        Task<IEnumerable<BillRate>> GetBillRateByOffices(string officeCodes);
        Task<IEnumerable<BillRate>> GetBillRates();
        #endregion

        #region ClientCaseAPI 
        Task<IEnumerable<CaseViewModel>> GetModifiedCasesAfterLastPolledTime(DateTime lastPolledTime);
        #endregion
    }
}