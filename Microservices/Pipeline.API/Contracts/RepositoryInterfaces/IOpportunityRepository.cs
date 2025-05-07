using Pipeline.API.Models;
using Pipeline.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pipeline.API.Contracts.RepositoryInterfaces
{
    public interface IOpportunityRepository
    {
        Task<IEnumerable<Opportunity>> GetOpportunitiesByOfficesActiveInDateRange(DateTime startDate, DateTime endDate, string officeCodes, string opportunityStatusTypeCodes, string clientCodes);
        Task<IEnumerable<Opportunity>> GetOpportunityDetailsByPipelineIds(string pipelineId);
        Task<IEnumerable<Opportunity>> GetOpportunitiesWithTaxonomiesByPipelineIds(string pipelineIds, string officeCodes, string opportunityStatusTypeCodes);
        Task<IEnumerable<Opportunity>> GetOpportunitiesForTypeahead(string searchString);
        Task<IEnumerable<Opportunity>> GetOpportunityMasterChangesSinceLastPolled(DateTime? lastPolledDateTime);
        Task<IEnumerable<OpportunityFlatViewModel>> GetOpportunitiesFlatData(DateTime? lastUpdated);
    }
}
