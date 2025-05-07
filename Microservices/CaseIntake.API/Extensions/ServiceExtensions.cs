using CaseIntake.API.Contracts.Helpers;
using CaseIntake.API.Contracts.Repository;
using CaseIntake.API.Contracts.Services;
using CaseIntake.API.Core.Helpers;
using CaseIntake.API.Core.Repository;
using CaseIntake.API.Core.Services;
using CaseIntake.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace CaseIntake.API.Extensions
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

            #region BaseRepository
            services.AddScoped<IBaseRepository<CaseIntakeLeadership>, BaseRepository<CaseIntakeLeadership>>();
            services.AddScoped<IBaseRepository<CaseIntakeRoleDetails>, BaseRepository<CaseIntakeRoleDetails>>();
            #endregion

            #region Repository
            services.AddScoped<ICaseIntakeRepository, CaseIntakeRepository>();
            #endregion

            #region Services

            services.AddHttpContextAccessor();

            services.AddScoped<ICaseIntakeService, CaseIntakeService>();
            #endregion

            #region HttpClients
            services.AddTransient<HttpClientAuthorizationDelegatingHandler>();
            services.AddHttpContextAccessor();


            services.AddHttpClient<IResourceApiClient, ResourceApiClient>();
            
            services.AddHttpClient<IStaffingApiClient, StaffingApiClient>()
            .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
            
            services.AddHttpClient<ICCMApiClient, CCMApiClient>()
            .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
            
            services.AddHttpClient<IPipelineApiClient, PipelineApiClient>()
            .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
            
            services.AddHttpClient<ISignalRHubClient, SignalRHubClient>()
            .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();



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

            #region Swagger
            services.AddSwaggerGen(options =>
            {
                // The generated Swagger JSON file will have these properties.
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CaseIntake API",
                    Version = "v1",
                    Description = "Get Case Intake Form Details",
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

            //#region Gzip Compression
            //services.Configure<GzipCompressionProviderOptions>(options =>
            //{
            //    options.Level = CompressionLevel.Optimal;
            //});
            //services.AddResponseCompression(options =>
            //{
            //    options.EnableForHttps = true;
            //    options.Providers.Add<GzipCompressionProvider>();
            //    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] {
            //        "ap­pli­ca­tion/json"
            //    });
            //});
            //#endregion

            return services;
        }
    }
}
