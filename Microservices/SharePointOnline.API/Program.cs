using SharePointOnline.API.Extensions;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microservices.Common.Core.HashiCorpVault;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();//.AddNewtonsoftJson();
builder.Services.RegisterAppServices();

builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var appconfig = builder.Configuration;

//var vaultAccessConfigs = appconfig.GetSection("VaultAccessConfigs").Get<VaultFluent.VaultObject>();
//appconfig.AddVaultConfigurationsAsync(vaultAccessConfigs).Wait();

//builder.Services.AddApplicationInsightsTelemetry(appconfig["APPINSIGHTS_CONNECTIONSTRING"]);

var app = builder.Build();

var env = app.Services.GetRequiredService<IWebHostEnvironment>();
if (env.ApplicationName != Environments.Production)
{
    app.UseSwagger(options =>
    options.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        var basePath = "sharePointOnlineApi";
#if !DEBUG
        swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"/{basePath}" } };
#endif
    }));
    app.UseSwaggerUI(c =>
    {
        #if DEBUG
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SharePoint Online API V1");
        #else
                c.SwaggerEndpoint("/sharePointOnlineApi/swagger/v1/swagger.json", "SharePoint Online V1");
        #endif
    });
}

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseCustomExceptionHandler();
//app.UseMiddleware<ResponseTimeMiddleware>();
app.UseHttpsRedirection();
app.Run();
