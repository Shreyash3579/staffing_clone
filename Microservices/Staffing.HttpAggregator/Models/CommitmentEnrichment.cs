using System.Collections.Generic;

namespace Staffing.HttpAggregator.Models
{
    public class CommitmentEnrichment
    {
        public IList<Commitment> commitments { get; set; }

        public IList<Resource> resources { get; set; }
    }
}
