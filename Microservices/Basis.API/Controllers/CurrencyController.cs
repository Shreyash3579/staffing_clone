using Basis.API.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Basis.API.Core.Helpers;

namespace Basis.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.BasisAllAccess)]
    public class CurrencyController : ControllerBase
    {
        public readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }
        /// <summary>
        /// get currency rates as per effective from date
        /// </summary>
        /// <param name="effectiveFromDate">effective from date of currency rate</param>
        /// <returns>collective of currency rates with their respective currency codes</returns>
        [HttpGet("getcurrencyratesbyeffectivefromdate")]
        public async Task<IActionResult> GetCurrencyRatesByEffectiveDate(DateTime? effectiveFromDate)
        {
            var result = await _currencyService.GetCurrencyRatesByEffectiveDate(effectiveFromDate);
            return Ok(result);
        }

        /// <summary>
        /// Get currency rates by vurrency codes between effective dates
        /// </summary>
        /// <param name="currencyCodes"></param>
        /// <param name="currencyRateTypeCodes"></param>
        /// <param name="effectiveFromDate"></param>
        /// <param name="effectiveToDate"></param>
        /// <returns></returns>
        [HttpGet("GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate")]
        public async Task<IActionResult> GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate(string currencyCodes,
            string currencyRateTypeCodes, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var result = await _currencyService.GetCurrencyRatesByCurrencyCodesBetweenEffectiveDate(currencyCodes, currencyRateTypeCodes, effectiveFromDate, effectiveToDate);
            return Ok(result);
        }

    }
}
