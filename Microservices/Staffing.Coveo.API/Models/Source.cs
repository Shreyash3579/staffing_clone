using Staffing.Coveo.API.Contracts.Services;

namespace Staffing.Coveo.API.Models
{
    public class Source 
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public string Sort { get; set; }
        public string NumberOfResults { get; set; }
        public string FirstResult { get; set; }
    }
}