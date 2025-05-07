using Polaris.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Polaris.API.Contracts.Services
{
    public interface ISecurityUserService
    {
        Task<IEnumerable<SecurityUser>> GetRevSecurityUsersWithGeography();
    }
}
