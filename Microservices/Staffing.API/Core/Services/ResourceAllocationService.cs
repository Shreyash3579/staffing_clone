using Hangfire;
using Newtonsoft.Json.Linq;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Core.Helpers;
using Staffing.API.Core.Repository;
using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using File = System.IO.File;

namespace Staffing.API.Core.Services
{
    public class ResourceAllocationService : IResourceAllocationService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IResourceAllocationRepository _resourceAllocationRepository;
        private readonly IResourceApiClient _resourceApiClient;
        private readonly ICommitmentRepository _commitmentRepository;

        public ResourceAllocationService(IResourceAllocationRepository resourceAllocationRepository,
            IBackgroundJobClient backgroundJobClient, IResourceApiClient resourceApiClient, ICommitmentRepository commitmentRepository)
        {
            _resourceAllocationRepository = resourceAllocationRepository;
            _backgroundJobClient = backgroundJobClient;
            _resourceApiClient = resourceApiClient;
            _commitmentRepository = commitmentRepository;
        }

        public async Task<IEnumerable<ResourceAllocation>> UpsertResourceAllocations(
            IEnumerable<ResourceAllocation> resourceAllocations)
        {
            if (resourceAllocations == null || !resourceAllocations.Any()) throw new ArgumentException("resourceAllocations cannot be null or empty");

            var employeeCodes = string.Join(",", resourceAllocations.Select(r => r.EmployeeCode).Distinct());

            var commitmentData = await _commitmentRepository.GetCommitmentBySelectedValues("NB", employeeCodes, null,null,null); //get all non -billable commitments for the employees

            foreach (var allocation in resourceAllocations)
            {
                var overlappingNonBillableCommitment = commitmentData.FirstOrDefault(commitment =>
                    commitment.EmployeeCode == allocation.EmployeeCode &&
                    commitment.StartDate <= allocation.EndDate &&
                    commitment.EndDate >= allocation.StartDate);

                if (overlappingNonBillableCommitment != null)   //if there is an overlapping non billable commitment, set the investment  to Internal PD
                {
                    allocation.InvestmentCode = 5; 
                    allocation.InvestmentName = "Internal PD"; 
                }
            }


            var resourceAllocationsDataTable = CreateAssignmentDataTable(resourceAllocations);
            var allocatedResources =
                await _resourceAllocationRepository.UpsertResourceAllocations(resourceAllocationsDataTable);

            var scheduleIds = string.Join(",", allocatedResources.Select(r => r.Id).Distinct());

            _backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x =>
                x.CreateAnalyticsReport(scheduleIds));

