using Staffing.HttpAggregator.Models;
using System.Collections;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.ViewModels
{
    public class ProjectDataViewModel
    {
        public IEnumerable<ProjectData> projects { get; set; }
        public IEnumerable<ProjectData> pinnedProjects { get; set; }
        public IEnumerable<ProjectData> hiddenProjects { get; set; }
    }
}
