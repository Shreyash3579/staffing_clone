using Hcpd.API.Contracts.RepositoryInterfaces;
using Hcpd.API.Contracts.Services;
using Hcpd.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hcpd.API.Core.Services
{
    public class SecurityUserService : ISecurityUserService
    {
        private readonly ISecurityUserRepository _securityRepository;
        public SecurityUserService(ISecurityUserRepository securityRepository)
        {
            _securityRepository = securityRepository;
        }
        public async Task<IEnumerable<SecurityUserViewModel>> GetSecurityUserDetails(string employeeCode)
        {
            var userDetails = await _securityRepository.GetSecurityUserDetails(employeeCode);
            var securityUserDetails = ConvertToSecurityUserViewModel(userDetails);
            return securityUserDetails;
        }

        private IEnumerable<SecurityUserViewModel> ConvertToSecurityUserViewModel(IEnumerable<SecurityUser> userDetails)
        {
            var securityUsers = userDetails.GroupBy(g => new { g.EmployeeCode, g.RoleCode })
                .Select(grp => new SecurityUserViewModel
                {
                    EmployeeCode = grp.Key.EmployeeCode,
                    RoleCode = grp.Key.RoleCode,
                    RoleName = grp.First().RoleName,
                    SecurityAccessList = grp.GroupBy(g => g.OfficeCode).Select(grp => new HcpdSecurityAccess
                    {
                        Office = grp.Key,
                        PDGradeAccess = grp.Select(x => x.PDGradeAccess).Distinct().ToArray()
                    }).ToList()
                }).ToList();

            return securityUsers;
        }
    }
}
