using Microservices.Common.Core.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Staffing.Coveo.API.Extensions;
using System;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microservices.Common.Core.HashiCorpVault;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddHttpServices().RegisterAppServices();

builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var appconfig = builder.Configuration;
builder.Services.AddApplicationInsightsTelemetry(appconfig["APPINSIGHTS_CONNECTIONSTRING"]);

//var vaultAccessConfigs = appconfig.GetSection("VaultAccessConfigs").Get<VaultFluent.VaultObject>();
//appconfig.AddVaultConfigurationsAsync(vaultAccessConfigs).Wait();

var app = builder.Build();

var env = app.Services.GetRequiredService<IWebHostEnvironment>();
if (env.ApplicationName != Environments.Production)
{
    app.UseSwagger(options =>
    options.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        var basePath = "coveoApi";
#if !DEBUG
        swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"/{basePath}" } };
#endif
    }));
    app.UseSwaggerUI(c =>
    {
#if DEBUG
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Coveo API V1");
#else
        c.SwaggerEndpoint("/coveoApi/swagger/v1/swagger.json", "Coveo API V1");
#endif
    });
}


app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseCustomExceptionHandler();
app.UseMiddleware<ResponseTimeMiddleware>();
app.UseHttpsRedirection();
app.Run();

//namespace Staffing.Coveo.API
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
