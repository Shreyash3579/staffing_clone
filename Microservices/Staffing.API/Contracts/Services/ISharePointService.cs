using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface ISharePointService
    {
        Task<IEnumerable<SMAPMission>> GetSmapMissionNotesByEmployeeCodes(string employeeCodes);
    }
}
