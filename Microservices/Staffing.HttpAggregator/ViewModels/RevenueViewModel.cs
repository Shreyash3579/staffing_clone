using System.Collections.Generic;

namespace Staffing.HttpAggregator.ViewModels
{

    public class RevenueViewModel
    {
        public IList<ServiceLinesRevenueByMonthModel> serviceLinesData { get; set; }
        public IList<AggregatedRevenuesByMonthsModel> aggregatedRevenueByMonths { get; set; }
    }
}
