using System;

namespace Staffing.HttpAggregator.Models
{
    public class Revenue
    {
        public int OfficeCode { get; set; }
        public string CurrencyCode { get; set; }
        public int CaseCode { get; set; }
        public int ClientCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double ManagementActivity { get; set; }
        public string ServiceLineCode { get; set; }
        public string OpportunityId { get; set; } = null;
    }
}
