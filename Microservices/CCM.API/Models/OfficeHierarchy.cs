using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCM.API.Models
{
    public class OfficeHierarchy
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public IList<OfficeHierarchy> Children { get; set; }
    }

    public class LocationHierarchy
    {
        public int OfficeCode { get; set; }
        public Office Office { get; set; }
        public int ParentOfficeCode { get; set; }
        public string EntityTypeCode { get; set; }
        public IList<LocationHierarchy> ChildLocations { get; set; }
    }
}
