using Dapper;
using Staffing.API.Contracts.Helpers;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class LookupRepository : ILookupRepository
    {
        private readonly IBaseRepository<InvestmentCategory> _baseRepository;

        public LookupRepository(IBaseRepository<InvestmentCategory> baseRepository, IDapperContext context)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = context;

        }
        public async Task<IEnumerable<InvestmentCategory>> GetInvestmentCategoryLookupList()
        {
            var investmentCategories =
                await _baseRepository.GetAllAsync(null,
                    StoredProcedureMap.GetInvestmentCategoryLookupList);

            return investmentCategories;
        }

        public async Task<IEnumerable<CaseRoleType>> GetCaseRoleTypeLookupList()
        {
            var caseRoleTypes = await Task.Run(() => _baseRepository.Context.Connection.Query<CaseRoleType>(
                StoredProcedureMap.GetCaseRoleTypeLookupList,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return caseRoleTypes;
        }

        public async Task<IEnumerable<StaffableAsType>> GetStaffableAsTypeLookupList()
        {
            var staffableAsTypes = await Task.Run(() => _baseRepository.Context.Connection.Query<StaffableAsType>(
                StoredProcedureMap.GetStaffableAsTypeLookupList,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return staffableAsTypes;
        }

        public async Task<IEnumerable<CasePlanningBoardBucket>> GetCasePlanningBoardBucketLookupListByEmployee(string employeeCode)
        {
            var casePlanningBoardBuckets = await Task.Run(() => _baseRepository.Context.Connection.Query<CasePlanningBoardBucket>(
                StoredProcedureMap.GetCasePlanningBoardBucketLookupListByEmployee,
                new { employeeCode },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return casePlanningBoardBuckets;
        }

        public async Task<IEnumerable<UserPersonaType>> GetUserPersonaTypeLookupList()
        {
            var userPersonaTypes = await Task.Run(() => _baseRepository.Context.Connection.Query<UserPersonaType>(
                StoredProcedureMap.GetUserPersonaTypeLookupList,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return userPersonaTypes;
        }

        public async Task<IEnumerable<SecurityRole>> GetSecurityRoles()
        {
            var userPersonaTypes = await Task.Run(() => _baseRepository.Context.Connection.Query<SecurityRole>(
                StoredProcedureMap.GetSecurityRoleLookupList,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return userPersonaTypes;
        }

        public async Task<IEnumerable<SecurityFeature>> GetSecurityFeatures()
        {
            var securityFeatures = await Task.Run(() => _baseRepository.Context.Connection.Query<SecurityFeature>(
                StoredProcedureMap.GetSecurityFeatureLookupList,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());
            return securityFeatures;
        }

        public async Task<IEnumerable<StaffingPreferences>> GetStaffingPreferences()
        {
            var staffingPreferences = await Task.Run(() => _baseRepository.Context.Connection.Query<StaffingPreferences>(
                StoredProcedureMap.GetStaffingPreferencesLookupList,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return staffingPreferences;
        }

        public async Task<IEnumerable<SKUTerm>> GetSKUTermList()
        {
            var skuTermList = await Task.Run(() => _baseRepository.Context.Connection.Query<SKUTerm>(
                StoredProcedureMap.GetSKUTermList,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return skuTermList;
        }
        public async Task<UserPreferences> GetUserPreferences(string employeeCode)
        {
            var userPreferences = await Task.Run(() => _baseRepository.Context.Connection.QueryFirstOrDefaultAsync<UserPreferences>(
                StoredProcedureMap.GetUserPreferences,
                new { employeeCode },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180
            ));

            return userPreferences;
        }

        public async Task<IEnumerable<CommitmentType>> GetCommitmentTypeLookupList(bool? showHidden)
        {
            var commitmentTypes = await Task.Run(() => _baseRepository.Context.Connection.QueryAsync<CommitmentType>(
                StoredProcedureMap.GetCommitmentTypeLookupList,
                new { showHidden },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return commitmentTypes;
        }

        public async Task<IEnumerable<CommitmentTypeReason>> GetCommitmentTypeReasonLookupList()
        {
            var commitmentTypes = await Task.Run(() => _baseRepository.Context.Connection.QueryAsync<CommitmentTypeReason>(
                StoredProcedureMap.GetCommitmentTypeReasonLookupList,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return commitmentTypes;
        }

    }
}
