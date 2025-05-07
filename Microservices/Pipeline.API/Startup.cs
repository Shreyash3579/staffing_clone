//using Microservices.Common.Core.Helpers;
//using Microsoft.AspNet.OData.Extensions;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc.Formatters;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Net.Http.Headers;
//using Newtonsoft.Json.Serialization;
//using Pipeline.API.Extensions;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;

namespace Pipeline.API
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
        //            services.AddControllers()
        //              .AddNewtonsoftJson(options =>
        //              // HACK: OData return response in Pascal Case, hence resolving contract to return data in camel case
        //              options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver());
        //            services.AddOData();

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

        //            services.RegisterAppServices();

        //        }

        //        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        //        {
        //            if (env.ApplicationName == Environments.Development) app.UseDeveloperExceptionPage();
        //            if (env.ApplicationName != Environments.Production)
        //            {
        //                app.UseSwagger();
        //                app.UseSwaggerUI(c =>
        //                {
        //#if DEBUG
        //                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pipeline API V1");
        //#else
        //                    c.SwaggerEndpoint("/pipelineApi/swagger/v1/swagger.json", "Pipeline API V1");
        //#endif
        //                });
        //            }
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
        //                endpoints.EnableDependencyInjection();
        //                endpoints.Select().Count().Filter().Expand().OrderBy().MaxTop(null);
        //            });
        //        }
    }
}
