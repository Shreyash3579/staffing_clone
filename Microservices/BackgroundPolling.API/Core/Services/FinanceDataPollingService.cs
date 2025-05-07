using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class FinanceDataPollingService : IFinanceDataPollingService
    {
        private readonly IFinanceDataPollingRepository _financeDataPollingRepository;
        private readonly ICcmApiClient _ccmApiClient;
        private readonly IRevenueApiClient _revenueApiClient;

        public FinanceDataPollingService(IFinanceDataPollingRepository financeDataPollingRepository, 
            IRevenueApiClient revenueApiClient, ICcmApiClient ccmApiClient)
        {
            _financeDataPollingRepository = financeDataPollingRepository;
            _revenueApiClient = revenueApiClient;
            _ccmApiClient = ccmApiClient;
        }

        public async Task UpsertBillRates()
        {
            var billRatesTask = _ccmApiClient.GetBillRates();
            var lastUpdatedBillRateDateTask = _financeDataPollingRepository.GetLastUpdatedBillRateDate();

            await Task.WhenAll(billRatesTask, lastUpdatedBillRateDateTask);
            var lastUpdatedBillRateDate = lastUpdatedBillRateDateTask.Result;
            var billRates = billRatesTask.Result;
            // Save whole set of bill rates into BOSS as there are records which might get deleted
            // along with insert and update
            var billRatesDTO = ConvertToBillRateDTO(billRates);
            await _financeDataPollingRepository.UpsertBillRates(billRatesDTO);
            var updatedBillRates = billRates?.Where(r => Convert.ToDateTime(r.LastUpdated) > lastUpdatedBillRateDate).ToList();
            if (updatedBillRates != null && updatedBillRates.Count > 0)
            {
                var monthlyCalculatedBillRatesDTO = ConvertToAnalystBillRateDTO(updatedBillRates);
                await _financeDataPollingRepository.UpdateCostForUpdatedBillRate(monthlyCalculatedBillRatesDTO);
            }
        }

        public async Task UpsertRevenueTransactions()
        {

            var lastUpdatedRevenueTransactionDate = await _financeDataPollingRepository.GetLastUpdatedRevenueTransactionDate();
            var revenueTransactions = await _revenueApiClient.GetRevenueTransactions(lastUpdatedRevenueTransactionDate);

            var revenueTransactionDTO = ConvertToRevenueTransactionDTO(revenueTransactions);
            await _financeDataPollingRepository.UpsertRevenueTransactions(revenueTransactionDTO);
        }

        public async Task DeleteRevenueTransactionsById(DateTime? lastUpdated)
        {
            var lastUpdatedRevenueTransactionDate = lastUpdated;
            if (lastUpdatedRevenueTransactionDate == null)
            {
                lastUpdatedRevenueTransactionDate = await _financeDataPollingRepository.GetLastUpdatedRevenueTransactionDate();
            }
            var deletedRevenueTransactions = await _revenueApiClient.GetDeletedRevenueTransactions((DateTime)lastUpdatedRevenueTransactionDate);
            var ids = string.Join(",", deletedRevenueTransactions.Select(x => x.RevId.ToString()).Distinct());
            await _financeDataPollingRepository.DeleteRevenueTransactionsById(ids);
        }


        public async Task<IEnumerable<RevOffice>> SaveOfficeListForTableau()
        {
            var officeFlatList = await
                   _ccmApiClient.GetRevOfficeList();

            if (officeFlatList.Any())
            {
                var officeDataTable = ConvertToOfficeDTO(officeFlatList);

                var savedOffices = await _financeDataPollingRepository.SaveOfficeListForTableau(officeDataTable);
                return savedOffices;
            }
            return Enumerable.Empty<RevOffice>();
        }



        private DataTable ConvertToOfficeDTO(IEnumerable<RevOffice> officeList)
        {
            var officeDataTable = new DataTable();
            officeDataTable.Columns.Add("officeLevel", typeof(int));
            officeDataTable.Columns.Add("officeCode", typeof(int));
            officeDataTable.Columns.Add("officeName", typeof(string));
            officeDataTable.Columns.Add("parentOfficeCode", typeof(int));
            officeDataTable.Columns.Add("currencyCode", typeof(string));
            officeDataTable.Columns.Add("officeHead", typeof(string));
            officeDataTable.Columns.Add("entityTypeCode", typeof(string));
            officeDataTable.Columns.Add("officeAbbreviation", typeof(string));
            officeDataTable.Columns.Add("officeStatus", typeof(string));
            officeDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var office in officeList)
            {
                var row = officeDataTable.NewRow();
                row["officeLevel"] = (object)office.OfficeLevel ?? DBNull.Value;
                row["officeCode"] = (object)office.OfficeCode ?? DBNull.Value;
                row["officeName"] = (object)office.OfficeName ?? DBNull.Value;
                row["parentOfficeCode"] = (object)office.ParentOfficeCode ?? DBNull.Value;
                row["currencyCode"] = (object)office.CurrencyCode ?? DBNull.Value;
                row["officeHead"] = (object)office.OfficeHead ?? DBNull.Value;
                row["entityTypeCode"] = (object)office.EntityTypeCode ?? DBNull.Value;
                row["officeAbbreviation"] = (object)office.OfficeAbbreviation ?? DBNull.Value;
                row["officeStatus"] = (object)office.OfficeStatus ?? DBNull.Value;
                row["lastUpdatedBy"] = "Auto-REVOffice";

                officeDataTable.Rows.Add(row);
            }

            return officeDataTable;
        }

        private DataTable ConvertToAnalystBillRateDTO(IEnumerable<BillRate> updatedBillRates)
        {
            var billRateDataTable = new DataTable();

            billRateDataTable.Columns.Add("billRate", typeof(decimal));
            billRateDataTable.Columns.Add("billCode", typeof(decimal));
            billRateDataTable.Columns.Add("billRateType", typeof(string));
            billRateDataTable.Columns.Add("billRateCurrency", typeof(string));
            billRateDataTable.Columns.Add("levelGrade", typeof(string));
            billRateDataTable.Columns.Add("officeCode", typeof(short));
            billRateDataTable.Columns.Add("startDate", typeof(DateTime));
            billRateDataTable.Columns.Add("endDate", typeof(DateTime));
            billRateDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var billRate in updatedBillRates)
            {
                var row = billRateDataTable.NewRow();
                row["billRate"] = billRate.Rate;
                row["billCode"] = billRate.BillCode;
                row["billRateType"] = (object)billRate.Type?.Trim() ?? DBNull.Value;
                row["billRateCurrency"] = (object)billRate.Currency?.Trim() ?? DBNull.Value;
                row["levelGrade"] = (object)billRate.LevelGrade?.Trim() ?? DBNull.Value;
                row["officeCode"] = Convert.ToInt16((object)billRate.OfficeCode);
                row["startDate"] = billRate.StartDate;
                row["endDate"] = (object)billRate.EndDate ?? DBNull.Value;
                row["lastUpdatedBy"] = "Auto-billRate";
                billRateDataTable.Rows.Add(row);
            }
            return billRateDataTable;
        }

        private DataTable ConvertToBillRateDTO(IEnumerable<BillRate> billRates)
        {
            var billRateDataTable = new DataTable();
            if (!billRates.Any()) return billRateDataTable;

            billRateDataTable.Columns.Add("billRateId", typeof(Guid));
            billRateDataTable.Columns.Add("officeCode", typeof(string));
            billRateDataTable.Columns.Add("levelGrade", typeof(string));
            billRateDataTable.Columns.Add("billCode", typeof(decimal));
            billRateDataTable.Columns.Add("type", typeof(string));
            billRateDataTable.Columns.Add("currency", typeof(string));
            billRateDataTable.Columns.Add("rate", typeof(decimal));
            billRateDataTable.Columns.Add("breakdown", typeof(string));
            billRateDataTable.Columns.Add("startDate", typeof(DateTime));
            billRateDataTable.Columns.Add("endDate", typeof(DateTime));
            billRateDataTable.Columns.Add("status", typeof(string));
            billRateDataTable.Columns.Add("lastUpdated", typeof(DateTime));
            billRateDataTable.Columns.Add("lastUpdatedBy", typeof(string));

            foreach (var billRate in billRates)
            {
                var row = billRateDataTable.NewRow();
                row["billRateId"] = (object)billRate.Id ?? DBNull.Value;
                row["officeCode"] = (object)billRate.OfficeCode?.Trim() ?? DBNull.Value;
                row["levelGrade"] = (object)billRate.LevelGrade?.Trim() ?? DBNull.Value;
                row["billCode"] = (object)billRate.BillCode ?? DBNull.Value;
                row["type"] = (object)billRate.Type?.Trim() ?? DBNull.Value;
                row["currency"] = (object)billRate.Currency?.Trim() ?? DBNull.Value;
                row["rate"] = (object)billRate.Rate ?? DBNull.Value;
                row["breakdown"] = (object)billRate.Breakdown?.Trim() ?? DBNull.Value;
                row["startDate"] = (object)billRate.StartDate ?? DBNull.Value;
                row["endDate"] = (object)billRate.EndDate ?? DBNull.Value;
                row["status"] = (object)billRate.Status?.Trim() ?? DBNull.Value;
                row["lastUpdated"] = (object)billRate.LastUpdated ?? DBNull.Value;
                row["lastUpdatedBy"] = "Auto-billRate";
                billRateDataTable.Rows.Add(row);
            }
            return billRateDataTable;
        }

        private DataTable ConvertToRevenueTransactionDTO(IEnumerable<RevenueTransaction> revenueTransactions)
        {
            var revenueTransactionDataTable = new DataTable();
            if (!revenueTransactions.Any()) return revenueTransactionDataTable;

            revenueTransactionDataTable.Columns.Add("revId", typeof(Guid));
            revenueTransactionDataTable.Columns.Add("clientId", typeof(int));
            revenueTransactionDataTable.Columns.Add("caseId", typeof(int));
            revenueTransactionDataTable.Columns.Add("pipelineId", typeof(Guid));
            revenueTransactionDataTable.Columns.Add("serviceLineCode", typeof(string));
            revenueTransactionDataTable.Columns.Add("transactionType", typeof(string));
            revenueTransactionDataTable.Columns.Add("transferType", typeof(string));
            revenueTransactionDataTable.Columns.Add("servedOfficeCode", typeof(int));
            revenueTransactionDataTable.Columns.Add("feeDate", typeof(DateTime));
            revenueTransactionDataTable.Columns.Add("probability", typeof(decimal));
            revenueTransactionDataTable.Columns.Add("transactionCurrency", typeof(string));
            revenueTransactionDataTable.Columns.Add("transactionAmount", typeof(decimal));
            revenueTransactionDataTable.Columns.Add("isFinancial", typeof(bool));
            revenueTransactionDataTable.Columns.Add("isMgmtActivity", typeof(bool));
            revenueTransactionDataTable.Columns.Add("isMgmtCash", typeof(bool));
            revenueTransactionDataTable.Columns.Add("atRiskType", typeof(string));
            revenueTransactionDataTable.Columns.Add("revenueDetail", typeof(string));
            revenueTransactionDataTable.Columns.Add("lastUpdatedBy", typeof(string));
            revenueTransactionDataTable.Columns.Add("lastUpdated", typeof(DateTime));

            foreach (var revenueTransaction in revenueTransactions)
            {
                var row = revenueTransactionDataTable.NewRow();
                row["revId"] = (object)revenueTransaction.RevId ?? DBNull.Value;
                row["clientId"] = (object)revenueTransaction.ClientId ?? DBNull.Value;
                row["caseId"] = (object)revenueTransaction.CaseId ?? DBNull.Value;
                row["pipelineId"] = (object)revenueTransaction.PipelineId ?? DBNull.Value;
                row["serviceLineCode"] = (object)revenueTransaction.ServiceLineCode ?? DBNull.Value;
                row["transactionType"] = (object)revenueTransaction.transactionAmount ?? DBNull.Value;
                row["transferType"] = (object)revenueTransaction.TransferType ?? DBNull.Value;
                row["servedOfficeCode"] = (object)revenueTransaction.ServedOfficeCode ?? DBNull.Value;
                row["feeDate"] = (object)revenueTransaction.FeeDate ?? DBNull.Value;
                row["probability"] = (object)revenueTransaction.Probability ?? DBNull.Value;
                row["transactionCurrency"] = (object)revenueTransaction.TransactionCurrency?.Trim() ?? DBNull.Value;
                row["transactionAmount"] = (object)revenueTransaction.transactionAmount ?? DBNull.Value;
                row["isFinancial"] = (object)revenueTransaction.IsFinancial ?? DBNull.Value;
                row["isMgmtActivity"] = (object)revenueTransaction.IsMgmtActivity ?? DBNull.Value;
                row["isMgmtCash"] = (object)revenueTransaction.IsMgmtCash ?? DBNull.Value;
                row["atRiskType"] = (object)revenueTransaction.AtRiskType ?? DBNull.Value;
                row["revenueDetail"] = (object)revenueTransaction.RevenueDetail ?? DBNull.Value;
                row["lastUpdatedBy"] = (object)revenueTransaction.LastUpdatedBy ?? DBNull.Value;
                row["lastUpdated"] = (object)revenueTransaction.LastUpdated ?? DBNull.Value;
                revenueTransactionDataTable.Rows.Add(row);
            }
            return revenueTransactionDataTable;
        }
    }
}
