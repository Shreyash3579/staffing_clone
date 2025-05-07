using Staffing.Analytics.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.Services
{
    public interface ICCMApiClient
    {
        Task<IEnumerable<CaseViewModel>> GetCaseDetailsByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<Office>> GetOfficeList();
        Task<IEnumerable<BillRate>> GetBillRateByOffices(string officeCodes);
    }
}
