using Basis.API.Contracts.Helpers;
using Basis.API.Contracts.RepositoryInterfaces;
using Basis.API.Contracts.Services;
using Basis.API.Core.Helpers;
using Basis.API.Core.Repository;
using Basis.API.Core.Services;
using Basis.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Basis.API.Extensions
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
                options.AddPolicy(Constants.Policy.BasisAllAccess, policy => policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("AllAccess")) ||
                    context.User.IsInRole(Constants.Role.BackgroundPollingApiAccess)
                ));

                options.AddPolicy(Constants.Policy.PracticeAreaLookupRead, policy => policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("PracticeAreaLookupReadAccess")) ||
                    context.User.HasClaim(c => c.Type == "Scope" && c.Value.Contains("AllAccess")) ||
                    context.User.IsInRole(Constants.Role.BackgroundPollingApiAccess)
                ));
            });

            #endregion

            #region BaseRepository

            services.AddScoped<IBaseRepository<PracticeAreaAffiliation>, BaseRepository<PracticeAreaAffiliation>>();
            services.AddScoped<IBaseRepository<CurrencyRates>, BaseRepository<CurrencyRates>>();
            services.AddScoped<IBaseRepository<PracticeAffiliation>, BaseRepository<PracticeAffiliation>>();
            services.AddScoped<IBaseRepository<Holiday>, BaseRepository<Holiday>>();

            #endregion

            #region Repository

            services.AddScoped<IPracticeAreaRepository, PracticeAreaRepository>();
            services.AddScoped<ICurrencyRepository, CurrencyRepository>();
            services.AddScoped<IPracticeAffiliationRepository, PracticeAffiliationRepository>();
            services.AddScoped<IHolidayRepository, HolidayRepository>();

            #endregion

            #region Services

            services.AddScoped<IPracticeAreaService, PracticeAreaService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IPracticeAffiliationService, PracticeAffiliationService>();
            services.AddScoped<IHolidayService, HolidayService>();

            #endregion

            #region Swagger

            services.AddSwaggerGen(options =>
            {
                // The generated Swagger JSON file will have these properties.
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Basis API",
                    Version = "v1",
                    Description = "Get data from basis DB"
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
