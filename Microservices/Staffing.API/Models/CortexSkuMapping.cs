using System;

namespace Staffing.API.Models
{
    public class CortexSkuMapping
    {
        public string CortexSKU { get; set; }
        public string MappedStaffingSKU { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
