using Microservices.Common.Core.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Staffing.Analytics.API.Extensions;
using Staffing.Analytics.API.Core.Helpers;
using System.Diagnostics.CodeAnalysis;
using Staffing.Analytics.API.Contracts.Services;
using Hangfire;
using System;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microservices.Common.Core.HashiCorpVault;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.RegisterAppServices();
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var appconfig = builder.Configuration;

//var vaultAccessConfigs = appconfig.GetSection("VaultAccessConfigs").Get<VaultFluent.VaultObject>();
//appconfig.AddVaultConfigurationsAsync(vaultAccessConfigs).Wait();


builder.Services.AddApplicationInsightsTelemetry(appconfig["APPINSIGHTS_CONNECTIONSTRING"]);
var app = builder.Build();
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

app.UseSwagger(options =>
    options.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        var basePath = "staffingAnalyticsApi";
#if !DEBUG
        swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"/{basePath}" } };
#endif
    }));
app.UseSwaggerUI(c =>
{
#if DEBUG
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Staffing Analytics API v1");
#else
    c.SwaggerEndpoint("/staffingAnalyticsApi/swagger/v1/swagger.json", "Staffing Analytics API v1");
#endif
});

#region hangfire
#if (!DEBUG)
    app.UseHangfireServer();
    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")))
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireDashboardAuthorizationFilter() },
            PrefixPath = "/staffingAnalyticsApi"
        });
    }
    else
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
        });
    }
    
    // Cleanup existing recurring jobs
    RecurringJob.RemoveIfExists("updateAnalyticsForExternalCommitments");
    RecurringJob.RemoveIfExists("upsertCapacityAnalysisDaily");
    RecurringJob.RemoveIfExists("UpdateCapacityAnalysisDailyForChangeinCaseAttribute");
    RecurringJob.RemoveIfExists("upsertCapacityAnalysisMonthly");
    RecurringJob.RemoveIfExists("CorrectAnalyticsData");
    RecurringJob.RemoveIfExists("CorrectPlaceholderAnalyticsData");

    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    if (Environments.Production == environment)
    {
        // Job Scheduling
        RecurringJob.AddOrUpdate<IAnalyticsService>("updateAnalyticsForExternalCommitments",
           service => service.UpdateAnlayticsDataForUpsertedExternalCommitment(null), "0 */6 * * *"); // Every 6 Hours
        RecurringJob.AddOrUpdate<IAnalyticsService>("upsertCapacityAnalysisDaily",
            service => service.UpsertCapacityAnalysisDaily(false, null), ConfigurationUtility.GetValue("HangfireJobSchedule:upsertCapacityAnalysisDaily")); // Every saturday
        RecurringJob.AddOrUpdate<IAnalyticsService>("UpdateCapacityAnalysisDailyForChangeinCaseAttribute",
            service => service.UpdateCapacityAnalysisDailyForChangeInCaseAttribute(null), Cron.Daily);
        RecurringJob.AddOrUpdate<IAnalyticsService>("upsertCapacityAnalysisMonthly",
            service => service.UpsertCapacityAnalysisMonthly(false), ConfigurationUtility.GetValue("HangfireJobSchedule:upsertCapacityAnalysisMonthly")); // Every 20 minutes
        RecurringJob.AddOrUpdate<IAnalyticsService>("CorrectAnalyticsData",
           service => service.CorrectAnalyticsData(), ConfigurationUtility.GetValue("HangfireJobSchedule:CorrectAnalyticsData"), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")); // At midnight
        RecurringJob.AddOrUpdate<IPlaceholderAnalyticsService>("CorrectPlaceholderAnalyticsData",
           service => service.CorrectPlaceholderAnalyticsData(), ConfigurationUtility.GetValue("HangfireJobSchedule:CorrectPlaceholderAnalyticsData"), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")); // At midnight
    }
    else
    {
        //non-prod environements no need to run multiple times.
        const string dailyAt12AM = "0 0 * * *";
        const string dailyAt12PM = "0 12 * * *";
        // Job Scheduling
        RecurringJob.AddOrUpdate<IAnalyticsService>("updateAnalyticsForExternalCommitments",
           service => service.UpdateAnlayticsDataForUpsertedExternalCommitment(null), dailyAt12AM); // Every 6 Hours
        RecurringJob.AddOrUpdate<IAnalyticsService>("upsertCapacityAnalysisDaily",
            service => service.UpsertCapacityAnalysisDaily(false, null), dailyAt12AM); // Every saturday
        RecurringJob.AddOrUpdate<IAnalyticsService>("UpdateCapacityAnalysisDailyForChangeinCaseAttribute",
            service => service.UpdateCapacityAnalysisDailyForChangeInCaseAttribute(null), Cron.Never);
        RecurringJob.AddOrUpdate<IAnalyticsService>("upsertCapacityAnalysisMonthly",
            service => service.UpsertCapacityAnalysisMonthly(false), dailyAt12PM); // Every 20 minutes
        RecurringJob.AddOrUpdate<IAnalyticsService>("CorrectAnalyticsData",
           service => service.CorrectAnalyticsData(), dailyAt12PM); // At midnight
        RecurringJob.AddOrUpdate<IPlaceholderAnalyticsService>("CorrectPlaceholderAnalyticsData",
           service => service.CorrectPlaceholderAnalyticsData(), dailyAt12PM); // At midnight
    }
#endif
#endregion

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseCustomExceptionHandler();
app.UseMiddleware<ResponseTimeMiddleware>();
app.UseHttpsRedirection();


app.Run();



//namespace Staffing.Analytics.API
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            CreateHostBuilder(args).Build().Run();
//        }

//        public static IHostBuilder CreateHostBuilder(string[] args) =>
//            Host.CreateDefaultBuilder(args)
//                .ConfigureWebHostDefaults(webBuilder =>
//                {
//                    webBuilder.UseStartup<Startup>();
//                });
//    }
//}
