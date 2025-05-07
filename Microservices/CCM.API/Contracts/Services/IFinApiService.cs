using CCM.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCM.API.Contracts.Services
{
    public interface IFinApiService
    {
        Task<IEnumerable<Office>> GetOfficesFlatListByRegionOrCluster(int regionOrClusterCode);
        Task<IEnumerable<Office>> GetOfficeList(IList<int> accessibleOffices = null);
        Task<IEnumerable<RevOffice>> GetOfficeListFromFinance();
        Task<OfficeHierarchy> GetOfficeHierarchy(IList<int> accessibleOffices = null);
        Task<OfficeHierarchy> GetOfficeHierarchyByOffices(string officeCodes);
        Task<IEnumerable<BillRate>> GetBillRateByOffices(string officeCodes);
        Task<IEnumerable<BillRate>> GetBillRates();
    }
}
