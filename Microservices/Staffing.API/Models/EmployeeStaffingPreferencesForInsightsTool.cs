using System;

namespace Staffing.API.Models
{
    public class EmployeeStaffingPreferencesForInsightsTool
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public string PdFocusAreas { get; set; }
        public string PdFocusAreasAdditionalInformation { get; set; }
        public string FirstPriority { get; set; }
        public string SecondPriority { get; set; }
        public string ThirdPriority { get; set; }
        public string IndustryCodesHappyToWorkIn { get; set; }
        public string IndustryCodesExcitedToWorkIn { get; set; }
        public string IndustryCodesNotInterestedToWorkIn { get; set; }
        public string CapabilityCodesHappyToWorkIn { get; set; }
        public string CapabilityCodesExcitedToWorkIn { get; set; }
        public string CapabilityCodesNotInterestedToWorkIn { get; set; }
        public string PreBainExperience { get; set; }
        public string TravelInterest { get; set; }
        public string TravelRegions { get; set; }
        public string TravelDuration { get; set; }
        public string AdditionalTravelInfo { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
