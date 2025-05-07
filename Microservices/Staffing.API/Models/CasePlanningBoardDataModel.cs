using System;
using System.Collections.Generic;

namespace Staffing.API.Models
{
    public class CasePlanningBoardDataModel
    {
        public IList<CasePlanningBoard> CasePlanningBoardData { get; set; }
        public IList<CasePlanningBoard> CasePlanningBoardPlanningCardData { get; set; }
    }
}
