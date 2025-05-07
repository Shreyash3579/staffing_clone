using Basis.API.Contracts.RepositoryInterfaces;
using Basis.API.Contracts.Services;
using Basis.API.Models;
using Basis.API.ViewModels;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Basis.API.Core.Services
{
    public class PracticeAreaService : IPracticeAreaService
    {
        private readonly IPracticeAreaRepository _practiceAreaRepository;

        public PracticeAreaService(IPracticeAreaRepository practiceAreaRepository)
        {
            _practiceAreaRepository = practiceAreaRepository;
 
        }
        public async Task<IEnumerable<PracticeAreaViewModel>> GetAllPracticeArea()
        {
            var practiceAreasAffiliations = await _practiceAreaRepository.GetAllPracticeArea();
            return ConvertToPracticeArea(practiceAreasAffiliations);
        }
        public async Task<IEnumerable<PracticeAreaViewModel>> GetIndustryPracticeAreaLookupList()
        {
            var practiceAreasAffiliations = await _practiceAreaRepository.GetIndustryPracticeAreaLookupList();
            return ConvertToPracticeArea(practiceAreasAffiliations);
        }
        public async Task<IEnumerable<PracticeAreaViewModel>> GetCapabilityPracticeAreaLookupList()
        {
            var practiceAreasAffiliations = await _practiceAreaRepository.GetCapabilityPracticeAreaLookupList();
            return ConvertToPracticeArea(practiceAreasAffiliations);
        }

        public async Task<IEnumerable<AffiliationRole>> GetAffiliationRoleList()
        {
            var affiliationRoles = await _practiceAreaRepository.GetAffiliationRoleList();
            return affiliationRoles;
        }

        public async Task<IEnumerable<EmployeePracticeAreaViewModel>> GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(string employeeCodes, string practiceAreaCodes, string affiliationRoleCodes="")
        {
            if (string.IsNullOrEmpty(employeeCodes)) return Enumerable.Empty<EmployeePracticeAreaViewModel>();
            var practiceAreasWithEmployeeCodes = await _practiceAreaRepository.GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(employeeCodes, practiceAreaCodes,affiliationRoleCodes);
            return ConvertToEmployeePracticeAreaAffiliations(practiceAreasWithEmployeeCodes);
        }

        private IEnumerable<PracticeAreaViewModel> ConvertToPracticeArea(IEnumerable<PracticeAreaAffiliation> practiceAreasAffiliations)
        {
            return practiceAreasAffiliations.Select(x => new PracticeAreaViewModel
            {
                PracticeAreaCode = x.PracticeAreaCode,
                PracticeAreaName = x.PracticeAreaName,
                PracticeAreaAbbreviation = x.PracticeAreaAbbreviation
            }).OrderBy(x => x.PracticeAreaName);
        }

        private IEnumerable<EmployeePracticeAreaViewModel> ConvertToEmployeePracticeAreaAffiliations(IEnumerable<PracticeAreaAffiliation> practiceAreasAffiliations)
        {
            return practiceAreasAffiliations.Select(x => new EmployeePracticeAreaViewModel
            {
                EmployeeCode = x.EmployeeCode,
                PracticeAreaCode = x.PracticeAreaCode,
                PracticeAreaName = x.PracticeAreaName,
                RoleCode=x.RoleCode,
                RoleName=x.RoleName
            }).GroupBy(x => new { x.EmployeeCode, x.PracticeAreaCode, x.RoleCode }).Select(y => y.FirstOrDefault()).ToList();
        }

    }
}
