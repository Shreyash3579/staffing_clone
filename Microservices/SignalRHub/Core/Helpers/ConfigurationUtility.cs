using Microsoft.Extensions.Configuration;
using System.IO;

namespace SignalRHub.Core.Helpers
{
    public static class ConfigurationUtility
    {
        static ConfigurationUtility()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        public static IConfiguration Configuration { get; set; }

        public static string GetValue(string key)
        {
            return Configuration[key];
        }
    }
}