using Microsoft.Extensions.DependencyInjection;
using Staffing.Analytics.API.Contracts.Helpers;
using Staffing.Analytics.API.Core.Helpers;
using System;
using System.Collections.Generic;
using Microservices.Common;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Staffing.Analytics.API.Contracts.RepositoryInterfaces;
using Staffing.Analytics.API.Models;
using Staffing.Analytics.API.Core.Repository;
using Hangfire;
using Staffing.Analytics.API.Contracts.Services;
using Staffing.Analytics.API.Core.AnalyticsService;
using Staffing.Analytics.API.Core.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;

namespace Staffing.Analytics.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection RegisterAppServices(this IServiceCollection services)
        {
            #region Dapper
            services.AddScoped<IDapperContext, DapperContext>();
            #endregion

            #region CORS
            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(7200));
            }));

            #endregion

            #region Hangfire

            var connectionString = ConfigurationUtility.GetValue("ConnectionStrings:HangfireDatabase").Decrypt();

            var sqlServerStorageOptions = new SqlServerStorageOptions
            {
                PrepareSchemaIfNecessary = false,
                SchemaName = "HangfireAnalytics",
            };
            services.AddHangfire(x => x.UseSqlServerStorage(connectionString, sqlServerStorageOptions).UseFilter(new LogFailureAttribute()));

            #endregion

            #region JWT Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidateAudience = true,
                        ValidateIssuer = true,

                        ValidIssuer = "Staffing Authentication API",
                        ValidAudience = "APIs accessed by Staffing App",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                            .GetBytes(Environment.GetEnvironmentVariable("staffing_secretKey")))
                    };
                });

            services.AddAuthorization(options =>
            {
                //options.AddPolicy("StaffingUserOnly", policy => policy.RequireClaim("EmployeeCode"));
                options.AddPolicy(Constants.Policy.StaffingUserOnly, policy => policy.RequireAssertion(context =>
                {
                    return context.User.IsInRole(Constants.Role.BackgroundPollingApiAccess) ||
                           context.User.HasClaim(c => c.Type == "EmployeeCode");
                }));
                options.AddPolicy(Constants.Policy.StaffingAnalyticsRead, policy => policy.RequireAssertion(context =>
                {
                    return context.User.IsInRole(Constants.Role.StaffingAnalyticsReadAccess) ||
                           context.User.HasClaim(c => c.Type == "EmployeeCode");
                }));
            });
            #endregion

            #region BaseRepository

            services.AddScoped<IBaseRepository<Office>, BaseRepository<Office>>();
            services.AddScoped<IBaseRepository<ResourceAllocation>, BaseRepository<ResourceAllocation>>();
            services.AddScoped<IBaseRepository<Commitment>, BaseRepository<Commitment>>();
            services.AddScoped<IBaseRepository<string>, BaseRepository<string>>();
            services.AddScoped<IBaseRepository<ResourceAvailability>, BaseRepository<ResourceAvailability>>();
            services.AddScoped<IBaseRepository<AvailabilityMetrics>, BaseRepository<AvailabilityMetrics>>();
            services.AddScoped<IBaseRepository<AvailabilityMetricsViewModel>, BaseRepository<AvailabilityMetricsViewModel>>();

            #endregion

            #region Repository

            services.AddScoped<IResourceAllocationRepository, ResourceAllocationRepository>();
            services.AddScoped<IResourceAvailabilityRepository, ResourceAvailabilityRepository>();
            services.AddScoped<IBackgroundJobClient, BackgroundJobClient>();
            services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
            services.AddScoped<IPlaceholderAllocationRepository, PlaceholderAllocationRepository>();
            services.AddScoped<ICasePlanningMetricsRepository, CasePlanningMetricsRepository>();
            services.AddScoped<ICasePlanningMetricsPlaygroundRepository, CasePlanningMetricsPlaygroundRepository>();
            services.AddScoped<IPollMasterRepository, PollMasterRepository>();


            #endregion

            #region Services
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            services.AddScoped<IPlaceholderAnalyticsService, PlaceholderAnalyticsService>();
            services.AddScoped<ICasePlanningMetricsService, CasePlanningMetricsService>();
            services.AddScoped<ICasePlanningMetricsPlaygroundService, CasePlanningMetricsPlaygroundService>();

            // API Clients            
            services.AddTransient<HttpClientAuthorizationDelegatingHandler>();
            services.AddHttpContextAccessor();

            services.AddHttpClient<IResourceApiClient, ResourceApiClient>();

            services.AddHttpClient<ICCMApiClient, CCMApiClient>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

            services.AddHttpClient<IPipelineApiClient, PipelineApiClient>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

            services.AddHttpClient<IStaffingApiClient, StaffingApiClient>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

            services.AddHttpClient<IBasisApiClient, BasisApiClient>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

            services.AddHttpClient<IBvuApiClient, BvuApiClient>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

            services.AddHttpClient<IVacationApiClient, VacationApiClient>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

            services.AddHttpClient<IAuthenticationApiClient, AuthenticationApiClient>();

            #endregion

            #region Swagger
            services.AddSwaggerGen(options =>
            {
                // The generated Swagger JSON file will have these properties.
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Staffing Analytics API",
                    Version = "v1",
                    Description = "API to insert analytics data for employee staffing"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                     }
                  });

                //Locate the XML file being generated by ASP.NET
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.XML";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                //Tell swagger to use XML comments
                options.IncludeXmlComments(xmlPath);
            });
            #endregion

            return services;
        }
    }
}
