//using Hangfire;
//using Microservices.Common.Core.Helpers;
//using Microsoft.AspNetCore.OData.Extensions;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc.ApiExplorer;
//using Microsoft.AspNetCore.Mvc.Formatters;
//using Microsoft.AspNetCore.OData;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Net.Http.Headers;
//using Newtonsoft.Json.Serialization;
//using Staffing.API.Core.Helpers;
//using Staffing.API.Extensions;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;

namespace Staffing.API
{
    //[ExcludeFromCodeCoverage]
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

//            services.AddControllers().AddNewtonsoftJson(options =>
//               // HACK: OData return response in Pascal Case, hence resolving contract to return data in camel case
//               options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver())
//                .AddOData(options => options.Select().Filter().Expand().OrderBy().SetMaxTop(null));
//            // services.AddOData(); COMMENTED ON 30-AUG-2022 DUE TO ONLY ONE LINE OF CODE REQUIRED OF ODATA IN .NET 6.0
//            services.RegisterAppServices();

//            //HACK: To generate Swagger Documentation when using OData otherwise swagger throws error
//            services.AddMvcCore(options =>
//            {
//                foreach (var outputFormatter in options.OutputFormatters.OfType<OutputFormatter>().Where(x => x.SupportedMediaTypes.Count == 0))
//                {
//                    outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
//                }

//                foreach (var inputFormatter in options.InputFormatters.OfType<InputFormatter>().Where(x => x.SupportedMediaTypes.Count == 0))
//                {
//                    inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
//                }
//            });
//        }

//        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
//        {
//            // Host swagger generated API specification only on server have Environment variable (ASPNETCORE_ENVIRONMENT) set to DevEnv
//            if (env.EnvironmentName == Environments.Development) app.UseDeveloperExceptionPage();

//            if (env.EnvironmentName != Environments.Production)
//            {
//                app.UseSwagger();
//                app.UseSwaggerUI(c =>
//                {
//#if DEBUG
//                    foreach (var description in provider.ApiVersionDescriptions)
//                    {
//                        c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
//                    }                    
//#else
//                    foreach (var description in provider.ApiVersionDescriptions)
//                    {
//                        c.SwaggerEndpoint($"/staffingApi/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
//                    }  
//#endif
//                });
//            }

//            app.UseHangfireServer();
//            app.UseHangfireDashboard("/hangfire", new DashboardOptions
//            {
//                Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
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
//                //endpoints.EnableDependencyInjection(); COMMENTED ON 30-AUG-2022 DUE TO ONLY ONE LINE OF CODE REQUIRED OF ODATA IN .NET 6.0
//                //endpoints.Select().Count().Filter().Expand().OrderBy().MaxTop(null); COMMENTED ON 30-AUG-2022 DEFINED ABOVE WITH SERVICES
//            });
//        }
    }
}