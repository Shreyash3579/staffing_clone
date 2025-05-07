using System;

namespace Staffing.HttpAggregator.Models
{
    public class CurrencyRate
    {
        public string CurrencyCode { get; set; }
        public string CurrencyRateTypeCode { get; set; }
        public double UsdRate { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
}
