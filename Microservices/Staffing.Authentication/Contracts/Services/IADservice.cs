using Staffing.Authentication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Authentication.Contracts.Services
{
    public interface IADservice
    {
        Task<bool> isGroupMember(string accountName, string employeeCode);
    }
}
