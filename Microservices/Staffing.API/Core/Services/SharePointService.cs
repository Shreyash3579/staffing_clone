using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class SharePointService : ISharePointService
    {
        private readonly ISharePointRepository _sharePointRepository;

        public SharePointService(ISharePointRepository sharePointRepository)
        {
            _sharePointRepository = sharePointRepository;
        }

        public async Task<IEnumerable<SMAPMission>> GetSmapMissionNotesByEmployeeCodes(string employeeCodes)
        {
            var resourceNotesForSmapMission = await _sharePointRepository.GetSmapMissionNotesByEmployeeCodes(employeeCodes);
            return resourceNotesForSmapMission;
        }
    }
}
