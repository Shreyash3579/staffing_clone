using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Repository
{
    public interface IPolarisPollingRepository
    {
        Task UpsertSecurityUsersForAnalytics(DataTable dataTable);
        Task UpsertSecurityUsersDataForBOSS(DataTable securityUsers, DataTable securityUsersWithFeatureAccessDataTable, DataTable securityUsersGeography);
    }
}
