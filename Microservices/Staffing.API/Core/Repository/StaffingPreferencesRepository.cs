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
    public class StaffingPreferencesRepository : IStaffingPreferencesRepository
    {
        private readonly IBaseRepository<EmployeeStaffingPreferencesForInsightsTool> _baseRepository;

        public StaffingPreferencesRepository(IBaseRepository<EmployeeStaffingPreferencesForInsightsTool> baseRepository, IDapperContext context)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = context;

        }

        public async Task<EmployeeStaffingPreferencesForInsightsTool> GetEmployeePreferences(string employeeCode)
        {
            var userPreferences = await
               _baseRepository.GetByDynamicAsync(new { employeeCode },
                   StoredProcedureMap.GetEmployeeStaffingPreferencesForInsightsTool);

            return userPreferences;
        }

        public async Task<IEnumerable<EmployeeStaffingPreferencesForInsightsTool>> GetAllEmployeePreferences()
        {
            var userPreferences = await Task.Run(() =>
               _baseRepository.Context.Connection.Query<EmployeeStaffingPreferencesForInsightsTool>(
                   StoredProcedureMap.GetAllEmployeeStaffingPreferencesForInsightsTool,
                   commandType: CommandType.StoredProcedure,
                   commandTimeout: 180).ToList());

            return userPreferences;
        }

        public async Task<EmployeeStaffingPreferencesForInsightsTool> UpsertEmployeePreferences(EmployeeStaffingPreferencesForInsightsTool staffingPreferences)
        {
            //Create upsert query just like UpsertUserPreferences using EmployeeStaffingPreferencesForInsightsTool model
            var savedUserPreferences = await
               _baseRepository.UpdateAsync(new
               {
                   staffingPreferences.EmployeeCode,
                   staffingPreferences.PdFocusAreas,
                   staffingPreferences.PdFocusAreasAdditionalInformation,
                   staffingPreferences.FirstPriority,
                   staffingPreferences.SecondPriority,
                   staffingPreferences.ThirdPriority,
                   staffingPreferences.IndustryCodesHappyToWorkIn,
                   staffingPreferences.IndustryCodesExcitedToWorkIn,
                   staffingPreferences.IndustryCodesNotInterestedToWorkIn,
                   staffingPreferences.CapabilityCodesHappyToWorkIn,
                   staffingPreferences.CapabilityCodesExcitedToWorkIn,
                   staffingPreferences.CapabilityCodesNotInterestedToWorkIn,
                   staffingPreferences.PreBainExperience,
                   staffingPreferences.TravelInterest,
                   staffingPreferences.TravelRegions,
                   staffingPreferences.TravelDuration,
                   staffingPreferences.AdditionalTravelInfo,
                   staffingPreferences.LastUpdatedBy
                }, StoredProcedureMap.UpsertEmployeeStaffingPreferencesForInsightsTool);

            return savedUserPreferences;

        }
    }
}
