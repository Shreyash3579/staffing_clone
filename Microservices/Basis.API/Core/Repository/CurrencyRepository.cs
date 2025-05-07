using Basis.API.Contracts.RepositoryInterfaces;
using Basis.API.Core.Helpers;
using Basis.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basis.API.Core.Repository
{
    public class CurrencyRepository :ICurrencyRepository
    {
        private readonly IBaseRepository<CurrencyRates> _baseRepository;

        public CurrencyRepository(IBaseRepository<CurrencyRates> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<CurrencyRates>> GetCurrencyRatesByEffectiveDate(DateTime? effectiveFromDate)
        {
            return await _baseRepository.GetAllAsync(new { effectiveFromDate }, StoredProcedureMap.GetCurrencyRatesByEffectiveDate);
        }

        public async Task<IEnumerable<CurrencyRates>> GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate(string currencyCodes,
            string currencyRateTypeCodes, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            return await _baseRepository.GetAllAsync(new 
            {
                currencyCodes,
                currencyRateTypeCodes,
                effectiveFromDate,
                effectiveToDate
            }, StoredProcedureMap.GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate);
        }
    }
}
