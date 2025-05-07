using SharePointOnline.API.Contracts.Services;
using SharePointOnline.API.Models;
using Newtonsoft.Json;
using System.Data;
using static SharePointOnline.API.Core.Helpers.Constants;
using SharePointOnline.API.Core.Helpers;
using System.Text.Json;
using System.Text;
using Microsoft.Kiota.Abstractions.Serialization;

namespace SharePointOnline.API.Core.Services
{
    public class SMAPMissionsService: ISMAPMissionsService
    {
        public async Task<IEnumerable<SMAPMission>> GetSMAPMissions()
        {
            var SiteId = ConfigurationUtility.GetValue("SmapMissionsConfigurationValues:SiteId");
            var clientsListID = ConfigurationUtility.GetValue("SmapMissionsConfigurationValues:clientsListID");

            try
            {

                var listItems = await SharepointGraphClientHelper.GetSharePointListData(SiteId, clientsListID);

                List<SMAPMission> signedOffMissions = new List<SMAPMission>();
                if (listItems != null && listItems.Any())
                {
                    var missionSignedOffItems = listItems.Where(x => x.Fields != null && x.Fields.AdditionalData.ContainsKey("field_21") && (Equals(x.Fields.AdditionalData["field_21"], MissionStatus.SignedOff) || Equals(x.Fields.AdditionalData["field_21"], MissionStatus.PendingOfficeSignOff))).ToList();

                    foreach (var item in missionSignedOffItems)
                    {
                        string json = JsonConvert.SerializeObject(item.Fields?.AdditionalData);
                        SMAPMission mission = JsonConvert.DeserializeObject<SMAPMission>(json);
                        signedOffMissions.Add(mission);
                    }
                }

                return signedOffMissions;

            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error while parsing SharePoint list data: {ex.Message}");
            }
        }

