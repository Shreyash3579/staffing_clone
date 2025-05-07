using Staffing.Authentication.Models;
using System.Threading.Tasks;

namespace Staffing.Authentication.Contracts.Services
{
    public interface IResourcesApiClient
    {
        Task<Employee> GetEmployee(string employeeCode);
    }
}