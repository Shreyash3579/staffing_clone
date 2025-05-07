using System.Collections.Generic;
using System.Threading.Tasks;
using CaseIntake.API.Models;

namespace CaseIntake.API.Contracts.Services
{
    public interface IStaffingApiClient
    {
        Task<IEnumerable<PlanningCard>> GetPlanningCardByPlanningCardIds(string planningCardIds);

    }
}