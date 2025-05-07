using Staffing.Coveo.API.Contracts.Services;
using Staffing.Coveo.API.Core.Helpers;
using Staffing.Coveo.API.Models;
using Staffing.Coveo.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.Coveo.API.Core.Services
{
    public class CoveoService : ICoveoService
    {
        private readonly ICoveoClient _coveoClient;
        private readonly ICoveoAnalyticsClient _coveoAnalyticsClient;

        public CoveoService(ICoveoClient coveoClient, ICoveoAnalyticsClient coveoAnalyticsClient)
        {
            _coveoClient = coveoClient;
            _coveoAnalyticsClient = coveoAnalyticsClient;
        }

        public async Task<dynamic> SearchByQuery(string searchTerm, string source, string userDisplayName, string username, string userIPAddress, bool? test = false)
        {
            source = string.IsNullOrEmpty(source) ? Constants.Source.Everything : source;
            switch (source.ToLower())
            {
                case Constants.Source.Resource:
                    var searchedData = await _coveoClient.SearchByResource(searchTerm, userDisplayName, username, test);
                    _coveoAnalyticsClient.Search(searchedData.Item2, source, userIPAddress);
                    return searchedData.Item1;
                case Constants.Source.Project:
                    var projects = await GetProjectsData(searchTerm, userDisplayName, username, source, userIPAddress);
                    return projects;
                case Constants.Source.Everything:
                    var result = await GetResourcesAndProjectsData(searchTerm, userDisplayName, username, source, userIPAddress);
                    return result;
                default:
                    return new List<dynamic>();
            }
        }

        public async Task<IEnumerable<Allocation>> UpsertOrDeleteAllocationIndexes(IEnumerable<ResourceAllocation> allocations)
        {
            if (!allocations.Any()) return Enumerable.Empty<Allocation>();

            var upsertedAllocations = await _coveoClient.UpsertOrDeleteAllocationIndexes(allocations);
            return upsertedAllocations;
        }

        public async Task<dynamic> LogClickEventInCoveoAnalytics(AnalyticsClickViewModel analyticsClickParams, string userIPAddress)
        {
            if (analyticsClickParams == null
                || string.IsNullOrEmpty(analyticsClickParams.documentUri)
                || string.IsNullOrEmpty(analyticsClickParams.documentUriHash)
                || analyticsClickParams.searchQueryUid == Guid.Empty
                || string.IsNullOrEmpty(analyticsClickParams.sourceName))
                return Enumerable.Empty<Allocation>();

            var returnObj = await _coveoAnalyticsClient.LogClickEventInCoveoAnalytics(analyticsClickParams, userIPAddress);
            return returnObj;
        }

        #region Private Methods

        private async Task<ResourcesAndProjectsViewModel> GetResourcesAndProjectsData(string searchTerm, string userDisplayName, string username, string sourceTab, string userIPAddress)
        {
            var result = new ResourcesAndProjectsViewModel();

            if (string.IsNullOrEmpty(searchTerm))
            {
                return result;
            }

            var employeesTask = _coveoClient.SearchByResource(searchTerm, userDisplayName, username);
            var projectsTask = GetProjectsData(searchTerm, userDisplayName, username, sourceTab, userIPAddress);

            await Task.WhenAll(employeesTask, projectsTask);

            var employees = employeesTask.Result;
            var projects = projectsTask.Result;

            result.Resources = employees.Item1;
            result.Projects = projects;

            _coveoAnalyticsClient.Search(employees.Item2, Constants.Source.Everything, userIPAddress);

            return result;
        }
        private async Task<IEnumerable<ProjectData>> GetProjectsData(string searchTerm, string userDisplayName, string username, string sourceTab, string userIPAddress)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return Enumerable.Empty<ProjectData>();
            }

            var casesDataTask = _coveoClient.SearchByCase(searchTerm, userDisplayName, username);
            var opportunitiesDataTask = _coveoClient.SearchByOpportunity(searchTerm, userDisplayName, username);

            await Task.WhenAll(casesDataTask, opportunitiesDataTask);

            var opportunities = opportunitiesDataTask.Result;
            var cases = casesDataTask.Result;

            // Fetch only active opps in the search result
            opportunities.Item1 = opportunities.Item1.Where(x => x.EndDate == null || x.EndDate > DateTime.Now);

            var projects = new List<ProjectData>();
            projects.AddRange(ConvertToProjectData(opportunities.Item1));
            projects.AddRange(ConvertToProjectData(cases.Item1));

            _coveoAnalyticsClient.Search(cases.Item2, sourceTab, userIPAddress);
            _coveoAnalyticsClient.Search(opportunities.Item2, sourceTab, userIPAddress);

            return projects;
        }

        private IEnumerable<ProjectData> ConvertToProjectData(IEnumerable<Case> cases, DateTime? startDate = null)
        {
            var startDateForNewDemand = startDate.HasValue ? startDate : DateTime.Now.Date;

            var projects = cases.Select(item => new ProjectData
            {
                CaseCode = item.CaseCode,
                CaseName = item.CaseName,
                ClientCode = item.ClientCode,
                ClientName = item.ClientName,
                StatusCode = item.StatusCode,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                OldCaseCode = item.OldCaseCode,
                CaseTypeName = item.CaseTypeName,
                CaseTypeCode = item.CaseTypeCode,
                ManagingOfficeCode = item.ManagingOfficeCode,
                ManagingOfficeAbbreviation = item.ManagingOfficeAbbreviation,
                ManagingOfficeName = item.ManagingOfficeName,
                BillingOfficeCode = item.BillingOfficeCode,
                BillingOfficeAbbreviation = item.BillingOfficeAbbreviation,
                BillingOfficeName = item.BillingOfficeName,
                PrimaryIndustry = item.PrimaryIndustry,
                PrimaryCapability = item.PrimaryCapability,
                PracticeAreaIndustry = item.PracticeAreaIndustry,
                PracticeAreaCapability = item.PracticeAreaCapability,
                IsPrivateEquity = item.IsPrivateEquity,
                CaseManagerCode = item.CaseManagerCode,
                CaseManagerName = item.CaseManagerName,
                ProjectStatus = item.EndDate == null ? Convert.ToString(Constants.ProjectStatus.Active)
                                                      : Convert.ToDateTime(item.EndDate).Date >= DateTime.Now.Date
                                                        ? Convert.ToString(Constants.ProjectStatus.Active)
                                                        : Convert.ToString(Constants.ProjectStatus.Inactive),
                Type = item.StartDate >= startDateForNewDemand ? Constants.DemandType.NewDemand : Constants.DemandType.ActiveCase,
                SysCollection = item.SysCollection,
                Source = item.Source,
                Uri = item.Uri,
                UriHash = item.UriHash,
                requestDuration = item.RequestDuration,
                SearchUid = item.SearchUid,
                Title = item.Title
            });

            return projects;
        }

        private IEnumerable<ProjectData> ConvertToProjectData(IEnumerable<Opportunity> opportunities)
        {
            var projects = opportunities.Select(item => new ProjectData
            {
                PipelineId = item.PipelineId,
                CoordinatingPartnerCode = item.CoordinatingPartnerCode,
                BillingPartnerCode = item.BillingPartnerCode,
                OpportunityName = item.OpportunityName,
                OpportunityStatus = item.OpportunityStatus,
                ClientCode = item.ClientCode,
                ClientName = item.ClientName,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                Duration = item.Duration,
                ProbabilityPercent = item.ProbabilityPercent,
                ManagingOfficeCode = item.ManagingOfficeCode,
                ManagingOfficeAbbreviation = item.ManagingOfficeAbbreviation,
                ManagingOfficeName = item.ManagingOfficeName,
                ProjectStatus = item.EndDate == null ? Convert.ToString(Constants.ProjectStatus.Active)
                                                      : Convert.ToDateTime(item.EndDate).Date >= DateTime.Now.Date
                                                        ? Convert.ToString(Constants.ProjectStatus.Active)
                                                        : Convert.ToString(Constants.ProjectStatus.Inactive),
                Type = Constants.DemandType.Opportunity,
                SysCollection = item.SysCollection,
                Source = item.Source,
                Uri = item.Uri,
                UriHash = item.UriHash,
                requestDuration = item.RequestDuration,
                SearchUid = item.SearchUid,
                Title = item.Title
            });
            return projects;
        }
        #endregion
    }
}
