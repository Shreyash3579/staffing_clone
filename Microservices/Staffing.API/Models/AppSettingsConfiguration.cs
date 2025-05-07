using System.Collections.Generic;

namespace Staffing.API.Models
{
    public class AppSettingsConfiguration
    {
        public IDictionary<string, string> AppSettings { get; set; }
        public IDictionary<string, string> ConnectionStrings { get; set; }
        public IDictionary<string, string> Serilog { get; set; }
        public IDictionary<string, string> Email { get; set; }
        public IDictionary<string, string> DebugEmail { get; set; }
        public IDictionary<string, string> EmeaPEGStaffingUserEmail { get; set; }
        public IDictionary<string, string> BCNAuditLogEmail { get; set; }
        public IDictionary<string, string> ClimateClubEmail { get; set; }
        public IDictionary<string, string> ApplicationInsights { get; set; }
        public IDictionary<string, string> Token { get; set; }
        public IDictionary<string, string> Threshold { get; set; }
        public int EmployeeTransactionCountToFetchFromWorkday { get; set; }
        public int EmployeeTimeOffCountToFetchFromWorkday { get; set; }
        public int EmployeeLoATransactionCountToFetchFromWorkday { get; set; }
        public IDictionary<string, string> AzureADCredentials { get; set; }
    }
}
