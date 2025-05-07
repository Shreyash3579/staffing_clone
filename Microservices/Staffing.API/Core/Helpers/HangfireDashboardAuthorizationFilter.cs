using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Staffing.API.Core.Helpers
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
