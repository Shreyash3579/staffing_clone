//using Microservices.Common.Core.Helpers;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Staffing.Coveo.API.Extensions;

namespace Staffing.Coveo.API
{
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
//            services.AddHttpServices().RegisterAppServices();
//            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
//        }

//        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//        {
//            // Host swagger generated API specification only on Development server
//            if (env.ApplicationName == Environments.Development) app.UseDeveloperExceptionPage();
//            if (env.ApplicationName != Environments.Production)
//            {
//                app.UseSwagger();
//                app.UseSwaggerUI(c =>
//                {
//#if DEBUG
//                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Coveo API V1");
//#else
//                    c.SwaggerEndpoint("/staffingCoveoApi/swagger/v1/swagger.json", "Coveo API V1");
//#endif
//                });
//            }
//            app.UseRouting();
//            app.UseAuthentication();
//            app.UseAuthorization();
//            app.UseCustomExceptionHandler();
//            app.UseMiddleware<ResponseTimeMiddleware>();
//            app.UseCors("CorsPolicy");
//            app.UseHttpsRedirection();
//            app.UseEndpoints(endpoints =>
//            {
//                endpoints.MapControllers();
//            });
//        }
    }
}
