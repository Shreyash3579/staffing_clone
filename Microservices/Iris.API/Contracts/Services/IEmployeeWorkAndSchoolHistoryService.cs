using Iris.API.Models;
using System;
using System.Threading.Tasks;

namespace Iris.API.Contracts.Services
{
    public interface IEmployeeWorkAndSchoolHistoryService
    {
        Task<EmployeeWorkAndSchoolHistory> GetEmployeeWorkAndSchoolHistory(string employeeCode);
        Task<EmployeesQualifications> GetWorkSchoolHistoryForAll(int pageNumber, int pageCount, bool includeAlumni);
        Task<EmployeesQualifications> GetWorkSchoolHistoryByModifiedDate(DateTime lastModifiedAfter, bool includeAlumni);
    }
}
