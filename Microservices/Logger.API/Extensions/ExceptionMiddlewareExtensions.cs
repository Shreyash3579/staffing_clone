using Microservices.Common.Core.Helpers;
using Microservices.Common.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;

namespace Logger.API.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();
        public static void UseCustomExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        //LogExceptions.SendLogRequest(contextFeature.Error, context, Configuration);
                        EmailHelper.SendMailToDeveloperHelpDesk(contextFeature.Error, context, Configuration);
                        //Log.Error($"Something went wrong: {contextFeature.Error}");
                    }
                    //Catch Exception globally
                    await context.Response.WriteAsync(new ErrorDetails()
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = contextFeature.Error.Message,
                        StackTrace = contextFeature.Error.StackTrace
                    }.ToString());
                });
            });
        }
    }
}
