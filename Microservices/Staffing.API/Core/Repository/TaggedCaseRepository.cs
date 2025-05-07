using Dapper;
using Staffing.API.Contracts.Helpers;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class TaggedCaseRepository : ITaggedCaseRepository
    {
        private readonly IBaseRepository<string> _baseRepository;

        public TaggedCaseRepository(IBaseRepository<string> baseRepository, IDapperContext dapperContext)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = dapperContext;
        }

        public async Task<IEnumerable<string>> GetCasesByResourceServiceLines(string oldCaseCodes,
            string serviceLineNames)
        {
            var taggedCases = await _baseRepository.GetAllAsync(new { oldCaseCodes, serviceLineNames },
                StoredProcedureMap.GetCasesByResourceServiceLines);
            return taggedCases;
        }

        public async Task<IEnumerable<Guid>> GetOpportunitiesByResourceServiceLines(string pipelineIds,
            string serviceLineNames)
        {
            var taggedOpportunities = await Task.Run(() => _baseRepository.Context.Connection.Query<Guid>(
                StoredProcedureMap.GetOpportunitiesByResourceServiceLines,
                new { pipelineIds, serviceLineNames },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return taggedOpportunities;
        }
    }
}