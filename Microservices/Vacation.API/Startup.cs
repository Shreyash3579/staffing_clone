using Microservices.Common.Core.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;
using Vacation.API.Extensions;

namespace Vacation.API
{
    // [ExcludeFromCodeCoverage]
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

        //        }

        //        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        //        {
        //            if (env.ApplicationName == Environments.Development) app.UseDeveloperExceptionPage();
        //            if (env.ApplicationName != Environments.Production)
        //            {
        //                app.UseDeveloperExceptionPage();
        //                app.UseSwagger();
        //                app.UseSwaggerUI(c =>
        //                {
        //#if DEBUG
        //                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vacation API V1");
        //#else
        //                    c.SwaggerEndpoint("/vacationApi/swagger/v1/swagger.json", "Vacation API V1");
        //#endif
        //                });
        //            }
        //            app.UseRouting();
        //            app.UseCors("CorsPolicy");
        //            app.UseAuthentication();
        //            app.UseAuthorization();
        //            app.UseMiddleware<ResponseTimeMiddleware>();
        //            app.UseCustomExceptionHandler();
        //            app.UseHttpsRedirection();
        //            app.UseEndpoints(endpoints =>
        //            {
        //                endpoints.MapControllers();
        //            });
        //        }
    }
}
