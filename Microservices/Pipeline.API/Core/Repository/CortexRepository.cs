using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pipeline.API.Contracts.Helpers;
using Pipeline.API.Contracts.RepositoryInterfaces;
using Pipeline.API.Core.Helpers;
using Pipeline.API.Models;


namespace Pipeline.API.Core.Repository
{
    public class CortexRepository : ICortexRepository
    {
        private readonly IBaseRepository<Opportunity> _baseRepository;

        public CortexRepository(IBaseRepository<Opportunity> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<Opportunity>> GetTeamSizeFromCortex(string cortexIds)
        {
            var opportunityWithteamSize = await Task.Run(() => _baseRepository.Context.CortexConnection.QueryAsync<Opportunity>(
                StoredProcedureMap.GetOpportunityByCortexIds,
                new { cortexIds },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return opportunityWithteamSize;
        }

    }
}
