using BackgroundPolling.API.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinanceDataPollingController : ControllerBase
    {
        private readonly IFinanceDataPollingService _financeDataPollingService;

        public FinanceDataPollingController(IFinanceDataPollingService financeDataPollingService)
        {
            _financeDataPollingService = financeDataPollingService;
        }

        /// <summary>
        /// This API is used to upsert bill rates from FinAPI to bill rates table in Analytics database
        /// It also updates the cost data in the Analytics database tables for changed bill rates
        /// 
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPut]
        [Route("upsertBillRates")]
        public async Task<IActionResult> UpsertBillRates()
        {
            await _financeDataPollingService.UpsertBillRates();
            return Ok("Bill Rates Updated");
        }

        /// <summary>
        /// This API is used to upsert revenue transactions since last polled date from Revenue API and saves it in revenue transactions table in Analytics database
        /// 
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPut]
        [Route("upsertRevenueTransactions")]
        public async Task<IActionResult> UpsertRevenueTransactions()
        {
            await _financeDataPollingService.UpsertRevenueTransactions();
            return Ok("Revenue Transactions Updated");
        }

        /// <summary>
        /// This API is used to delete revenue transactions since last polled date from Revenue API and deletes it in revenue transactions table in Analytics database
        /// 
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPut]
        [Route("deleteRevenueTransactions")]
        public async Task<IActionResult> DeleteRevenueTransactionsById(DateTime? lastUpdated)
        {
            await _financeDataPollingService.DeleteRevenueTransactionsById(lastUpdated);
            return Ok("Revenue Transactions Deleted");
        }

        /// <summary>
        /// Save CCM office hierarchy to Analytics database
        /// 
        /// </summary>
        /// <returns>Office Hierarchy</returns>
        [HttpPost]
        [Route("saveOfficeListForTableau")]
        public async Task<IActionResult> SaveOfficeListForTableau()
        {
            var offices = await _financeDataPollingService.SaveOfficeListForTableau();
            return Ok(offices);
        }
    }
}