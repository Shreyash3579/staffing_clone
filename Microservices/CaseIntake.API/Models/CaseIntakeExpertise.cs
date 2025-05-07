using System;

namespace CaseIntake.API.Models
{
    public class CaseIntakeExpertise
    {
        public Guid ExpertiseAreaCode { get; set; }
        public string ExpertiseAreaName { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
