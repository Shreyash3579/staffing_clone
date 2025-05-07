using Hcpd.API.Contracts.RepositoryInterfaces;
using Hcpd.API.Core.Helpers;
using Hcpd.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hcpd.API.Core.Repository
{
    public class SecurityUserRepsitory : ISecurityUserRepository
    {
        private readonly IBaseRepository<SecurityUser> _baseRepository;

        public SecurityUserRepsitory(IBaseRepository<SecurityUser> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task<IEnumerable<SecurityUser>> GetSecurityUserDetails(string employeeCode)
        {
            var result = await
                _baseRepository.GetAllAsync(new { employeeCode },
                    StoredProcedureMap.GetUserSecurityAccess);

            return result;
        }
    }
}
