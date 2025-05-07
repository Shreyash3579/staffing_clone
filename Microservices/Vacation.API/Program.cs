using Microservices.Common.Core.HashiCorpVault;
using Microservices.Common.Core.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using Vacation.API.Extensions;

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
    app.UseDeveloperExceptionPage();
    app.UseSwagger(options =>
    options.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        var basePath = "vacationApi";
#if !DEBUG
        swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"/{basePath}" } };
#endif
    }));
    app.UseSwaggerUI(c =>
    {
#if DEBUG
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vacation API V1");
#else
        c.SwaggerEndpoint("/vacationApi/swagger/v1/swagger.json", "Vacation API V1");
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


//namespace Vacation.API
//{
//    [ExcludeFromCodeCoverage]
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            CreateWebHostBuilder(args).Build().Run();
//        }

//        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
//            WebHost.CreateDefaultBuilder(args)
//                .UseApplicationInsights()
//                .UseStartup<Startup>();
//    }
//}
