using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IProjectAggregatorService
    {
        Task<IEnumerable<ProjectData>> GetOpportunitiesAndCasesWithAllocationsBySelectedValues(DemandFilterCriteria filterCriteria, string loggedInUser = null);
        Task<ProjectDataViewModel> GetOpportunitiesAndNewDemandWithAllocationsBySelectedValues(DemandFilterCriteria filterCriteria, string loggedInUser = null);
        
        Task<ProjectData> GetOpportunityDetailsWithAllocationByPipelineId(string pipleineId, string loggedInUser = null);
        Task<ProjectData> GetCaseDetailsWithAllocationByCaseCode(string oldCaseCode, string loggedInUser = null);

        Task<ProjectDataViewModel> GetOnGoingCasesWithAllocationsBySelectedValues(DemandFilterCriteria filterCriteria, string loggedInUser = null);
        Task<IEnumerable<ProjectData>> GetProjectsForTypeahead(string searchString);
        Task<IEnumerable<ProjectData>> GetCasesForTypeahead(string searchString);
        Task<IEnumerable<PlanningCardViewModel>> GetPlanningCardAndAllocationsByEmployeeCodeAndFilters(string employeeCode, string officeCodes, string staffingTags, bool isStaffedFromSupply, string loggedInUser = null);
        Task<CaseViewNoteViewModel> UpsertCaseViewNote(CaseViewNote caseViewNote);
        Task<IEnumerable<CaseViewNoteViewModel>> GetCaseViewNote(string oldCaseCode, string piplineId, string planningCardId, string loggedInUser);
    }
}