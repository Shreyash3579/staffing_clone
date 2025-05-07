//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Staffing.Analytics.API.Extensions;
//using Hangfire;
//using Microservices.Common.Core.Helpers;
//using Staffing.Analytics.API.Core.Helpers;
//using System.Diagnostics.CodeAnalysis;
//using Staffing.Analytics.API.Contracts.Services;

namespace Staffing.Analytics.API
{
//    [ExcludeFromCodeCoverage]
   public class Startup
    {
//        public Startup(IConfiguration configuration)
//        {
//            Configuration = configuration;
//        }

//        public IConfiguration Configuration { get; }

//        // This method gets called by the runtime. Use this method to add services to the container.
//        public void ConfigureServices(IServiceCollection services)
//        {
//            services.AddControllers().AddNewtonsoftJson();
//            services.RegisterAppServices();
//            services.AddMemoryCache();
//            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
//        }

//        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//        {
//            // Host swagger generated API specification only on server have Environment variable (ASPNETCORE_ENVIRONMENT) set to DevEnv
//            if (env.EnvironmentName == Environments.Development) app.UseDeveloperExceptionPage();

//            app.UseSwagger();
//            app.UseSwaggerUI(c =>
//            {
//#if DEBUG
//                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Staffing Analytics API v1");
//#else
//                c.SwaggerEndpoint("/staffingAnalyticsApi/swagger/v1/swagger.json", "Staffing Analytics API v1");
//#endif
//            });
//#if (!DEBUG)
//            app.UseHangfireServer();
//            app.UseHangfireDashboard("/hangfire", new DashboardOptions
//            {
//                Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
//            });

//            // Cleanup existing recurring jobs
//            RecurringJob.RemoveIfExists("updateAnalyticsForExternalCommitments");
//            RecurringJob.RemoveIfExists("upsertCapacityAnalysisDaily");
//            RecurringJob.RemoveIfExists("UpdateCapacityAnalysisDailyForChangeinCaseAttribute");
//            RecurringJob.RemoveIfExists("upsertCapacityAnalysisMonthly");
//            RecurringJob.RemoveIfExists("CorrectAnalyticsData");

//            // Job Scheduling
//            RecurringJob.AddOrUpdate<IAnalyticsService>("updateAnalyticsForExternalCommitments",
//               service => service.UpdateAnlayticsDataForUpsertedExternalCommitment(null), "0 */6 * * *"); // Every 6 Hours
//            RecurringJob.AddOrUpdate<IAnalyticsService>("upsertCapacityAnalysisDaily",
//                service => service.UpsertCapacityAnalysisDaily(false, null), "*/5 * * * *"); // Every saturday
//            RecurringJob.AddOrUpdate<IAnalyticsService>("UpdateCapacityAnalysisDailyForChangeinCaseAttribute",
//                service => service.UpdateCapacityAnalysisDailyForChangeInCaseAttribute(null), Cron.Daily);
//            RecurringJob.AddOrUpdate<IAnalyticsService>("upsertCapacityAnalysisMonthly",
//                service => service.UpsertCapacityAnalysisMonthly(false), "*/20 * * * *"); // Every 20 minutes
//            RecurringJob.AddOrUpdate<IAnalyticsService>("CorrectAnalyticsData",
//               service => service.CorrectAnalyticsData(), "0 0 * * *"); // At midnight
//#endif
//            app.UseRouting();
//            app.UseCors("CorsPolicy");
//            app.UseAuthentication();
//            app.UseAuthorization();
//            app.UseCustomExceptionHandler();
//            app.UseMiddleware<ResponseTimeMiddleware>();
//            app.UseHttpsRedirection();
//            app.UseEndpoints(endpoints =>
//            {
//                endpoints.MapControllers();
//            });
//        }
    }
}
