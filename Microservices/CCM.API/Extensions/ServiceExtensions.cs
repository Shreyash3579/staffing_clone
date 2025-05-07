using CCM.API.Contracts.Helpers;
using CCM.API.Contracts.RepositoryInterfaces;
using CCM.API.Contracts.Services;
using CCM.API.Core.Helpers;
using CCM.API.Core.Repository;
using CCM.API.Core.Services;
using CCM.API.Models;
using Microservices.Common.Core.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace CCM.API.Extensions
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

            #region JWT Authentication

            var signingKeys = new List<SecurityKey>
            {
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("staffing_secretKey"))),
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
                options.AddPolicy(Constants.Policy.CCMAllAccess, policy => policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("AllAccess")) ||
                    context.User.IsInRole(Constants.Role.BackgroundPollingApiAccess)
                ));

                options.AddPolicy(Constants.Policy.OfficeLookupReadAccess, policy => policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("OfficeLookupReadAccess")) ||
                    context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("AllAccess")) ||
                    context.User.IsInRole(Constants.Role.BackgroundPollingApiAccess)
                ));
                options.AddPolicy(Constants.Policy.CaseInfoReadAccess, policy=> policy.RequireAssertion(context =>
                    context.User.IsInRole(Constants.Policy.CaseInfoReadAccess) ||
                    context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("CaseInfoReadAccess")) ||
                    context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("AllAccess")) ||
                    context.User.IsInRole(Constants.Role.BackgroundPollingApiAccess)
                ));
                options.AddPolicy(Constants.Policy.CaseTypeLookupReadAccess, policy => policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("CaseTypeLookupReadAccess")) ||
                    context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("AllAccess")) ||
                    context.User.IsInRole(Constants.Role.BackgroundPollingApiAccess)
                ));

            });


            #endregion

            #region BaseRepository

            services.AddScoped<IBaseRepository<Case>, BaseRepository<Case>>();
            services.AddScoped<IBaseRepository<CaseType>, BaseRepository<CaseType>>();
            services.AddScoped<IBaseRepository<CaseAttribute>, BaseRepository<CaseAttribute>>();
            services.AddScoped<IBaseRepository<CaseAttributeModel>, BaseRepository<CaseAttributeModel>>();
            services.AddScoped<IBaseRepository<CaseOpportunityMap>, BaseRepository<CaseOpportunityMap>>();

            #endregion

            #region Repository

            services.AddScoped<ICaseRepository, CaseRepository>();
            services.AddScoped<ICaseTypeRepository, CaseTypeRepository>();
            services.AddScoped<ILookupRepository, LookupRepository>();
            services.AddScoped<ICaseOpportunityMapRepository, CaseOpportunityMapRepository>();
            services.AddScoped<ICaseAttributeRepository, CaseAttributeRepository>();

            #endregion

            #region Services

            services.AddScoped<ICaseService, CaseService>();
            services.AddScoped<ICaseTypeService, CaseTypeService>();
            services.AddScoped<ILookupService, LookupService>();
            services.AddScoped<ICaseOpportunityMapService, CaseOpportunityMapService>();
            services.AddScoped<ICaseAttributeService, CaseAttributeService>();
            services.AddScoped<IFinApiService, FinApiService>();
            services.AddScoped<IClientCaseAPIService, ClientCaseAPIService>();

            #endregion

            #region Swagger

            services.AddSwaggerGen(options =>
            {
                // The generated Swagger JSON file will have these properties.
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CCM API",
                    Version = "v1",
                    Description = "Get Case, client information"
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

            #region Http Client
            services.AddHttpContextAccessor();
            services.AddHttpClient<IFinApiClient, FinApiClient>();
            services.AddHttpClient<IClientCaseAPIClient, ClientCaseAPIClient>();
            #endregion

            return services;
        }
    }
}