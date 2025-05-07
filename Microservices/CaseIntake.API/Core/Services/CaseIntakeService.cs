using CaseIntake.API.Contracts.Repository;
using CaseIntake.API.Contracts.Services;
using CaseIntake.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CaseIntake.API.Core.Services
{
    public class CaseIntakeService : ICaseIntakeService
    {
        private readonly ICaseIntakeRepository _caseIntakeRepository;
        private readonly IResourceApiClient _resourceApiClient;
        private readonly IPipelineApiClient _pipelineApiClient;
        private readonly ICCMApiClient _ccmApiClient;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly ISignalRHubClient _signalRHubClient;

        public CaseIntakeService(ICaseIntakeRepository caseIntakeRepository, IResourceApiClient resourceApiClient, IPipelineApiClient pipelineApiClient, ICCMApiClient ccmApiClient, IStaffingApiClient staffingApiClient, ISignalRHubClient signalRHubClient)
        {
            _caseIntakeRepository = caseIntakeRepository;
            _resourceApiClient = resourceApiClient;
            _pipelineApiClient = pipelineApiClient;
            _ccmApiClient = ccmApiClient;
            _signalRHubClient = signalRHubClient;
            _staffingApiClient = staffingApiClient;
        }

        public async Task<IEnumerable<CaseIntakeLeadership>> GetLeadershipDetailsForCaseOrOpportunityId(string oldCaseCode, string opportunityId, string planningCardId)
        {
            var employeesIncludingTerminatedTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            var leadershipDetailsTask = _caseIntakeRepository.GetLeadershipDetailsByCaseCodeOrOpportunityId(oldCaseCode, opportunityId, planningCardId);

            await Task.WhenAll(employeesIncludingTerminatedTask, leadershipDetailsTask);

            var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;
            var leadershipDetails = leadershipDetailsTask.Result;

            var caseIntakeLeadership = leadershipDetails.Select(leadershipDetail =>
            {
                var employee = employeesIncludingTerminated.FirstOrDefault(e => e.EmployeeCode == leadershipDetail.EmployeeCode);
                var lastUpdatedByEmployee = employeesIncludingTerminated.FirstOrDefault(e => e.EmployeeCode == leadershipDetail.LastUpdatedBy);
                return new CaseIntakeLeadership
                {  
                    Id = leadershipDetail.Id,
                    EmployeeCode = leadershipDetail.EmployeeCode,
                    EmployeeName = employee?.FullName,
                    CaseRoleCode = leadershipDetail.CaseRoleCode,
                    CaseRoleName = leadershipDetail.CaseRoleName,
                    OfficeAbbreviation = employee?.Office?.OfficeAbbreviation,
                    AllocationPercentage = leadershipDetail.AllocationPercentage,
                    OldCaseCode = leadershipDetail.OldCaseCode,
                    OpportunityId = leadershipDetail.OpportunityId,
                    PlanningCardId = leadershipDetail.PlanningCardId,
                    LastUpdated = leadershipDetail.LastUpdated,
                    LastUpdatedBy = leadershipDetail.LastUpdatedBy,
                    LastUpdatedByName = lastUpdatedByEmployee?.FullName
                };
            }).ToList();

            return caseIntakeLeadership;

        }

        public async Task<CaseIntakeDetail> GetCaseIntakeDetailsByCaseCodeOrOpportunityId(string oldCaseCode, string opportunityId, string planningCardId)
        {
            var caseIntakeDetail = await _caseIntakeRepository.GetCaseIntakeDetailsByCaseCodeOrOpportunityId(oldCaseCode, opportunityId, planningCardId);

            return caseIntakeDetail;
        }

        public async Task<IEnumerable<CaseIntakeLeadership>> UpsertLeadershipDetails(IEnumerable<CaseIntakeLeadership> leadershipDetails)
        {
            var employeesIncludingTerminatedTask = _resourceApiClient.GetActiveEmployees();

            var leadershipDetailsDataTable = ConvertToLeadershipDataTable(leadershipDetails);

            var upsertedDataTask = _caseIntakeRepository.UpsertLeadershipDetails(leadershipDetailsDataTable);

            await Task.WhenAll(employeesIncludingTerminatedTask, upsertedDataTask);

            var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;
            var upsertedDataList = upsertedDataTask.Result;

            // Map the employee name to the upserted leadership details
            var resultList = upsertedDataList.Select(upsertedData =>
            {
                var employee = employeesIncludingTerminated.FirstOrDefault(e => e.EmployeeCode == upsertedData.EmployeeCode);

                return new CaseIntakeLeadership
                {
                    Id = upsertedData.Id,
                    EmployeeCode = upsertedData.EmployeeCode,
                    EmployeeName = employee?.FullName,
                    CaseRoleCode = upsertedData.CaseRoleCode,
                    CaseRoleName = upsertedData.CaseRoleName,
                    OfficeAbbreviation = employee?.Office?.OfficeAbbreviation,
                    AllocationPercentage = upsertedData.AllocationPercentage,
                    OldCaseCode = upsertedData.OldCaseCode,
                    OpportunityId = upsertedData.OpportunityId,
                    PlanningCardId = upsertedData.PlanningCardId,
                    LastUpdated = upsertedData.LastUpdated,
                    LastUpdatedBy = upsertedData.LastUpdatedBy,
                };
            }).ToList();

            // Trigger notifications asynchronously without blocking the upsert return
            TriggerSendNotifications(
                resultList.FirstOrDefault().OpportunityId,
                resultList.FirstOrDefault().PlanningCardId,
                resultList.FirstOrDefault().OldCaseCode,
                resultList.FirstOrDefault().LastUpdatedBy
            );

            return resultList;

        }
        public async Task<CaseIntakeDetail> UpsertCaseIntakeDetails(CaseIntakeDetail caseIntakeDetails)
        {
            var upsertedCaseIntakeDetail = await _caseIntakeRepository.UpsertCaseIntakeDetails(caseIntakeDetails);

            TriggerSendNotifications(
                upsertedCaseIntakeDetail.OpportunityId,
                upsertedCaseIntakeDetail.PlanningCardId,
                upsertedCaseIntakeDetail.OldCaseCode,
                upsertedCaseIntakeDetail.LastUpdatedBy
            );

            return upsertedCaseIntakeDetail;
        }

        // Helper method to convert list to DataTable
        private DataTable ConvertToLeadershipDataTable(IEnumerable<CaseIntakeLeadership> data)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Id", typeof(Guid));
            dataTable.Columns.Add("EmployeeCode", typeof(string));
            dataTable.Columns.Add("CaseRoleCode", typeof(string));
            dataTable.Columns.Add("AllocationPercentage", typeof(int));
            dataTable.Columns.Add("OldCaseCode", typeof(string));
            dataTable.Columns.Add("OpportunityId", typeof(Guid));
            dataTable.Columns.Add("PlanningCardId", typeof(Guid));
            dataTable.Columns.Add("LastUpdatedBy", typeof(string));

            foreach (var item in data)
            {
                var row = dataTable.NewRow();
                row["Id"] = item.Id == Guid.Empty ? DBNull.Value : item.Id;
                row["EmployeeCode"] = item.EmployeeCode;
                row["CaseRoleCode"] = item.CaseRoleCode;
                row["AllocationPercentage"] = item.AllocationPercentage.HasValue ? (object)item.AllocationPercentage : DBNull.Value;
                row["OldCaseCode"] = item.OldCaseCode;
                row["OpportunityId"] = (object)item.OpportunityId ?? DBNull.Value;
                row["PlanningCardId"] = (object)item.PlanningCardId ?? DBNull.Value;
                row["LastUpdatedBy"] = item.LastUpdatedBy;

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        public async Task<CaseIntakeRoleAndWorkstream> GetRoleAndWorkstreamByCaseCodeOrOpportunityId(string oldCaseCode, string opportunityId, string planningCardId)
        {
            var roleDetailsAndWorkstreams = await _caseIntakeRepository.GetRoleAndWorkstreamByCaseCodeOrOpportunityId(oldCaseCode, opportunityId, planningCardId);
            var employeesIncludingTerminated = await _resourceApiClient.GetEmployeesIncludingTerminated();
            var employee = employeesIncludingTerminated.FirstOrDefault(e => e.EmployeeCode == roleDetailsAndWorkstreams.lastUpdatedBy);

            return roleDetailsAndWorkstreams;
        }


        public async Task<CaseIntakeRoleAndWorkstream> UpsertRoleAndWorkStreamDetails(CaseIntakeRoleAndWorkstream caseIntakeWorkstream)
        {
            //need to maintain the order FK constraint (in case of insert)
            var workstreamDetailsDataTable = ConvertToWorkstreamDetailsDataTable(caseIntakeWorkstream.workStreamDetails.ToList());
            var roleDetailsDataTable = ConvertToRoleDetailsDataTable(caseIntakeWorkstream.roleDetails.ToList());
            var upsertedWorkstreamAndRoleDetails = await _caseIntakeRepository.UpsertRoleAndWorkStreamDetails(workstreamDetailsDataTable, roleDetailsDataTable);


            // Trigger notifications asynchronously without blocking the upsert return
            TriggerSendNotifications(
                caseIntakeWorkstream.OpportunityId,
                caseIntakeWorkstream.PlanningCardId,
                caseIntakeWorkstream.OldCaseCode,
                caseIntakeWorkstream.lastUpdatedBy
            );

            return upsertedWorkstreamAndRoleDetails;
        }

        public async Task<IEnumerable<CaseIntakeRoleDetails>> UpsertRoleDetailsByCaseCodeOrOpportunityId(IEnumerable<CaseIntakeRoleDetails> caseIntakeRoles, string? oldCasecode, Guid? opportunityId, Guid? planningCardId)
        {
            var dataTable = ConvertToRoleDetailsDataTable(caseIntakeRoles.ToList());

            var upsertedRoleDetails = await _caseIntakeRepository.UpsertRoleDetailsByCaseCodeOrOpportunityId(dataTable, oldCasecode, opportunityId, planningCardId);

            TriggerSendNotifications(
                upsertedRoleDetails.FirstOrDefault().OpportunityId,
                upsertedRoleDetails.FirstOrDefault().PlanningCardId,
                upsertedRoleDetails.FirstOrDefault().OldCaseCode,
                upsertedRoleDetails.FirstOrDefault().LastUpdatedBy
            );

            return upsertedRoleDetails;
        }

        public async Task DeleteWorkStreamByIds(CaseIntakeBasicDetail workstreamToBeDeleted)
        {
            if (string.IsNullOrEmpty(workstreamToBeDeleted.Id))
                throw new ArgumentException("Id cannot be null or empty");

            await _caseIntakeRepository.DeleteWorkStreamByIds(workstreamToBeDeleted.Id, workstreamToBeDeleted.LastUpdatedBy);

            TriggerSendNotifications(
            workstreamToBeDeleted.OpportunityId,
            workstreamToBeDeleted.PlanningCardId,
            workstreamToBeDeleted.OldCaseCode,
            workstreamToBeDeleted.LastUpdatedBy
            );

        }


        public async Task DeleteRoleByIds(CaseIntakeBasicDetail rolesToBeDeleted)
        {
            if (string.IsNullOrEmpty(rolesToBeDeleted.Id))
                throw new ArgumentException("Id cannot be null or empty");

            await _caseIntakeRepository.DeleteRoleByIds(rolesToBeDeleted.Id, rolesToBeDeleted.LastUpdatedBy);

            TriggerSendNotifications(
              rolesToBeDeleted.OpportunityId,
              rolesToBeDeleted.PlanningCardId,
              rolesToBeDeleted.OldCaseCode,
              rolesToBeDeleted.LastUpdatedBy
          );

        }

        public async Task DeleteLeadershipById(CaseIntakeBasicDetail deleteLeadershipDetail)
        {
            if (string.IsNullOrEmpty(deleteLeadershipDetail.Id))
                throw new ArgumentException("Id cannot be null or empty");

            await _caseIntakeRepository.DeleteLeadershipById(deleteLeadershipDetail.Id, deleteLeadershipDetail.LastUpdatedBy);
            // Trigger notifications asynchronously 
            TriggerSendNotifications(
                deleteLeadershipDetail.OpportunityId,
                deleteLeadershipDetail.PlanningCardId,
                deleteLeadershipDetail.OldCaseCode,
                deleteLeadershipDetail.LastUpdatedBy
            );

        }

        public async Task DeleteLeadershipByCaseRoleCode(CaseIntakeBasicDetail deleteLeadershipDetail)
        {
            if (string.IsNullOrEmpty(deleteLeadershipDetail.CaseRoleCode))
                throw new ArgumentException("Id cannot be null or empty");

            await _caseIntakeRepository.DeleteLeadershipByCaseRoleCode(deleteLeadershipDetail.CaseRoleCode, deleteLeadershipDetail.OldCaseCode, deleteLeadershipDetail.OpportunityId, deleteLeadershipDetail.PlanningCardId, deleteLeadershipDetail.LastUpdatedBy);
            // Trigger notifications asynchronously 
            TriggerSendNotifications(
                deleteLeadershipDetail.OpportunityId,
                deleteLeadershipDetail.PlanningCardId,
                deleteLeadershipDetail.OldCaseCode,
                deleteLeadershipDetail.LastUpdatedBy
            );

        }

        public async Task<LastUpdates> GetMostRecentUpdateInCaseIntake(string oldCaseCode, string opportunityId, string planningCardId)
        {
            var lastUpdatedDetails = await _caseIntakeRepository.GetMostRecentUpdateInCaseIntake(oldCaseCode, opportunityId, planningCardId);

            if (lastUpdatedDetails != null)
            {
                var lastUpdatedByEmployee = await _resourceApiClient.GetEmployeeByEmployeeCode(lastUpdatedDetails.LastUpdatedBy);

                lastUpdatedDetails.LastUpdatedByName = lastUpdatedByEmployee?.FullName;
            }

            return lastUpdatedDetails;

        }

        public async Task<IEnumerable<CaseIntakeExpertise>> GetExpertiseRequirementList()
        {
            return await _caseIntakeRepository.GetExpertiseRequirementList();
        }

        public async Task<CaseIntakeExpertise> UpsertExpertiseRequirementList(CaseIntakeExpertise expertise)
        {
            return await _caseIntakeRepository.UpsertExpertiseRequirementList(expertise);
        }
        
        public async Task<IEnumerable<CaseIntakeAlertViewModel>> GetCaseIntakeAlert(string employeeCode)
        {
            var caseIntakeAlertTask = _caseIntakeRepository.GetCaseIntakeAlert(employeeCode);
            var resourcesTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(caseIntakeAlertTask, resourcesTask);

            var resources = resourcesTask.Result;
            var caseIntakeAlertList = caseIntakeAlertTask.Result;

            var caseIntakeAlerts = caseIntakeAlertList.Select(caseIntakeAlert =>
            {
                var lastUpdatedByEmployee = resources.FirstOrDefault(e => e.EmployeeCode == caseIntakeAlert.LastUpdatedBy);
                return new CaseIntakeAlertViewModel
                {
                    Id = caseIntakeAlert.Id,
                    EmployeeCode = caseIntakeAlert.EmployeeCode,
                    OpportunityId = caseIntakeAlert.OpportunityId,
                    PlanningCardId = caseIntakeAlert.PlanningCardId,
                    OldCaseCode = caseIntakeAlert.OldCaseCode,
                    DemandName = caseIntakeAlert.DemandName,
                    LastUpdated = caseIntakeAlert.LastUpdated,
                    LastUpdatedBy = caseIntakeAlert.LastUpdatedBy,
                    LastUpdatedByName = lastUpdatedByEmployee?.FullName
                };
            }).ToList();
            
            return caseIntakeAlerts;

        }

        private DataTable ConvertToRoleDetailsDataTable(IList<CaseIntakeRoleDetails> details)
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add("Id", typeof(Guid));
            dataTable.Columns.Add("OldCaseCode", typeof(string));
            dataTable.Columns.Add("OpportunityId", typeof(Guid));
            dataTable.Columns.Add("PlanningCardId", typeof(Guid));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("WorkstreamId", typeof(Guid));
            dataTable.Columns.Add("PositionCode", typeof(string));
            dataTable.Columns.Add("ExpertiseRequirementCodes", typeof(string));
            dataTable.Columns.Add("MustHaveExpertiseCodes", typeof(string));
            dataTable.Columns.Add("NiceToHaveExpertiseCodes", typeof(string));
            dataTable.Columns.Add("OfficeCodes", typeof(string));
            dataTable.Columns.Add("LanguageCodes", typeof(string));
            dataTable.Columns.Add("MustHaveLanguageCodes", typeof(string));
            dataTable.Columns.Add("NiceToHaveLanguageCodes", typeof(string));
            dataTable.Columns.Add("ClientEngagementModel", typeof(string));
            dataTable.Columns.Add("ClientEngagementModelCodes", typeof(string));
            dataTable.Columns.Add("RoleDescription", typeof(string));
            dataTable.Columns.Add("ServiceLineCode", typeof(string));
            dataTable.Columns.Add("IsLead", typeof(bool));
            dataTable.Columns.Add("LastUpdatedBy", typeof(string));

            foreach (var detail in details)
            {
                var row = dataTable.NewRow();

                row["Id"] = (object)detail.Id ?? DBNull.Value;
                row["OldCaseCode"] = detail.OldCaseCode ?? (object)DBNull.Value;
                row["OpportunityId"] = detail.OpportunityId ?? (object)DBNull.Value;
                row["PlanningCardId"] = detail.PlanningCardId ?? (object)DBNull.Value;
                row["WorkstreamId"] = detail.WorkstreamId ?? (object)DBNull.Value;
                row["Name"] = detail.Name ?? (object)DBNull.Value;
                row["PositionCode"] = detail.PositionCode ?? (object)DBNull.Value;
                row["ExpertiseRequirementCodes"] = detail.ExpertiseRequirementCodes ?? (object)DBNull.Value;
                row["NiceToHaveExpertiseCodes"] = detail.NiceToHaveExpertiseCodes ?? (object)DBNull.Value;
                row["MustHaveExpertiseCodes"] = detail.MustHaveExpertiseCodes ?? (object)DBNull.Value;
                row["OfficeCodes"] = detail.OfficeCodes ?? (object)DBNull.Value;
                row["LanguageCodes"] = detail.LanguageCodes ?? (object)DBNull.Value;
                row["MustHaveLanguageCodes"] = detail.MustHaveLanguageCodes ?? (object)DBNull.Value;
                row["NiceToHaveLanguageCodes"] = detail.NiceToHaveLanguageCodes ?? (object)DBNull.Value;
                row["ClientEngagementModel"] = detail.ClientEngagementModel ?? (object)DBNull.Value;
                row["ClientEngagementModelCodes"] = detail.ClientEngagementModelCodes ?? (object)DBNull.Value;
                row["RoleDescription"] = detail.RoleDescription ?? (object)DBNull.Value;
                row["ServiceLineCode"] = detail.ServiceLineCode ?? (object)DBNull.Value;
                row["IsLead"] = detail.IsLead;
                row["LastUpdatedBy"] = detail.LastUpdatedBy ?? (object)DBNull.Value;

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        private DataTable ConvertToWorkstreamDetailsDataTable(IList<CaseIntakeWorkstreamDetails> details)
        {
            var dataTable = new DataTable();

            // Define the columns based on the properties of CaseIntakeWorkstreamDetails
            dataTable.Columns.Add("Id", typeof(Guid));
            dataTable.Columns.Add("OldCaseCode", typeof(string));
            dataTable.Columns.Add("OpportunityId", typeof(Guid));
            dataTable.Columns.Add("PlanningCardId", typeof(Guid));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("SkuSize", typeof(string));
            dataTable.Columns.Add("LastUpdatedBy", typeof(string));

            // Iterate through each workstream detail and populate the DataTable
            foreach (var detail in details)
            {
                var row = dataTable.NewRow();

                row["Id"] = (object)detail.Id ?? DBNull.Value;
                row["OldCaseCode"] = detail.OldCaseCode ?? (object)DBNull.Value;
                row["OpportunityId"] = detail.OpportunityId ?? (object)DBNull.Value;
                row["PlanningCardId"] = detail.PlanningCardId ?? (object)DBNull.Value;
                row["Name"] = detail.Name ?? (object)DBNull.Value;
                row["SkuSize"] = detail.SkuSize ?? (object)DBNull.Value;
                row["LastUpdatedBy"] = detail.LastUpdatedBy ?? (object)DBNull.Value;

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }







        private async Task SendNotificationsToUsersAsync(Guid? opporunityId, Guid? planningCardId, string oldCaseCode, string lastUpdatedBy)
        {
            //get demand details
            var opportunityDetailsTask = _pipelineApiClient.GetOpportunityDetailsByPipelineId(opporunityId);
            var planningCardDetailsTask = _staffingApiClient.GetPlanningCardByPlanningCardIds(planningCardId.ToString());
            var caseDetailsTask = _ccmApiClient.GetCaseDetailsByCaseCode(oldCaseCode);

            await Task.WhenAll(opportunityDetailsTask, planningCardDetailsTask, caseDetailsTask);

            var opportunityDetails = opporunityId != Guid.Empty ? opportunityDetailsTask.Result : null;
            var planningCard = planningCardId != Guid.Empty ? planningCardDetailsTask.Result.FirstOrDefault() : null;
            var caseDetails = !string.IsNullOrEmpty(oldCaseCode) ? caseDetailsTask.Result : null;


            var message = new MessageObject
            {
                LastUpdated = DateTime.Now,
                LastUpdatedBy = lastUpdatedBy
            };


            string demandStatus = string.Empty;
            string demandOffice = string.Empty;
            if(!string.IsNullOrEmpty(oldCaseCode))
            {
                demandStatus = "Case";
                demandOffice = caseDetails.ManagingOfficeCode.ToString();
                var startDate = caseDetails?.StartDate.ToString("dd-MMM-yyyy") ?? "N/A";
                var endDate = caseDetails?.EndDate.ToString("dd-MMM-yyyy") ?? "N/A";
                message.Name = $"{caseDetails?.ClientName ?? "Client - N/A"}, {caseDetails?.CaseName ?? "Case - N/A"}, {startDate} to {endDate}";
                message.Id = oldCaseCode ?? "";

            }
            else if (opporunityId != null)
            {
                demandStatus = "Opportunity";

                var resources = await _resourceApiClient.GetEmployeesIncludingTerminated();

                var coordinatingPartner = resources.FirstOrDefault(e => e.EmployeeCode == opportunityDetails.CoordinatingPartnerCode);
                var coordinatingOffice = coordinatingPartner?.Office?.OfficeCode.ToString() ?? "";

                var otherOffices = new List<string>();
                if (opportunityDetails.OtherPartnersCodes != null)
                {
                    // Split the OtherPartnersCodes by comma and loop through each partner code
                    foreach (var partnerCode in opportunityDetails.OtherPartnersCodes.Split(','))
                    {
                        var otherPartner = resources.FirstOrDefault(e => e.EmployeeCode == partnerCode.Trim());

                        var officeCode = otherPartner?.Office?.OfficeCode.ToString();

                        // Add the office code to the list if it is not empty
                        if (!string.IsNullOrEmpty(officeCode))
                        {
                            otherOffices.Add(officeCode);
                        }
                    }
                }

                var billingPartner = resources.FirstOrDefault(e => e.EmployeeCode == opportunityDetails.BillingPartnerCode);
                var billingOffice = billingPartner?.Office?.OfficeCode.ToString() ?? "";

                // Combine all office codes into a comma-separated list
                demandOffice = string.Join(",", new[] { coordinatingOffice, billingOffice }
                    .Concat(otherOffices)
                    .Where(o => !string.IsNullOrEmpty(o))
                    .Distinct());

                var startDate = opportunityDetails?.StartDate?.ToString("dd-MMM-yyyy") ?? "N/A";
                var endDate = opportunityDetails?.EndDate?.ToString("dd-MMM-yyyy") ?? "N/A";

                message.Name = $"{opportunityDetails?.ClientName ?? "Client - N/A"}, {opportunityDetails?.OpportunityName ?? "Opportunity - N/A"}, {startDate} to {endDate}";
                message.Id = opporunityId.ToString();
            }

            else if (planningCardId!=null)
            {
                demandStatus = "PlanningCard";
                demandOffice = planningCard.SharedOfficeCodes.ToString();
                var startDate = planningCard?.StartDate?.ToString("dd-MMM-yyyy") ?? "N/A";
                var endDate = planningCard?.EndDate?.ToString("dd-MMM-yyyy") ?? "N/A";
                message.Name = $"{planningCard?.Name ?? "Planning Card - N/A"}, {startDate} to {endDate}";
                message.Id = planningCardId.ToString();
            }

            var employeesList = await _caseIntakeRepository.GetEmployeesForCaseIntakeAlert(demandOffice, demandStatus, lastUpdatedBy);


            //upsert the list of employees in notesAlert table
            var upsertSuccess = await UpsertCaseIntakeNotifactionForEmployees(employeesList, lastUpdatedBy, oldCaseCode, opporunityId, planningCardId, message.Name);


            if (upsertSuccess)
            {
                //call signalr
                var response = await _signalRHubClient.GetUpdateOnCaseIntakeChanges(employeesList);
            }


        }

        private async Task<bool> UpsertCaseIntakeNotifactionForEmployees(string employeesList, string lastUpdatedBy, string oldCaseCode, Guid? opportunityId, Guid? planningCardId, string demandName)
        {
            if (string.IsNullOrEmpty(employeesList) || string.IsNullOrEmpty(lastUpdatedBy))
                throw new ArgumentException("employeesList or lastUpdatedBy cannot be null or empty");

            await _caseIntakeRepository.UpsertCaseIntakeAlertsForEmployees(employeesList, lastUpdatedBy, oldCaseCode, opportunityId, planningCardId, demandName);
            return true;

        }

        private async void TriggerSendNotifications(Guid? opportunityId, Guid? planningCardId, string oldCaseCode, string lastUpdatedBy)
        {
            try
            {
                await SendNotificationsToUsersAsync(opportunityId, planningCardId, oldCaseCode, lastUpdatedBy);
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine($"Error in SendNotificationsToUsersAsync: {ex.Message}");
            }
        }

        public async Task<IEnumerable<GeoLocation>> GetPlacesForTypeAhead(string searchString)
        {
            if (string.IsNullOrEmpty(searchString) || searchString.Length <= 2)
            {
                return Enumerable.Empty<GeoLocation>();
            }

            var geolocations = await
                _caseIntakeRepository.GetPlacesForTypeAhead(searchString);


            return geolocations;
        }


    }
}
