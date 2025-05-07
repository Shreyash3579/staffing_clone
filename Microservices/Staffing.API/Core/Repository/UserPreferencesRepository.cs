using Staffing.API.Contracts.Helpers;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class UserPreferencesRepository : IUserPreferencesRepository
    {
        private readonly IBaseRepository<UserPreferences> _baseRepository;

        public UserPreferencesRepository(IBaseRepository<UserPreferences> baseRepository, IDapperContext context)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = context;

        }

        public async Task<UserPreferences> GetUserPreferences(string employeeCode)
        {
            var userPreferences = await
                _baseRepository.GetByDynamicAsync(new { employeeCode },
                    StoredProcedureMap.GetUserPreferences);

            return userPreferences;
        }

        public async Task<UserPreferences> InsertUserPreferences(UserPreferences userPreferences)
        {
            var savedUserPreferences = await
                _baseRepository.InsertAsync(new
                {
                    userPreferences.EmployeeCode,
                    userPreferences.SupplyViewOfficeCodes,
                    userPreferences.SupplyViewStaffingTags,
                    userPreferences.LevelGrades,
                    userPreferences.AvailabilityIncludes,
                    userPreferences.GroupBy,
                    userPreferences.SortBy,
                    userPreferences.SupplyWeeksThreshold,
                    userPreferences.VacationThreshold,
                    userPreferences.TrainingThreshold,
                    userPreferences.DemandTypes,
                    userPreferences.DemandViewOfficeCodes,
                    userPreferences.CaseTypeCodes,
                    userPreferences.DemandWeeksThreshold,
                    userPreferences.CaseExceptionShowList,
                    userPreferences.CaseExceptionHideList,
                    userPreferences.OpportunityExceptionShowList,
                    userPreferences.OpportunityExceptionHideList,
                    userPreferences.CaseAttributeNames,
                    userPreferences.OpportunityStatusTypeCodes,
                    userPreferences.MinOpportunityProbability,
                    userPreferences.CaseAllocationsSortBy,
                    userPreferences.LastUpdatedBy,
                    userPreferences.PracticeAreaCodes,
                    userPreferences.PositionCodes,
                    userPreferences.PlanningCardsSortOrder,
                    userPreferences.CaseOppSortOrder,
                    userPreferences.AffiliationRoleCodes,
                    userPreferences.IndustryPracticeAreaCodes,
                    userPreferences.CapabilityPracticeAreaCodes,
                    userPreferences.IsDefault,
                    userPreferences.IsHistoricalDemandPinned,
                    userPreferences.StaffableAsTypeCodes
                }, StoredProcedureMap.InsertUserPreferences);

            return savedUserPreferences;
        }

        public async Task<UserPreferences> UpdateUserPreferences(UserPreferences userPreferences)
        {
            var savedUserPreferences = await
                _baseRepository.InsertAsync(new
                {
                    userPreferences.EmployeeCode,
                    userPreferences.SupplyViewOfficeCodes,
                    userPreferences.SupplyViewStaffingTags,
                    userPreferences.LevelGrades,
                    userPreferences.AvailabilityIncludes,
                    userPreferences.GroupBy,
                    userPreferences.SortBy,
                    userPreferences.SupplyWeeksThreshold,
                    userPreferences.VacationThreshold,
                    userPreferences.TrainingThreshold,
                    userPreferences.DemandViewOfficeCodes,
                    userPreferences.CaseTypeCodes,
                    userPreferences.DemandTypes,
                    userPreferences.DemandWeeksThreshold,
                    userPreferences.CaseExceptionShowList,
                    userPreferences.CaseExceptionHideList,
                    userPreferences.OpportunityExceptionShowList,
                    userPreferences.OpportunityExceptionHideList,
                    userPreferences.CaseAttributeNames,
                    userPreferences.OpportunityStatusTypeCodes,
                    userPreferences.MinOpportunityProbability,
                    userPreferences.CaseAllocationsSortBy,
                    userPreferences.LastUpdatedBy,
                    userPreferences.PracticeAreaCodes,
                    userPreferences.PositionCodes,
                    userPreferences.PlanningCardsSortOrder,
                    userPreferences.CaseOppSortOrder,
                    userPreferences.AffiliationRoleCodes,
                    userPreferences.IndustryPracticeAreaCodes,
                    userPreferences.CapabilityPracticeAreaCodes,
                    userPreferences.IsDefault,
                    userPreferences.IsHistoricalDemandPinned,
                    userPreferences.StaffableAsTypeCodes
                }, StoredProcedureMap.UpdateUserPreferences);

            return savedUserPreferences;
        }

        public async Task<UserPreferences> UpsertUserPreferences(UserPreferences userPreferences)
        {
            var savedUserPreferences = await
                _baseRepository.UpdateAsync(new
                {
                    userPreferences.EmployeeCode,
                    userPreferences.SupplyViewOfficeCodes,
                    userPreferences.SupplyViewStaffingTags,
                    userPreferences.LevelGrades,
                    userPreferences.AvailabilityIncludes,
                    userPreferences.GroupBy,
                    userPreferences.SortBy,
                    userPreferences.SupplyWeeksThreshold,
                    userPreferences.VacationThreshold,
                    userPreferences.TrainingThreshold,
                    userPreferences.DemandViewOfficeCodes,
                    userPreferences.CaseTypeCodes,
                    userPreferences.DemandTypes,
                    userPreferences.DemandWeeksThreshold,
                    userPreferences.CaseExceptionShowList,
                    userPreferences.CaseExceptionHideList,
                    userPreferences.OpportunityExceptionShowList,
                    userPreferences.OpportunityExceptionHideList,
                    userPreferences.CaseAttributeNames,
                    userPreferences.OpportunityStatusTypeCodes,
                    userPreferences.MinOpportunityProbability,
                    userPreferences.CaseAllocationsSortBy,
                    userPreferences.LastUpdatedBy,
                    userPreferences.PracticeAreaCodes,
                    userPreferences.PositionCodes,
                    userPreferences.PlanningCardsSortOrder,
                    userPreferences.CaseOppSortOrder,
                    userPreferences.AffiliationRoleCodes,
                    userPreferences.IndustryPracticeAreaCodes,
                    userPreferences.CapabilityPracticeAreaCodes,
                    userPreferences.IsDefault,
                    userPreferences.IsHistoricalDemandPinned,
                    userPreferences.StaffableAsTypeCodes
                }, StoredProcedureMap.UpsertUserPreferences);

            return savedUserPreferences;
        }

        public async Task<UserPreferences> DeleteUserPreferences(string employeeCode)
        {
            return await _baseRepository.DeleteAsync(new { employeeCode }, StoredProcedureMap.DeleteUserPreferences);
        }
    }
}
