using Dapper;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class SharePointRepository : ISharePointRepository
    {
        private readonly IBaseRepository<SMAPMission> _baseRepository;

        public SharePointRepository(IBaseRepository<SMAPMission> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<SMAPMission>> GetSmapMissionNotesByEmployeeCodes(string employeeCodes)
        {
            var resourceNotesForSmapMission = await _baseRepository.GetAllAsync(
                 new
                 {
                     employeeCodes
                 },
                 StoredProcedureMap.GetSmapMissionNotesByEmployeeCodes
             );

            return resourceNotesForSmapMission;
        }
    }
}
