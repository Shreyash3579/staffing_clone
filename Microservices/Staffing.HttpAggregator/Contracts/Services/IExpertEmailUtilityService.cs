using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IExpertEmailUtilityService
    {
        Task<string> SendMonthlyStaffingAllocationsEmailToExperts(string employeeCodes);
        Task<string> SendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign(string employeeCodes);
    }
}
