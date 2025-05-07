using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using System;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class TaggedCaseService : ITaggedCaseService
    {
        private readonly ITaggedCaseRepository _taggedCaseRepository;

        public TaggedCaseService(ITaggedCaseRepository taggedCaseRepository)
        {
            _taggedCaseRepository = taggedCaseRepository;
        }

        public async Task<string> GetCasesByResourceServiceLines(string oldCaseCodes, string serviceLineNames)
        {
            if (string.IsNullOrEmpty(oldCaseCodes) || string.IsNullOrEmpty(serviceLineNames))
                return String.Empty;

            var taggedCases =
                await _taggedCaseRepository.GetCasesByResourceServiceLines(oldCaseCodes, serviceLineNames);
            return string.Join(",", taggedCases);
        }

        public async Task<string> GetOpportunitiesByResourceServiceLines(string pipelineIds, string serviceLineNames)
        {
            if (string.IsNullOrEmpty(pipelineIds) || string.IsNullOrEmpty(serviceLineNames))
                return String.Empty;

            var taggedOpportunitiesGuid =
                await _taggedCaseRepository.GetOpportunitiesByResourceServiceLines(pipelineIds, serviceLineNames);

            return string.Join(",", taggedOpportunitiesGuid);
        }
    }
}