using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basis.API.Models
{
    public class CurrencyRates
    {
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyRateTypeCode { get; set; }
        public string CurrencyRateTypeName { get; set; }
        public DateTime EffectiveDate { get; set; }
        public decimal UsdRate { get; set; }
        public string ServiceCode { get; set; }

    }
}
