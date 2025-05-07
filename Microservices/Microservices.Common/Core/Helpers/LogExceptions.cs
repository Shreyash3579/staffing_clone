using Microservices.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microservices.Common.Core.Helpers
{
    public static class LogExceptions
    {

        public static Action<Exception, HttpContext, IConfiguration> SendLogRequest = async (exception, context, Configuration) =>
        {
            var errorLogs = new ErrorLogs
            {
                ApplicationName = Configuration["Serilog:ApplicationName"],
                Error = exception,
                EmployeeCode = context?.Request?.Headers["EmployeeCode"].ToString()
            };

            if (errorLogs != null && string.IsNullOrEmpty(errorLogs.EmployeeCode))
            {
                var claims = context?.User?.Claims?.ToList();
                if (claims != null && claims?.Count > 0)
                    errorLogs.EmployeeCode = claims[0]?.Value?.ToString();
            }

            //hotfix 
           // var client = new HttpClient();
           // var dbLogTask = client.PostAsJsonAsync(Configuration["Serilog:QueryString"], errorLogs);
           // var emailLogTask = client.PostAsJsonAsync(Configuration["Email:QueryString"], errorLogs);
           // await Task.WhenAll(dbLogTask, emailLogTask);


            //var queryString = Configuration["Serilog:QueryString"];
            //var client = new HttpClient();
            //HttpResponseMessage response = await client.PostAsJsonAsync(queryString, errorLogs);

            ////Send email from Logger API to developers
            ///core 3.1 glitch
            ///because 'context' is getting lost (not entirely) before calling the below SendMailToDeveloperHelpDesk
            ///accessing context?.User is throwing error, therefore, had to call SendMailToDeveloperHelpDesk right here
            ///to fetch context?.User properties
            //EmailHelper.SendMailToDeveloperHelpDesk(exception, context, Configuration);
        };
    }
}
