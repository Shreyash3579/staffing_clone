using CaseIntake.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CaseIntake.API.Contracts.Repository
{
    public interface ICaseIntakeRepository
    {
        Task<IEnumerable<CaseIntakeLeadership>> GetLeadershipDetailsByCaseCodeOrOpportunityId(string oldCaseCode, string opportunityId, string planningCardId);
        Task<CaseIntakeDetail> GetCaseIntakeDetailsByCaseCodeOrOpportunityId(string oldCaseCode, string opportunityId, string planningCardId);
        Task<IEnumerable<CaseIntakeLeadership>> UpsertLeadershipDetails(DataTable leadershipDetailsDataTable);
        Task<CaseIntakeDetail> UpsertCaseIntakeDetails(CaseIntakeDetail caseIntakeDetails);
        Task<CaseIntakeRoleAndWorkstream> GetRoleAndWorkstreamByCaseCodeOrOpportunityId(string oldCaseCode, string opportunityId, string planningCardId);
        Task<IEnumerable<CaseIntakeRoleDetails>> UpsertRoleDetailsByCaseCodeOrOpportunityId(DataTable dataTable, string oldCasecode, Guid? opportunityId, Guid? planningCardId);
        Task<CaseIntakeRoleAndWorkstream> UpsertRoleAndWorkStreamDetails(DataTable workstreamDetails, DataTable roleDetails);
        Task DeleteWorkStreamByIds(string ids, string lastUpdatedBy);

        Task DeleteRoleByIds(string ids, string lastUpdatedBy);
        Task DeleteLeadershipById(string id, string lastUpdatedBy);

        Task DeleteLeadershipByCaseRoleCode(string caseRoleCode, string oldCaseCode, Guid? opportunityId, Guid? planningCardId, string lastUpdatedBy);
        Task<LastUpdates> GetMostRecentUpdateInCaseIntake(string oldCaseCode, string opportunityId, string planningCardId);
        Task<IEnumerable<CaseIntakeExpertise>> GetExpertiseRequirementList();
        Task<CaseIntakeExpertise> UpsertExpertiseRequirementList(CaseIntakeExpertise expertise);
        Task<bool> UpsertCaseIntakeAlertsForEmployees(string employeeCodes, string lastUpdatedBy, string oldCaseCode, Guid? opportunityId, Guid? planningCardId, string demandName);
        Task<IEnumerable<CaseIntakeAlertViewModel>> GetCaseIntakeAlert(string employeeCode);
        Task<string> GetEmployeesForCaseIntakeAlert(string demandViewOfficeCode, string demandType, string lastUpdatedBy);
        Task<IEnumerable<GeoLocation>> GetPlacesForTypeAhead(string searchstring);

    }
}
