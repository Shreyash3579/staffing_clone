using Hangfire;
using Microservices.Common.Core.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.NewtonsoftJson;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Serialization;
using Staffing.API.Core.Helpers;
using Staffing.API.Extensions;
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
builder.Services.AddHangfireServer();
builder.Services.AddApplicationInsightsTelemetry();

var appconfig = builder.Configuration;

//var vaultAccessConfigs = appconfig.GetSection("VaultAccessConfigs").Get<VaultFluent.VaultObject>();
//appconfig.AddVaultConfigurationsAsync(vaultAccessConfigs).Wait();

var app = builder.Build();

var env = app.Services.GetRequiredService<IWebHostEnvironment>();
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

if (env.ApplicationName != Environments.Production)
{
    app.UseSwagger(options =>
    options.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        var basePath = "staffingApi";
#if !DEBUG
        swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"/{basePath}" } };
#endif
    }));
    app.UseSwaggerUI(c =>
    {
#if DEBUG
        foreach (var description in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
#else
        foreach (var description in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"/staffingApi/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }  
#endif
    });
};
//app.UseHangfireServer();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")))
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireDashboardAuthorizationFilter() },
        PrefixPath = "/staffingApi"
    });
}
else
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
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
