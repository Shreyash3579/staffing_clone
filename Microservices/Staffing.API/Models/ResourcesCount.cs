using System;

namespace Staffing.API.Models
{
    public class ResourcesCount
    {
        public string? OldCaseCode { get; set; }
        public Guid? PipelineId { get; set; }
        public int ResourceCount { get; set; }
    }
}
