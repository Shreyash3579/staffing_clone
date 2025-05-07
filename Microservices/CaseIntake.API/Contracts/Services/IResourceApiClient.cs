using System.Collections.Generic;
using System.Threading.Tasks;
using CaseIntake.API.Models;

namespace CaseIntake.API.Contracts.Services
{
    public interface IResourceApiClient
    {
        Task<List<ResourceModel>> GetEmployeesIncludingTerminated(); 
        Task<List<ResourceModel>> GetActiveEmployees();
        Task<ResourceModel> GetEmployeeByEmployeeCode(string employeeCode);
    }
}
