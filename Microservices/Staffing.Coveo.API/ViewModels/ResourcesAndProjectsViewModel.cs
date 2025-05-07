using Staffing.Coveo.API.Models;
using System.Collections.Generic;

namespace Staffing.Coveo.API.ViewModels
{
    public class ResourcesAndProjectsViewModel
    {
        public ResourcesViewModel Resources { get; set; }
        public IEnumerable<ProjectData> Projects { get; set; }
    }
}
