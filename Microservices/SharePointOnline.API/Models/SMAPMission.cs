using Newtonsoft.Json;

namespace SharePointOnline.API.Models
{
    public class SMAPMission
    {
        [JsonProperty("Title")]
        public string EmployeeCode { get; set; } 
        [JsonProperty("field_21")]
        public string MissionStatus { get; set; }
        [JsonProperty("field_4")]
        public string PositionShortName { get; set; }
        [JsonProperty("field_5")]
        public string ClusterAbbreviation { get; set; }
        [JsonProperty("field_6")]
        public string OfficeName { get; set; }
        [JsonProperty("field_7")]
        public string L1Affiliation { get; set; }
        [JsonProperty("field_8")]
        public string L2Affiliation { get; set; }
        [JsonProperty("field_14")]
        public string WhatIWantToBeKnownFor { get; set; }
        [JsonProperty("field_15")]
        public string CapabilityToBuild { get; set; }
        [JsonProperty("field_16")]
        public string IndustryToBuild { get; set; }
        [JsonProperty("field_17")]
        public string DevelopExpertise { get; set; }
        [JsonProperty("field_18")]
        public string BuildFirmForFuture { get; set; }
        [JsonProperty("field_19")]
        public string GenerateDemand { get; set; }
        //[JsonProperty("field_20")]
        //public string WhoCanHelpToAchieveGoals { get; set; }
        [JsonProperty("Created")]
        public DateTime Created { get; set; }
        [JsonProperty("Modified")]
        public DateTime LastUpdated { get; set; }
    }
}
