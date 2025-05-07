//using Microservices.Common.Core.Helpers;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Newtonsoft.Json.Converters;
//using Staffing.Authentication.Extensions;
//using Staffing.Authentication.Models;
//using System.Diagnostics.CodeAnalysis;

namespace Staffing.Authentication
{
    public class Startup
    {
//        [ExcludeFromCodeCoverage]
//        public Startup(IConfiguration configuration)
//        {
//            Configuration = configuration;
//        }

//        public IConfiguration Configuration { get; }

//        // This method gets called by the runtime. Use this method to add services to the container.
//        public void ConfigureServices(IServiceCollection services)
//        {
//            services.Configure<CookiePolicyOptions>(options =>
//            {
//                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
//                options.CheckConsentNeeded = context => true;
//                options.MinimumSameSitePolicy = SameSiteMode.None;
//            });
//            services.AddControllers().AddNewtonsoftJson(options => 
//            options.SerializerSettings.Converters.Add(new StringEnumConverter()));
//            services.Configure<AppSettingsConfiguration>(Configuration);
//            services.RegisterAppServices(Configuration);
//        }

//        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//        {
//            app.UseMiddleware<ResponseTimeMiddleware>();
//            if (env.ApplicationName == Environments.Development) app.UseDeveloperExceptionPage();

//            app.UseSwagger();
//            app.UseSwaggerUI(c =>
//            {
//#if DEBUG
//                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Authentication API V1");
//#else
//                    c.SwaggerEndpoint("/AuthenticationApi/swagger/v1/swagger.json", "Authentication API V1");
//#endif
//            });
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