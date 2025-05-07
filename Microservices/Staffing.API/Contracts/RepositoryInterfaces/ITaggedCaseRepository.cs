using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface ITaggedCaseRepository
    {
        Task<IEnumerable<string>> GetCasesByResourceServiceLines(string oldCaseCodes, string serviceLineNames);
        Task<IEnumerable<Guid>> GetOpportunitiesByResourceServiceLines(string pipelineIds, string serviceLineNames);
    }
}
