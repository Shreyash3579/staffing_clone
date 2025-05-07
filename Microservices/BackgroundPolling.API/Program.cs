
using BackgroundPolling.API.Extensions;
using BackgroundPolling.API.Models;
using Microservices.Common.Core.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microservices.Common.Core.HashiCorpVault;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.RegisterAppServices();
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var appconfig = builder.Configuration;
//var vaultAccessConfigs = appconfig.GetSection("VaultAccessConfigs").Get<VaultFluent.VaultObject>();

//appconfig.AddVaultConfigurationsAsync(vaultAccessConfigs).Wait();


builder.Services.Configure<AppSettingsConfiguration>(appconfig);
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

app.UseSwagger(options =>
    options.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        var basePath = "backgroundPollingApi";
#if !DEBUG
        swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"/{basePath}" } };
#endif
    }));
app.UseSwaggerUI(c =>
{

#if DEBUG
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Background Polling API V1");
#else
    c.SwaggerEndpoint("/backgroundPollingApi/swagger/v1/swagger.json", "Background Polling API V1");
#endif

});

#region hangfire
#if (!DEBUG)
            app.ConfigureHangfireMiddleware();
#endif
#endregion

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseCustomExceptionHandler();
app.UseMiddleware<ResponseTimeMiddleware>();
app.UseHttpsRedirection();


app.Run();
