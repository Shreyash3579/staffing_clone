using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Core.Repository;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class LookupService : ILookupService
    {
        private readonly ILookupRepository _lookupRepository;

        public LookupService(ILookupRepository lookupRepository)
        {
            _lookupRepository = lookupRepository;
        }

        public async Task<IEnumerable<InvestmentCategory>> GetInvestmentCategoryLookupList()
        {
            return await _lookupRepository.GetInvestmentCategoryLookupList();
        }

        public async Task<IEnumerable<CaseRoleType>> GetCaseRoleTypeLookupList()
        {
            return await _lookupRepository.GetCaseRoleTypeLookupList();
        }

        public async Task<IEnumerable<StaffableAsType>> GetStaffableAsTypeLookupList()
        {
            return await _lookupRepository.GetStaffableAsTypeLookupList();
        }

        public async Task<IEnumerable<CasePlanningBoardBucket>> GetCasePlanningBoardBucketLookupListByEmployee(string employeeCode)
        {
            return await _lookupRepository.GetCasePlanningBoardBucketLookupListByEmployee(employeeCode);
        }

        public async Task<IEnumerable<UserPersonaType>> GetUserPersonaTypeLookupList()
        {
            return await _lookupRepository.GetUserPersonaTypeLookupList();
        }

        public async Task<IEnumerable<SecurityRole>> GetSecurityRoles()
        {
            return await _lookupRepository.GetSecurityRoles();
        }

        public async Task<IEnumerable<SecurityFeature>> GetSecurityFeatures()
        {
            return await _lookupRepository.GetSecurityFeatures();
        }

        public async Task<IEnumerable<StaffingPreferences>> GetStaffingPreferences()
        {
            return await _lookupRepository.GetStaffingPreferences();
        }

        public async Task<IEnumerable<SKUTerm>> GetSKUTermList()
        {
            var skuTermList = await _lookupRepository.GetSKUTermList();
            return skuTermList;
        }

        public async Task<UserPreferences> GetUserPreferences(string employeeCode)
        {
            return await _lookupRepository.GetUserPreferences(employeeCode);
        }

        public async Task<IEnumerable<CommitmentType>> GetCommitmentTypeLookupList(bool? showHidden)
        {
            return await _lookupRepository.GetCommitmentTypeLookupList(showHidden);
        }

        public async Task<IEnumerable<CommitmentTypeReason>> GetCommitmentTypeReasonLookupList()
        {
            return await _lookupRepository.GetCommitmentTypeReasonLookupList();
        }

    }
}
