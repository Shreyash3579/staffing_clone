using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class AuditTrailService : IAuditTrailService
    {
        private readonly IAuditTrailRepository _auditTrailRepository;

        public AuditTrailService(IAuditTrailRepository auditTrailRepository)
        {
            _auditTrailRepository = auditTrailRepository;
        }
        public async Task<IEnumerable<AuditCaseHistory>> GetAuditTrailForCaseOrOpportunity(string oldCaseCode, string pipelineId, int? limit, int? offset)
        {
            var auditData = await _auditTrailRepository.GetAuditTrailForCaseOrOpportunity(oldCaseCode, pipelineId, limit, offset);
            return auditData;

        }

        public async Task<IEnumerable<AuditEmployeeHistory>> GetAuditTrailForEmployee(string employeeCode, int? limit, int? offset)
        {
            var auditEmployeeHistories = await _auditTrailRepository.GetAuditTrailForEmployee(employeeCode, limit, offset);
            return auditEmployeeHistories;
        }

    }
}
