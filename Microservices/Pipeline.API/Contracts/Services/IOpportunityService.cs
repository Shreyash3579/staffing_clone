using Pipeline.API.Models;
using Pipeline.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pipeline.API.Contracts.Services
{
    public interface IOpportunityService
    {
        Task<IEnumerable<OpportunityViewModel>> GetOpportunitiesByOfficesActiveInDateRange(DateTime startDate, DateTime endDate, string officeCodes, string opportunityStatusTypes, string clientCodes);
        Task<IEnumerable<OpportunityDetailsViewModel>> GetOpportunityDetailsByPipelineIds(string pipelineIds);

        Task<IEnumerable<OpportunityViewModel>> GetOpportunitiesForTypeahead(string searchString);
        Task<IEnumerable<OpportunityViewModel>> GetOpportunitiesWithTaxonomiesByPipelineIds(string pipelineIdList,
            string officeCodes = null, string opportunityStatusTypeCodes = null);
        Task<IEnumerable<OpportunityViewModel>> GetOpportunityMasterChangesSinceLastPolled(DateTime? lastPolledDateTime);
        Task<IEnumerable<OpportunityFlatViewModel>> GetOpportunitiesFlatData(DateTime? lastUpdated);
        Task<IEnumerable<Opportunity>> GetOppDataFromCortex(string CortexOpportunityId);
    }
}
