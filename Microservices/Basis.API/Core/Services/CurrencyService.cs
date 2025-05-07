using Basis.API.Contracts.RepositoryInterfaces;
using Basis.API.Contracts.Services;
using Basis.API.Models;
using Basis.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basis.API.Core.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyRepository _currencyRepository;
        public CurrencyService(ICurrencyRepository currencyRepository)
        {
            _currencyRepository = currencyRepository;

        }
        private IEnumerable<CurrencyViewModel> ConvertToCurrencyData(IEnumerable<CurrencyRates> currencyratesbyeffectivefromdate)
        {
            return currencyratesbyeffectivefromdate.Select(x => new CurrencyViewModel
            {
                CurrencyCode = x.CurrencyCode,
                CurrencyRateTypeCode = x.CurrencyRateTypeCode,
                EffectiveDate = x.EffectiveDate,
                UsdRate = x.UsdRate,
                ServiceCode = x.ServiceCode,
                CurrencyName = x.CurrencyName,
                CurrencyRateTypeName = x.CurrencyRateTypeName
            });
        }
        public async Task<IEnumerable<CurrencyViewModel>> GetCurrencyRatesByEffectiveDate(DateTime? effectiveFromDate)
        {
            var currencyratesbyeffectivefromdate = await _currencyRepository.GetCurrencyRatesByEffectiveDate(effectiveFromDate);
            return ConvertToCurrencyData(currencyratesbyeffectivefromdate);
        }
        public async Task<IEnumerable<CurrencyViewModel>> GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate(string currencyCodes,
            string currencyRateTypeCodes, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var currencyRates = await _currencyRepository.GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate(
                currencyCodes, currencyRateTypeCodes, effectiveFromDate, effectiveToDate);
            return ConvertToCurrencyData(currencyRates);
        }
    }
}
