using Dapper;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class AuditTrailRepository : IAuditTrailRepository
    {
        private readonly IBaseRepository<AuditCaseHistory> _baseRepository;

        public AuditTrailRepository(IBaseRepository<AuditCaseHistory> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task<IEnumerable<AuditCaseHistory>> GetAuditTrailForCaseOrOpportunity(string oldCaseCode, string pipelineId, int? limit, int? offset)
        {
            var auditTrail = await _baseRepository.GetAllAsync(new { oldCaseCode, pipelineId, limit, offset }, StoredProcedureMap.GetAuditTrailForCaseOrOpportunity);
            return auditTrail;
        }

        public async Task<IEnumerable<AuditEmployeeHistory>> GetAuditTrailForEmployee(string employeeCode, int? limit, int? offset)
        {
            var auditTrail = await Task.Run(() => _baseRepository.Context.Connection.Query<AuditEmployeeHistory>(
                StoredProcedureMap.GetAuditTrailForEmployee,
                new { employeeCode, limit, offset },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return auditTrail;
        }
    }
}
