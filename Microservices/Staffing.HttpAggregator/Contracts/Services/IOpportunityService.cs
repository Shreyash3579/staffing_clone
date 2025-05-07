using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IOpportunityService
    {
        Task<IEnumerable<OpportunityData>> GetOpportunities(DemandFilterCriteria filterObj, IEnumerable<Resource> employeesIncludingTerminated, IEnumerable<Office> offices,
            IEnumerable<SKUTerm> skuTerms, IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesOpps,
            IEnumerable<ResourceAssignmentViewModel> allocationsStaffedBySupply, string lstPipelineIdsOfOppsUpdatedInBOSS, IEnumerable<Guid?> planningBoardPipelineIds,
            string loggedInUser = null);
        Task<IEnumerable<OpportunityData>> GetOpportunitiesForNewStaffingTab(DemandFilterCriteria filterObj, IEnumerable<ResourceAssignmentViewModel> allocationsStaffedBySupply,
              IEnumerable<Resource> employeesIncludingTerminated,
              IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup, IEnumerable<InvestmentCategory> investmentCategories,
              IList<Revenue> revenueByServiceLinesOpps, IEnumerable<Guid?> planningBoardPipelineIds, IList<CaseAttribute> caseAttributeLookupList,
              IEnumerable<CaseRoleType> caseRoleTypeList, IEnumerable<CommitmentType> commitmentTypeList, string loggedInUser = null);
        Task<IEnumerable<OpportunityData>> GetPinnedOpportunitiesDetails(DemandFilterCriteria filterObj, string opportunityExceptionShowList, IEnumerable<Resource> employeesIncludingTerminated,
            IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup, IEnumerable<InvestmentCategory> investmentCategories,
            IList<Revenue> revenueByServiceLinesOpps, IEnumerable<Guid?> planningBoardPipelineIds, IList<CaseAttribute> caseAttributeLookupList,
            IEnumerable<CaseRoleType> caseRoleTypeList, IEnumerable<CommitmentType> commitmentTypeList, string loggedInUser = null);

        Task<IEnumerable<OpportunityData>> GetHiddenOpportunitiesDetails(DemandFilterCriteria filterObj, IEnumerable<Resource> employeesIncludingTerminated,
            IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup, IEnumerable<InvestmentCategory> investmentCategories,
            IList<Revenue> revenueByServiceLinesOpps, IEnumerable<Guid?> planningBoardPipelineIds, IList<CaseAttribute> caseAttributeLookupList,
            IEnumerable<CaseRoleType> caseRoleTypeList, IEnumerable<CommitmentType> commitmentTypeList, string loggedInUser = null);
        Task<IEnumerable<OpportunityData>> GetOpportunitiesByPipelineIdsAndFilterValues(string pipelineIds, DemandFilterCriteria filterObj,
            IEnumerable<Resource> employeesIncludingTerminated, IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup,
            IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesOpps, string loggedInUser = null);
        Task<IEnumerable<OpportunityData>> GetFilteredOpportunitiesByPipelineIds(string pipelineIds, DemandFilterCriteria filterObj,
            IEnumerable<Resource> employeesIncludingTerminated, IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup,
            IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesOpps, string loggedInUser = null);
        Task<IEnumerable<OpportunityData>> GetOpportunitiesByFilterValues(DemandFilterCriteria filterObj,
            IEnumerable<Resource> employeesIncludingTerminated, IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup,
            IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesOpps);

        Task<OpportunityDetails> GetOpportunityAndAllocationsByPipelineId(Guid pipelineId);
        Task<OpportunityDetails> GetOpportunity(Guid pipelineId);

        Task<IEnumerable<OpportunityData>> GetOpportunitiesForTypeahead(string searchString);
    }
}
