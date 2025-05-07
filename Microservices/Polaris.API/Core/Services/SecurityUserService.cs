using Polaris.API.Contracts.RepositoryInterfaces;
using Polaris.API.Contracts.Services;
using Polaris.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Polaris.API.Core.Services
{
    public class SecurityUserService: ISecurityUserService
    {
        private readonly ISecurityUserRepository _securityUserRepository;

        public SecurityUserService(ISecurityUserRepository securityUserRepository)
        {
            _securityUserRepository = securityUserRepository;
        }

        public async Task<IEnumerable<SecurityUser>> GetRevSecurityUsersWithGeography() 
        {
            return await _securityUserRepository.GetRevSecurityUsersWithGeography();
        }
    }
}
