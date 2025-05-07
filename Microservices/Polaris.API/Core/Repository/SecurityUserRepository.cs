using Polaris.API.Contracts.RepositoryInterfaces;
using Polaris.API.Core.Helpers;
using Polaris.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Polaris.API.Core.Repository
{
    public class SecurityUserRepository : ISecurityUserRepository
    {
        private readonly IBaseRepository<SecurityUser> _baseRepository;

        public SecurityUserRepository(IBaseRepository<SecurityUser> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task<IEnumerable<SecurityUser>> GetRevSecurityUsersWithGeography() 
        {
            var users = await _baseRepository.GetAllAsync(StoredProcedureMap.GetRevUserPersonaDetails);

            return users;
        }
    }
}