            return resourceAllocations;
        }

        public async Task DeleteResourceAllocationById(Guid id, string lastUpdatedBy)
        {
            if (id == Guid.Empty) throw new ArgumentException("Id cannot be null or empty");
            if (string.IsNullOrEmpty(lastUpdatedBy))
                throw new ArgumentException("lastUpdatedBy cannot be null or empty");
            await _resourceAllocationRepository.DeleteResourceAllocationById(id, lastUpdatedBy);

            _backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x =>
                x.DeleteAnalyticsDataForDeletedAllocationByScheduleIds(id.ToString()));

            return;
        }

        public async Task DeleteResourceAllocationByIds(string ids, string lastUpdatedBy)
        {
            if (string.IsNullOrEmpty(ids)) throw new ArgumentException("Id cannot be null or empty");
            if (string.IsNullOrEmpty(lastUpdatedBy))
                throw new ArgumentException("lastUpdatedBy cannot be null or empty");
            await _resourceAllocationRepository.DeleteResourceAllocationByIds(ids, lastUpdatedBy);

            _backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x =>
                x.DeleteAnalyticsDataForDeletedAllocationByScheduleIds(ids));

            return;
        }

        public async Task<IEnumerable<ResourceAllocationViewModel>> GetResourceAllocationsByCaseCodes(
            string oldCaseCodes)
        {
            if (string.IsNullOrEmpty(oldCaseCodes))
                throw new ArgumentException("Error while getting resource data. Case Codes cannot be null or empty.");
            var allocations = await _resourceAllocationRepository.GetResourceAllocationsByCaseCodes(oldCaseCodes);

            return ConvertToResourceAllocationViewModel(allocations);
        }

        public async Task<IEnumerable<ResourcesCount>> GetResourceAllocationsCountByCaseCodes(
            string oldCaseCodes)
        {
            if (string.IsNullOrEmpty(oldCaseCodes))
                throw new ArgumentException("Error while getting resource data. Case Codes cannot be null or empty.");
            var allocationsCount = await _resourceAllocationRepository.GetResourceAllocationsCountByCaseCodes(oldCaseCodes);

            return allocationsCount;
        }

        public async Task<IEnumerable<ResourceAllocationViewModel>> GetResourceAllocationsByPipelineIds(
            string pipelineIds)
        {
            if (string.IsNullOrEmpty(pipelineIds))
                throw new ArgumentException("Error while getting resource data. PipelineIds cannot be null or empty.");

            var allocations = await _resourceAllocationRepository.GetResourceAllocationsByPipelineIds(pipelineIds);

            return ConvertToResourceAllocationViewModel(allocations);
        }

        public async Task<IEnumerable<ResourceAllocationViewModel>> GetResourceAllocationsByScheduleIds(
            string scheduleIds)
        {
            if (string.IsNullOrEmpty(scheduleIds))
                throw new ArgumentException("Error while getting resources data. ScheduleIds cannot be null or empty.");

            var allocations = await _resourceAllocationRepository.GetResourceAllocationsByScheduleIds(scheduleIds);

            return ConvertToResourceAllocationViewModel(allocations);
        }

        public async Task<(string, IEnumerable<ResourceAllocationViewModel>)> GetResourceAllocationsBySelectedValues(string oldCaseCodes,
            string employeeCodes, DateTime? lastUpdated, DateTime? startDate, DateTime? endDate, string caseRoleCodes, string clientId)
        {
            if (string.IsNullOrEmpty(oldCaseCodes) && string.IsNullOrEmpty(employeeCodes) && lastUpdated == null && startDate == null && endDate == null)
            {
                return ("Atleast one parameter is mandatory", Enumerable.Empty<ResourceAllocationViewModel>());
            }
            if (!ValidateParamValuesByClient(clientId, oldCaseCodes, employeeCodes, lastUpdated, startDate, endDate, caseRoleCodes,
                Constants.ApiEndPoints.GetResourceAllocationsBySelectedValues))
            {
                return ("Mandatory params not provided", Enumerable.Empty<ResourceAllocationViewModel>());
            }

            if (startDate == null && endDate != null)
            {
                return ("Error while getting resource data. Start Date should be provided with end date",
                    Enumerable.Empty<ResourceAllocationViewModel>());
            }

            var allocations = await _resourceAllocationRepository.GetResourceAllocationsBySelectedValues(oldCaseCodes,
                employeeCodes, lastUpdated, startDate, endDate, caseRoleCodes);

            var resourceAllocations = ConvertToResourceAllocationViewModel(allocations);
            var filteredResourceAllocations = await FilterRDSResourceAllocations(resourceAllocations);

            var returnData = (string.Empty, filteredResourceAllocations);

            return returnData;
        }

        public async Task<(string, IEnumerable<ResourceAllocationViewModel>)> GetResourceAllocationsBySelectedValuesV2(string oldCaseCodes,
            string employeeCodes, DateTime? lastUpdated, DateTime? startDate, DateTime? endDate, string caseRoleCodes, string clientId, string action)
        {
            if (string.IsNullOrEmpty(oldCaseCodes) && string.IsNullOrEmpty(employeeCodes) && lastUpdated == null && startDate == null && endDate == null)
            {
                return ("Atleast one parameter is mandatory", Enumerable.Empty<ResourceAllocationViewModel>());
            }
            if (!ValidateParamValuesByClient(clientId, oldCaseCodes, employeeCodes, lastUpdated, startDate, endDate, caseRoleCodes,
                Constants.ApiEndPoints.GetResourceAllocationsBySelectedValues))
            {
                return ("Mandatory params not provided", Enumerable.Empty<ResourceAllocationViewModel>());
            }

            if (startDate == null && endDate != null)
            {
                return ("Error while getting resource data. Start Date should be provided with end date",
                    Enumerable.Empty<ResourceAllocationViewModel>());
            }

            var allocations = await _resourceAllocationRepository.GetResourceAllocationsBySelectedValuesV2(oldCaseCodes,
                employeeCodes, lastUpdated, startDate, endDate, caseRoleCodes, action);

            var resourceAllocations = ConvertToResourceAllocationViewModel(allocations);
            var filteredResourceAllocations = await FilterRDSResourceAllocations(resourceAllocations);
            
            var returnData = (string.Empty, filteredResourceAllocations);

            return returnData;
        }

        public async Task<(string, IEnumerable<ResourceAllocationViewModel>)> GetResourceAllocationsBySelectedValues(string action, string oldCaseCodes,
         string employeeCodes, DateTime? lastUpdated, DateTime? startDate, DateTime? endDate, string caseRoleCodes, string clientId)
        {
            if (string.IsNullOrEmpty(action) || action.Contains(','))
            {
                return ("Action parameter is mandatory and should be one of upserted or deleted", Enumerable.Empty<ResourceAllocationViewModel>());
            }

            if (string.IsNullOrEmpty(oldCaseCodes) && string.IsNullOrEmpty(employeeCodes) && lastUpdated == null && startDate == null && endDate == null)
            {
                return ("Atleast one parameter is mandatory", Enumerable.Empty<ResourceAllocationViewModel>());
            }
            if (!ValidateParamValuesByClient(clientId, oldCaseCodes, employeeCodes, lastUpdated, startDate, endDate, caseRoleCodes,
                Constants.ApiEndPoints.GetResourceAllocationsBySelectedValues))
            {
                return ("Mandatory params not provided", Enumerable.Empty<ResourceAllocationViewModel>());
            }

            if (startDate == null && endDate != null)
            {
                return ("Error while getting resource data. Start Date should be provided with end date",
                    Enumerable.Empty<ResourceAllocationViewModel>());
            }

            var allocations = await _resourceAllocationRepository.GetResourceAllocationsBySelectedValues(action,oldCaseCodes,
                employeeCodes, lastUpdated, startDate, endDate, caseRoleCodes);

            var resourceAllocations = ConvertToResourceAllocationViewModel(allocations);

            var filteredResourceAllocations = await FilterRDSResourceAllocations(resourceAllocations);

            var returnData = (string.Empty, filteredResourceAllocations);

            return returnData;
        }

        public async Task<IEnumerable<ResourceAllocationViewModel>> GetResourceAllocationsByOfficeCodes(
            string officeCodes,
            DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(officeCodes) || startDate > endDate)
                return Enumerable.Empty<ResourceAllocationViewModel>();
            var allocations =
                await _resourceAllocationRepository.GetResourceAllocationsByOfficeCodes(officeCodes, startDate,
                    endDate);
            return ConvertToResourceAllocationViewModel(allocations);
        }

        public async Task<IEnumerable<ResourceAllocationViewModel>> GetResourceAllocationsByEmployeeCodes(
            string employeeCodes,
            DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes)) return Enumerable.Empty<ResourceAllocationViewModel>();
            var allocations =
                await _resourceAllocationRepository.GetResourceAllocationsByEmployeeCodes(employeeCodes, startDate,
                    endDate);
            return ConvertToResourceAllocationViewModel(allocations);
        }

        public async Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsOnCaseRollByCaseCodes(string oldCaseCodes)
        {
            if (string.IsNullOrEmpty(oldCaseCodes))
                throw new ArgumentException("Error while getting resource data. Case Codes cannot be null or empty.");

            return await _resourceAllocationRepository.GetResourceAllocationsOnCaseRollByCaseCodes(oldCaseCodes);
        }

        public async Task<IEnumerable<ResourceAllocation>> GetAllocationsForEmployeesOnPrePost()
        {
            return await _resourceAllocationRepository.GetAllocationsForEmployeesOnPrePost();
        }

        public async Task<IEnumerable<ResourceAllocationViewModel>> GetResourceAllocationsBySelectedSupplyValues(string officeCodes, DateTime startDate, DateTime endDate,
            string staffingTags, string currentLevelGrades)
        {
            if (string.IsNullOrEmpty(officeCodes) && string.IsNullOrEmpty(staffingTags) &&
                startDate == null && endDate == null)
            {
                return Enumerable.Empty<ResourceAllocationViewModel>();
            }

            if (startDate == null && endDate != null)
            {
                throw new ArgumentException("Error while getting resource data. Start Date should be provided with end date");
            }

            var allocations = await _resourceAllocationRepository.GetResourceAllocationsBySelectedSupplyValues(officeCodes, startDate, endDate,
                staffingTags, currentLevelGrades);

            return ConvertToResourceAllocationViewModel(allocations);
        }

        public async Task<IEnumerable<ResourceAllocationViewModel>> GetLastTeamByEmployeeCode(string employeeCode, DateTime? date)
        {
            if (string.IsNullOrEmpty(employeeCode))
            {
                return Enumerable.Empty<ResourceAllocationViewModel>();
            }

            var overlappedAllocations = await _resourceAllocationRepository.GetLastTeamByEmployeeCode(employeeCode, date);

            return ConvertToResourceAllocationViewModel(overlappedAllocations);
        }

        public async Task<IEnumerable<EmployeeLastBillableDateViewModel>> GetLastBillableDateByEmployeeCodes(string employeeCodes, DateTime? date)
        {
            if (string.IsNullOrEmpty(employeeCodes))
            {
                return Enumerable.Empty<EmployeeLastBillableDateViewModel>();
            }

            var result = await _resourceAllocationRepository.GetLastBillableDateByEmployeeCodes(employeeCodes, date);

            return result;
        }

        #region Private Methods

        private async Task<List<ResourceAllocationViewModel>> FilterRDSResourceAllocations(IEnumerable<ResourceAllocationViewModel> allocations)
        {
            //var allEmployees = await _resourceApiClient.GetEmployeesIncludingTerminated();
            //string rdsDepartmentCodes = "O2,O3,O4,O5";

            //var rdsDepartmentEmployees = allEmployees.Where(resource => rdsDepartmentCodes.ToLower().Contains(resource.Department?.DepartmentCode?.ToLower())).ToList();
            var rdsEmployeeList = ConfigurationUtility.GetValue("rdsEmployeeList").Split(",");

            var filteredResourceAllocations = new List<ResourceAllocationViewModel>();
            
            var environment = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"); //used for getting the azue instance service the request
            foreach (var allocation in allocations)
            {
                if (!rdsEmployeeList.Any(emp => string.Equals(emp, allocation.EmployeeCode, StringComparison.InvariantCultureIgnoreCase)))
                {
                    filteredResourceAllocations.Add(allocation);
                }
            }

            return filteredResourceAllocations;
        }

        private bool ValidateParamValuesByClient(string clientId, string oldCaseCodes, string employeeCodes, DateTime? lastUpdated, DateTime? startDate,
            DateTime? endDate, string caseRoleCodes, string currentMethodName)
        {
            if (string.IsNullOrEmpty(clientId))
                return true;

            var allClientDetails = JObject.Parse(File.ReadAllText(ConfigurationUtility.GetValue("clientAccessFilePath")));

            var clientDetails = new TSGClientNameAndMandatoryParams()
            {
                ClientName = allClientDetails[clientId]?[Constants.ClientProperties.ClientName].ToString(),
                MandatoryParams = allClientDetails[clientId]?[currentMethodName][Constants.ClientProperties.MandatoryParams].ToString()
            };

            if (!string.IsNullOrEmpty(clientDetails?.MandatoryParams))
            {
                var mandatoryParams = clientDetails.MandatoryParams.Trim(',').Split(',');
                foreach (var parameter in mandatoryParams)
                {
                    switch (parameter)
                    {
                        case "oldCaseCodes":
                            if (string.IsNullOrEmpty(oldCaseCodes)) return false;
                            break;
                        case "employeeCodes":
                            if (string.IsNullOrEmpty(employeeCodes)) return false;
                            break;
                        case "lastUpdated":
                            if (lastUpdated == null || lastUpdated == DateTime.MinValue) return false;
                            break;
                        case "startDate":
                            if (startDate == null || startDate == DateTime.MinValue) return false;
                            break;
                        case "endDate":
                            if (endDate == null || endDate == DateTime.MinValue) return false;
                            break;
                        case "caseRoleCodes":
                            if (string.IsNullOrEmpty(caseRoleCodes)) return false;
                            break;
                        default:
                            return false;
                    }
                }
            }


            return true;
        }

        private static DataTable CreateAssignmentDataTable(IEnumerable<ResourceAllocation> resourceAllocations)
        {
            var resourceAllocationsDataTable = new DataTable();
            resourceAllocationsDataTable.Columns.Add("id", typeof(Guid));
            resourceAllocationsDataTable.Columns.Add("oldCaseCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("caseCode", typeof(int));
            resourceAllocationsDataTable.Columns.Add("caseName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("clientCode", typeof(int));
            resourceAllocationsDataTable.Columns.Add("clientName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("caseTypeCode", typeof(short));
            resourceAllocationsDataTable.Columns.Add("caseTypeName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("pipelineId", typeof(Guid));
            resourceAllocationsDataTable.Columns.Add("opportunityName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("employeeCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("employeeName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("employeeStatusCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("fte", typeof(decimal));
            resourceAllocationsDataTable.Columns.Add("serviceLineCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("serviceLineName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("position", typeof(string));
            resourceAllocationsDataTable.Columns.Add("positionCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("positionName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("positionGroupName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("currentLevelGrade", typeof(string));
            resourceAllocationsDataTable.Columns.Add("operatingOfficeCode", typeof(short));
            resourceAllocationsDataTable.Columns.Add("operatingOfficeName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("operatingOfficeAbbreviation", typeof(string));
            resourceAllocationsDataTable.Columns.Add("managingOfficeCode", typeof(short));
            resourceAllocationsDataTable.Columns.Add("managingOfficeName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("managingOfficeAbbreviation", typeof(string));
            resourceAllocationsDataTable.Columns.Add("billingOfficeCode", typeof(short));
            resourceAllocationsDataTable.Columns.Add("billingOfficeName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("billingOfficeAbbreviation", typeof(string));
            resourceAllocationsDataTable.Columns.Add("allocation", typeof(short));
            resourceAllocationsDataTable.Columns.Add("startDate", typeof(DateTime));
            resourceAllocationsDataTable.Columns.Add("endDate", typeof(DateTime));
            resourceAllocationsDataTable.Columns.Add("investmentCode", typeof(short));
            resourceAllocationsDataTable.Columns.Add("investmentName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("caseRoleCode", typeof(string));
            resourceAllocationsDataTable.Columns.Add("caseRoleName", typeof(string));
            resourceAllocationsDataTable.Columns.Add("actualCost", typeof(decimal));
            resourceAllocationsDataTable.Columns.Add("effectiveCost", typeof(decimal));
            resourceAllocationsDataTable.Columns.Add("effectiveCostReason", typeof(string));
            resourceAllocationsDataTable.Columns.Add("billRate", typeof(decimal));
            resourceAllocationsDataTable.Columns.Add("billCode", typeof(decimal));
            resourceAllocationsDataTable.Columns.Add("billRateType", typeof(string));
            resourceAllocationsDataTable.Columns.Add("billRateCurrency", typeof(string));
            resourceAllocationsDataTable.Columns.Add("lastUpdatedBy", typeof(string));
            resourceAllocationsDataTable.Columns.Add("notes", typeof(string));

            foreach (var allocations in resourceAllocations)
            {
                var row = resourceAllocationsDataTable.NewRow();
                row["id"] = (object)allocations.Id ?? DBNull.Value;
                row["oldCaseCode"] = (object)allocations.OldCaseCode ?? DBNull.Value;
                row["caseCode"] = (object)allocations.CaseCode ?? DBNull.Value;
                row["caseName"] = (object)allocations.CaseName ?? DBNull.Value;
                row["clientCode"] = allocations.ClientCode;
                row["clientName"] = (object)allocations.ClientName ?? DBNull.Value;
                row["caseTypeCode"] = (object)allocations.CaseTypeCode ?? DBNull.Value;
                row["caseTypeName"] = (object)allocations.CaseTypeName ?? DBNull.Value;
                row["pipelineId"] = (object)allocations.PipelineId ?? DBNull.Value;
                row["opportunityName"] = (object)allocations.OpportunityName ?? DBNull.Value;
                row["employeeCode"] = allocations.EmployeeCode;
                row["employeeName"] = (object)allocations.EmployeeName ?? DBNull.Value;
                row["employeeStatusCode"] = (object)((int)allocations.EmployeeStatusCode) ?? DBNull.Value;
                row["fte"] = allocations.Fte;
                row["serviceLineCode"] = (object)allocations.ServiceLineCode ?? DBNull.Value;
                row["serviceLineName"] = (object)allocations.ServiceLineName ?? DBNull.Value;
                row["position"] = (object)allocations.Position ?? DBNull.Value;
                row["positionCode"] = (object)allocations.PositionCode ?? DBNull.Value;
                row["positionName"] = (object)allocations.PositionName ?? DBNull.Value;
                row["positionGroupName"] = (object)allocations.PositionGroupName ?? DBNull.Value;
                row["currentLevelGrade"] = allocations.CurrentLevelGrade;
                row["operatingOfficeCode"] = allocations.OperatingOfficeCode;
                row["operatingOfficeName"] = (object)allocations.OperatingOfficeName ?? DBNull.Value;
                row["operatingOfficeAbbreviation"] = (object)allocations.OperatingOfficeAbbreviation ?? DBNull.Value;
                row["managingOfficeCode"] = (object)allocations.ManagingOfficeCode ?? DBNull.Value;
                row["managingOfficeName"] = (object)allocations.ManagingOfficeName ?? DBNull.Value;
                row["managingOfficeAbbreviation"] = (object)allocations.ManagingOfficeAbbreviation ?? DBNull.Value;
                row["billingOfficeCode"] = (object)allocations.BillingOfficeCode ?? DBNull.Value;
                row["billingOfficeName"] = (object)allocations.BillingOfficeName ?? DBNull.Value;
                row["billingOfficeAbbreviation"] = (object)allocations.BillingOfficeAbbreviation ?? DBNull.Value;
                row["allocation"] = allocations.Allocation;
                row["startDate"] = allocations.StartDate;
                row["endDate"] = allocations.EndDate;
                row["investmentCode"] = (object)allocations.InvestmentCode ?? DBNull.Value;
                row["investmentName"] = (object)allocations.InvestmentName ?? DBNull.Value;
                row["caseRoleCode"] = (object)allocations.CaseRoleCode ?? DBNull.Value;
                row["caseRoleName"] = (object)allocations.CaseRoleName ?? DBNull.Value;
                row["actualCost"] = (object)allocations.ActualCost ?? DBNull.Value;
                row["effectiveCost"] = (object)allocations.EffectiveCost ?? DBNull.Value;
                row["effectiveCostReason"] = (object)allocations.EffectiveCostReason ?? DBNull.Value;
                row["billRate"] = (object)allocations.BillRate ?? DBNull.Value;
                row["billCode"] = (object)allocations.BillCode ?? DBNull.Value;
                row["billRateType"] = (object)allocations.BillRateType ?? DBNull.Value;
                row["billRateCurrency"] = (object)allocations.BillRateCurrency ?? DBNull.Value;
                row["lastUpdatedBy"] = allocations.LastUpdatedBy;
                row["notes"] = allocations.Notes;
                resourceAllocationsDataTable.Rows.Add(row);
            }

            return resourceAllocationsDataTable;
        }

        private static IEnumerable<ResourceAllocationViewModel> ConvertToResourceAllocationViewModel(
            IEnumerable<ResourceAllocation> allocations)
        {
            var viewModel = allocations.Select(item => new ResourceAllocationViewModel
            {
                Action = item.Action,
                Id = item.Id,
                CaseCode = item.CaseCode,
                ClientCode = item.ClientCode,
                OldCaseCode = item.OldCaseCode,
                PipelineId = item.PipelineId,
                EmployeeCode = item.EmployeeCode,
                CurrentLevelGrade = item.CurrentLevelGrade,
                OperatingOfficeCode = item.OperatingOfficeCode,
                ServiceLineCode = item.ServiceLineCode,
                Allocation = item.Allocation,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                InvestmentCode = item.InvestmentCode,
                InvestmentName = item.InvestmentName,
                CaseRoleCode = item.CaseRoleCode,
                CaseRoleName = item.CaseRoleName,
                CaseTypeCode = item.CaseTypeCode,
                LastUpdatedBy = item.LastUpdatedBy,
                LastUpdated = item.LastUpdated,
                CreatedAt = item.CreatedAt,
                Notes = item.Notes,
                IsPlaceholderAllocation = item.IsPlaceholderAllocation
            });

            return viewModel;
        }

        #endregion
    }
}