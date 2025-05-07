using System;

namespace CaseIntake.API.Models
{
    public class CaseIntakeBasicDetail
    {

        public string Id { get; set; }

        public string CaseRoleCode { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? OpportunityId { get; set; }
        public Guid? PlanningCardId { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
