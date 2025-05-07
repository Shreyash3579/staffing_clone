using System;

namespace Staffing.AzureServiceBus.Models
{
    public class PricingSkuViewModel
    {
        public Guid? Id { get; set; }
        public string sf_opportunity_id { get; set; }
        public string sf_opportunity_name { get; set; }
        public string sf_opportunity_substage { get; set; }
        public string title { get; set; }
        public string teamname { get; set; }
        public Guid? country_id { get; set; }
        public string countryname { get; set; }
        public string name { get; set; }
        public string abbreviation { get; set; }
        public string updated_at { get; set; }
        public int allocation_percentage { get; set; }
        public string username { get; set; }
        public string lastUpdatedBy { get; set; }
    }
}
