using Staffing.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface IEmployeeStaffingInfoRepository
    {
        Task<IEnumerable<StaffingResponsible>> GetResourceStaffingResponsibleByEmployeeCodes(string employeeCodes);
        Task<IEnumerable<StaffingResponsible>> UpsertEmployeeStaffingResponsible(DataTable employeeStaffingResponsibleData);
    }
}
