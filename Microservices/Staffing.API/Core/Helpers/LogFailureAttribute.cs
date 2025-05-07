using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using Microservices.Common.Core.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;


namespace Staffing.API.Core.Helpers
{
    public class LogFailureAttribute : JobFilterAttribute, IApplyStateFilter
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            // Keeping deleted/succeeded Jobs records for 30 days in Database
            var succededJobs = context.NewState as SucceededState;
            if (succededJobs != null)
                context.JobExpirationTimeout = TimeSpan.FromDays(30);
            var failedState = context.NewState as FailedState;
            if (failedState != null)
            {
                //Log Exception in DB and send email
                LogExceptions.SendLogRequest(failedState.Exception, null, Configuration);
            }
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            var failedState = context.NewState as FailedState;
        }
    }
}
