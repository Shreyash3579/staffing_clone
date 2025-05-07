using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class SearchService : ISearchService
    {
        private readonly IAzureSearchApiClient _azureSearchApiClient;
        private readonly ICommonResourcesService _commonResourcesService;

        public SearchService(IAzureSearchApiClient azureSearchApiClient, ICommonResourcesService commonResourcesService)
        {
            _azureSearchApiClient = azureSearchApiClient;
            _commonResourcesService = commonResourcesService;
        }

        public async Task<SearchResponseViewModel> GetResourcesBySearchString(BossSearchCriteria bossSearchCriteria)
        {
            var searchResult = await _azureSearchApiClient.GetResourcesBySearchString(bossSearchCriteria);

            if(searchResult.searches == null || searchResult.searches.Count == 0)
            {
                return new SearchResponseViewModel { Searches = new List<SearchResourceViewModel>(), GeneratedLuceneSearchQuery = searchResult.generatedLuceneSearchQuery };
            }

            var distinctEmployeeCodes = string.Join(",", searchResult.searches.Select(x => x.document).Select(y => y.employeeCode));
            DateTime startDate = DateTime.Today.Date;

            var resourceCommitments = distinctEmployeeCodes.Length > 0
                ? await _commonResourcesService.GetResourcesCommitmentsForStaffingTab(distinctEmployeeCodes, startDate, null, null)
                : new ResourceCommitment();

            var resourcesData = ConvertToSearchResourceViewModel(searchResult.searches, resourceCommitments);

            return new SearchResponseViewModel { 
                Searches = resourcesData, 
                GeneratedLuceneSearchQuery = searchResult.generatedLuceneSearchQuery 
            };
        }

        #region  Private Methods

        private IEnumerable<SearchResourceViewModel> ConvertToSearchResourceViewModel(List<dynamic> searches, ResourceCommitment resourcesCommitments)
        {
            var allocations = resourcesCommitments.Allocations != null ? resourcesCommitments.Allocations.GroupBy(x => x.EmployeeCode).ToList() : new List<IGrouping<string, ResourceAssignmentViewModel>>();
            var placeholderAndPlanningCardAllocations = resourcesCommitments.PlaceholderAllocations != null ? resourcesCommitments.PlaceholderAllocations?.Where(x => x.IsPlanningCardShared.Equals(true)).GroupBy(y => y.EmployeeCode).ToList() : new List<IGrouping<string, ScheduleMasterPlaceholder>>();
            var loAs = resourcesCommitments.LoAs != null ? resourcesCommitments.LoAs.GroupBy(x => x.EmployeeCode).ToList() : new List<IGrouping<string, ResourceLoA>>();
            var commitments = resourcesCommitments.Commitments != null ? resourcesCommitments.Commitments.GroupBy(x => x.EmployeeCode).ToList(): new List<IGrouping<string, CommitmentViewModel>>();
            var vacations = resourcesCommitments.Vacations != null ? resourcesCommitments.Vacations.GroupBy(x => x.EmployeeCode).ToList(): new List<IGrouping<string, VacationRequestViewModel>>();
            var trainings = resourcesCommitments.Trainings != null ? resourcesCommitments.Trainings?.GroupBy(x => x.EmployeeCode).ToList(): new List<IGrouping<string, TrainingViewModel>>();
            var timeOffs = resourcesCommitments.TimeOffs != null ? resourcesCommitments.TimeOffs?.GroupBy(x => x.EmployeeCode).ToList(): new List<IGrouping<string, ResourceTimeOff>>();
            var holidays = resourcesCommitments.Holidays != null ? resourcesCommitments.Holidays?.GroupBy(x => x.EmployeeCode).ToList(): new List<IGrouping<string, HolidayViewModel>>();
            var transfers = resourcesCommitments.Transfers != null ? resourcesCommitments.Transfers?.GroupBy(x => x.EmployeeCode).ToList(): new List<IGrouping<string, ResourceTransfer>>();
            var transitions = resourcesCommitments.Transitions != null ? resourcesCommitments.Transitions?.GroupBy(x => x.EmployeeCode).ToList(): new List<IGrouping<string, ResourceTransition>>();
            var terminations = resourcesCommitments.Terminations != null ? resourcesCommitments.Terminations?.GroupBy(x => x.EmployeeCode).ToList(): new List<IGrouping<string, ResourceTermination>>();

            var data = searches.Select(s => s).Select(x =>
                new SearchResourceViewModel
                {
                    Document = x.document,
                    Score = x.score,
                    //return bare minimum resource object to avoid API call to resources API so as to assist in the availability calculation
                    Resource = new Resource
                    {
                        EmployeeCode = x.document.employeeCode,
                        FullName = x.document.fullName,
                        SchedulingOffice = new Office
                        {
                            OfficeCode = x.document.operatingOfficeCode,
                            OfficeName = x.document.operatingOfficeName,
                            OfficeAbbreviation = x.document.operatingOfficeAbbreviation
                        },
                        LevelGrade = x.document.levelGrade,
                        ServiceLine = new ServiceLine
                        {
                            ServiceLineCode = x.document.serviceLineCode,
                            ServiceLineName = x.document.serviceLineName
                        },
                        StartDate = x.document.startDate,
                        TerminationDate = x.document.terminationDate,
                        FTE = x.document.fte,
                        PositionNameWithAbbreviation = x.document.positionNameWithAbbreviation,
                        OfficeDetail = x.document.officeDetail
                    },
                    Allocations = allocations.Where(rs => rs.Key.Equals(x.document.employeeCode.ToString(), StringComparison.OrdinalIgnoreCase)).SelectMany(grp => grp).ToList(),
                    PlaceholderAllocations = placeholderAndPlanningCardAllocations.Where(rs => rs.Key.Equals(x.document.employeeCode.ToString(), StringComparison.OrdinalIgnoreCase)).SelectMany(grp => grp).ToList(),
                    LoAs = loAs.Where(rs => rs.Key.Equals(x.document.employeeCode.ToString(), StringComparison.OrdinalIgnoreCase)).SelectMany(grp => grp),
                    Commitments = commitments.Where(rs => rs.Key.Equals(x.document.employeeCode.ToString(), StringComparison.OrdinalIgnoreCase)).SelectMany(grp => grp),
                    Vacations = vacations.Where(rs => rs.Key.Equals(x.document.employeeCode.ToString(), StringComparison.OrdinalIgnoreCase)).SelectMany(grp => grp),
                    Trainings = trainings.Where(rs => rs.Key.Equals(x.document.employeeCode.ToString(), StringComparison.OrdinalIgnoreCase)).SelectMany(grp => grp),
                    TimeOffs = timeOffs.Where(rs => rs.Key.Equals(x.document.employeeCode.ToString(), StringComparison.OrdinalIgnoreCase)).SelectMany(grp => grp),
                    Holidays = holidays.Where(rs => rs.Key.Equals(x.document.employeeCode.ToString(), StringComparison.OrdinalIgnoreCase)).SelectMany(grp => grp),
                    Transfers = transfers.Where(rs => rs.Key.Equals(x.document.employeeCode.ToString(), StringComparison.OrdinalIgnoreCase)).SelectMany(grp => grp),
                    Transitions = transitions.Where(rs => rs.Key.Equals(x.document.employeeCode.ToString(), StringComparison.OrdinalIgnoreCase)).SelectMany(grp => grp),
                    Terminations = terminations.Where(rs => rs.Key.Equals(x.document.employeeCode.ToString(), StringComparison.OrdinalIgnoreCase)).SelectMany(grp => grp)
                });

            return data;
        }

        #endregion
    }
}
