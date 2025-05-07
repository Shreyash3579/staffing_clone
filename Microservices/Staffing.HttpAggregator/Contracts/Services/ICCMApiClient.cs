using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface ICCMApiClient
    {
        Task<IList<CaseData>> GetCaseDataBasicByCaseCodes(string oldCaseCodeList);
        Task<IList<CaseData>> GetCaseDataByCaseCodes(string oldCaseCodes);
        Task<CaseDetails> GetCaseDetailsByCaseCode(string oldCaseCode);
        //Task<IList<CaseData>> GetCasesByOffices(string officeCodes, string caseTypeCodes, DateTime startDate, DateTime endDate);//TODO:08-mar-2023 -  remove if not used
        Task<IList<CaseData>> GetNewDemandCasesByOffices(string officeCodes, string caseTypeCodes, DateTime startDate, DateTime endDate,string clientCodes);
        Task<IList<CaseData>> GetActiveCasesExceptNewDemandsByOffices(string officeCodes, string caseTypeCodes, DateTime startDate, DateTime endDate, string clientCodes);
        Task<IList<CaseAttribute>> GetCaseAttributeLookupList();
        Task<IEnumerable<CaseData>> GetCasesForTypeahead(string searchString, string officeCodes);
        Task<IList<CaseData>> GetCasesWithTaxonomiesByCaseCodes(string oldCaseCodeList);
        Task<IEnumerable<CaseData>> GetCasesActiveAfterSpecifiedDate(DateTime? date);

        #region FinAPI 
        Task<IEnumerable<Office>> GetOfficeList();
        Task<IEnumerable<Office>> GetOfficesFlatListByRegionOrCluster(int regionOrClusterCode);
        Task<OfficeHierarchy> GetOfficeHierarchyByOffices(string officeCodes);
        #endregion
    }
}
