using CCM.API.Extensions;
using Microservices.Common.Core.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.NewtonsoftJson;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microservices.Common.Core.HashiCorpVault;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson(options =>
               // HACK: OData return response in Pascal Case, hence resolving contract to return data in camel case
               options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver())
                .AddOData(options => options.Select().Filter().Expand().OrderBy().SetMaxTop(null))
                .AddODataNewtonsoftJson();

builder.Services.RegisterAppServices();
//HACK: To generate Swagger Documentation when using OData otherwise swagger throws error
builder.Services.AddMvcCore(options =>
{
    foreach (var outputFormatter in options.OutputFormatters.OfType<OutputFormatter>().Where(x => x.SupportedMediaTypes.Count == 0))
    {
        outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
    }

    foreach (var inputFormatter in options.InputFormatters.OfType<InputFormatter>().Where(x => x.SupportedMediaTypes.Count == 0))
    {
        inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
    }
});
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var appconfig = builder.Configuration;
//var vaultAccessConfigs = appconfig.GetSection("VaultAccessConfigs").Get<VaultFluent.VaultObject>();

//appconfig.AddVaultConfigurationsAsync(vaultAccessConfigs).Wait();

builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

var env = app.Services.GetRequiredService<IWebHostEnvironment>();
if (env.ApplicationName != Environments.Production)
{
    app.UseSwagger(options =>
    options.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        var basePath = "ccmApi";
#if !DEBUG
        swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"/{basePath}" } };
#endif
    }));
    app.UseSwaggerUI(c =>
    {
#if DEBUG
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CCM API V1");
#else
        c.SwaggerEndpoint("/ccmApi/swagger/v1/swagger.json", "CCM API V1");
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


//namespace CCM.API
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            CreateWebHostBuilder(args).Build().Run();
//        }

//        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
//            WebHost.CreateDefaultBuilder(args)
//                .UseStartup<Startup>();
//    }
//}
