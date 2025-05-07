using System.Collections.Generic;

namespace Staffing.AzureServiceBus.Models
{
    public class PricingSkuMap
    {
        public IEnumerable<PricingSkuViewModel> scenario { get; set; }
    }
}
