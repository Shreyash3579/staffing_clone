using System;

namespace Staffing.API.Models
{
    public class SMAPMission
    {
        public string EmployeeCode { get; set; }
        public string MissionStatus { get; set; }
        public string PositionShortName { get; set; }
        public string ClusterAbbreviation { get; set; }
        public string OfficeName { get; set; }
        public string L1Affiliation { get; set; }
        public string L2Affiliation { get; set; }
        public string WhatIWantToBeKnownFor { get; set; }
        public string CapabilityToBuild { get; set; }
        public string IndustryToBuild { get; set; }
        public string DevelopExpertise { get; set; }
        public string BuildFirmForFuture { get; set; }
        public string GenerateDemand { get; set; }
        public string WhoCanHelpToAchieveGoals { get; set; }
        public DateTime Created { get; set; } 
        public DateTime LastUpdated { get; set; }
    }
}
