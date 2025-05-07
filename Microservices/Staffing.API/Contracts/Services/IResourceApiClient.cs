using System.Collections.Generic;
using System.Threading.Tasks;
using Staffing.API.Models;

namespace Staffing.API.Contracts.Services
{
    public interface IResourceApiClient
    {
        Task<List<ResourceModel>> GetEmployeesIncludingTerminated(); 
        Task<List<ResourceModel>> GetActiveEmployees();
    }
}
