using Dapper;
using Pipeline.API.Contracts.Helpers;
using Pipeline.API.Contracts.RepositoryInterfaces;
using Pipeline.API.Core.Helpers;
using Pipeline.API.Models;
using Pipeline.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Pipeline.API.Core.Repository
{
    public class OpportunityRepository : IOpportunityRepository
    {
        private readonly IBaseRepository<Opportunity> _baseRepository;

        public OpportunityRepository(IDapperContext context, IBaseRepository<Opportunity> baseRepository)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = context;
        }

        public async Task<IEnumerable<Opportunity>> GetOpportunitiesByOfficesActiveInDateRange(DateTime startDate, DateTime endDate, string officeCodes, string opportunityStatusTypeCodes, string clientCodes)
        {
            var opportunities = await _baseRepository.GetAllAsync(new { startDate, endDate, officeCodes, opportunityStatusTypeCodes, clientCodes },
                StoredProcedureMap.GetOpportunitiesByOffices);

            return opportunities;
        }

        public async Task<IEnumerable<Opportunity>> GetOpportunityDetailsByPipelineIds(string pipelineIds)
        {
            var opportunities = await
                _baseRepository.GetAllAsync(new { pipelineIds },
                    StoredProcedureMap.GetOpportunityByPipelineIds);

            return opportunities;
        }

        public async Task<IEnumerable<Opportunity>> GetOpportunitiesForTypeahead(string searchString)
        {
            var opportunities = await _baseRepository.GetAllAsync(new { searchString },
                StoredProcedureMap.GetOpportunitiesForTypeahead);

            return opportunities;
        }

        public async Task<IEnumerable<Opportunity>> GetOpportunitiesWithTaxonomiesByPipelineIds(string pipelineIds, 
            string officeCodes, string opportunityStatusTypeCodes)
        {
            var opportunities = await _baseRepository.GetAllAsync(new { pipelineIds, officeCodes, opportunityStatusTypeCodes },
                StoredProcedureMap.GetOpportunitiesWithTaxonomiesByPipelineIds);

            return opportunities;
        }

        public async Task<IEnumerable<Opportunity>> GetOpportunityMasterChangesSinceLastPolled(DateTime? lastPolledDateTime)
        {
            var opportunities = await _baseRepository.GetAllAsync(new { lastPolledDateTime },
                StoredProcedureMap.GetOpportunityMasterChangesSinceLastPolled);

            return opportunities;
        }

        public async Task<IEnumerable<OpportunityFlatViewModel>> GetOpportunitiesFlatData(DateTime? lastUpdated)
        {
            var opportunitiesFlatData = await _baseRepository.Context.Connection.QueryAsync<OpportunityFlatViewModel>(
               StoredProcedureMap.GetOpportunitiesFlatData,
               new
               {
                   lastUpdated
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return opportunitiesFlatData;
        }
    }
}
