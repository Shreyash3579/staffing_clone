using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Staffing.Authentication.Contracts.Helpers;
using Staffing.Authentication.Contracts.RepositoryInterfaces;
using Staffing.Authentication.Contracts.Services;
using Staffing.Authentication.Core.Helpers;
using Staffing.Authentication.Core.Repository;
using Staffing.Authentication.Core.Services;
using Staffing.Authentication.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Staffing.Authentication.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection RegisterAppServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            #region JWT Authentication

            var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("staffing_secretKey"));
            services.AddAuthentication(x =>
            {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            #endregion

            #region Dapper

            services.AddScoped<IDapperContext, DapperContext>();

            #endregion

            #region CORS

            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder.WithOrigins(ConfigurationUtility.GetValue("CorsAllowedOrigins").Split(","))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(7200));
            }));

            #endregion

            #region Delegating Handlers

            services.AddTransient<HttpClientAuthorizationDelegatingHandler>();
            services.AddHttpContextAccessor();

            #endregion

            #region Http Services

            services.AddHttpClient<IResourcesApiClient, ResourcesApiClient>();

            services.AddHttpClient<IHcpdApiClient, HcpdApiClient>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

            services.AddHttpClient<IPegC2CApiClient, PegC2CApiClient>();

            #endregion

            #region BaseRepository

            services.AddScoped<IBaseRepository<SecurityUser>, BaseRepository<SecurityUser>>();

            #endregion

            #region Repository

            services.AddScoped<ISecurityUserRepository, SecurityUserRepository>();

            #endregion

            #region Services

            services.AddScoped<ISecurityUserService, SecurityUserService>();
            services.AddScoped<IADservice, ADService>();

            #endregion

            #region Swagger

            services.AddSwaggerGen(options =>
            {
                // The generated Swagger JSON file will have these properties.
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Authentication API",
                    Version = "v1",
                    Description = "Authenticate user/app and generate JWT Token"
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

            services.AddSwaggerGenNewtonsoftSupport();

            #endregion

            return services;
        }
    }
}