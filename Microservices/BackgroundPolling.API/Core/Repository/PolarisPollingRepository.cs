using BackgroundPolling.API.Contracts.Helpers;
using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using Dapper;
using System;
using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Repository
{
    public class PolarisPollingRepository : IPolarisPollingRepository
    {
        private readonly IBaseRepository<PolarisSecurityUser> _baseRepository;
        public PolarisPollingRepository(IBaseRepository<PolarisSecurityUser> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task UpsertSecurityUsersForAnalytics(DataTable dataTable)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                StoredProcedureMap.UpsertSecurityUsers,
                new
                {
                    securityUsers =
                        dataTable.AsTableValuedParameter(
                            "[polaris].[securityUserTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

        }

        public async Task UpsertSecurityUsersDataForBOSS(DataTable securityUsers, DataTable securityUsersWithFeatureAccess, DataTable securityUsersGeography)
        {
            await _baseRepository.Context.Connection.QueryAsync(
                StoredProcedureMap.UpsertSecurityUsersDataFromPolaris,
                new
                {
                    securityUsers = securityUsers.AsTableValuedParameter("[dbo].[securityUserTableType]"),
                    securityUsersWithFeatureAccess = securityUsersWithFeatureAccess.AsTableValuedParameter("[dbo].[securityUsersWithFeatureAccessTableType]"),
                    securityUsersGeography = securityUsersGeography.AsTableValuedParameter("[dbo].[securityUserGeographyTableType]")

                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

    }
}
