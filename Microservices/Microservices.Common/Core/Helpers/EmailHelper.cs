using Microservices.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http;

namespace Microservices.Common.Core.Helpers
{
    public static class EmailHelper
    {
        public static Action<Exception, HttpContext, IConfiguration> SendMailToDeveloperHelpDesk =
            async (exception, context, Configuration) =>
            {
                var errorLogs = new ErrorLogs
                {
                    ApplicationName = Configuration["Email:ApplicationName"],
                    Error = exception,
                    EmployeeCode = context?.Request?.Headers["EmployeeCode"].ToString()
                };

                if (errorLogs != null && string.IsNullOrEmpty(errorLogs.EmployeeCode))
                {
                    var claims = context?.User?.Claims?.ToList();
                    if (claims != null && claims?.Count > 0)
                        errorLogs.EmployeeCode = claims[0]?.Value;
                }

                var queryString = Configuration["Email:QueryString"];
                var client = new HttpClient();
                var response = await client.PostAsJsonAsync(queryString, errorLogs);
            };
    }
}