using System;

namespace Staffing.Analytics.API.Models
{
    public class DemandFilterCriteria
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string OfficeCodes { get; set; }
        public string CaseTypeCodes { get; set; }
        public string CaseAttributeNames { get; set; }
        public string OpportunityStatusTypeCodes { get; set; }
        public string DemandTypes { get; set; }
        public short MinOpportunityProbability { get; set; }
        public string CaseExceptionShowList { get; set; }
        public string CaseExceptionHideList { get; set; }
        public string OpportunityExceptionShowList { get; set; }
        public string OpportunityExceptionHideList { get; set; }
        public short ProjectStartIndex { get; set; }
        public short PageSize { get; set; }
        public string IndustryPracticeAreaCodes { get; set; }
        public bool IsStaffedFromSupply { get; set; }
        public string CapabilityPracticeAreaCodes { get; set; }
        public SupplyFilterCriteria supplyFilterCriteria { get; set; }
    }
}
