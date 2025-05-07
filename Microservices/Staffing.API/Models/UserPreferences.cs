namespace Staffing.API.Models
{
    public class UserPreferences
    {
        public string EmployeeCode { get; set; }
        public string SupplyViewOfficeCodes { get; set; }
        public string LevelGrades { get; set; }
        public string SupplyViewStaffingTags { get; set; }
        public string AvailabilityIncludes { get; set; }
        public string GroupBy { get; set; }
        public string SortBy { get; set; }
        public short SupplyWeeksThreshold { get; set; }
        public short VacationThreshold { get; set; }
        public short TrainingThreshold { get; set; }
        public string DemandViewOfficeCodes { get; set; }
        public string CaseTypeCodes { get; set; }
        public string CaseAttributeNames { get; set; }
        public string OpportunityStatusTypeCodes { get; set; }
        public short MinOpportunityProbability { get; set; }
        public string DemandTypes { get; set; }
        public short DemandWeeksThreshold { get; set; }
        public string CaseExceptionShowList { get; set; }
        public string CaseExceptionHideList { get; set; }
        public string OpportunityExceptionShowList { get; set; }
        public string OpportunityExceptionHideList { get; set; }
        public string LastUpdatedBy { get; set; }
        public string CaseAllocationsSortBy { get; set; }
        public string PracticeAreaCodes { get; set; }
        public string PositionCodes { get; set; }
        public string PlanningCardsSortOrder { get; set; }
        public string CaseOppSortOrder { get; set; }
        public string AffiliationRoleCodes { get; set; }
        public string IndustryPracticeAreaCodes { get; set; }
        public string CapabilityPracticeAreaCodes { get; set; }
        public bool IsDefault { get; set; }
        public bool IsHistoricalDemandPinned { get; set; }
        public string StaffableAsTypeCodes { get; set; }
    }
}
