using Logger.API.Extensions;
using Microservices.Common;
using Microservices.Common.Core.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Security.Principal;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Options;


var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.RegisterAppServices();
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
configurationBuilder.Build();

var appconfig = builder.Configuration;
var connectionString = appconfig["ConnectionStrings:StaffingSerilogDatabase"].Decrypt();
var app = builder.Build();
var env = app.Services.GetRequiredService<IWebHostEnvironment>();

var columnOption = new ColumnOptions
{

    AdditionalDataColumns = new Collection<DataColumn>
            {
                new DataColumn {DataType = typeof (string), ColumnName = "EmployeeCode", DefaultValue = WindowsIdentity.GetCurrent().Name},
                new DataColumn {DataType = typeof (string), ColumnName = "ApplicationName", DefaultValue = "Logger.API"},
            }
};
columnOption.Store.Remove(StandardColumn.LogEvent);
Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Error()//To specify the minimum level of events that have to be logged by Serilog.
           .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
           .WriteTo.MSSqlServer(connectionString, appconfig["Serilog:TableName"], autoCreateSqlTable: true, columnOptions: columnOption)
           .Enrich.FromLogContext()
           //Following line is used in order to write errors into a file. This will create the file and log exceptions into it.
           //.WriteTo.File("LogFile.txt", rollingInterval: RollingInterval.Day,
           //outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
           .CreateBootstrapLogger();
if (env.ApplicationName != Environments.Production)
{

    app.UseSwagger(options =>
    options.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        var basePath = "loggerApi";
#if !DEBUG
        swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"/{basePath}" } };
#endif
    }));
    app.UseSwaggerUI(c =>
    {
#if DEBUG
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Logger API V1");
#else
        c.SwaggerEndpoint("/loggerApi/swagger/v1/swagger.json", "Logger API V1");
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
//namespace Logger.API
//{

//    public class Program
//    {
//        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
//        .SetBasePath(Directory.GetCurrentDirectory())
//        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//        .Build();
//        public static int Main(string[] args)
//        {
//            var connectionString = Configuration["ConnectionStrings:StaffingSerilogDatabase"].Decrypt();

//            var columnOption = new ColumnOptions
//            {
//                AdditionalDataColumns = new Collection<DataColumn>
//            {
//                new DataColumn {DataType = typeof (string), ColumnName = "EmployeeCode", DefaultValue = WindowsIdentity.GetCurrent().Name},
//                new DataColumn {DataType = typeof (string), ColumnName = "ApplicationName", DefaultValue = "Logger.API"},
//            }
//            };
//            columnOption.Store.Remove(StandardColumn.LogEvent);

//            Log.Logger = new LoggerConfiguration()
//            //.ReadFrom.Configuration(Configuration) //In order to read configurations from Configuration (appsettings.json)
//            .MinimumLevel.Error()//To specify the minimum level of events that have to be logged by Serilog.
//            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
//            .WriteTo.MSSqlServer(connectionString, Configuration["Serilog:TableName"], autoCreateSqlTable: true, columnOptions: columnOption)
//            .Enrich.FromLogContext()
//            //Following line is used in order to write errors into a file. This will create the file and log exceptions into it.
//            //.WriteTo.File("LogFile.txt", rollingInterval: RollingInterval.Day,
//            //outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
//            .CreateLogger();

//            /*
//            //If ever the serilog is not logging exceptions, create the txt file in the path provided below.
//            //Then uncomment the following block of code.
//            //It will provide the log of exceptions faced by Serilog. 
//            //For example, the insert query permissions is blocking Serilog to write exceptions into the database.
//           StreamWriter file = File.CreateText("C:\\Users\\45088\\Desktop\\Serilog.txt");
//           Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
//           */
//            try
//            {
//                //Log.Error("Starting host");
//                //CreateWebHostBuilder(args).Build().Run();
//                BuildWebHost(args).Run();
//                return 0;
//            }
//            catch (Exception ex)
//            {
//                Log.Fatal(ex, "Host terminated unexpectedly");
//                return 1;
//            }
//            finally
//            {
//                Log.CloseAndFlush();
//            }
//        }
//        public static IWebHost BuildWebHost(string[] args) =>
//        WebHost.CreateDefaultBuilder(args)
//                .UseApplicationInsights()
//            .UseStartup<Startup>()
//            .UseConfiguration(Configuration)
//            .UseSerilog()
//            .Build();
//    }
//}
