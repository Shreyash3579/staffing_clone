using System.Collections.Generic;
using System;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface ICommonResourcesService
    {
        Task<ResourceCommitment> GetResourcesCommitmentsForStaffingTab(string distinctEmployeeCodes, DateTime? startDate, DateTime? endDate, string commitmentTypes);
        Task<(ResourceView resourcesStaffingAndCommitment, IList<Resource> resources)> GetResourcesAllocationsAndCommitmentsForResourcesTab(IEnumerable<Resource> resourcesWithSelectedFields, DateTime? startDate,
            DateTime? endDate, string commitmentTypes, ResourceView resourcesStaffingAndCommitmentDataReferences);
        Task<(ResourceView resourcesStaffingAndCommitment, IList<Resource> resources)> GetResourcesAllocationsAndCommitmentsForResourcesTab(IEnumerable<Resource> resourcesWithSelectedFields, DateTime? startDate,
            DateTime? endDate, string commitmentTypes, ResourceView resourcesStaffingAndCommitmentDataReferences, string loggedInuser);

        IEnumerable<Resource> GetResourcesFilteredByStaffingTags(DateTime startDate, DateTime endDate, IEnumerable<Resource> resources, IEnumerable<CommitmentViewModel> commitments,
           string staffingTags);
        Task<(IEnumerable<Resource>, IEnumerable<EmployeePracticeArea>)> GetEmployeesFilteredByAdditionalFilters(SupplyFilterCriteria supplyFilterCriteria,
                IEnumerable<Resource> filteredResources, ResourceView resourcesStaffingAndCommitmentDataReferences);
        IEnumerable<ResourceViewNoteViewModel> ConvertToResourceViewNotesViewModel(IEnumerable<ResourceViewNote> resourceViewNotes, List<Resource> resources);

        IEnumerable<ResourceViewCDViewModel> ConvertToResourceCDViewModel(IEnumerable<ResourceCD> resourceCD, IEnumerable<Resource> resources);

        IEnumerable<ResourceViewCommercialModelViewModel> ConvertToResourceCommercialModelViewModel(IEnumerable<ResourceCommercialModel> resourceCommercialModel, IEnumerable<Resource> resources);
        Task<IEnumerable<EmployeePracticeArea>> GetEmployeesWithPracticeAreaAffiliationsDataTask( string unfilteredEmployeeCodes, string practiceAreaCodes, string affiliationRoleCodes);
        IEnumerable<Resource> FilterResourcesBySelectedPracticeAreaAffiliations(string supplyCriteriaPracticeAreas, string supplyCriteriaAffiliationRole, IEnumerable<EmployeePracticeArea> employeesWithPracticeAreaAffiliations,
            IEnumerable<Resource> filteredResources);
        Task<IEnumerable<EmployeeCertificates>> GetEmployeesWithCertificatesByEmployeeCodes(string employeeCodes);
        Task<IEnumerable<EmployeeLanguages>> GetEmployeesWithLanguagesByEmployeeCodes(string employeeCodes);
        IEnumerable<Resource> FilterResourcesByLevelGradePositionAndStaffableAs(IEnumerable<Resource> resourcesData,IEnumerable<StaffableAs> employeesWithStaffableAsRoles,SupplyFilterCriteria supplyFilterCriteria);
    }
}