using Staffing.Coveo.API.Models;
using System.Collections.Generic;

namespace Staffing.Coveo.API.ViewModels
{
    public class ResourcesViewModel
    {
        public IEnumerable<Resource> Resources { get; set; }
        public IEnumerable<TimeOff> TimeOffs { get; set; }
        public IEnumerable<ResourceAllocation> Allocations { get; set; }
        public IEnumerable<Commitment> Commitments { get; set; }
        public IEnumerable<Training> Trainings { get; set; }
        public IEnumerable<ResourceTermination> Terminations { get; set; }
        public IEnumerable<ResourcePromotion> Promotions { get; set; }
        public IEnumerable<ResourceTransfer> Transfers { get; set; }
        public IEnumerable<ResourceTransition> Transitions { get; set; }
        public IEnumerable<ResourceLoA> LOAs { get; set; }
    }
}
