using Staffing.HttpAggregator.Core.Helpers;
using Staffing.HttpAggregator.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.ViewModels
{
    public class ResourceAssignmentViewModel
    {
        public Guid? Id { get; set; }
        public Guid? PlanningCardId { get; set; }
        public string PlanningCardTitle { get; set; }
        public string OldCaseCode { get; set; }
        public string CaseName { get; set; }
        public Guid? PipelineId { get; set; }
        public string CortexId { get; set; }
        public string EstimatedTeamSize { get; set; }
        public string OpportunityName { get; set; }
        public string ClientName { get; set; }
        public Constants.CaseType? CaseTypeCode { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string InternetAddress { get; set; }
        public string CurrentLevelGrade { get; set; }
        public int? OperatingOfficeCode { get; set; }
        public string OperatingOfficeAbbreviation { get; set; }
        public int? Allocation { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public String ServiceLineCode { get; set; }
        public String ServiceLineName { get; set; }
        public int? InvestmentCode { get; set; }
        public string InvestmentName { get; set; }
        public string CaseRoleCode { get; set; }
        public string CaseRoleName { get; set; }
        public string PrimaryIndustry { get; set; }
        public string PrimaryCapability { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? CaseStartDate { get; set; }
        public DateTime? CaseEndDate { get; set; }
        public DateTime? OpportunityStartDate { get; set; }
        public DateTime? OpportunityEndDate { get; set; }
        public int? ProbabilityPercent { get; set; }
        public string Notes { get; set; }
        public bool? IsPlaceholderAllocation { get; set; }
        public IEnumerable<SKUDemand> SkuTerms { get; set; }
        public string CombinedSkuTerm { get; set; }
        public DateTime? TerminationDate { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string CaseManagerName { get; set; }
        public string CaseManagerCode { get; set; }
        public string CommitmentTypeCode { get; set; }
        public string CommitmentTypeName { get; set; }
        public bool? isConfirmed { get; set; }
        public bool? IsPlanningCardShared { get; set; }
        public string PositionGroupCode { get; set; }
        public bool? IncludeInCapacityReporting { get; set; }
        public string PegIndustryTerm { get; set; }

    }
}
