using Staffing.Coveo.API.Models;
using Staffing.Coveo.API.ViewModels;
using System;
using System.Threading.Tasks;

namespace Staffing.Coveo.API.Contracts.Services
{
    public interface ICoveoAnalyticsClient
    {
        Task Search(AnalyticsSearchViewModel analyticsData, string sourceTab, string userIPAddress);
        Task<dynamic> LogClickEventInCoveoAnalytics(AnalyticsClickViewModel analyticsData, string userIPAddress);
        //Task Click();
    }
}
