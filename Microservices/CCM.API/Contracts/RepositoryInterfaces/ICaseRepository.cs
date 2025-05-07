using CCM.API.Models;
using CCM.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCM.API.Contracts.RepositoryInterfaces
{
    public interface ICaseRepository
    {
        Task<IEnumerable<Case>> GetNewDemandCasesByOffices(DateTime startDate, DateTime endDate, string officeCodes, string caseTypeCodes, string clientCodes);
        Task<IEnumerable<Case>> GetActiveCasesExceptNewDemandsByOffices(DateTime startDate, DateTime endDate, string officeCodes, string caseTypeCodes, string clientCodes);
        Task<IEnumerable<Case>> GetCaseDetailsByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<Case>> GetCaseDataByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<Case>> GetCasesForTypeahead(string searchString);
        Task<IEnumerable<Case>> GetCasesEndingBySpecificDate(int caseEndsBeforeNumberOfDays);
        Task<IEnumerable<Case>> GetCasesWithTaxonomiesByCaseCodes(string oldCaseCodeList);
        Task<IEnumerable<Case>> GetCasesActiveAfterSpecifiedDate(DateTime date);
        Task<IEnumerable<Case>> GetCasesWithStartOrEndDateUpdatedInCCM(string columnName, DateTime? lastPollDateTime);
        Task<CaseMasterViewModel> GetCaseMasterAndCaseMasterHistoryChangesSinceLastPolled(DateTime? lastPolledDateTime);
        Task<IEnumerable<CaseAdditionalInfo>> GetCaseAdditionalInfo(DateTime? lastUpdated);
    }
}
