using System.Collections.Generic;

namespace Staffing.HttpAggregator.ViewModels
{
    public class ServiceLinesRevenueByMonthModel
    {
        public string serviceLine { get; set; }
        public IList<RevenueDataModel> revenueData { get; set; }
    }
}
