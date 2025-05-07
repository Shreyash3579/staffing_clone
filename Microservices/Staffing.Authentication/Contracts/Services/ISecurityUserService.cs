using Staffing.Authentication.Core.Enums;
using Staffing.Authentication.Models;
using System.Threading.Tasks;

namespace Staffing.Authentication.Contracts.Services
{
    public interface ISecurityUserService
    {
        Task<(SecurityUserViewModel securityUser, string token)> AuthenticateEmployee(string employeeCode);
        string AuthenticateApp(string appName, string appSecret);
        Task<Employee> GetEmployeeByEmployeeCode(string employeeCode);
        Task<object> RefreshToken(string token, string refreshToken);
        Task<object> RegisterApp(string employeeCode, string appName, AppClaim claims);
    }
}