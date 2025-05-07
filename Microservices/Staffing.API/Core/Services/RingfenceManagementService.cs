using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class RingfenceManagementService : IRingfenceManagementService
    {
        private readonly IRingfenceManagementRepository _ringfenceManagementRepository;

        public RingfenceManagementService(IRingfenceManagementRepository ringfenceManagementRepository)
        {
            _ringfenceManagementRepository = ringfenceManagementRepository;
        }

        public async Task<IEnumerable<RingfenceManagement>> GetRingfenceAuditLogByOfficeAndCommitmentCode(string officeCode, string commitmentTypeCode)
        {
            if (string.IsNullOrEmpty(officeCode) || string.IsNullOrEmpty(commitmentTypeCode))
            {
                return Enumerable.Empty<RingfenceManagement>();
            }

            var auditLog = await _ringfenceManagementRepository.GetRingfenceAuditLogByOfficeAndCommitmentCode(officeCode, commitmentTypeCode);

            return auditLog ?? Enumerable.Empty<RingfenceManagement>();
        }
        public async Task<IEnumerable<RingfenceManagement>> GetRingfencesDetailsByOfficesAndCommitmentCodes(string officeCodes, string commitmentTypeCodes)
        {
            if (string.IsNullOrEmpty(officeCodes) || string.IsNullOrEmpty(commitmentTypeCodes))
            {
                return Enumerable.Empty<RingfenceManagement>();
            }

            var ringfenceDetails = await _ringfenceManagementRepository.GetRingfencesDetailsByOfficesAndCommitmentCodes(officeCodes, commitmentTypeCodes);
            
            return ringfenceDetails ?? Enumerable.Empty<RingfenceManagement>();
        }
        public async Task<RingfenceManagement> UpsertRingfenceDetails(RingfenceManagement ringfenceManagement)
        {
            var upsertedData = await _ringfenceManagementRepository.UpsertRingfenceDetails(ringfenceManagement);

            return upsertedData;
        }
    }
}
