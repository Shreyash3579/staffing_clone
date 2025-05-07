using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface ILookupRepository
    {
        Task<IEnumerable<InvestmentCategory>> GetInvestmentCategoryLookupList();
        Task<IEnumerable<CaseRoleType>> GetCaseRoleTypeLookupList();
        Task<IEnumerable<StaffableAsType>> GetStaffableAsTypeLookupList();
        Task<IEnumerable<CasePlanningBoardBucket>> GetCasePlanningBoardBucketLookupListByEmployee(string employeeCode);
        Task<IEnumerable<UserPersonaType>> GetUserPersonaTypeLookupList();
        Task<IEnumerable<SecurityRole>> GetSecurityRoles();
        Task<IEnumerable<SecurityFeature>> GetSecurityFeatures();
        Task<IEnumerable<StaffingPreferences>> GetStaffingPreferences();

        Task<IEnumerable<SKUTerm>> GetSKUTermList();

        Task<UserPreferences> GetUserPreferences(string employeeCode);

        Task<IEnumerable<CommitmentType>> GetCommitmentTypeLookupList(bool? showHidden);

        Task<IEnumerable<CommitmentTypeReason>> GetCommitmentTypeReasonLookupList();


    }
}
