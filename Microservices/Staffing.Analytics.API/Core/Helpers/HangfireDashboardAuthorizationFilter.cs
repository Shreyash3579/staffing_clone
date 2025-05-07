using Hangfire.Dashboard;
using System.Diagnostics.CodeAnalysis;

namespace Staffing.Analytics.API.Core.Helpers
{
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            //var httpContext = context.GetHttpContext();

            //return httpContext.User.Identity.IsAuthenticated;
            return true;
        }
    }
}
