using Staffing.HttpAggregator.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface INoteLogService
    {
        Task<IEnumerable<ResourceViewNoteViewModel>> GetResourceNotes(string employeeCode, string loggedInEmployeeCode, string noteTypeCode);
    }
}
