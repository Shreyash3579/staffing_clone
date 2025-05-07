using Staffing.Authentication.Contracts.RepositoryInterfaces;
using Staffing.Authentication.Core.Helpers;
using Staffing.Authentication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Authentication.Core.Repository
{
    public class SecurityUserRepository : ISecurityUserRepository
    {
        private readonly IBaseRepository<SecurityUser> _baseRepository;

        public SecurityUserRepository(IBaseRepository<SecurityUser> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<SecurityUser>> Authenticate(string employeeCode)
        {
            var userCode = await _baseRepository.GetAllAsync(new { employeeCode },
                    StoredProcedureMap.GetUserAuthentication);

            return userCode;
        }
    }
}
