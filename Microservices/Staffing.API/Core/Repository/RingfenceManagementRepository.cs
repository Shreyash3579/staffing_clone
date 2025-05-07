using Dapper;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class RingfenceManagementRepository : IRingfenceManagementRepository
    {
        private readonly IBaseRepository<RingfenceManagement> _baseRepository;

        public RingfenceManagementRepository(IBaseRepository<RingfenceManagement> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<RingfenceManagement>> GetRingfenceAuditLogByOfficeAndCommitmentCode(string officeCode, string commitmentTypeCode)
        {
            var auditLog = await _baseRepository.GetAllAsync(new { officeCode, commitmentTypeCode }, StoredProcedureMap.GetRingfenceAuditLogByOfficeAndCommitmentCode);

            return auditLog;
        }
        public async Task<IEnumerable<RingfenceManagement>> GetRingfencesDetailsByOfficesAndCommitmentCodes(string officeCodes, string commitmentTypeCodes)
        {
            var ringfencesData = await _baseRepository.GetAllAsync(new { officeCodes, commitmentTypeCodes }, StoredProcedureMap.GetRingfencesDetailsByOfficesAndCommitmentCodes);

            return ringfencesData;
        }
        public async Task<RingfenceManagement> UpsertRingfenceDetails(RingfenceManagement ringfenceManagement)
        {
            var upsertedDetails = await _baseRepository.UpsertAsync(
                    StoredProcedureMap.UpsertRingfenceDetails,
                    new
                    {
                        ringfenceManagement.Id,
                        ringfenceManagement.OfficeCode,
                        ringfenceManagement.CommitmentTypeCode,
                        ringfenceManagement.RfTeamsOwed,
                        ringfenceManagement.EffectiveDate,
                        ringfenceManagement.LastUpdatedBy
                    }
                );

            return upsertedDetails;
        }
    }
}
