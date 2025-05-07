using System.Collections.Generic;

namespace Staffing.Authentication.Models
{
    public class AppSettingsConfiguration
    {
        public IDictionary<string, string> AppSettings { get; set; }
        public IDictionary<string, string> ConnectionStrings { get; set; }
        public IDictionary<string, string> Serilog { get; set; }
        public IDictionary<string, string> Email { get; set; }
        public IDictionary<string, string> ApplicationInsights { get; set; }
        public IDictionary<string, string> AppToken { get; set; }
        public IDictionary<string, string> AppClientId { get; set; }
        public IDictionary<string, string> AppRefreshToken { get; set; }
        public IDictionary<string, string> ClaimRoles { get; set; }
        public IDictionary<string, string> LDAP { get; set; }
        public IDictionary<string, string> ADGroup { get; set; }
        public IDictionary<string, string> ScreenPermissionsToBossUser { get; set; }
        public IDictionary<string, string> ScreenPermissionsToADGroup { get; set; }
        public string AuthorizedToRegisterApp { get; set; }
        public string CorsAllowedOrigins { get; set; }
    }
}