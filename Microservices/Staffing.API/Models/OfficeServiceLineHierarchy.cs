using System.Collections.Generic;

namespace Staffing.API.Models
{
    public class OfficeServiceLineHierarchy
    {
        public IEnumerable<OfficeHierarchy> NewOffices { get; set; }
        public IEnumerable<ServiceLineHierarchy> NewServiceLines { get; set; }
    }
}
