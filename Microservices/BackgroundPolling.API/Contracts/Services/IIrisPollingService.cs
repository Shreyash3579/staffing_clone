using System;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IIrisPollingService
    {
        public Task UpsertWorkAndSchoolHistoryForAllActiveEmployeesFromIris();
        public Task<string> InsertWorkAndEducationHistoryAfterLastModifiedDateForActiveEmployeesFromIris(DateTime? lastModifiedDateAdter);
    }
}
