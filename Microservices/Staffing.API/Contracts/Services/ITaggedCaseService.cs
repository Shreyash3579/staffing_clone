using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface ITaggedCaseService
    {
        Task<string> GetCasesByResourceServiceLines(string oldCaseCodes, string serviceLineNames);
        Task<string> GetOpportunitiesByResourceServiceLines(string pipelineIds, string serviceLineNames);
    }
}
