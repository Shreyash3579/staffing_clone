using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Staffing.HttpAggregator.Contracts;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Helpers;
using Staffing.HttpAggregator.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Reflection;
using System.Text;


namespace Staffing.HttpAggregator.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddHttpServices(this IServiceCollection services)
        {
            //register delegating handlers
            services.AddTransient<HttpClientAuthorizationDelegatingHandler>();
            services.AddHttpContextAccessor();

            //register http services
            services.AddHttpClient<IPipelineApiClient, PipelineApiClient>()
            .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
             .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
                   retryCount: 2,
                   sleepDurationProvider: retryAttempt =>
                       TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
               ));
            services.AddHttpClient<IStaffingApiClient, StaffingApiClient>()
               .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
               .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
                   retryCount: 2,
                   sleepDurationProvider: retryAttempt =>
                       TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) 
               ));


            services.AddHttpClient<IResourceApiClient, ResourceApiClient>()
                 .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
                   retryCount: 2,
                   sleepDurationProvider: retryAttempt =>
                       TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
               ));

            services.AddHttpClient<ICCMApiClient, CCMApiClient>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
                  .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
                   retryCount: 2,
                   sleepDurationProvider: retryAttempt =>
                       TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) 
               ));

            services.AddHttpClient<IVacationApiClient, VacationApiClient>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
                  .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
                   retryCount: 2,
                   sleepDurationProvider: retryAttempt =>
                       TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
               ));

            services.AddHttpClient<IBvuCDApiClient, BvuCDApiClient>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
                   retryCount: 2,
                   sleepDurationProvider: retryAttempt =>
                       TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
               ));

            services.AddHttpClient<IHcpdApiClient, HcpdApiClient>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
                   retryCount: 2,
                   sleepDurationProvider: retryAttempt =>
                       TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
               ));

            services.AddHttpClient<IBasisApiClient, BasisApiClient>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
                 .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
                   retryCount: 2,
                   sleepDurationProvider: retryAttempt =>
                       TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
               ));

            services.AddHttpClient<IRevenueApiClient, RevenueApiClient>()
                 .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
                   retryCount: 2,
                   sleepDurationProvider: retryAttempt =>
                       TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
               ));

            services.AddHttpClient<IIrisApiClient, IrisApiClient>()
                 .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
                       retryCount: 3,
                       sleepDurationProvider: retryAttempt =>
                           TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
               ));

            services.AddHttpClient<IAzureServiceBusApiClient, AzureServiceBusApiClient>()
                    .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
                   .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
                       retryCount: 3,
                       sleepDurationProvider: retryAttempt =>
                           TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
               ));

            services.AddHttpClient<IAzureSearchApiClient, AzureSearchApiClient>()
                    .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
                    .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
                       retryCount: 3,
                       sleepDurationProvider: retryAttempt =>
                           TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
               ));

            services.AddHttpClient<ISignalRHubClient, SignalRHubClient>()
                    .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
                    .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
                       retryCount: 3,
                       sleepDurationProvider: retryAttempt =>
                           TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
               ));

            return services;
        }

        public static IServiceCollection RegisterAppServices(this IServiceCollection services)
        {
            services.AddScoped<IOpportunityService, OpportunityService>();
            services.AddScoped<ICaseService, CaseService>();
            services.AddScoped<IAuditTrailService, AuditTrailService>();
            services.AddScoped<IResourceService, ResourceService>();
            services.AddScoped<IStaffingService, StaffingService>();
            services.AddScoped<IProjectAggregatorService, ProjectAggregatorService>();
            services.AddScoped<IResourceAllocationService, ResourceAllocationService>();
            services.AddScoped<IRevenueService, RevenueService>();
            services.AddScoped<IResourcePlaceholderAllocationService, ResourcePlaceholderAllocationService>();
            services.AddScoped<INoteLogService, NoteLogService>();
            services.AddScoped<IEmployeeStaffingPreferenceService, EmployeeStaffingPreferenceService>();
            services.AddScoped<ICasePlanningMetricsService, CasePlanningMetricsService>();
            services.AddScoped<IAggregationService, AggregationService>();
            services.AddScoped<IRingfenceManagementService, RingfenceManagementService>();
            services.AddScoped<IExpertEmailUtilityService, ExpertEmailUtilityService>();
            services.AddScoped<IPlanningCardService, PlanningCardService>();
            services.AddScoped<IUserPreferenceGroupService, UserPreferenceGroupService>();
            services.AddScoped<ICommonResourcesService, CommonResourcesService>();
            services.AddScoped<IPreponedCasesAllocationsAuditService, PreponedCasesAllocationsAuditService>();
            services.AddScoped<ISearchService, SearchService>();

            #region Swagger
            services.AddSwaggerGen(options =>
            {
                // The generated Swagger JSON file will have these properties.
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Staffing Http Aggregator API",
                    Version = "v1",
                    Description = "Aggregates http response"
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

            return services;
        }
    }
}
