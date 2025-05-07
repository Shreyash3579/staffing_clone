using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface ICaseService
    {
        Task<IEnumerable<CaseData>> GetNewDemandCasesAndAllocationsByOffices(DemandFilterCriteria filterCriteria, IEnumerable<Resource> employeesIncludingTerminated, IEnumerable<Office> offices,
            IEnumerable<SKUTerm> skuTerms, IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesOpps,
            IEnumerable<ResourceAssignmentViewModel> allocationsStaffedBySupply, string lstCasesUpdatedInBOSSInDateRange, IEnumerable<string> planningBoardOldCaseCodes, string loggedInUser = null);
        Task<IEnumerable<CaseData>> GetNewDemandCasesAndAllocationsByOfficesForNewStaffingTab(DemandFilterCriteria filterObj, IEnumerable<ResourceAssignmentViewModel> allocationsStaffedBySupply, IEnumerable<Resource> employeesIncludingTerminated,
             IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTerms, IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesOpps,
             string lstCasesUpdatedInBOSSInDateRange, IEnumerable<string> planningBoardOldCaseCodes, string loggedInUser = null);
        Task<IEnumerable<CaseData>> GetOnGoingCasesAndAllocationsByOfficesForNewStaffingtab(DemandFilterCriteria filterObj, IEnumerable<ResourceAssignmentViewModel> allocationsStaffedBySupply, IEnumerable<Resource> employeesIncludingTerminated,
            IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTerms, IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesCases,
            string lstCasesUpdatedInBOSSInDateRange, IEnumerable<string> planningBoardOldCaseCodes, string loggedInUser = null);
        Task<IEnumerable<CaseData>> GetPinnedCasesDetails(DemandFilterCriteria filterObj, string caseExceptionShowList, IEnumerable<Resource> employeesIncludingTerminated,
            IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTerms, IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesOpps,
            string lstCasesUpdatedInBOSSInDateRange, IEnumerable<string> planningBoardOldCaseCodes, string loggedInUser = null);
        Task<IEnumerable<CaseData>> GetHiddenCasesDetails(DemandFilterCriteria filterObj, IEnumerable<Resource> employeesIncludingTerminated,
            IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTerms, IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesOpps,
            string lstCasesUpdatedInBOSSInDateRange, IEnumerable<string> planningBoardOldCaseCodes, string loggedInUser = null);
        Task<IEnumerable<CaseData>> GetNewDemandCasesByOldCaseCodesAndFilterValues(string oldCaseCodes, DemandFilterCriteria filterObj,
            IEnumerable<Resource> employeesIncludingTerminated, IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup, IList<Revenue> revenueByServiceLinesCases, string loggedInUser = null);
        Task<IEnumerable<CaseData>> GetFilteredNewDemandCasesByOldCaseCodes(string oldCaseCodes, DemandFilterCriteria filterObj,
            IEnumerable<Resource> employeesIncludingTerminated, IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup, IList<Revenue> revenueByServiceLinesCases, string loggedInUser = null);
        Task<IEnumerable<CaseData>> GetNewDemandCasesByFilterValues(DemandFilterCriteria filterObj,
            IEnumerable<Resource> employeesIncludingTerminated, IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup, IList<Revenue> revenueByServiceLinesCases);
        Task<IEnumerable<CaseData>> GetActiveCasesExceptNewDemandsAndAllocationsByOffices(DemandFilterCriteria filterCriteria, IEnumerable<Resource> employeesIncludingTerminated,
            IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTerms, IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesCases,
            IEnumerable<ResourceAssignmentViewModel> allocationsStaffedBySupply, string lstCasesUpdatedInBOSSInDateRange, IEnumerable<string> planningBoardOldCaseCodes, string loggedInUser = null);
        Task<CaseDetails> GetCaseAndAllocationsByCaseCode(string oldCaseCode);
        Task<CaseData> GetCaseDataByCaseCodes(string oldCaseCode);
        Task<CaseDetails> GetCaseDetailsByCaseCode(string oldCaseCode);
        Task<IEnumerable<CaseData>> GetCasesForTypeahead(string searchString);
        Task<IEnumerable<CaseData>> GetCasesActiveAfterSpecifiedDate(DateTime? date);
    }
}
