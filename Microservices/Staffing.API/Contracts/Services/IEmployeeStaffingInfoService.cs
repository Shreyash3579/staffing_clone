using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IEmployeeStaffingInfoService
    {
            Task<IEnumerable<StaffingResponsible>> GetResourceStaffingResponsibleByEmployeeCodes(string employeeCodes);
            Task<IEnumerable<StaffingResponsible>> UpsertResourceStaffingResponsible(IEnumerable<StaffingResponsible> staffingResponsibleData);
        }
    }
