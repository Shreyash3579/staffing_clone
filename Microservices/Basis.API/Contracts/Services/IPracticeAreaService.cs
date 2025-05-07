using Basis.API.Models;
using Basis.API.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Basis.API.Contracts.Services
{
    public interface IPracticeAreaService
    {
        Task<IEnumerable<PracticeAreaViewModel>> GetAllPracticeArea();
        Task<IEnumerable<PracticeAreaViewModel>> GetIndustryPracticeAreaLookupList();
        Task<IEnumerable<PracticeAreaViewModel>> GetCapabilityPracticeAreaLookupList();
        Task<IEnumerable<EmployeePracticeAreaViewModel>> GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(string employeeCodes, string practiceAreaCodes, string affiliationRoleCodes);
        Task<IEnumerable<AffiliationRole>> GetAffiliationRoleList();

    }
}
