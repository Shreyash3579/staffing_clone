using System.Collections.Generic;

namespace Staffing.HttpAggregator.ViewModels
{
    public class CasePlanningBoardColumn
    {
        public string Title { get; set; }
        public IEnumerable<CasePlanningBoardViewModel> Projects { get; set; }
    }
}
