using BackgroundPolling.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IADSecurityUserService
    {
        Task<SecurityUserModel> GetBOSSSecurityUsersFromAD(IEnumerable<SecurityGroup> securityGroups, IEnumerable<Office> offices);
    }
}
