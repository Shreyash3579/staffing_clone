﻿//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using SignalRHub.Extensions;
//using SignalRHub.HubConfig;

//namespace SignalRHub
//{
//    public class Startup
//    {
//        public Startup(IConfiguration configuration)
//        {
//            Configuration = configuration;
//        }

//        public IConfiguration Configuration { get; }

//        This method gets called by the runtime.Use this method to add services to the container.
//        public void ConfigureServices(IServiceCollection services)
//        {
//            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
//            services.RegisterAppServices();
//        }

//        This method gets called by the runtime.Use this method to configure the HTTP request pipeline.
//        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
//        {
//            if (env.IsDevelopment())
//            {
//                app.UseDeveloperExceptionPage();
//            }
//            else
//            {
//                app.UseHsts();
//            }

//            app.UseHttpsRedirection();
//            app.UseCors("CorsPolicy");
//            app.UseSignalR(routes =>
//            {
//                routes.MapHub<StaffingHub>("/staffinghub");
//            });
//            app.UseMvc();
//        }
//    }
//}
