﻿using System;

namespace Staffing.API.Models
{
    public class PlanningCardAllocation
    {
        public Guid Id { get; set; }
        public Guid? PlanningCardId { get; set; }
        public string PlanningCardTitle { get; set; }
        public bool? IsPlanningCardShared { get; set; }
        public int? ClientCode { get; set; }
        public int? CaseCode { get; set; }
        public string OldCaseCode { get; set; }
        public Guid? PipelineId { get; set; }
        public short? InvestmentCode { get; set; }
        public string CaseRoleCode { get; set; }
        public string EmployeeCode { get; set; }
        public short? OperatingOfficeCode { get; set; }
        public string CurrentLevelGrade { get; set; }
        public string ServiceLineName { get; set; }
        public string ServiceLineCode { get; set; }
        public short? Allocation { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Notes { get; set; }
        public string CommitmentTypeCode { get; set; }
        public bool? IsPlaceholderAllocation { get; set; }

        //This does a shallow copy ONLY
        public PlanningCardAllocation Clone()
        {
            return (PlanningCardAllocation)this.MemberwiseClone();
        }
    }
}
