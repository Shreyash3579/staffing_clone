using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IPlanningCardService
    {
        Task<bool> MergePlanningCard(PlanningCard mergedPlanningCard, IEnumerable<ResourceAssignmentViewModel> resourceAllocations, IEnumerable<ResourceAssignmentViewModel> placeholderAllocations);
        Task<IList<PlanningCard>> GetPlanningCardsWithNotes(IEnumerable<PlanningCard> planningCards, string employeeCode);
    }
}
