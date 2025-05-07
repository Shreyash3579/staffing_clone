using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace BackgroundPolling.API.Core.Helpers
{
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            //var httpContext = context.GetHttpContext();

            return true;// httpContext.User.Identity.IsAuthenticated;
        }
    }
}
