using Dapper;
using Staffing.API.Contracts.Helpers;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class EmployeeStaffingPreferenceRepository : IEmployeeStaffingPreferenceRepository
    {
        private readonly IBaseRepository<EmployeeStaffingPreferences> _baseRepository;

        public EmployeeStaffingPreferenceRepository(IBaseRepository<EmployeeStaffingPreferences> baseRepository, IDapperContext context)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = context;
        }

        public async Task<IEnumerable<EmployeeStaffingPreferences>> GetEmployeeStaffingPreferences(string employeeCodes)
        {
            var employeeStaffingPrefereces = await _baseRepository
                .GetAllAsync(new { employeeCodes}, StoredProcedureMap.GetEmployeeStaffingPreferences);
            return employeeStaffingPrefereces;
        }
        public async Task<IEnumerable<EmployeeStaffingPreferences>> UpsertEmployeeStaffingPreferences(DataTable employeeStaffingPreferenes) 
        {
            var upsertedEmployeeStaffingPreferences = await _baseRepository.Context.Connection.QueryAsync<EmployeeStaffingPreferences>(
                StoredProcedureMap.UpsertEmployeeStaffingPreferences,
                new
                {
                    @employeeStaffingPreferenes =
                        employeeStaffingPreferenes.AsTableValuedParameter(
                            "[dbo].[employeeStaffingPreferenceType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return upsertedEmployeeStaffingPreferences;
        }

          public async Task DeleteEmployeeStaffingPreferenceByType(string employeeCode, string preferenceTypeCode)
        {
            await _baseRepository.DeleteAsync(new { employeeCode, preferenceTypeCode }, StoredProcedureMap.DeleteEmployeeStaffingPreferenceByType);
        }
    }
}
