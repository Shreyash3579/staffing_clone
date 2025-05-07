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
    public class StaffableAsRepository : IStaffableAsRepository
    {
        private readonly IBaseRepository<StaffableAs> _baseRepository;

        public StaffableAsRepository(IBaseRepository<StaffableAs> baseRepository, IDapperContext dapperContext)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = dapperContext;
        }

        public async Task<IEnumerable<StaffableAs>> GetResourceActiveStaffableAsByEmployeeCodes(string employeeCodes)
        {
            var staffableAsData = await
                _baseRepository.GetAllAsync(new { employeeCodes },
                    StoredProcedureMap.GetResourceActiveStaffableAsByEmployeeCodes);

            return staffableAsData;
        }
        public async Task<IEnumerable<StaffableAs>> UpsertResourceStaffableAs(DataTable employeeStaffableAsData)
        {
            var upsertedData = await _baseRepository.Context.Connection.QueryAsync<StaffableAs>(
               StoredProcedureMap.UpsertResourceStaffableAs,
               new
               {
                   @employeeStaffableAs =
                       employeeStaffableAsData.AsTableValuedParameter(
                           "[dbo].[staffableAsTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return upsertedData;
        }
        public async Task DeleteResourceStaffableAsById(string idToDelete, string lastUpdatedBy)
        {
            await _baseRepository.DeleteAsync(new { idToDelete, lastUpdatedBy }, StoredProcedureMap.DeleteResourceStaffableAsById);
        }

    }
}