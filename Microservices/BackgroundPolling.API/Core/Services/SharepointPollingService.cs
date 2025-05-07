using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class SharepointPollingService : ISharepointPollingService
    {
        private readonly ISharepointApiClient _sharepointApiClient;
        private readonly IStaffingPollingRepository _staffingPollingRepository;
        public SharepointPollingService(IStaffingPollingRepository staffingPollingRepository, ISharepointApiClient sharepointApiClient)
        {
            _staffingPollingRepository = staffingPollingRepository;
            _sharepointApiClient = sharepointApiClient;
        }
        public async Task UpsertSignedOffSMAPMissions()
        {
            var smapMissions = await _sharepointApiClient.GetSMAPMissions();
            if(!smapMissions.Any())
            {
                return;
            }

            var dataTable = ConvertToSMAPMissionsDataTable(smapMissions);
            await _staffingPollingRepository.UpsertSMAPMissions(dataTable);
        }

        public async Task UpsertStaffingPreferences()
        {
            var staffingPreferences = await _sharepointApiClient.GetStaffingPreferences();
            if (!staffingPreferences.Any())
            {
                return;
            }

            var dataTable = ConvertToStaffingPreferencesDataTable(staffingPreferences);
            await _staffingPollingRepository.UpsertStaffingPreferencesFromSharepoint(dataTable);
            //await _staffingPollingRepository.UpsertStaffingPreferencesFromSharepointToAnalyticsDB(dataTable);
        }

        #region Private Members
        private DataTable ConvertToSMAPMissionsDataTable(IEnumerable<SMAPMission> signedOffMissions)
        {
            var smapMissionsDataTable = new DataTable();
            smapMissionsDataTable.Columns.Add("employeeCode", typeof(string));
            smapMissionsDataTable.Columns.Add("missionStatus", typeof(string));
            smapMissionsDataTable.Columns.Add("positionShortName", typeof(string));
            smapMissionsDataTable.Columns.Add("clusterAbbreviation", typeof(string));
            smapMissionsDataTable.Columns.Add("officeName", typeof(string));
            smapMissionsDataTable.Columns.Add("l1Affiliation", typeof(string));
            smapMissionsDataTable.Columns.Add("l2Affiliation", typeof(string));
            smapMissionsDataTable.Columns.Add("whatIWantToBeKnownFor", typeof(string));
            smapMissionsDataTable.Columns.Add("capabilityToBuild", typeof(string));
            smapMissionsDataTable.Columns.Add("industryToBuild", typeof(string));
            smapMissionsDataTable.Columns.Add("developExpertise", typeof(string));
            smapMissionsDataTable.Columns.Add("buildFirmForFuture", typeof(string));
            smapMissionsDataTable.Columns.Add("generateDemand", typeof(string));
            smapMissionsDataTable.Columns.Add("created", typeof(DateTime));
            smapMissionsDataTable.Columns.Add("lastUpdated", typeof(DateTime));
            smapMissionsDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var missions in signedOffMissions)
            {

                var row = smapMissionsDataTable.NewRow();

                row["employeeCode"] = missions.EmployeeCode;
                row["missionStatus"] = missions.MissionStatus;
                row["positionShortName"] = missions.PositionShortName;
                row["clusterAbbreviation"] = missions.ClusterAbbreviation;
                row["officeName"] = missions.OfficeName;
                row["l1Affiliation"] = missions.L1Affiliation;
                row["l2Affiliation"] = missions.L2Affiliation;
                row["whatIWantToBeKnownFor"] = missions.WhatIWantToBeKnownFor;
                row["capabilityToBuild"] = missions.CapabilityToBuild;
                row["industryToBuild"] = missions.IndustryToBuild;
                row["developExpertise"] = missions.DevelopExpertise;
                row["buildFirmForFuture"] = missions.BuildFirmForFuture;
                row["generateDemand"] = missions.GenerateDemand;
                row["created"] = missions.Created;
                row["lastUpdated"] = (object)missions.LastUpdated ?? DateTime.Today.Date;
                row["lastUpdatedBy"] = "Auto-SmapMissions";

                smapMissionsDataTable.Rows.Add(row);
            }

            return smapMissionsDataTable;
        }

        private DataTable ConvertToStaffingPreferencesDataTable(IEnumerable<StaffingPreference> staffingPreferences)
        {
            var staffingPreferencesDataTable = new DataTable();
            staffingPreferencesDataTable.Columns.Add("employeeEmail", typeof(string));
            staffingPreferencesDataTable.Columns.Add("employeeCode", typeof(string));
            staffingPreferencesDataTable.Columns.Add("firstPriority", typeof(string));
            staffingPreferencesDataTable.Columns.Add("secondPriority", typeof(string));
            staffingPreferencesDataTable.Columns.Add("thirdPriority", typeof(string));
            staffingPreferencesDataTable.Columns.Add("casePriorities", typeof(string));
            staffingPreferencesDataTable.Columns.Add("industryPreference1", typeof(string));
            staffingPreferencesDataTable.Columns.Add("industryPreference2", typeof(string));
            staffingPreferencesDataTable.Columns.Add("industryPreference3", typeof(string));
            staffingPreferencesDataTable.Columns.Add("practiceAreasHappyStaffing", typeof(string));
            staffingPreferencesDataTable.Columns.Add("practiceAreasExcitedStaffing", typeof(string));
            staffingPreferencesDataTable.Columns.Add("practiceAreasToAvoidStaffing", typeof(string));
            staffingPreferencesDataTable.Columns.Add("interestPracticeAreasForStaffing", typeof(string));
            staffingPreferencesDataTable.Columns.Add("preBainIndustryExperience", typeof(string));
            staffingPreferencesDataTable.Columns.Add("capabilityPreference1", typeof(string));
            staffingPreferencesDataTable.Columns.Add("capabilityPreference2", typeof(string));
            staffingPreferencesDataTable.Columns.Add("capabilityPreference3", typeof(string));
            staffingPreferencesDataTable.Columns.Add("preBainCapabilityExperience", typeof(string));
            staffingPreferencesDataTable.Columns.Add("travelAvailability", typeof(string));
            staffingPreferencesDataTable.Columns.Add("flyingPreference", typeof(string));
            staffingPreferencesDataTable.Columns.Add("travelRegions", typeof(string));
            staffingPreferencesDataTable.Columns.Add("reasonUnableToTravel", typeof(string));
            staffingPreferencesDataTable.Columns.Add("additionalTravelInfo", typeof(string));
            staffingPreferencesDataTable.Columns.Add("consolidatedPreferences", typeof(string));
            staffingPreferencesDataTable.Columns.Add("pdFocusAreas", typeof(string));
            staffingPreferencesDataTable.Columns.Add("pleasedescribe", typeof(string));
            staffingPreferencesDataTable.Columns.Add("created", typeof(DateTime));
            staffingPreferencesDataTable.Columns.Add("createdBy", typeof(string));
            staffingPreferencesDataTable.Columns.Add("lastUpdated", typeof(DateTime));
            staffingPreferencesDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var preference in staffingPreferences)
            {

                var row = staffingPreferencesDataTable.NewRow();

                row["employeeCode"] = preference.EmployeeCode;
                row["employeeEmail"] = preference.EmployeeEmail;
                row["firstPriority"] = (object) preference.FirstPriority ?? DBNull.Value;
                row["secondPriority"] = (object)preference.SecondPriority ?? DBNull.Value;
                row["thirdPriority"] = (object)preference.ThirdPriority ?? DBNull.Value;
                row["casePriorities"] = (object)preference.CasePriorities ?? DBNull.Value;
                row["industryPreference1"] = (object)preference.IndustryPreference1 ?? DBNull.Value;
                row["industryPreference2"] = (object)preference.IndustryPreference2 ?? DBNull.Value;
                row["industryPreference3"] = (object)preference.IndustryPreference3 ?? DBNull.Value;
                row["practiceAreasHappyStaffing"] = (object)preference.PracticeAreasHappyStaffing ?? DBNull.Value;
                row["practiceAreasExcitedStaffing"] = (object)preference.PracticeAreasExcitedStaffing ?? DBNull.Value;
                row["practiceAreasToAvoidStaffing"] = (object)preference.PracticeAreasToAvoidStaffing ?? DBNull.Value;
                row["interestPracticeAreasForStaffing"] = (object)preference.InterestPracticeAreasForStaffing ?? DBNull.Value;
                row["preBainIndustryExperience"] = (object)preference.PreBainIndustryExperience ?? DBNull.Value;
                row["capabilityPreference1"] = (object)preference.CapabilityPreference1 ?? DBNull.Value;
                row["capabilityPreference2"] = (object)preference.CapabilityPreference2 ?? DBNull.Value;
                row["capabilityPreference3"] = (object)preference.CapabilityPreference3 ?? DBNull.Value;
                row["preBainCapabilityExperience"] = (object)preference.PreBainCapabilityExperience ?? DBNull.Value;
                row["travelAvailability"] = (object)preference.TravelAvailability ?? DBNull.Value;
                row["flyingPreference"] = (object)preference.FlyingPreference ?? DBNull.Value;
                row["travelRegions"] = preference.TravelRegions != null ? (object)string.Join(",", preference.TravelRegions) : DBNull.Value;
                row["reasonUnableToTravel"] = (object)preference.ReasonUnableToTravel ?? DBNull.Value;
                row["additionalTravelInfo"] = (object)preference.AdditionalTravelInfo ?? DBNull.Value;
                row["consolidatedPreferences"] = (object)preference.ConsolidatedPreferences ?? DBNull.Value;
                row["pdFocusAreas"] = row["pdFocusAreas"] = preference.PdFocusAreas != null ? (object)string.Join(",", preference.PdFocusAreas) : DBNull.Value;
                row["pleasedescribe"] = (object)preference.Pleasedescribe ?? DBNull.Value;
                row["created"] = preference.Created;
                row["createdBy"] = preference.CreatedBy;
                row["lastUpdated"] = (object)preference.LastUpdated ?? DateTime.Today.Date;
                row["lastUpdatedBy"] = preference.LastUpdatedBy;

                staffingPreferencesDataTable.Rows.Add(row);
            }

            return staffingPreferencesDataTable;
        }
        #endregion
    }
}
