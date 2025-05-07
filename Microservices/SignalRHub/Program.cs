using Microservices.Common.Core.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Serialization;
using SignalRHub.API.Extensions;
using SignalRHub.Core.Helpers;
using SignalRHub.Extensions;
using SignalRHub.HubConfig;
using System;
using System.Linq;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Azure.Identity;
using Microsoft.Azure.SignalR;
using Microservices.Common.Core.HashiCorpVault;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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

builder.Services.AddSignalR().AddAzureSignalR(option => {
    option.Endpoints = new ServiceEndpoint[] {
        new ServiceEndpoint(new Uri(ConfigurationUtility.GetValue("SignalRServiceEndpoint")), new ManagedIdentityCredential())
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin",
        builder => builder
        .WithOrigins("http://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .SetIsOriginAllowed((hosts) => true));
});

var appconfig = builder.Configuration;

//var vaultAccessConfigs = appconfig.GetSection("VaultAccessConfigs").Get<VaultFluent.VaultObject>();
//appconfig.AddVaultConfigurationsAsync(vaultAccessConfigs).Wait();

var app = builder.Build();

var env = app.Services.GetRequiredService<IWebHostEnvironment>();

if (env.ApplicationName != Environments.Production)
{
    app.UseSwagger(options =>
    //Workaround to use the Swagger UI "Try Out" functionality when deployed behind Aplication Gateway with API prefix /sub context configured
    options.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        var basePath = "signalr";
#if !DEBUG
        swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"/{basePath}" } };
#endif
    }));

    app.UseSwaggerUI(c =>
    {
#if DEBUG
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SignalR API V1");
#else
        c.SwaggerEndpoint("/signalrAPI/swagger/v1/swagger.json", "SignalR API V1");
#endif
    });
}


app.UseCors("AllowOrigin");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseCustomExceptionHandler();
app.UseMiddleware<ResponseTimeMiddleware>();
app.UseHttpsRedirection();


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<StaffingHub>("/offers");
});
app.Run();
