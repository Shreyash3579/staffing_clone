using System;

namespace Staffing.API.Models
{
    public class ResourceCommercialModel
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public string CommercialModel { get; set; }
        public string LastUpdatedBy { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
