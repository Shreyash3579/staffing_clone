using Iris.API.Contracts.Services;
using Iris.API.Models;
using System;
using System.Threading.Tasks;

namespace Iris.API.Core.Services
{
    public class EmployeeWorkAndSchoolHistoryService : IEmployeeWorkAndSchoolHistoryService
    {
        private readonly IirisApiClient _irisApiClient;
        public EmployeeWorkAndSchoolHistoryService(IirisApiClient irisApiClient)
        {
            _irisApiClient = irisApiClient;
        }
        public async Task<EmployeeWorkAndSchoolHistory> GetEmployeeWorkAndSchoolHistory(string employeeCode) 
        {
            var employeeWorkAndShoolHistory = await _irisApiClient.GetEmployeeWorkAndSchoolHistory(employeeCode);
            return employeeWorkAndShoolHistory;
        }

        public async Task<EmployeesQualifications> GetWorkSchoolHistoryForAll(int pageNumber, int pageCount, bool includeAlumni)
        {
            var paginatedQualificationsForEmployees = await _irisApiClient.GetWorkSchoolHistoryForAll(pageNumber, pageCount, includeAlumni);
            return paginatedQualificationsForEmployees;
        }

        public async Task<EmployeesQualifications> GetWorkSchoolHistoryByModifiedDate(DateTime lastModifiedAfter, bool includeAlumni)
        {
            var allQualificatiosnAfterLastModifiedDate = await _irisApiClient.GetWorkSchoolHistoryByModifiedDate(lastModifiedAfter, includeAlumni);
            return allQualificatiosnAfterLastModifiedDate;
        }
    }
}
