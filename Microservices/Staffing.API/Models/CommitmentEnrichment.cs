using Microsoft.Graph.Models;
using System.Collections.Generic;

namespace Staffing.API.Models
{
    public class CommitmentEnrichment
    {
        public IList<Commitment> commitments { get; set; }

        public IList<ResourceModel> resources { get; set; }

    }
}
