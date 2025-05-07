using Hangfire;
using Hangfire.SqlServer;
using Microservices.Common;
using Microservices.Common.Core.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Staffing.API.Contracts.Helpers;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Core.Helpers;
using Staffing.API.Core.Repository;
using Staffing.API.Core.Services;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Staffing.API.Extensions
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
                SchemaName = "HangfireStf",
            };
            services.AddHangfire(x => x.UseSqlServerStorage(connectionString, sqlServerStorageOptions).UseFilter(new LogFailureAttribute()));

            #endregion

            #region JWT Authentication
            var signingKeys = new List<SecurityKey>
            {
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("staffing_secretKey")))
                //removed old secret key since users have started using new token
            };

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
                        //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                        //    .GetBytes(Environment.GetEnvironmentVariable("staffing_secretKey"))),
                        IssuerSigningKeys = signingKeys, //this is done to support backward compatability for external teams using this API. After dotnet 8 upgrade secret key was updated to support min size requiremets
                        IssuerSigningKeyResolver = (tokenString, securityToken, identifier, parameters) =>
                        {
                            return JWTHelper.GetIssuerSigningKeyResolverForMinSecretKeySize(tokenString, securityToken, identifier, parameters);
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                //options.AddPolicy("StaffingUserOnly", policy => policy.RequireClaim("EmployeeCode"));
                options.AddPolicy(Constants.Policy.StaffingAllAccess, policy => policy.RequireAssertion(context =>
                {
                    return context.User.IsInRole(Constants.Role.BackgroundPollingApiAccess) ||
                           context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("AllAccess"));
                }));
                options.AddPolicy(Constants.Policy.CRRatioRead, policy => policy.RequireAssertion(context =>
                    {
                        return context.User.IsInRole(Constants.Role.CRRatioReadAccess) ||
                                 context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("AllAccess"));
                    }));
                options.AddPolicy(Constants.Policy.StaffingApiLookupRead, policy => policy.RequireAssertion(context =>
                {
                    return context.User.IsInRole(Constants.Role.BackgroundPollingApiAccess) ||
                           context.User.IsInRole(Constants.Role.StaffingApiLookupReadAccess) ||
                           context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("AllAccess")) ||
                           context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("StaffingApiLookupReadAccess"));
                }));
                options.AddPolicy(Constants.Policy.ResourceAllocationRead, policy => policy.RequireAssertion(context =>
                {
                    return context.User.IsInRole(Constants.Role.BackgroundPollingApiAccess) ||
                           context.User.IsInRole(Constants.Role.ResourceAllocationReadAccess) ||
                             context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("AllAccess"));
                }));
                options.AddPolicy(Constants.Policy.CommitmentRead, policy => policy.RequireAssertion(context =>
                {
                    return context.User.IsInRole(Constants.Role.BackgroundPollingApiAccess) ||
                           context.User.IsInRole(Constants.Role.CommitmentReadAccess) ||
                           context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("AllAccess"));
                }));
                options.AddPolicy(Constants.Policy.PlanningCardDetailsRead, policy => policy.RequireAssertion(context =>
                {
                    return context.User.IsInRole(Constants.Role.BackgroundPollingApiAccess) ||
                           context.User.IsInRole(Constants.Role.CommitmentReadAccess) ||
                           context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("AllAccess"))
                           || context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("PlanningCardDetailsReadAccess"));
                }));
            });
            #endregion

            #region BaseRepository

            services.AddScoped<IBaseRepository<Office>, BaseRepository<Office>>();
            services.AddScoped<IBaseRepository<ResourceAllocation>, BaseRepository<ResourceAllocation>>();
            services.AddScoped<IBaseRepository<EmailUtilityData>, BaseRepository<EmailUtilityData>>();
            services.AddScoped<IBaseRepository<Commitment>, BaseRepository<Commitment>>();
            services.AddScoped<IBaseRepository<InvestmentCategory>, BaseRepository<InvestmentCategory>>();
            services.AddScoped<IBaseRepository<AuditCaseHistory>, BaseRepository<AuditCaseHistory>>();
            services.AddScoped<IBaseRepository<CaseRoll>, BaseRepository<CaseRoll>>();
            services.AddScoped<IBaseRepository<UserPreferences>, BaseRepository<UserPreferences>>();
            services.AddScoped<IBaseRepository<UserPreferenceSupplyGroup>, BaseRepository<UserPreferenceSupplyGroup>>();
            services.AddScoped<IBaseRepository<UserPreferenceGroupSharedInfo>, BaseRepository<UserPreferenceGroupSharedInfo>>();
            services.AddScoped<IBaseRepository<UserNotification>, BaseRepository<UserNotification>>();
            services.AddScoped<IBaseRepository<SKUCaseTerms>, BaseRepository<SKUCaseTerms>>();
            services.AddScoped<IBaseRepository<CaseOppChanges>, BaseRepository<CaseOppChanges>>();
            services.AddScoped<IBaseRepository<string>, BaseRepository<string>>();
            services.AddScoped<IBaseRepository<ResourceAvailability>, BaseRepository<ResourceAvailability>>();
            services.AddScoped<IBaseRepository<ScheduleMasterPlaceholder>, BaseRepository<ScheduleMasterPlaceholder>>();
            services.AddScoped<IBaseRepository<ResourceViewNote>, BaseRepository<ResourceViewNote>>();
            services.AddScoped<IBaseRepository<PlanningCard>, BaseRepository<PlanningCard>>();
            services.AddScoped<IBaseRepository<SMAPMission>, BaseRepository<SMAPMission>>();
            services.AddScoped<IBaseRepository<SecurityUser>, BaseRepository<SecurityUser>>();
            services.AddScoped<IBaseRepository<EmployeeStaffingPreferences>, BaseRepository<EmployeeStaffingPreferences>>();
            services.AddScoped<IBaseRepository<ResourceFilter>, BaseRepository<ResourceFilter>>();
            services.AddScoped<IBaseRepository<StaffableAs>, BaseRepository<StaffableAs>>();
            services.AddScoped<IBaseRepository<StaffingResponsible>, BaseRepository<StaffingResponsible>>();
            services.AddScoped<IBaseRepository<RingfenceManagement>, BaseRepository<RingfenceManagement>>();
            services.AddScoped<IBaseRepository<OfficeClosureCases>, BaseRepository<OfficeClosureCases>>();
            services.AddScoped<IBaseRepository<CasePlanningBoard>, BaseRepository<CasePlanningBoard>>();
            services.AddScoped<IBaseRepository<Sku>, BaseRepository<Sku>>();
            services.AddScoped<IBaseRepository<CasePlanningBoardStaffableTeams>, BaseRepository<CasePlanningBoardStaffableTeams>>();
            services.AddScoped<IBaseRepository<PreponedCasesAllocationsAudit>, BaseRepository<PreponedCasesAllocationsAudit>>();
            services.AddScoped<IBaseRepository<CortexSkuMapping>, BaseRepository<CortexSkuMapping>>();
            services.AddScoped<IBaseRepository<MismatchLog>, BaseRepository<MismatchLog>>();
            services.AddScoped<IBaseRepository<AzureSearchQueryLog>, BaseRepository<AzureSearchQueryLog>>();
            services.AddScoped<IBaseRepository<EmployeeStaffingPreferencesForInsightsTool>, BaseRepository<EmployeeStaffingPreferencesForInsightsTool>>();

            #endregion

            #region Repository
            services.AddScoped<IResourceAllocationRepository, ResourceAllocationRepository>();
            services.AddScoped<IEmailUtilityDataLogRepository, EmailUtilityDataLogRepository>();
            services.AddScoped<IResourceHistoryRepository, ResourceHistoryRepository>();
            services.AddScoped<ICommitmentRepository, CommitmentRepository>();
            services.AddScoped<ILookupRepository, LookupRepository>();
            services.AddScoped<IAuditTrailRepository, AuditTrailRepository>();
            services.AddScoped<ICaseRollRepository, CaseRollRepository>();
            services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
            services.AddScoped<IUserPreferenceSupplyGroupRepository, UserPreferenceSupplyGroupRepository>();
            services.AddScoped<IUserPreferenceGroupSharedInfoRepository, UserPreferenceGroupSharedInfoRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<ISKUCaseTermsRepository, SKUCaseTermsRepository>();
            services.AddScoped<ITaggedCaseRepository, TaggedCaseRepository>();
            services.AddScoped<ICaseOppChangesRepository, CaseOppChangesRepository>();
            services.AddScoped<IBackgroundJobClient, BackgroundJobClient>();
            services.AddScoped<IScheduleMasterPlaceholderRepository, ScheduleMasterPlaceholderRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<ISharePointRepository, SharePointRepository>();
            services.AddScoped<IPlanningCardRepository, PlanningCardRepository>();
            services.AddScoped<ISecurityUserRepository, SecurityUserRepository>();
            services.AddScoped<IEmployeeStaffingPreferenceRepository, EmployeeStaffingPreferenceRepository>();
            services.AddScoped<IUserCustomFilterRepository, UserCustomFilterRepository>();
            services.AddScoped<IStaffableAsRepository, StaffableAsRepository>();
            services.AddScoped<IEmployeeStaffingInfoRepository,EmployeeStaffingInfoRepository>();
            services.AddScoped<IRingfenceManagementRepository, RingfenceManagementRepository>();
            services.AddScoped<IOfficeClosureCasesRepository, OfficeClosureCasesRepository>();
            services.AddScoped<ICasePlanningRepository, CasePlanningRepository>();
            services.AddScoped<ISkuRepository, SkuRepository>();
            services.AddScoped<ICasePlanningStaffableTeamsRepository, CasePlanningStaffableTeamsRepository>();
            services.AddScoped<IPreponedCasesAllocationsAuditRepository, PreponedCasesAllocationsAuditRepository>();
            services.AddScoped<ICortexSkuRepository, CortexSkuRepository>();
            services.AddScoped<IDataSyncMismatchRepository, DataSyncMismatchRepository>();
            services.AddScoped<IAzureSearchQueryLogRepository, AzureSearchQueryLogRepository>();
            services.AddScoped<IStaffingPreferencesRepository, StaffingPreferencesRepository>();

            #endregion

            #region Services

            services.AddScoped<IResourceAllocationService, ResourceAllocationService>();
            services.AddScoped<IEmailUtilityDataLogService, EmailUtilityDataLogService>();
            services.AddScoped<IResourceHistoryService, ResourceHistoryService>();
            services.AddScoped<ICommitmentService, CommitmentService>();
            services.AddScoped<ILookupService, LookupService>();
            services.AddScoped<IAuditTrailService, AuditTrailService>();
            services.AddScoped<ICaseRollService, CaseRollService>();
            services.AddScoped<IUserPreferencesService, UserPreferencesService>();
            services.AddScoped<IUserPreferenceSupplyGroupService, UserPreferenceSupplyGroupService>();
            services.AddScoped<IUserPreferenceGroupSharedInfoService, UserPreferenceGroupSharedInfoService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ISKUCaseTermsService, SKUCaseTermsService>();
            services.AddScoped<ITaggedCaseService, TaggedCaseService>();
            services.AddScoped<ICaseOppChangesService, CaseOppChangesService>();
            services.AddScoped<IScheduleMasterPlaceholderService, ScheduleMasterPlaceholderService>();
            services.AddScoped<INoteService, NoteService>();
            services.AddScoped<ISharePointService, SharePointService>();
            services.AddScoped<IPlanningCardService, PlanningCardService>();
            services.AddScoped<ISecurityUserService, SecurityUserService>();
            services.AddScoped<IEmployeeStaffingPreferenceService, EmployeeStaffingPreferenceService>();
            services.AddScoped<IUserCustomFilterService, UserCustomFilterService>();
            services.AddScoped<IStaffableAsService, StaffableAsService>();
            services.AddScoped<IEmployeeStaffingInfoService, EmployeeStaffingInfoService>();
            services.AddScoped<IRingfenceManagementService, RingfenceManagementService>();
            services.AddScoped<IOfficeClosureCasesService, OfficeClosureCasesService>();
            services.AddScoped<ICasePlanningService, CasePlanningService>();
            services.AddScoped<ISkuService, SkuService>();
            services.AddScoped<ICasePlanningStaffableTeamsService, CasePlanningStaffableTeamsService>();
            services.AddScoped<IPreponedCasesAllocationsAuditService, PreponedCasesAllocationsAuditService>();
            services.AddScoped<ICortexSkuService, CortexSkuService>();
            services.AddScoped<IDataSyncMismatchService, DataSyncMismatchService>();
            services.AddScoped<IAzureSearchQueryLogService, AzureSearchQueryLogService>();
            services.AddScoped<IStaffingPreferencesService, StaffingPreferencesService>();

            // API Clients            
            services.AddTransient<HttpClientAuthorizationDelegatingHandler>();
            services.AddHttpContextAccessor();

            services.AddHttpClient<IStaffingAnalyticsApiClient, StaffingAnalyticsApiClient>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

            services.AddHttpClient<IResourceApiClient, ResourceApiClient>();

            #endregion

            #region API Versioning

            services.AddApiVersioning(options =>
            {
                options.ApiVersionReader = new QueryStringApiVersionReader();
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
            });

            #endregion

            #region Swagger

            services.AddVersionedApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });
            services.AddSwaggerGen(options =>
            {
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
            services.ConfigureOptions<ConfigureSwaggerOptions>();

            #endregion

            return services;
        }
    }
}