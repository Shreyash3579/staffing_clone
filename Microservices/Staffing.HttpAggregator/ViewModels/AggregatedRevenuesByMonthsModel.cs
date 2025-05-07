namespace Staffing.HttpAggregator.ViewModels
{
    public class AggregatedRevenuesByMonthsModel
    {
        public string Month { get; set; }
        public double AggregatedRevenue { get; set; }
        public string CurrencyCode { get; set; }
    }
}
