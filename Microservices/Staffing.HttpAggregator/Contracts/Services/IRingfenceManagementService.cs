using Staffing.HttpAggregator.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IRingfenceManagementService
    {
        Task<IEnumerable<RingfenceManagementViewModel>> GetTotalResourcesByOfficesAndRingfences(string officeCodes, string commitmentTypeCodes);
        Task<IEnumerable<RingfenceManagementViewModel>> GetRingfenceAuditLogByOfficeAndCommitmentCode(string officeCode, string commitmentTypeCode);
    }
}
