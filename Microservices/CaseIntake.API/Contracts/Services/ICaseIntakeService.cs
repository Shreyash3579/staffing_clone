using CaseIntake.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseIntake.API.Contracts.Services
{
    public interface ICaseIntakeService
    {
        Task<IEnumerable<CaseIntakeLeadership>> GetLeadershipDetailsForCaseOrOpportunityId(string oldCaseCode, string opportunityId, string planningCardId);
        Task<CaseIntakeDetail> GetCaseIntakeDetailsByCaseCodeOrOpportunityId(string oldCaseCode, string opportunityId, string planningCardId);
        Task<IEnumerable<CaseIntakeLeadership>> UpsertLeadershipDetails(IEnumerable<CaseIntakeLeadership> leadershipDetail);
        Task<CaseIntakeDetail> UpsertCaseIntakeDetails(CaseIntakeDetail caseIntakeDetails);
        Task<CaseIntakeRoleAndWorkstream> GetRoleAndWorkstreamByCaseCodeOrOpportunityId(string oldCaseCode, string opportunityId, string planningCardId);
        Task<IEnumerable<CaseIntakeRoleDetails>> UpsertRoleDetailsByCaseCodeOrOpportunityId(IEnumerable<CaseIntakeRoleDetails> roleDetails, string oldCasecode, Guid? opportunityId, Guid? planningCardId);

        Task<CaseIntakeRoleAndWorkstream> UpsertRoleAndWorkStreamDetails(CaseIntakeRoleAndWorkstream caseIntakeWorkstream);
        Task DeleteWorkStreamByIds(CaseIntakeBasicDetail workstreamsToBeDeleted);


        Task DeleteRoleByIds(CaseIntakeBasicDetail rolesToBeDeleted);
        Task DeleteLeadershipById(CaseIntakeBasicDetail deleteLeadershipDetail);

        Task DeleteLeadershipByCaseRoleCode(CaseIntakeBasicDetail deleteLeadershipDetail);
        Task<LastUpdates> GetMostRecentUpdateInCaseIntake(string oldCaseCode, string opportunityId, string planningCardId);
        Task<IEnumerable<CaseIntakeExpertise>> GetExpertiseRequirementList();
        Task<CaseIntakeExpertise> UpsertExpertiseRequirementList(CaseIntakeExpertise expertise);
        Task<IEnumerable<CaseIntakeAlertViewModel>> GetCaseIntakeAlert(string employeeCode);
        Task<IEnumerable<GeoLocation>> GetPlacesForTypeAhead(string searchstring);
    }
}
