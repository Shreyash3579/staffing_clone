using Dapper;
using Staffing.API.Contracts.Helpers;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class AzureSearchQueryLogRepository : IAzureSearchQueryLogRepository
    {
        private readonly IBaseRepository<AzureSearchQueryLog> _baseRepository;

        public AzureSearchQueryLogRepository(IBaseRepository<AzureSearchQueryLog> baseRepository, IDapperContext context)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = context;
        }

        public async Task InsertAzureSearchQueryLog(AzureSearchQueryLog azureSearchQueryLog)
        {
            var insertedLog = await Task.Run(() => _baseRepository.Context.Connection.QueryAsync<AzureSearchQueryLog>(
                StoredProcedureMap.InsertAzureSearchQueryLog,
                new
                {
                    azureSearchQueryLog.EmployeeCode,
                    azureSearchQueryLog.SearchString,
                    azureSearchQueryLog.SearchTriggeredFrom,
                    azureSearchQueryLog.OpenAIGeneratedSearchQuery,
                    azureSearchQueryLog.SearchResultsCount,
                    azureSearchQueryLog.IsErrorInOpenAiGeneratedSearchQuery,
                    azureSearchQueryLog.LastUpdatedBy
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return;
        }

    }
}
