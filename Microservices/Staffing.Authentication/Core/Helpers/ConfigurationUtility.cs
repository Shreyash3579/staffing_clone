using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Staffing.Authentication.Core.Helpers
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

        public static Func<string, string> GetValue = key => Configuration[key];
        public static void AddOrUpdateAppSetting<T>(string key, T value)
        {

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var json = File.ReadAllText(filePath);
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            var sectionPath = key.Split(":")[0];
            if (!string.IsNullOrEmpty(sectionPath))
            {
                var keyPath = key.Split(":")[1];
                jsonObj[sectionPath][keyPath] = value;
            }
            else
            {
                jsonObj[sectionPath] = value; // if no section path just set the value
            }
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, output);
        }

    }
}
