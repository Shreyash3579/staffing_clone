using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.Graph.Models;
using NuGet.Protocol;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class UserPreferenceGroupService : IUserPreferenceGroupService
    {
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IResourceApiClient _resourceApiClient;

        public UserPreferenceGroupService(IStaffingApiClient staffingApiClient, IResourceApiClient resourceApiClient)
        {
            _staffingApiClient = staffingApiClient;
            _resourceApiClient = resourceApiClient;
        }

        public async Task<IList<UserPreferenceSupplyGroupViewModel>> GetUserPreferenceSupplyGroupsDetails(string employeeCode)
        {
            var userPreferenceSupplyGroupsTask = _staffingApiClient.GetUserPreferenceSupplyGroups(employeeCode);
            var employeesIncludingTerminatedTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(userPreferenceSupplyGroupsTask, employeesIncludingTerminatedTask);

            var userPreferenceSupplyGroups = userPreferenceSupplyGroupsTask.Result;
            var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;

            var userPreferenceSupplyGroupsAndGroupMembers = ConvertUserPreferenceSupplyGroupsToViewModel(userPreferenceSupplyGroups, employeesIncludingTerminated);

            return userPreferenceSupplyGroupsAndGroupMembers;
        }

        public async Task<IList<UserPreferenceSavedGroupWithSharedInfo>> GetUserPreferenceSavedGroupsDetails(string employeeCode)
        {
            var userPreferenceSavedGroupsTask = _staffingApiClient.GetUserPreferenceSavedGroups(employeeCode);
            var employeesIncludingTerminatedTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(userPreferenceSavedGroupsTask, employeesIncludingTerminatedTask);

            var userPreferenceSavedGroups = userPreferenceSavedGroupsTask.Result;
            var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;

            var userPreferenceSavedGroupsAndSharedMembers = ConvertUserPreferenceSavedGroupsToViewModel(userPreferenceSavedGroups, employeesIncludingTerminated);

            return userPreferenceSavedGroupsAndSharedMembers;
        }

        private IList<UserPreferenceSupplyGroupViewModel> ConvertUserPreferenceSupplyGroupsToViewModel(IEnumerable<UserPreferenceSupplyGroup> userPreferenceSupplyGroups,
            IEnumerable<Resource> resources)
        {
            var viewModel = userPreferenceSupplyGroups.Select(item => new UserPreferenceSupplyGroupViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                IsDefault = item.IsDefault,
                IsDefaultForResourcesTab = item.IsDefaultForResourcesTab,
                IsShared = item.SharedWith.Count() > 1,
                CreatedBy = item.CreatedBy,
                GroupMembers = ConvertToUserPreferenceSupplyGroupMembersViewModel(item, resources).ToList(),
                SortBy = item.SortBy,
                FilterBy = item.FilterBy,
                sharedWith = ConvertToUserPreferenceSharedWithGroupMembersViewModel(item.SharedWith, resources).ToList()
            }).ToList();

            return viewModel;
        }

        private IList<UserPreferenceSavedGroupWithSharedInfo> ConvertUserPreferenceSavedGroupsToViewModel(IEnumerable<UserPreferenceSavedGroup> userPreferenceSavedGroups,
            IEnumerable<Resource> resources)
        {
            var viewModel = userPreferenceSavedGroups.Select(item => new UserPreferenceSavedGroupWithSharedInfo
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                isDefault = item.isDefault,
                OfficeCodes = item.OfficeCodes,
                StaffingTags = item.StaffingTags,
                LevelGrades = item.LevelGrades,
                PositionCodes = item.PositionCodes,
                EmployeeStatuses = item.EmployeeStatuses,
                PracticeAreaCodes = item.PracticeAreaCodes,
                MinAvailabilityThreshold = item.MinAvailabilityThreshold,
                MaxAvailabilityThreshold = item.MaxAvailabilityThreshold,
                LastUpdatedBy = item.LastUpdatedBy,
                StaffableAsTypeCodes = item.StaffableAsTypeCodes,
                AffiliationRoleCodes = item.AffiliationRoleCodes,
                ResourcesTabSortBy = item.ResourcesTabSortBy,
                FilterBy = item.FilterBy,
                SharedWith = ConvertToUserPreferenceSharedWithGroupMembersViewModel(item.SharedWith, resources).ToList()
            }).ToList();

            return viewModel;
        }

        private static IEnumerable<UserPreferenceGroupMemberViewModel> ConvertToUserPreferenceSupplyGroupMembersViewModel(UserPreferenceSupplyGroup supplyGroup,
            IEnumerable<Resource> resources)
        {
            if (string.IsNullOrEmpty(supplyGroup.GroupMemberCodes))
                return Enumerable.Empty<UserPreferenceGroupMemberViewModel>();

            var userPreferenceSupplyGroupMembersViewModel = resources.Where(x => supplyGroup.GroupMemberCodes.ToLower()
                .Contains(x.EmployeeCode, System.StringComparison.InvariantCultureIgnoreCase))
                .Select(item => new UserPreferenceGroupMemberViewModel
                {
                    Id = supplyGroup.Id.Value,
                    EmployeeName = item.FullName,
                    PositionName = item.Position?.PositionName,
                    CurrentLevelGrade = item.LevelGrade,
                    OperatingOfficeAbbreviation = item.Office?.OfficeAbbreviation,
                    EmployeeCode = item.EmployeeCode
                }).ToList();

            return userPreferenceSupplyGroupMembersViewModel;
        }

        private static IEnumerable<UserPreferenceGroupSharedInfoViewModel> ConvertToUserPreferenceSharedWithGroupMembersViewModel(IEnumerable<UserPreferenceGroupSharedInfo> sharedInfo,
            IEnumerable<Resource> resources)
        {
            if (sharedInfo == null || sharedInfo?.Count() == 0)
                return Enumerable.Empty<UserPreferenceGroupSharedInfoViewModel>();

            var userPreferenceSharedWithGroupMembersViewModel = sharedInfo.SelectMany(sharedWith =>
            resources.Where(x => sharedWith.SharedWith == x.EmployeeCode)
                .Select(item => new UserPreferenceGroupSharedInfoViewModel
                {
                    Id = sharedWith.Id,
                    UserPreferenceGroupId = sharedWith.UserPreferenceGroupId,
                    SharedWith = sharedWith.SharedWith,
                    IsDefault = sharedWith.IsDefault,
                    LastUpdatedBy = sharedWith.LastUpdatedBy,
                    SharedWithMemberDetails = new UserPreferenceGroupMemberViewModel
                    {
                        EmployeeName = item.FullName,
                        PositionName = item.Position?.PositionName,
                        CurrentLevelGrade = item.LevelGrade,
                        OperatingOfficeAbbreviation = item.Office?.OfficeAbbreviation,
                        EmployeeCode = item.EmployeeCode
                    }    
                })).ToList();

            return userPreferenceSharedWithGroupMembersViewModel;
        }

        public async Task<IList<UserPreferenceGroupSharedInfoViewModel>> GetUserPreferenceGroupSharedInfo(string groupId)
        {
            var userPreferenceGroupSharedInfoTask = _staffingApiClient.GetUserPreferenceGroupSharedInfo(groupId);
            var employeesIncludingTerminatedTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(userPreferenceGroupSharedInfoTask, employeesIncludingTerminatedTask);

            var userPreferenceGroupSharedInfo = userPreferenceGroupSharedInfoTask.Result;
            var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;

            var userPreferenceGroupsAndGroupMembers = ConvertUserPreferenceGroupSharedInfoToViewModel(userPreferenceGroupSharedInfo, employeesIncludingTerminated);

            return userPreferenceGroupsAndGroupMembers;
        }

        public async Task<IEnumerable<UserPreferenceSupplyGroupViewModel>> UpsertUserPreferencesSupplyGroupWithSharedInfo(IEnumerable<UserPreferenceSupplyGroupWithSharedInfo> supplyGroupsWithSharedInfoToUpsert)
        {
            var sharedWithInfo = new List<UserPreferenceGroupSharedInfo>();
            //var supplyGroupWithSharedInfoToUpsert = supplyGroupsWithSharedInfoToUpsert.FirstOrDefault();

            foreach(var group in supplyGroupsWithSharedInfoToUpsert)
            {
                if (group.Id == null)
                {
                    group.Id = Guid.NewGuid();
                    if (group.FilterBy != null)
                    {
                        foreach (var filterBy in group.FilterBy)
                        {
                            filterBy.GroupId = (Guid)group.Id;
                        }
                    }
                    var sharedWith = new UserPreferenceGroupSharedInfo()
                    {
                        Id = Guid.NewGuid(),
                        UserPreferenceGroupId = group.Id,
                        SharedWith = group.LastUpdatedBy,
                        LastUpdatedBy = group.LastUpdatedBy
                    };
                    sharedWithInfo.Add(sharedWith);

                }
                if (group.SharedWith != null && group?.SharedWith.Count() != 0)
                {
                    var sharedWith = group.SharedWith.Select(item => new UserPreferenceGroupSharedInfo
                        {
                            Id = item.Id ?? Guid.NewGuid(),
                            UserPreferenceGroupId = group.Id,
                            SharedWith = item.SharedWith,
                            LastUpdatedBy = item.LastUpdatedBy ?? group.LastUpdatedBy
                        }).ToList();

                    sharedWithInfo.AddRange(sharedWith);
                }
            };      
            
            sharedWithInfo = sharedWithInfo.DistinctBy(item =>  new { item.UserPreferenceGroupId, item.SharedWith }).ToList();

            var supplyGroupsToUpsert = ConvertToUserPreferenceSupplyGroup(supplyGroupsWithSharedInfoToUpsert);

            await _staffingApiClient.UpsertUserPreferenceGroupSharedInfo(sharedWithInfo);

            var userPreferenceSupplyGroupTask = _staffingApiClient.UpsertUserPreferenceSupplyGroups(supplyGroupsToUpsert);
            var employeesIncludingTerminatedTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(userPreferenceSupplyGroupTask, employeesIncludingTerminatedTask);
            var userPreferenceSupplyGroups = userPreferenceSupplyGroupTask.Result;
            var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;

            var userPreferenceSupplyGroupsAndSharedMembers = ConvertUserPreferenceSupplyGroupsToViewModel(userPreferenceSupplyGroups, employeesIncludingTerminated);
            
            return userPreferenceSupplyGroupsAndSharedMembers;
        }

        public async Task<IEnumerable<UserPreferenceSavedGroupWithSharedInfo>> UpsertUserPreferencesSavedGroupWithSharedInfo(IEnumerable<UserPreferenceSavedGroupWithSharedInfo> savedGroupsWithSharedInfoToUpsert)
        {
            var sharedWithInfo = new List<UserPreferenceGroupSharedInfo>();
            var savedGroupWithSharedInfoToUpsert = savedGroupsWithSharedInfoToUpsert.FirstOrDefault();

            if (savedGroupWithSharedInfoToUpsert.Id == null)
            {
                savedGroupWithSharedInfoToUpsert.Id = Guid.NewGuid();
                if (savedGroupWithSharedInfoToUpsert.FilterBy != null)
                {
                    foreach (var filterBy in savedGroupWithSharedInfoToUpsert.FilterBy)
                    {
                        filterBy.GroupId = (Guid)savedGroupWithSharedInfoToUpsert.Id;
                    }
                }
                var sharedWith = savedGroupsWithSharedInfoToUpsert.Select(group => new UserPreferenceGroupSharedInfo
                {
                    Id = Guid.NewGuid(),
                    UserPreferenceGroupId = group.Id,
                    SharedWith = group.LastUpdatedBy,
                    LastUpdatedBy = group.LastUpdatedBy
                }).ToList();

                sharedWithInfo.AddRange(sharedWith);
            }
            if (savedGroupWithSharedInfoToUpsert.SharedWith != null && savedGroupWithSharedInfoToUpsert.SharedWith?.Count() != 0)
            {
                var sharedWith = savedGroupsWithSharedInfoToUpsert.SelectMany(group =>
                    group?.SharedWith.Select(item => new UserPreferenceGroupSharedInfo
                    {
                        Id = item.Id ?? Guid.NewGuid(),
                        UserPreferenceGroupId = group.Id,
                        SharedWith = item.SharedWith,
                        LastUpdatedBy = item.LastUpdatedBy ?? group.LastUpdatedBy,
                    })
                ).ToList();

                sharedWithInfo.AddRange(sharedWith);
            }

            sharedWithInfo = sharedWithInfo.DistinctBy(item => new { item.UserPreferenceGroupId, item.SharedWith }).ToList();

            await _staffingApiClient.UpsertUserPreferenceGroupSharedInfo(sharedWithInfo);

            var savedGroupsToUpsert = ConvertToUserPreferenceSavedGroup(savedGroupsWithSharedInfoToUpsert);
            var userPreferenceSavedGroupsTask = _staffingApiClient.UpsertUserPreferenceSavedGroups(savedGroupsToUpsert);
            var employeesIncludingTerminatedTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(userPreferenceSavedGroupsTask, employeesIncludingTerminatedTask);
            var userPreferenceSavedGroups = userPreferenceSavedGroupsTask.Result;
            var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;

            var userPreferenceSavedGroupsAndSharedMembers = ConvertUserPreferenceSavedGroupsToViewModel(userPreferenceSavedGroups, employeesIncludingTerminated);

            return userPreferenceSavedGroupsAndSharedMembers;

        }

        public async Task<IEnumerable<UserPreferenceGroupSharedInfoViewModel>> UpsertUserPreferenceGroupSharedInfo(IEnumerable<UserPreferenceGroupSharedInfo> groupsSharedInfoToUpsert)
        {
            var sharedWith = groupsSharedInfoToUpsert.Select(item => new UserPreferenceGroupSharedInfo
            {
                Id = item.Id ?? Guid.NewGuid(),
                UserPreferenceGroupId = item.UserPreferenceGroupId,
                SharedWith = item.SharedWith,
                LastUpdatedBy = item.LastUpdatedBy
            }).ToList();

            var userPreferenceGroupSharedInfoTask = _staffingApiClient.UpsertUserPreferenceGroupSharedInfo(sharedWith);
            var employeesIncludingTerminatedTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(userPreferenceGroupSharedInfoTask, employeesIncludingTerminatedTask);

            var userPreferenceGroupSharedInfo = userPreferenceGroupSharedInfoTask.Result;
            var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;

            var userPreferenceGroupSharedInfoViewModel = ConvertToUserPreferenceSharedWithGroupMembersViewModel(userPreferenceGroupSharedInfo, employeesIncludingTerminated);

            return userPreferenceGroupSharedInfoViewModel;
        }


            private static IEnumerable<UserPreferenceSupplyGroup> ConvertToUserPreferenceSupplyGroup(
            IEnumerable<UserPreferenceSupplyGroupWithSharedInfo> supplyGroupsWithSharedInfoToUpsert)
        {
            var userPreferenceSupplyGroups = supplyGroupsWithSharedInfoToUpsert.Select(item => new UserPreferenceSupplyGroup
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                CreatedBy = item.CreatedBy,
                IsDefault = item.IsDefault,
                IsDefaultForResourcesTab = item.IsDefaultForResourcesTab,
                IsShared = item.IsShared,
                LastUpdatedBy = item.LastUpdatedBy,
                GroupMemberCodes = item.GroupMemberCodes,
                SortBy = item.SortBy,
                FilterBy = item.FilterBy
            });

            return userPreferenceSupplyGroups;
        }

        private static IEnumerable<UserPreferenceSavedGroup> ConvertToUserPreferenceSavedGroup(
            IEnumerable<UserPreferenceSavedGroupWithSharedInfo> savedGroupsWithSharedInfoToUpsert)
        {
            var userPreferenceSavedGroups = savedGroupsWithSharedInfoToUpsert.Select(item => new UserPreferenceSavedGroup
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                isDefault = item.isDefault,
                OfficeCodes = item.OfficeCodes,
                StaffingTags = item.StaffingTags,
                LevelGrades = item.LevelGrades,
                PositionCodes = item.PositionCodes,
                EmployeeStatuses = item.EmployeeStatuses,
                PracticeAreaCodes = item.PracticeAreaCodes,
                MinAvailabilityThreshold = item.MinAvailabilityThreshold,
                MaxAvailabilityThreshold = item.MaxAvailabilityThreshold,
                LastUpdatedBy = item.LastUpdatedBy,
                StaffableAsTypeCodes = item.StaffableAsTypeCodes,
                AffiliationRoleCodes = item.AffiliationRoleCodes,
                ResourcesTabSortBy = item.ResourcesTabSortBy,
                FilterBy = item.FilterBy
            });

            return userPreferenceSavedGroups;
        }

        private static IList<UserPreferenceGroupSharedInfoViewModel> ConvertUserPreferenceGroupSharedInfoToViewModel(
            IEnumerable<UserPreferenceGroupSharedInfo> groupSharedInfo, IEnumerable<Resource> resources)
        {
            var viewModel = groupSharedInfo.Select(item => new UserPreferenceGroupSharedInfoViewModel
            {
                Id = item.Id,
                UserPreferenceGroupId = item.UserPreferenceGroupId,
                SharedWith = item.SharedWith,
                IsDefault = item.IsDefault,
                LastUpdatedBy = item.LastUpdatedBy,
                SharedWithMemberDetails = ConvertToUserPreferenceGroupSharedInfoViewModel(item, resources)
            }).ToList();

            return viewModel;
        }

        private static UserPreferenceGroupMemberViewModel ConvertToUserPreferenceGroupSharedInfoViewModel(
            UserPreferenceGroupSharedInfo groupSharedInfo, IEnumerable<Resource> resources)
        {
            if (string.IsNullOrEmpty(groupSharedInfo.SharedWith))
                return new UserPreferenceGroupMemberViewModel();

            var userPreferenceGroupSharedWithMemberViewModel = resources.Where(x => groupSharedInfo.SharedWith.ToLower()
               .Equals(x.EmployeeCode, System.StringComparison.InvariantCultureIgnoreCase))
                .Select(item => new UserPreferenceGroupMemberViewModel
                {
                    Id = groupSharedInfo.Id,
                    EmployeeName = item.FullName,
                    PositionName = item.Position?.PositionName,
                    CurrentLevelGrade = item.LevelGrade,
                    OperatingOfficeAbbreviation = item.Office?.OfficeAbbreviation,
                    EmployeeCode = item.EmployeeCode
                }).FirstOrDefault();

            return userPreferenceGroupSharedWithMemberViewModel;
        }

    }
}
