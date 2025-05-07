using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Pipeline.API.Contracts.Helpers;
using Pipeline.API.Contracts.RepositoryInterfaces;
using Pipeline.API.Contracts.Services;
using Pipeline.API.Core.Helpers;
using Pipeline.API.Core.Repository;
using Pipeline.API.Core.Services;
using Pipeline.API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Pipeline.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection RegisterAppServices(this IServiceCollection services)
        {
            services.AddScoped<IDapperContext, DapperContext>();

            #region CORS

            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(7200));
            }));

            #endregion

            #region BaseRepository

            services.AddScoped<IBaseRepository<Opportunity>, BaseRepository<Opportunity>>();
            services.AddScoped<IBaseRepository<OpportunityStatusType>, BaseRepository<OpportunityStatusType>>();

            #endregion

            #region Repository

            services.AddScoped<ILookupRepository, LookupRepository>();
            services.AddScoped<ICortexRepository, CortexRepository>();
            services.AddScoped<IOpportunityRepository, OpportunityRepository>();

            #endregion

            #region Services
            services.AddScoped<ILookupService, LookupService>();
            services.AddScoped<IOpportunityService, OpportunityService>();

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
                options.AddPolicy(Constants.Policy.PipelineAllAccess, policy => policy.RequireAssertion(context =>
                  context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("AllAccess")) ||
                    context.User.IsInRole(Constants.Role.BackgroundPollingApiAccess) ||
                    context.User.IsInRole(Constants.Role.PipelineApiAllAccess)
                ));

                options.AddPolicy(Constants.Policy.OpportunityDetailsReadAccess, policy => policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("OpportunityDetailsReadAccess")) ||
                    context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("AllAccess")) ||
                    context.User.IsInRole(Constants.Role.BackgroundPollingApiAccess) ||
                    context.User.IsInRole(Constants.Role.PipelineApiAllAccess)
                ));
                options.AddPolicy(Constants.Policy.OpportunityLookupReadAccess, policy => policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("OpportunityLookupReadAccess")) ||
                    context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("AllAccess")) ||
                    context.User.IsInRole(Constants.Role.BackgroundPollingApiAccess) ||
                    context.User.IsInRole(Constants.Role.PipelineApiAllAccess)
                ));
                
            });

            #endregion

            #region Swagger

            services.AddSwaggerGen(options =>
            {
                // The generated Swagger JSON file will have these properties.
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Pipeline API",
                    Version = "v1",
                    Description = "Get opportunity information"
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