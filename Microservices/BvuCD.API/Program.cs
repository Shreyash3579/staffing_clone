using BvuCD.API.Extensions;
using Microservices.Common.Core.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microservices.Common.Core.HashiCorpVault;
using Microsoft.Extensions.Configuration;

using Microsoft.Identity.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.RegisterAppServices();
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var appconfig = builder.Configuration;
//var vaultAccessConfigs = appconfig.GetSection("VaultAccessConfigs").Get<VaultFluent.VaultObject>();

//appconfig.AddVaultConfigurationsAsync(vaultAccessConfigs).Wait();


var app = builder.Build();

var env = app.Services.GetRequiredService<IWebHostEnvironment>();
if (env.ApplicationName != Environments.Production)
{
    app.UseSwagger(options =>
    options.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        var basePath = "BvuCDApi";
#if !DEBUG
        swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"/{basePath}" } };
#endif
    }));
    app.UseSwaggerUI(c =>
    {
#if DEBUG
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BvuCD API V1");
#else
        c.SwaggerEndpoint("/BvuCDApi/swagger/v1/swagger.json", "BvuCD API V1");
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

//namespace BvuCD.API
//{
//    [ExcludeFromCodeCoverage]
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            CreateWebHostBuilder(args).Build().Run();
//        }

//        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
//        {
//            return WebHost.CreateDefaultBuilder(args)
//                .UseApplicationInsights()
//                .UseStartup<Startup>();
//        }
//    }
//}