using System.Collections.Generic;

namespace BackgroundPolling.API.Models
{
    public class OfficeHierarchy
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public IList<OfficeHierarchy> Children { get; set; }
    }
}
