using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Helpers;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class RevenueService : IRevenueService
    {
        private readonly IRevenueApiClient _revenueApiClient;
        private readonly IStaffingApiClient _staffingApiClient;

        public RevenueService(IRevenueApiClient revenueApiClient, IStaffingApiClient staffingApiClient)
        {
            _revenueApiClient = revenueApiClient;
            _staffingApiClient = staffingApiClient;
        }

        private IList<Revenue> GetRevenueinUSD(IList<Revenue> revenueData, IList<CurrencyRate> currencyRates, string currency)
        {
            var revenues = (from revenue in revenueData
                            join curr in currencyRates on revenue.CurrencyCode equals curr.CurrencyCode
                            select new Revenue()
                            {
                                OfficeCode = revenue.OfficeCode,
                                CurrencyCode = currency,
                                CaseCode = revenue.CaseCode,
                                ClientCode = revenue.ClientCode,
                                StartDate = revenue.StartDate,
                                EndDate = revenue.EndDate,
                                ManagementActivity = (revenue.ManagementActivity * curr.UsdRate),
                                ServiceLineCode = revenue.ServiceLineCode,
                                OpportunityId = revenue.OpportunityId,
                            }).ToList();

            return revenues;
        }

        public async Task<RevenueViewModel> GetRevenueByCaseCodeAndClientCode(int? clientCode, int? caseCode, string pipelineId, string currency)
        {
            if (clientCode == null || (clientCode != null && clientCode < 1))
                throw new ArgumentException("Client Code can not be null or 0");

            if (caseCode != null && caseCode < 1)
                throw new ArgumentException("Case code can not be or less than 0");

            if (caseCode == null && string.IsNullOrEmpty(pipelineId))
                throw new ArgumentException("Opportunity/Pipeline Id can not be empty/null");

            var monthsBracket = Convert.ToInt32(ConfigurationUtility.GetValue("noOfMonthsForRevenueData")) / 2;
            var initialMonth = DateTime.Now.AddMonths(-monthsBracket).Month;
            var finalMonth = DateTime.Now.AddMonths(monthsBracket).Month;
            DateTime startDate = new DateTime(DateTime.Now.Year, initialMonth, 1);
            DateTime endDate = new DateTime(DateTime.Now.Year, finalMonth, 1).AddMonths(1).AddDays(-1);

            IList<Revenue> caseRevenueData;

            caseRevenueData = await _revenueApiClient.GetRevenueByClientCodeAndCaseCode(clientCode, caseCode, startDate, endDate);

            //if pipelineId exists then filter to get Opportunity data only
            if (caseCode == null && !string.IsNullOrEmpty(pipelineId))
                caseRevenueData = caseRevenueData.Where(x => x.OpportunityId == pipelineId).ToList();

            if (caseRevenueData == null || caseRevenueData.Count() == 0)
            {
                return new RevenueViewModel();
            }

            string distinctCurrencyCodes = string.Join(",", caseRevenueData.Select(x => x.CurrencyCode).Distinct());
            var currencyRates = await _staffingApiClient.GetCurrencyRatesByCurrencyCodesAndDate(distinctCurrencyCodes, "B", startDate, endDate);
            caseRevenueData = GetRevenueinUSD(caseRevenueData, currencyRates.ToList(), currency);


            var revenueViewModel = new RevenueViewModel
            {
                serviceLinesData = new List<ServiceLinesRevenueByMonthModel>()
            };
            var monthsRange = Enumerable.Range(initialMonth, Convert.ToInt32(ConfigurationUtility.GetValue("noOfMonthsForRevenueData"))).ToArray();
            var serviceLinesRevenueByMonthModelList = new List<ServiceLinesRevenueByMonthModel>();

            foreach (var serviceLine in caseRevenueData.GroupBy(x => x.ServiceLineCode).Select(x => x.Key))
            {
                var revenueDataModelList = new List<RevenueDataModel>();
                var serviceLinesRevenueByMonthModel = new ServiceLinesRevenueByMonthModel();
                var revenueByServiceLine = caseRevenueData.Where(x => x.ServiceLineCode == serviceLine);
                serviceLinesRevenueByMonthModel.serviceLine = revenueByServiceLine.Select(x => x.ServiceLineCode).FirstOrDefault();
                foreach (var month in monthsRange)
                {
                    var revenueDataModelByMonth = new RevenueDataModel
                    {
                        CurrencyCode = currency,
                        Month = month.ToString(),
                        TotalRevenue = revenueByServiceLine.Where(x => x.StartDate.Month == month).GroupBy(t => t.ServiceLineCode).Select(y => Math.Round(y.Sum(z => z.ManagementActivity))).FirstOrDefault()
                    };
                    revenueDataModelList.Add(revenueDataModelByMonth);
                }
                serviceLinesRevenueByMonthModel.revenueData = revenueDataModelList;
                serviceLinesRevenueByMonthModelList.Add(serviceLinesRevenueByMonthModel);
            }

            var aggregatedRevenuesByMonthsModelList = new List<AggregatedRevenuesByMonthsModel>();
            foreach (var month in monthsRange)
            {
                var aggregatedRevenuesByMonthsModel = caseRevenueData.Where(x => x.StartDate.Month == month).GroupBy(t => t.StartDate.Month)
                                                                   .Select(y => new AggregatedRevenuesByMonthsModel()
                                                                   {
                                                                       Month = y.Key.ToString(),
                                                                       AggregatedRevenue = Math.Round(y.Sum(z => z.ManagementActivity)),
                                                                       CurrencyCode = currency
                                                                   });
                if (aggregatedRevenuesByMonthsModel.Count() > 0)
                {
                    aggregatedRevenuesByMonthsModelList.Add(aggregatedRevenuesByMonthsModel.ToList().FirstOrDefault());
                }
                else
                {
                    aggregatedRevenuesByMonthsModelList.Add(new AggregatedRevenuesByMonthsModel
                    {
                        Month = month.ToString(),
                        AggregatedRevenue = 0,
                        CurrencyCode = currency
                    });
                }
            }

            revenueViewModel.serviceLinesData = serviceLinesRevenueByMonthModelList;
            revenueViewModel.aggregatedRevenueByMonths = aggregatedRevenuesByMonthsModelList;

            return revenueViewModel;
        }
    }
}
