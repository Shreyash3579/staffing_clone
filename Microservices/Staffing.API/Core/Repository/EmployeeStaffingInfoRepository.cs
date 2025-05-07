using Dapper;
using Staffing.API.Contracts.Helpers;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class EmployeeStaffingInfoRepository : IEmployeeStaffingInfoRepository
    {
        private readonly IBaseRepository<StaffingResponsible> _baseRepository;

        public EmployeeStaffingInfoRepository(IBaseRepository<StaffingResponsible> baseRepository, IDapperContext dapperContext)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = dapperContext;
        }

        public async Task<IEnumerable<StaffingResponsible>> GetResourceStaffingResponsibleByEmployeeCodes(string employeeCodes)
        {
            var staffingResponsibleData = await
                _baseRepository.GetAllAsync(new { employeeCodes },
                    StoredProcedureMap.GetResourceStaffingResponsibleByEmployeeCodes);

            return staffingResponsibleData;
        }
        public async Task<IEnumerable<StaffingResponsible>> UpsertEmployeeStaffingResponsible(DataTable employeeStaffingResponsibleData)
        {
            var upsertedData = await _baseRepository.Context.Connection.QueryAsync<StaffingResponsible>(
               StoredProcedureMap.UpsertEmployeeStaffingResponsible,
               new
               {
                   @employeeStaffingInfo =
                       employeeStaffingResponsibleData.AsTableValuedParameter(
                           "[dbo].[employeeStaffingInfoTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return upsertedData;
        }

    }
}