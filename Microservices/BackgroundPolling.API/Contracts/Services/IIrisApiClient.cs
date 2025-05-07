using BackgroundPolling.API.Models;
using System;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IIrisApiClient
    {
        Task<EmployeesQualifications> GetWorkSchoolHistoryForAll(int pageNumber, int pageCount, bool includeAlumni);
        Task<EmployeesQualifications> GetWorkSchoolHistoryByModifiedDate(DateTime lastModifiedAfter, bool includeAlumni);
    }
}
