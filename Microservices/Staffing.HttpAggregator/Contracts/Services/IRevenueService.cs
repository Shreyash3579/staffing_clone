using Staffing.HttpAggregator.ViewModels;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IRevenueService
    {
        Task<RevenueViewModel> GetRevenueByCaseCodeAndClientCode(int? clientCode, int? caseCode, string pipelineId, string currency);
    }
}
