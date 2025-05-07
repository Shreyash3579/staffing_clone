using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.ViewModels
{
    public class PlanningCardViewModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Office { get; set; }
        public bool? IsShared { get; set; }
        public string SharedOfficeCodes { get; set; }
        public bool? IncludeInCapacityReporting { get; set; }
        public string SharedOfficeAbbreviations { get; set; }
        public string SharedStaffingTags { get; set; }
        public string PegOpportunityId { get; set; }
        public int? ProbabilityPercent { get; set; }
        public string CreatedBy { get; set; }
        public bool? isMerged { get; set; }
        public string? MergedCaseCode { get; set; }
        public bool? IncludeInDemand { get; set; }
        public bool? IsFlagged { get; set; }
        public IEnumerable<SKUDemand> SKUTerm { get; set; }
        public string CombinedSkuTerm { get; set; }

        public bool isSTACommitmentCreated { get; set; }
        public IEnumerable<CaseViewNoteViewModel> CasePlanningViewNotes { get; set; }
        public IList<ResourceAssignmentViewModel> allocations { get; set; }
    }
}