        public async Task<IEnumerable<StaffingPreference>> GetStaffingPreferences()
        {
            var SiteId = ConfigurationUtility.GetValue("StaffingPreferencesConfigurationValues:SiteId");
            var clientsListID = ConfigurationUtility.GetValue("StaffingPreferencesConfigurationValues:clientsListID");

            try
            {

                var listItems = await SharepointGraphClientHelper.GetSharePointListData(SiteId, clientsListID);

                List<StaffingPreference> staffingPreferences = new List<StaffingPreference>();
                if (listItems != null && listItems.Any())
                {
                    foreach (var item in listItems.Where(x => x.Fields != null))
                    {
                        string json = JsonConvert.SerializeObject(item.Fields.AdditionalData);
                        StaffingPreference preference = JsonConvert.DeserializeObject<StaffingPreference>(json);
                        preference.CreatedBy = item.CreatedBy.User.AdditionalData.FirstOrDefault().Value.ToString();
                        preference.LastUpdatedBy = item.LastModifiedBy.User.AdditionalData.FirstOrDefault().Value.ToString();

                        // Extract Travel Regions
                        if (item.Fields.AdditionalData.TryGetValue("Travel_x0020_regions", out var travelRegionsArray))
                        {
                            preference.TravelRegions = GetMultiChoiceArray(travelRegionsArray as UntypedArray);
                        }

                        // Extract PD Focus Areas
                        if (item.Fields.AdditionalData.TryGetValue("DoyouhaveanyPDfocusareasthatarei", out var pdArray))
                        {
                            preference.PdFocusAreas = GetMultiChoiceArray(pdArray as UntypedArray);
                        }


                        UpdateConsolidatePreferenceForEmployee(preference);

                        staffingPreferences.Add(preference);    
                    }
                }

                return staffingPreferences;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Helper Methods

        private List<string> GetMultiChoiceArray(UntypedArray arrayData)
        {
            var multiChoiceList = new List<string>();
            if (arrayData != null)
            {
                var tempJson = KiotaJsonSerializer.SerializeAsString(arrayData);
                var listData = JsonConvert.DeserializeObject<string[]>(tempJson);
                foreach (var data in listData)
                {
                    multiChoiceList.Add(data.ToString());
                }
            }

            return multiChoiceList;
        }


        private void UpdateConsolidatePreferenceForEmployee(StaffingPreference preference)
        {
            StringBuilder stringBuilder = new StringBuilder();

            //---------------Staffing Priorities-------------------------
            if (!string.IsNullOrEmpty(preference.FirstPriority))
            {
                stringBuilder.Append($"First Priority is {preference.FirstPriority}.").AppendLine();
            }
            if (!string.IsNullOrEmpty(preference.SecondPriority))
            {
                stringBuilder.Append($"Second Priority is {preference.SecondPriority}.").AppendLine();
            }
            if (!string.IsNullOrEmpty(preference.ThirdPriority))
            {
                stringBuilder.Append($"Third Priority is {preference.ThirdPriority}.").AppendLine();
            }

            //-----------------PD---------------------------
            
            if (!string.IsNullOrEmpty(preference.Pleasedescribe))
            {
                stringBuilder.Append($"Please descibe field for PD is {preference.Pleasedescribe}.").AppendLine();
            }

            if (stringBuilder.Length > 0)
            {
                preference.CasePriorities = stringBuilder.ToString();
            }
            
            //---------------Travel Preferences-------------------------
            if (!string.IsNullOrEmpty(preference.TravelAvailability))
            {
                stringBuilder.Append("My travel availability is ").Append(preference.TravelAvailability).Append(".").AppendLine();
            }
            if (preference.TravelRegions != null && preference.TravelRegions.Any())
            {
                stringBuilder.Append("I can travel to following regions - ").Append(string.Join(",", preference.TravelRegions)).Append(".").AppendLine();
            }
            if (!string.IsNullOrEmpty(preference.FlyingPreference))
            {
                stringBuilder.Append("My flying time preference is that I can fly ").Append(preference.FlyingPreference).Append(".").AppendLine();
            }
            if (!string.IsNullOrEmpty(preference.ReasonUnableToTravel))
            {
                stringBuilder.Append("I cannot travel due to ").Append(preference.ReasonUnableToTravel).Append(".").AppendLine();
            }
            if (!string.IsNullOrEmpty(preference.AdditionalTravelInfo))
            {
                stringBuilder.Append(preference.AdditionalTravelInfo).Append(".").AppendLine();
            }

            Type myModelType = typeof(StaffingPreference);
            Dictionary<string,List<string>> industryPreferences = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> capabilityPreferences = new Dictionary<string, List<string>>();

            foreach (var propertyToMatch in INDUSTRY_PRACTICE_AREAS)
            {
                foreach (var property in myModelType.GetProperties())
                {
                    var propertyCode = property.Name.Split("_").Last();
                    if (propertyCode == propertyToMatch.PracticeAreaCode)
                    {
                        string industryPreference = (string)property.GetValue(preference);
                        string formattedIndustry = $"{propertyToMatch.PracticeAreaName} ({propertyToMatch.PracticeAreaAbbreviation})";
                        if (!string.IsNullOrEmpty(industryPreference))
                        {
                            if (industryPreferences.ContainsKey(industryPreference))
                            {
                                // Add the formatted string to the existing list of strings for that code
                                industryPreferences[industryPreference].Add(formattedIndustry);
                            }
                            else
                            {
                                // Create a new list for the code and add the formatted string
                                industryPreferences[industryPreference] = new List<string> { formattedIndustry };
                            }
                        }
                    }
                }
            }

            foreach (var propertyToMatch in CAPABILITY_PRACTICE_AREAS)
            {
                foreach (var property in myModelType.GetProperties())
                {
                    var propertyCode = property.Name.Split("_").Last();
                    if (propertyCode == propertyToMatch.PracticeAreaCode)
                    {
                        string capabilityPreference = (string)property.GetValue(preference);
                        string formattedCapabiltiy = $"{propertyToMatch.PracticeAreaName} ({propertyToMatch.PracticeAreaAbbreviation})";
                        if (!string.IsNullOrEmpty(capabilityPreference))
                        {
                            if (capabilityPreferences.ContainsKey(capabilityPreference))
                            {
                                // Add the formatted string to the existing list of strings for that code
                                capabilityPreferences[capabilityPreference].Add(formattedCapabiltiy);
                            }
                            else
                            {
                                // Create a new list for the code and add the formatted string
                                capabilityPreferences[capabilityPreference] = new List<string> { formattedCapabiltiy };
                            }
                        }
                    }
                }
            }

            foreach (var key in industryPreferences.Keys)
            {
                List<string> formattedStrings = industryPreferences[key];
                string combinedString = $"{key} work in following industries : {string.Join(", ", formattedStrings)}";
                stringBuilder.Append(combinedString).Append(".").AppendLine();

                if (key.Contains("willing") || key.Contains("happy"))
                {
                    preference.PracticeAreasHappyStaffing = string.Join(", ", combinedString);
                    preference.IndustryPreference1 = string.Join("| ", formattedStrings);
                }else if (key.Contains("excited"))
                {
                    preference.PracticeAreasExcitedStaffing = string.Join(", ", combinedString);
                    preference.IndustryPreference2 = string.Join("| ", formattedStrings);
                }
                else
                {
                    preference.PracticeAreasToAvoidStaffing = string.Join(", ", combinedString);
                    preference.IndustryPreference3 = string.Join("| ", formattedStrings);
                }

                preference.InterestPracticeAreasForStaffing = preference.PracticeAreasHappyStaffing + preference.PracticeAreasExcitedStaffing;
            }
            foreach (var key in capabilityPreferences.Keys)
            {
                List<string> formattedStrings = capabilityPreferences[key];
                string combinedString = $"{key} work in following capabilities : {string.Join(", ", formattedStrings)}";
                stringBuilder.Append(combinedString).Append(".").AppendLine();

                if (key.Contains("willing") || key.Contains("happy"))
                {
                    preference.PracticeAreasHappyStaffing = string.IsNullOrEmpty(preference.PracticeAreasHappyStaffing) ? string.Join(", ", combinedString) : "," + preference.PracticeAreasHappyStaffing + string.Join(", ", combinedString);

                    preference.CapabilityPreference1 = string.Join("| ", formattedStrings);
                }
                else if (key.Contains("excited"))
                {
                    preference.PracticeAreasExcitedStaffing = string.IsNullOrEmpty(preference.PracticeAreasExcitedStaffing) ? string.Join(", ", combinedString) : "," + preference.PracticeAreasExcitedStaffing + string.Join(", ", combinedString);
                    preference.CapabilityPreference2 = string.Join("| ", formattedStrings);
                }
                else
                {
                    preference.PracticeAreasToAvoidStaffing = string.IsNullOrEmpty(preference.PracticeAreasToAvoidStaffing) ? string.Join(", ", combinedString) : "," + preference.PracticeAreasToAvoidStaffing + string.Join(", ", combinedString);
                    preference.CapabilityPreference3 = string.Join("| ", formattedStrings);
                }

                preference.InterestPracticeAreasForStaffing = preference.PracticeAreasHappyStaffing + preference.PracticeAreasExcitedStaffing;
            }

            string resultString = stringBuilder.ToString();

            preference.ConsolidatedPreferences =  resultString;
        }
        
        #endregion
    }
}
