using Staffing.HttpAggregator.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IAuditTrailService
    {
        Task<IEnumerable<AuditTrailViewModel>> GetAuditTrailForCaseOrOpportunity(string oldCaseCode, string pipelineId, int? limit, int? offset);
        Task<IEnumerable<AuditTrailViewModel>> GetAuditTrailForEmployee(string employeeCode, int? limit, int? offset);
    }
}