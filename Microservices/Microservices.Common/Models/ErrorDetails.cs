using Newtonsoft.Json;
using System;

namespace Microservices.Common.Models
{
    [Serializable]
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        [JsonIgnore]
        public string HelpUrl { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
