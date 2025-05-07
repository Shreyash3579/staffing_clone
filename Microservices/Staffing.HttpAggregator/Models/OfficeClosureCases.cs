using System;

namespace Staffing.HttpAggregator.Models
{
    public class OfficeClosureCases
    {
        public Guid? Id { get; set; }
        public int OfficeCode { get; set; }
        public string OldCaseCodes { get; set; }
        public string CaseTypeCodes { get; set; }
        public DateTime OfficeClosureStartDate { get; set; }
        public DateTime OfficeClosureEndDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
