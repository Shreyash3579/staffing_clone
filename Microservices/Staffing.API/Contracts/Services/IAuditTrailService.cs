using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IAuditTrailService
    {
        Task<IEnumerable<AuditCaseHistory>> GetAuditTrailForCaseOrOpportunity(string oldCaseCode, string pipelineId, int? limit, int? offset);
        Task<IEnumerable<AuditEmployeeHistory>> GetAuditTrailForEmployee(string employeeCode, int? limit, int? offset);
    }
}
