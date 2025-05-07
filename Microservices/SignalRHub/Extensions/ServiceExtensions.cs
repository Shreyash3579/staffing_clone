using Microsoft.Extensions.DependencyInjection;
using SignalRHub.Contracts.RepositoryInterfaces;
using SignalRHub.Contracts.Services;
using SignalRHub.Core.Repository;
using SignalRHub.Core.Services;
using SignalRHub.HubConfig;
using SignalRHub.Models;
using System;
using SignalRHub.Core.Helpers;
using SignalRHub.Contracts.Helpers;

namespace SignalRHub.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection RegisterAppServices(this IServiceCollection services)
        {
            #region CORS
            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(7200));
            }));

            #endregion

            #region Dapper
            services.AddScoped<IDapperContext, DapperContext>();
            #endregion

            services.AddScoped<ISignalRHubService, SignalRHubService>();
            services.AddScoped<ISignalRHubRepository, SignalRHubRepository>();

            services.AddScoped<IBaseRepository<UserConnectionMapping>, BaseRepository<UserConnectionMapping>>();

            services.AddSignalR();
            return services;
        }
    }
}