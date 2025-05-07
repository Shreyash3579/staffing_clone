using Basis.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Basis.API.Contracts.RepositoryInterfaces
{
    public interface IPracticeAreaRepository
    {
        Task<IEnumerable<PracticeAreaAffiliation>> GetAllPracticeArea();
        Task<IEnumerable<PracticeAreaAffiliation>> GetIndustryPracticeAreaLookupList();
        Task<IEnumerable<PracticeAreaAffiliation>> GetCapabilityPracticeAreaLookupList();
        Task<IEnumerable<PracticeAreaAffiliation>> GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(string employeeCodes, string practiceAreaCodes, string affiliationRoleCodes);
        Task<IEnumerable<AffiliationRole>> GetAffiliationRoleList();
    }
}
