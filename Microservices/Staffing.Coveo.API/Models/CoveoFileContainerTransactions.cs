using System.Collections.Generic;

namespace Staffing.Coveo.API.Models
{
    public class CoveoFileContainerTransactions
    {
        public IEnumerable<dynamic> addOrUpdate { get; set; }
        public IEnumerable<dynamic> delete { get; set; }
    }
}
