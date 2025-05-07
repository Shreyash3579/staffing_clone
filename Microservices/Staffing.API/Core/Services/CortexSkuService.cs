using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class CortexSkuService : ICortexSkuService
    {
        private readonly ICortexSkuRepository _cortexSkuRepository;

        public CortexSkuService(ICortexSkuRepository cortexSkuRepository)
        {
            _cortexSkuRepository = cortexSkuRepository;
        }

        public async Task<IEnumerable<CortexSkuMapping>> GetCortexSkuMappings()
        {
            return await _cortexSkuRepository.GetCortexSkuMappings();
        }

        public async Task<IEnumerable<CaseOppCortexTeamSize>> UpsertPlaceholderCreatedForCortexSKUs(CaseOppCortexTeamSize caseOppCortexTeamSize)
        {
            if (string.IsNullOrEmpty(caseOppCortexTeamSize.OldCaseCode) && (caseOppCortexTeamSize.PipelineId == null || caseOppCortexTeamSize.PipelineId.Value == Guid.Empty))
            {
                throw new ArgumentException("Either oldCaseCode or pipelineId is mandatory");
            }
            return await _cortexSkuRepository.UpsertPlaceholderCreatedForCortexSKUs(caseOppCortexTeamSize);
        }

        public async Task<IEnumerable<CaseOppCortexTeamSize>> GetOppCortexPlaceholderInfoByPipelineIds(string pipelineIds)
        {
            if (string.IsNullOrEmpty(pipelineIds))
            {
                throw new ArgumentException("At least one pipelineId is mandatory");
            }
            return await _cortexSkuRepository.GetOppCortexPlaceholderInfoByPipelineIds(pipelineIds);
        }

        public async Task<IEnumerable<CaseOppCortexTeamSize>> UpsertPricingSKU(CaseOppCortexTeamSize caseOppTeamSize)
        {
            if (caseOppTeamSize == null || (caseOppTeamSize?.PipelineId == null || caseOppTeamSize?.PipelineId.Value == Guid.Empty))
            {
                throw new ArgumentException("Either oldCaseCode or pipelineId is mandatory");
            }
            return await _cortexSkuRepository.UpsertPricingSKU(caseOppTeamSize);
        }

        public async Task<IEnumerable<PricingSkuViewModel>> UpsertPricingSkuDataLog(IEnumerable<PricingSkuViewModel> pricingTeamSizeDataLogs)
        {
            if (pricingTeamSizeDataLogs.Count() < 1)
            {
                return Enumerable.Empty<PricingSkuViewModel>();
            }

            var pricingTeamSizeDataTable = CreatePricingTeamSizeDataTable(pricingTeamSizeDataLogs);
            return await _cortexSkuRepository.UpsertPricingSkuDataLog(pricingTeamSizeDataTable);
        }

        private DataTable CreatePricingTeamSizeDataTable(IEnumerable<PricingSkuViewModel> pricingTeamSizeDataLogs)
        {
            var pricingTeamSizeDataTable = new DataTable();
            pricingTeamSizeDataTable.Columns.Add("id", typeof(Guid));
            pricingTeamSizeDataTable.Columns.Add("sf_opportunity_id", typeof(string));
            pricingTeamSizeDataTable.Columns.Add("sf_opportunity_name", typeof(string));
            pricingTeamSizeDataTable.Columns.Add("sf_opportunity_substage", typeof(string));
            pricingTeamSizeDataTable.Columns.Add("title", typeof(string));
            pricingTeamSizeDataTable.Columns.Add("teamname", typeof(string));
            pricingTeamSizeDataTable.Columns.Add("country_id", typeof(Guid));
            pricingTeamSizeDataTable.Columns.Add("countryname", typeof(string));
            pricingTeamSizeDataTable.Columns.Add("name", typeof(string));
            pricingTeamSizeDataTable.Columns.Add("abbreviation", typeof(string));
            pricingTeamSizeDataTable.Columns.Add("updated_at", typeof(string));
            pricingTeamSizeDataTable.Columns.Add("allocation_percentage", typeof(Int16));
            pricingTeamSizeDataTable.Columns.Add("username", typeof(string));
            pricingTeamSizeDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var pricingTeamSizeDataLog in pricingTeamSizeDataLogs)
            {
                var row = pricingTeamSizeDataTable.NewRow();
                row["sf_opportunity_id"] = (object)pricingTeamSizeDataLog.sf_opportunity_id ?? DBNull.Value;
                row["sf_opportunity_name"] = (object)pricingTeamSizeDataLog.sf_opportunity_name ?? DBNull.Value;
                row["sf_opportunity_substage"] = (object)pricingTeamSizeDataLog.sf_opportunity_substage ?? DBNull.Value;
                row["title"] = (object)pricingTeamSizeDataLog.title ?? DBNull.Value;
                row["teamname"] = (object)pricingTeamSizeDataLog.teamname ?? DBNull.Value;
                row["country_id"] = (object)pricingTeamSizeDataLog.country_id ?? DBNull.Value;
                row["countryname"] = (object)pricingTeamSizeDataLog.countryname ?? DBNull.Value;
                row["name"] = (object)pricingTeamSizeDataLog.name ?? DBNull.Value;
                row["abbreviation"] = (object)pricingTeamSizeDataLog.abbreviation ?? DBNull.Value;
                row["updated_at"] = (object)pricingTeamSizeDataLog.updated_at ?? DBNull.Value;
                row["allocation_percentage"] = (object)pricingTeamSizeDataLog.allocation_percentage ?? DBNull.Value;
                row["username"] = (object)pricingTeamSizeDataLog.username ?? DBNull.Value;
                row["lastUpdatedBy"] = (object)pricingTeamSizeDataLog.lastUpdatedBy;
                pricingTeamSizeDataTable.Rows.Add(row);
            }

            return pricingTeamSizeDataTable;
        }

    }
}
