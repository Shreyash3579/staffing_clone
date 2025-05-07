using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Models
{
    public class UserPreferenceGroupFilters
    {
        public Guid? Id { get; set; }
        public Guid? GroupId { get; set; }
        public string AndOr { get; set; }
        public string FilterField { get; set; }
        public string FilterOperator { get; set;}
        public string FilterValue { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
