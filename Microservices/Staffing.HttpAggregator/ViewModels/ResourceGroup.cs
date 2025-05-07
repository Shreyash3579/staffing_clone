using Staffing.HttpAggregator.Models;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.ViewModels
{
    public class ResourceGroup
    {
        public string GroupTitle { get; set; }
        public IList<Resource> Resources { get; set; }
    }
}
