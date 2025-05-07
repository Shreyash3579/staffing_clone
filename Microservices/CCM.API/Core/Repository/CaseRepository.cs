using CCM.API.Contracts.RepositoryInterfaces;
using CCM.API.Core.Helpers;
using CCM.API.Models;
using CCM.API.ViewModels;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CCM.API.Core.Repository
{
    public class CaseRepository : ICaseRepository
    {
        private readonly IBaseRepository<Case> _baseRepository;

        public CaseRepository(IBaseRepository<Case> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task<IEnumerable<Case>> GetNewDemandCasesByOffices(DateTime startDate, DateTime endDate, string officeCodes, string caseTypeCodes, string clientCodes)
        {
            var cases = await
                _baseRepository.GetAllAsync(new { startDate, endDate, officeCodes, caseTypeCodes, clientCodes },
                    StoredProcedureMap.GetNewDemandCasesByOffices);

            return cases;
        }
        public async Task<IEnumerable<Case>> GetActiveCasesExceptNewDemandsByOffices(DateTime startDate, DateTime endDate, string officeCodes, string caseTypeCodes, string clientCodes)
        {
            var cases = await
                _baseRepository.GetAllAsync(new { startDate, endDate, officeCodes, caseTypeCodes, clientCodes },
                    StoredProcedureMap.GetActiveCasesExceptNewDemandsByOffices);

            return cases;
        }

        public async Task<IEnumerable<Case>> GetCaseDetailsByCaseCodes(string oldCaseCodes)
        {
            var casesData = await _baseRepository.GetAllAsync(new { oldCaseCodes }, StoredProcedureMap.GetCaseDetailsByCaseCodes);

            return casesData;
        }

        public async Task<IEnumerable<Case>> GetCaseDataByCaseCodes(string oldCaseCodes)
        {
            var casesData = await _baseRepository.GetAllAsync(new { oldCaseCodes }, StoredProcedureMap.GetCaseDataByCaseCodes);

            return casesData;
        }


        public async Task<IEnumerable<Case>> GetCasesForTypeahead(string searchString)
        {
            var cases = await _baseRepository.GetAllAsync(new { searchString }, StoredProcedureMap.GetCasesForTypeahead);
            return cases;
        }

        public async Task<IEnumerable<Case>> GetCasesEndingBySpecificDate(int caseEndsBeforeNumberOfDays)
        {
            var cases = await _baseRepository.GetAllAsync(new { caseEndsBeforeNumberOfDays },
                StoredProcedureMap.GetCasesEndingBySpecificDate);
            return cases;
        }

        public async Task<IEnumerable<Case>> GetCasesWithTaxonomiesByCaseCodes(string oldCaseCodeList)
        {
            var cases = await _baseRepository.GetAllAsync(new { oldCaseCodeList },
                StoredProcedureMap.GetCasesWithTaxonomiesByCaseCodes);
            return cases;
        }

        public async Task<IEnumerable<Case>> GetCasesActiveAfterSpecifiedDate(DateTime date)
        {
            var cases = await _baseRepository.GetAllAsync(new { date },
                StoredProcedureMap.GetCasesActiveAfterSpecifiedDate);
            return cases;
        }

        public async Task<IEnumerable<Case>> GetCasesWithStartOrEndDateUpdatedInCCM(string columnName, DateTime? lastPollDateTime)
        {
            var cases = await _baseRepository.GetAllAsync(new { columnName, lastPollDateTime },
                StoredProcedureMap.GetCasesWithStartOrEndDateUpdatedInCCM);
            return cases;
        }

        public async Task<CaseMasterViewModel> GetCaseMasterAndCaseMasterHistoryChangesSinceLastPolled(DateTime? lastPolledDateTime)
        {
            var result = await _baseRepository.Context.Connection.QueryMultipleAsync(
               StoredProcedureMap.GetCaseMasterAndCaseMasterHistoryChangesSinceLastPolled,
               new
               {
                   lastPolledDateTime
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

            var caseMasterData = result.Read<CaseMaster>().ToList();
            var caseMasterHistoryData = result.Read<CaseMasterHistory>().ToList();

            var caseMasterAndCaseMasterHistoryDataChanges = ConvertToCaseMasterViewModel(caseMasterData, caseMasterHistoryData);
            return caseMasterAndCaseMasterHistoryDataChanges;
        }

        public async Task<IEnumerable<CaseAdditionalInfo>> GetCaseAdditionalInfo(DateTime? lastUpdated)
        {
            var caseAdditionalInfos = await _baseRepository.Context.Connection.QueryAsync<CaseAdditionalInfo>(
               StoredProcedureMap.GetCaseAdditionalInfo,
               new
               {
                   lastUpdated
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return caseAdditionalInfos;
        }

        #region private methods
        private CaseMasterViewModel ConvertToCaseMasterViewModel(List<CaseMaster> caseMasterData, List<CaseMasterHistory> caseMasterHistoryData)
        {
            var caseMasterViewModel = new CaseMasterViewModel
            {
                CaseMaster = caseMasterData,
                CaseMasterHistory = caseMasterHistoryData
            };

            return caseMasterViewModel;
        }

        #endregion
    }
}
