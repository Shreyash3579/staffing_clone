using System;

namespace Staffing.HttpAggregator.Models
{
    public class ResourceViewCommercialModelViewModel
    {


            public Guid? Id { get; set; }
            public string EmployeeCode { get; set; }
            public string CommercialModel { get; set; }
            public string CreatedBy { get; set; }
            public string CreatedByName { get; set; }
            public DateTime LastUpdated { get; set; }
            public string LastUpdatedBy { get; set; }
    }
}

