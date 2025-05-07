using Hangfire;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class ScheduleMasterPlaceholderService : IScheduleMasterPlaceholderService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IScheduleMasterPlaceholderRepository _placeholderRepository;

        public ScheduleMasterPlaceholderService(IScheduleMasterPlaceholderRepository placeholderRepository, IBackgroundJobClient backgroundJobClient)
        {
            _placeholderRepository = placeholderRepository;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<IEnumerable<ResourceAllocationViewModel>> GetPlaceholderAllocationsByPlaceholderScheduleIds(
            string placeholderScheduleIds)
        {
            if (string.IsNullOrEmpty(placeholderScheduleIds))
                throw new ArgumentException("Error while getting resources data. PlaceholderScheduleIds cannot be null or empty.");

            var allocations = await _placeholderRepository.GetPlaceholderAllocationsByPlaceholderScheduleIds(placeholderScheduleIds);

            return ConvertToResourceAllocationViewModel(allocations);
        }

        public async Task<IEnumerable<ResourceAllocationViewModel>> GetPlaceholderAllocationsByCaseCodes(
            string oldCaseCodes)
        {
            if (string.IsNullOrEmpty(oldCaseCodes))
                throw new ArgumentException("Error while getting resource data. Case Codes cannot be null or empty.");
            var allocations = await _placeholderRepository.GetPlaceholderAllocationsByCaseCodes(oldCaseCodes);

            return ConvertToResourceAllocationViewModel(allocations);
        }

        public async Task<IEnumerable<ResourceAllocationViewModel>> GetPlaceholderAllocationsByPipelineIds(
            string pipelineIds)
        {
            if (string.IsNullOrEmpty(pipelineIds))
                throw new ArgumentException("Error while getting resource data. PipelineIds cannot be null or empty.");

            var allocations = await _placeholderRepository.GetPlaceholderAllocationsByPipelineIds(pipelineIds);

            return ConvertToResourceAllocationViewModel(allocations);
        }

        public async Task DeletePlaceholderAllocationsByIds(string placeholderIds, string lastUpdatedBy)
        {
            if (placeholderIds == null) throw new ArgumentException("Ids cannot be null or empty");
            if (string.IsNullOrEmpty(lastUpdatedBy))
                throw new ArgumentException("lastUpdatedBy cannot be null or empty");
            await _placeholderRepository.DeletePlaceholderAllocationsByIds(placeholderIds, lastUpdatedBy);

            _backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x =>
                x.DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(placeholderIds));
            return;
        }

        public async Task<IEnumerable<ScheduleMasterPlaceholder>> UpsertPlaceholderAllocation(IEnumerable<ScheduleMasterPlaceholder> placeholderAllocations)
        {
            if (placeholderAllocations == null || !placeholderAllocations.Any()) throw new ArgumentException("placeholder allocation cannot be null or empty");

            if (placeholderAllocations.Any( x => string.IsNullOrEmpty(x.OldCaseCode) && !x.PipelineId.HasValue && !x.PlanningCardId.HasValue)) throw new ArgumentException("placeholder allocation should be assigned on case or opportunity or planning card");

            var placeholderAllocationDataTable = CreatePlaceholderAllocationDataTable(placeholderAllocations);
            var savedAllocations =
                await _placeholderRepository.UpsertPlaceholderAllocation(placeholderAllocationDataTable);   
            
            
            // add call to get planning card ids and then call to analytics api to create placeholder analytics report



            /* save either forecast data or placeholder allocations in analytics db*/
            var placeholderAllocationsForAnalytics = savedAllocations.Where(x => !string.IsNullOrEmpty(x.ServiceLineCode)
                                                                                    && !string.IsNullOrEmpty(x.CurrentLevelGrade)
                                                                                    && x.OperatingOfficeCode != null
                                                                                    && (!string.IsNullOrEmpty(x.OldCaseCode) || x.PipelineId != null)
                                                                                    && (x.IsConfirmed == null || !(bool)x.IsConfirmed));

            var placeholderAllocationsToIncludeForPlanningCards = savedAllocations.Where(x => string.IsNullOrEmpty(x.OldCaseCode) && x.PlanningCardId != null && !string.IsNullOrEmpty(x.EmployeeCode) && !(bool)x.IsPlaceholderAllocation);
            placeholderAllocationsForAnalytics = placeholderAllocationsForAnalytics.Concat(placeholderAllocationsToIncludeForPlanningCards);

            var placeholderScheduleIds = string.Join(",",placeholderAllocationsForAnalytics.Select(x => x.Id).Distinct());
            _backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x => x.CreatePlaceholderAnalyticsReport(placeholderScheduleIds));
        
            /* when forecast data deleted, placeholder allocations from analytics db should also get deleted */
            var allocationIdsToBeDeleted = string.Join(',', placeholderAllocations.Where(x => !placeholderScheduleIds.Contains(x.Id.ToString())).Select(x => x.Id).Distinct());

            if (allocationIdsToBeDeleted.Count() > 0)
            {
                _backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x => x.DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(allocationIdsToBeDeleted));
            }          

            return savedAllocations;
        }

        public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderPlanningCardAllocationsWithinDateRange(string employeeCodes, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return Enumerable.Empty<ScheduleMasterPlaceholder>();
            var allocations = await _placeholderRepository.GetPlaceholderPlanningCardAllocationsWithinDateRange(employeeCodes, startDate, endDate);
            return allocations;
        }


        public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByEmployeeCodes(
            string employeeCodes,
            DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes)) return Enumerable.Empty<ScheduleMasterPlaceholder>();
            var allocations =
                await _placeholderRepository.GetPlaceholderAllocationsByEmployeeCodes(employeeCodes, startDate,
                    endDate);
            return allocations;
        }

        public async Task<IEnumerable<ResourceAllocationViewModel>> GetPlaceholderAllocationsByPlanningCardIds(
            string planningCardIds)
        {
            if (string.IsNullOrEmpty(planningCardIds))
                return Enumerable.Empty<ResourceAllocationViewModel>();
            var allocations = await _placeholderRepository.GetPlaceholderAllocationsByPlanningCardIds(planningCardIds);

            return ConvertToResourceAllocationViewModel(allocations);
        }

        public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetAllocationsByPlanningCardIds(
        string planningCardIds)
        {
            if (string.IsNullOrEmpty(planningCardIds))
                return Enumerable.Empty<ScheduleMasterPlaceholder>();
            var allocations = await _placeholderRepository.GetAllocationsByPlanningCardIds(planningCardIds);

            return allocations;
        }

        #region Private Methods

        private static DataTable CreatePlaceholderAllocationDataTable(IEnumerable<ScheduleMasterPlaceholder> records)
        {
            var placeholderAllocationsDataTable = new DataTable();
            placeholderAllocationsDataTable.Columns.Add("id", typeof(Guid));
            placeholderAllocationsDataTable.Columns.Add("planningCardId", typeof(Guid));
            placeholderAllocationsDataTable.Columns.Add("clientCode", typeof(int));
            placeholderAllocationsDataTable.Columns.Add("caseCode", typeof(int));
            placeholderAllocationsDataTable.Columns.Add("oldCaseCode", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("caseName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("clientName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("opportunityName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("caseTypeCode", typeof(short));
            placeholderAllocationsDataTable.Columns.Add("caseTypeName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("employeeCode", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("employeeName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("serviceLineCode", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("serviceLineName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("operatingOfficeCode", typeof(short));
            placeholderAllocationsDataTable.Columns.Add("operatingOfficeName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("operatingOfficeAbbreviation", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("currentLevelGrade", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("allocation", typeof(short));
            placeholderAllocationsDataTable.Columns.Add("startDate", typeof(DateTime));
            placeholderAllocationsDataTable.Columns.Add("endDate", typeof(DateTime));
            placeholderAllocationsDataTable.Columns.Add("pipelineId", typeof(Guid));
            placeholderAllocationsDataTable.Columns.Add("investmentCode", typeof(short));
            placeholderAllocationsDataTable.Columns.Add("investmentName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("caseRoleCode", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("caseRoleName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("lastUpdated", typeof(DateTime));
            placeholderAllocationsDataTable.Columns.Add("lastUpdatedBy", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("notes", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("isConfirmed", typeof(bool));
            placeholderAllocationsDataTable.Columns.Add("isPlaceholderAllocation", typeof(bool));
            placeholderAllocationsDataTable.Columns.Add("billingOfficeCode", typeof(short));
            placeholderAllocationsDataTable.Columns.Add("billingOfficeName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("billingOfficeAbbreviation", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("managingOfficeCode", typeof(short));
            placeholderAllocationsDataTable.Columns.Add("managingOfficeName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("managingOfficeAbbreviation", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("commitmentTypeCode", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("commitmentTypeName", typeof(string));
            placeholderAllocationsDataTable.Columns.Add("positionGroupCode", typeof(string));
            

            foreach (var record in records)
            {
                var row = placeholderAllocationsDataTable.NewRow();
                row["id"] = (object)record.Id ?? DBNull.Value;
                row["planningCardId"] = (object)record.PlanningCardId ?? DBNull.Value;
                row["clientCode"] = (object)record.ClientCode ?? DBNull.Value;
                row["caseCode"] = (object)record.CaseCode ?? DBNull.Value;
                row["oldCaseCode"] = (object)record.OldCaseCode ?? DBNull.Value;
                row["caseName"] = (object)record.CaseName ?? DBNull.Value;
                row["clientName"] = (object)record.ClientName ?? DBNull.Value;
                row["opportunityName"] = (object)record.OpportunityName ?? DBNull.Value;
                row["caseTypeCode"] = (object)record.CaseTypeCode ?? DBNull.Value;
                row["caseTypeName"] = (object)record.CaseTypeName ?? DBNull.Value;
                row["employeeCode"] = (object)record.EmployeeCode ?? DBNull.Value;
                row["employeeName"] = (object)record.EmployeeName ?? DBNull.Value;
                row["serviceLineCode"] = (object)record.ServiceLineCode ?? DBNull.Value;
                row["serviceLineName"] = (object)record.ServiceLineName ?? DBNull.Value;
                row["operatingOfficeCode"] = (object)record.OperatingOfficeCode ?? DBNull.Value;
                row["operatingOfficeName"] = (object)record.OperatingOfficeName ?? DBNull.Value;
                row["operatingOfficeAbbreviation"] = (object)record.OperatingOfficeAbbreviation ?? DBNull.Value;
                row["currentLevelGrade"] = (object)record.CurrentLevelGrade ?? DBNull.Value;
                row["allocation"] = (object)record.Allocation ?? DBNull.Value;
                row["startDate"] = (object)record.StartDate ?? DBNull.Value;
                row["endDate"] = (object)record.EndDate ?? DBNull.Value;
                row["pipelineId"] = (object)record.PipelineId ?? DBNull.Value;
                row["investmentCode"] = (object)record.InvestmentCode ?? DBNull.Value;
                row["investmentName"] = (object)record.InvestmentName ?? DBNull.Value;
                row["caseRoleCode"] = (object)record.CaseRoleCode ?? DBNull.Value;
                row["caseRoleName"] = (object)record.CaseRoleName ?? DBNull.Value;
                row["lastUpdated"] = (object)record.LastUpdated ?? DBNull.Value;
                row["lastUpdatedBy"] = (object)record.LastUpdatedBy ?? DBNull.Value;
                row["notes"] = (object)record.Notes ?? DBNull.Value;
                row["isConfirmed"] = (object)record.IsConfirmed ?? DBNull.Value;
                row["isPlaceholderAllocation"] = (object)record.IsPlaceholderAllocation ?? DBNull.Value;
                row["billingOfficeCode"] = (object)record.BillingOfficeCode ?? DBNull.Value;
                row["billingOfficeName"] = (object)record.BillingOfficeName ?? DBNull.Value;
                row["billingOfficeAbbreviation"] = (object)record.BillingOfficeAbbreviation ?? DBNull.Value;
                row["managingOfficeCode"] = (object)record.ManagingOfficeCode ?? DBNull.Value;
                row["managingOfficeName"] = (object)record.ManagingOfficeName ?? DBNull.Value;
                row["managingOfficeAbbreviation"] = (object)record.ManagingOfficeAbbreviation ?? DBNull.Value;
                row["commitmentTypeCode"] = (object)record.CommitmentTypeCode ?? DBNull.Value;
                row["commitmentTypeName"] = (object)record.CommitmentTypeName ?? DBNull.Value;
                row["positionGroupCode"] = (object)record.PositionGroupCode ?? DBNull.Value;
                placeholderAllocationsDataTable.Rows.Add(row);
            }
            return placeholderAllocationsDataTable;
        }

        private static IEnumerable<ResourceAllocationViewModel> ConvertToResourceAllocationViewModel(
            IEnumerable<ScheduleMasterPlaceholder> allocations)
        {
            var viewModel = allocations?.Select(item => new ResourceAllocationViewModel
            {
                Id = item.Id,
                CaseCode = item.CaseCode,
                ClientCode = item.ClientCode,
                OldCaseCode = item.OldCaseCode,
                PipelineId = item.PipelineId,
                PlanningCardId = item.PlanningCardId,
                EmployeeCode = item.EmployeeCode,
                CurrentLevelGrade = item.CurrentLevelGrade,
                OperatingOfficeCode = item.OperatingOfficeCode,
                ServiceLineCode = item.ServiceLineCode,
                Allocation = item.Allocation,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                InvestmentCode = item.InvestmentCode,
                CaseRoleCode = item.CaseRoleCode,
                LastUpdatedBy = item.LastUpdatedBy,
                LastUpdated = item.LastUpdated,
                IsPlaceholderAllocation = item.IsPlaceholderAllocation ?? true,
                Notes = item.Notes,
                CommitmentTypeCode = item.CommitmentTypeCode,
                PositionGroupCode = item.PositionGroupCode
            });

            return viewModel;
        }
        #endregion
    }
}
