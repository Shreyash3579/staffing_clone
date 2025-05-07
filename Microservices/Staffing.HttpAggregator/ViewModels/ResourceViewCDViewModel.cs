using System.Collections.Generic;
using System;

namespace Staffing.HttpAggregator.ViewModels
{
    public class ResourceViewCDViewModel
    {

            public Guid? Id { get; set; }
            public string EmployeeCode { get; set; }
            public string RecentCD { get; set; }
            public string CreatedBy { get; set; }
            public string CreatedByName { get; set; }
            public DateTime LastUpdated { get; set; }
            public string LastUpdatedBy { get; set; }
    }
}


