using System;
using System.Text.Json.Serialization;

namespace Microservices.Common.Models
{
    [Serializable]
    public class ErrorLogs
    {
        public Exception Error { get; set; }
        public string ApplicationName { get; set; }
        public string EmployeeCode { get; set; }
        [JsonIgnore]
        public string HelpURL { get; set; } 
    }
}
