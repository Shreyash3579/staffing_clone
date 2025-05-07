using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface IAuditTrailRepository
    {
        Task<IEnumerable<AuditCaseHistory>> GetAuditTrailForCaseOrOpportunity(string oldCaseCode, string pipelineId, int? limit, int? offset);
        Task<IEnumerable<AuditEmployeeHistory>> GetAuditTrailForEmployee(string employeeCode, int? limit, int? offset);
    }
}
