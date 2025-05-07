using BackgroundPolling.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Repository
{
    public interface ICCMPollingRepository
    {
        Task<IList<Guid>> GetOpportunitiesNotConvertedToCase();
        Task<IList<Guid>> GetOpportunitiesPinnedByUsers();
        Task UpdateSkuTermsForOpportunitiesConvertedToCase(DataTable opportunityCaseMapDataTable);
        Task UpdateUserPreferencesForOpportunitiesConvertedToCase(DataTable opportunityCaseMapDataTable);
        Task UpdateRingfenceForOpportunitiesConvertedToCase(DataTable opportunityCaseMapDataTable);
        Task UpsertCaseMasterAndCaseMasterHistory(DataTable caseMasterDataTable, DataTable caseMasterHistoryDataTable, DateTime? lastPolledDateTime);
        Task UpsertCaseAdditionalInfo(DataTable caseAdditionalInfoDataTable, bool isFullLoad);
        Task UpsertCaseAdditionalInfoInBasis(DataTable caseAdditionalInfoDataTable, bool isFullLoad);
        Task UpsertCurrencyRates(DataTable currencyDataTable);
        Task UpsertCaseAttributes(DataTable caseAttributeDataTable);
        Task<IEnumerable<CurrencyRate>> GetCurrencyRatesChangedRecently(DateTime lastUpdated);
        Task UpdateUSDCostForChangeInCurrencyRate(DataTable currencyDataTable);
    }
}
