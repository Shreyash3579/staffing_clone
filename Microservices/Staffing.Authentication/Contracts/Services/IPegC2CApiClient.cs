using System.Threading.Tasks;

namespace Staffing.Authentication.Contracts.Services
{
    public interface IPegC2CApiClient
    {
        Task<bool> GetSecurityUserAccess(string employeeCode);
    }
}
