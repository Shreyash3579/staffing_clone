using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IPipelineApiClient
    {
        Task<IList<OpportunityData>> GetOpportunitiesByOfficesActiveInDateRange(string officeCodes, DateTime startDate,
            DateTime endDate, string opportunityStatusTypeCodes, string clientCodes);
        Task<OpportunityDetails> GetOpportunityDetailsByPipelineId(Guid pipelineId);
        Task<IList<OpportunityData>> GetOpportunitiesByPipelineIds(string pipelineIds, string officeCodes = null,
            string opportunityStatusTypeCodes = null);

        Task<IList<OpportunityData>> GetOpportunitiesWithTaxonomiesByPipelineIds(string pipelineIds);

        Task<IEnumerable<OpportunityData>> GetOpportunitiesForTypeahead(string searchString);
    }
}
