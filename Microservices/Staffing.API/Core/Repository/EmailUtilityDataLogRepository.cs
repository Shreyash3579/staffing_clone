using Dapper;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class EmailUtilityDataLogRepository : IEmailUtilityDataLogRepository
    {
        private readonly IBaseRepository<EmailUtilityData> _baseRepository;

        public EmailUtilityDataLogRepository(IBaseRepository<EmailUtilityData> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<EmailUtilityData>> GetEmailUtilityDataLogsByDateAndEmailType(DateTime dateOfEmail, string emailType)
        {
          var dataLog =  await _baseRepository.GetAllAsync( new { dateOfEmail , emailType },
                StoredProcedureMap.GetEmailUtilityDataLogsByDate
            );

            return dataLog ?? Enumerable.Empty<EmailUtilityData>();
        }

        public async Task<IEnumerable<EmailUtilityData>> UpsertEmailUtilityDataLog(DataTable emailUtilityDataTable)
        {
               var emailUtilityData = await _baseRepository.Context.Connection.QueryAsync<EmailUtilityData>(
                StoredProcedureMap.UpsertEmailUtilityDataLog,
                new
                {
                    emailUtilityDataLogs =
                        emailUtilityDataTable.AsTableValuedParameter(
                            "[dbo].[EmailUtilityDataLogTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return emailUtilityData;
        }
    }
}