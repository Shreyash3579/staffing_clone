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
    public class PreponedCasesAllocationsAuditRepository: IPreponedCasesAllocationsAuditRepository
    {
        private readonly IBaseRepository<PreponedCasesAllocationsAudit> _baseRepository;

        public PreponedCasesAllocationsAuditRepository(IBaseRepository<PreponedCasesAllocationsAudit> baseRepository, IDapperContext context)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = context;
        }

        public async Task<IEnumerable<PreponedCasesAllocationsAudit>> UpsertPreponedCaseAllocationsAudit(DataTable preponedCasesAllocationsAuditDataTable)
        {
            var auditData = await _baseRepository.Context.Connection.QueryAsync<PreponedCasesAllocationsAudit>(
                StoredProcedureMap.UpsertPreponedCaseAllocationsAudit,
                new
                {
                    preponedCaseAllocationAudit =
                        preponedCasesAllocationsAuditDataTable.AsTableValuedParameter(
                            "[dbo].[preponedCaseAllocationAuditTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return auditData;
        }

        public async Task<IEnumerable<PreponedCasesAllocationsAudit>> GetPreponedCaseAllocationsAudit(string serviceLineCodes, string officeCodes,
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var auditData = await
                _baseRepository.GetAllAsync(new { serviceLineCodes, officeCodes, startDate, endDate },
                    StoredProcedureMap.GetPreponedCaseAllocationsAudit);

            return auditData;
        }
    }
}
