using Microsoft.Extensions.Configuration;
using Staffing.Coveo.API.Models;
using System.IO;

namespace Staffing.Coveo.API.Core.Helpers
{
    public static class ConfigurationUtility
    {
        public static AppSettings AppSettings;
        static ConfigurationUtility()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
            AppSettings = Configuration.Get<AppSettings>();

        }

        public static IConfiguration Configuration { get; set; }

        public static string GetValue(string key)
        {
            return Configuration[key];
        }
    }
}