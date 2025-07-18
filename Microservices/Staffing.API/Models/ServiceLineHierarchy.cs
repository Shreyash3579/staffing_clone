﻿using System;

namespace Staffing.API.Models
{
    public class ServiceLineHierarchy
    {
            public Guid ServiceLineId { get; set; }
            public string ServiceLineHierarchyCode { get; set; }
            public string ServiceLineHierarchyName { get; set; }
            public string ServiceLineCode { get; set; }
            public string ServiceLineName { get; set; }
            public int EmployeeCount { get; set; }
            public bool InActive { get; set; }
    }
}
