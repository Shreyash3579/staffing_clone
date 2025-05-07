using CCM.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCM.API.Contracts.Services
{
    public interface IFinApiClient
    {
        Task<IEnumerable<RevOffice>> GetOffices(string hierarchyType, string status);
        Task<IEnumerable<BillRate>> GetBillRateByOffices(string officeCodes);
        Task<IEnumerable<BillRate>> GetBillRates();
        Task<IEnumerable<BillRateType>> GetBillRateType();
    }
}
