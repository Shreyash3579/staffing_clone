using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Values;
using OcelotAPIGateway.Helpers;
using System;
using System.IO.Compression;
using System.Linq;
using Microservices.Common.Core.Helpers;
using Ocelot.Cache.CacheManager;
using Ocelot.Middleware;

//var builder = WebApplication.CreateBuilder(args);
//var appconfig = builder.Configuration;
//builder.Services.AddOcelot(appconfig)
//               .AddDelegatingHandler<GetToPostHandler>()
//               .AddCacheManager(x => { x.WithDictionaryHandle(); });

//builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
//{
//    builder.AllowAnyOrigin()
//        .AllowAnyMethod()
//        .AllowAnyHeader()
//        .SetPreflightMaxAge(TimeSpan.FromSeconds(7200));
//}));

//builder.Services.Configure<GzipCompressionProviderOptions>(options =>
//{
//    options.Level = CompressionLevel.Optimal;
//});
//builder.Services.AddResponseCompression(options =>
//{
//    options.EnableForHttps = true;
//    options.Providers.Add<GzipCompressionProvider>();
//    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] {
//                    "ap­pli­ca­tion/json"
//                });
//});
//IWebHostBuilder HostBuilder = new WebHostBuilder();
//HostBuilder.ConfigureAppConfiguration((host, config) =>
// {
//     config
//         .SetBasePath(host.HostingEnvironment.ContentRootPath)
//#if DEBUG
//           .AddOcelot(host.HostingEnvironment)
//#else
//                         .AddOcelot($"{host.HostingEnvironment.EnvironmentName}",host.HostingEnvironment)
//#endif
//          .AddEnvironmentVariables();

// });
//builder.Services.AddMemoryCache();
//builder.Services.AddEndpointsApiExplorer();
//var app = builder.Build();
////var host = HostBuilder.Build();
//app.UseCors("CorsPolicy");
//app.UseMiddleware<ResponseTimeMiddleware>();
//app.UseHttpsRedirection();
//app.UseResponseCompression();
//app.UseOcelot();
//app.Run();

namespace OcelotAPIGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args);

            builder
               .ConfigureAppConfiguration((host, config) =>
               {
                   config
                       .SetBasePath(host.HostingEnvironment.ContentRootPath)
#if DEBUG
                         .AddOcelot(host.HostingEnvironment)
#else
                         .AddOcelot($"{host.HostingEnvironment.EnvironmentName}",host.HostingEnvironment)
#endif
                        .AddEnvironmentVariables();

               })
               .UseStartup<Startup>();

            var host = builder.Build();
            return host;
        }
    }
}