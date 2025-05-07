using Dapper;
using Staffing.API.Contracts.Helpers;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class CortexSkuRepository : ICortexSkuRepository
    {
        private readonly IBaseRepository<CortexSkuMapping> _baseRepository;

        public CortexSkuRepository(IBaseRepository<CortexSkuMapping> baseRepository, IDapperContext context)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = context;
        }

        public async Task<IEnumerable<CortexSkuMapping>> GetCortexSkuMappings()
        {
            var cortexSkuMappings = await Task.Run(() => _baseRepository.Context.Connection.QueryAsync<CortexSkuMapping>(
                StoredProcedureMap.GetCortexSkuMappings,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return cortexSkuMappings;
        }

        public async Task<IEnumerable<CaseOppCortexTeamSize>> UpsertPlaceholderCreatedForCortexSKUs(CaseOppCortexTeamSize caseOppCortexTeamSize)
        {
            var upsertedCaseOppCortexTeamSize = await Task.Run(() => _baseRepository.Context.Connection.QueryAsync<CaseOppCortexTeamSize>(
                StoredProcedureMap.UpsertPlaceholderCreatedForCortexSKUs,
                new {
                        caseOppCortexTeamSize.OldCaseCode,
                        caseOppCortexTeamSize.PipelineId,
                        caseOppCortexTeamSize.cortexOpportunityId,
                        caseOppCortexTeamSize.EstimatedTeamSize,
                        caseOppCortexTeamSize.IsPlaceholderCreatedFromCortex,
                        caseOppCortexTeamSize.LastUpdatedBy
                    },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return upsertedCaseOppCortexTeamSize;
        }

        public async Task<IEnumerable<CaseOppCortexTeamSize>> GetOppCortexPlaceholderInfoByPipelineIds(string pipelineIds)
        {
            var oppsCortexPlaceholderInfo = await Task.Run(() => _baseRepository.Context.Connection.QueryAsync<CaseOppCortexTeamSize>(
                StoredProcedureMap.GetOppCortexPlaceholderInfoByPipelineIds,
                new { pipelineIds },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return oppsCortexPlaceholderInfo;
        }

        public async Task<IEnumerable<CaseOppCortexTeamSize>> UpsertPricingSKU(CaseOppCortexTeamSize caseOppTeamSize)
        {
            var upsertedCaseOppTeamSize = await Task.Run(() => _baseRepository.Context.Connection.QueryAsync<CaseOppCortexTeamSize>(
                StoredProcedureMap.UpsertPricingSKU,
                new
                {
                    caseOppTeamSize.PipelineId,
                    caseOppTeamSize.cortexOpportunityId,
                    caseOppTeamSize.PricingTeamSize,
                    caseOppTeamSize.LastUpdatedBy
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return upsertedCaseOppTeamSize;
        }

        
        public async Task<IEnumerable<PricingSkuViewModel>> UpsertPricingSkuDataLog(DataTable pricingTeamSizeDataTable)
        {
            var upsertedPricingTeamSizeDataLog = await _baseRepository.Context.Connection.QueryAsync<PricingSkuViewModel>(
                StoredProcedureMap.UpsertPricingSkuDataLog,
                new
                {
                    pricingSkuDataLogs =
                        pricingTeamSizeDataTable.AsTableValuedParameter(
                            "[dbo].[pricingSkuDataLogTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return upsertedPricingTeamSizeDataLog;
        }

    }
}
