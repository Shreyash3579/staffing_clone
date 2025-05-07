using CCM.API.Models;
using CCM.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCM.API.Contracts.Services
{
    public interface ICaseService
    {
        Task<IEnumerable<CaseViewModelBasic>> GetNewDemandCasesByOffices(DateTime startDate, DateTime endDate, string officeCode, string caseTypeCodes, string clientCodes);
        Task<IEnumerable<CaseViewModelBasic>> GetActiveCasesExceptNewDemandsByOffices(DateTime startDate, DateTime endDate, string officeCode, string caseTypeCodes, string clientCodes);
        Task<IEnumerable<CaseViewModel>> GetCaseDetailsByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<CaseViewModelBasic>> GetCaseDataByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<CaseViewModelBasic>> GetCasesForTypeahead(string searchString);
        Task<IEnumerable<CaseViewModelBasic>> GetCasesEndingBySpecificDate(int caseEndsBeforeNumberOfDays);
        Task<IEnumerable<CaseViewModelBasic>> GetCasesWithTaxonomiesByCaseCodes(string oldCaseCodeList);
        Task<IEnumerable<CaseViewModel>> GetCasesActiveAfterSpecifiedDate(DateTime? date);
        Task<IEnumerable<CaseViewModel>> GetCasesWithStartOrEndDateUpdatedInCCM(string columnName, DateTime? lastPollDateTime);
        Task<CaseMasterViewModel> GetCaseMasterAndCaseMasterHistoryChangesSinceLastPolled(DateTime? lastPolledDateTime);
        Task<IEnumerable<CaseAdditionalInfo>> GetCaseAdditionalInfo(DateTime? lastUpdated);
    }
}
