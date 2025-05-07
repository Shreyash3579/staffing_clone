using System;
using System.Collections.Generic;

namespace BackgroundPolling.API.Models
{
    public class StaffingPreference
    {
        public string EmployeeCode { get; set; }
        public string EmployeeEmail { get; set; }
        //-------------------Staffing Priorities---------------------
        public string FirstPriority { get; set; }
        public string SecondPriority { get; set; }
        public string ThirdPriority { get; set; }

        //-------------------Industries---------------------
        public string PreBainIndustryExperience { get; set; }
        public string IndustryPreference1 { get; set; }
        public string IndustryPreference2 { get; set; }
        public string IndustryPreference3 { get; set; }

        //-------------------Capabilities---------------------
        public string PreBainCapabilityExperience { get; set; }
        public string CapabilityPreference1 { get; set; }
        public string CapabilityPreference2 { get; set; }
        public string CapabilityPreference3 { get; set; }

        //-------------------Travel---------------------
        public string TravelAvailability { get; set; }
        public string FlyingPreference { get; set; }
        public List<string> TravelRegions { get; set; }
        public string ReasonUnableToTravel { get; set; }
        public string AdditionalTravelInfo { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }

        //---------------Combined Preference----------------
        public string ConsolidatedPreferences { get; set; }
        public string CasePriorities { get; set; }
        public string PracticeAreasToAvoidStaffing { get; set; }
        public string PracticeAreasHappyStaffing { get; set; }
        public string PracticeAreasExcitedStaffing { get; set; }
        public string InterestPracticeAreasForStaffing { get; set; }

        //----------------PD-------------------------------
        public List<string> PdFocusAreas { get; set; }
        public string Pleasedescribe { get; set; }

    }
}
