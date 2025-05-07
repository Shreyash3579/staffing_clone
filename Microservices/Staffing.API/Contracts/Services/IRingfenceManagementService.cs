using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IRingfenceManagementService
    {
        Task<IEnumerable<RingfenceManagement>> GetRingfencesDetailsByOfficesAndCommitmentCodes(string officeCodes, string commitmentTypeCodes);
        Task<IEnumerable<RingfenceManagement>> GetRingfenceAuditLogByOfficeAndCommitmentCode(string officeCode, string commitmentTypeCode);
        Task<RingfenceManagement> UpsertRingfenceDetails(RingfenceManagement ringfenceManagement);
    }
}
