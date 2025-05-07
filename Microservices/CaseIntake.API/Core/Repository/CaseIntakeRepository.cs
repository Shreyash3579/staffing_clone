using CaseIntake.API.Contracts.Repository;
using CaseIntake.API.Core.Helpers;
using CaseIntake.API.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CaseIntake.API.Core.Repository
{
    public class CaseIntakeRepository : ICaseIntakeRepository
    {
        private readonly IBaseRepository<CaseIntakeLeadership> _baseRepository;
        private readonly IBaseRepository<CaseIntakeRoleDetails> _baseRepositoryContext;

        public CaseIntakeRepository(IBaseRepository<CaseIntakeLeadership> baseRepository, IBaseRepository<CaseIntakeRoleDetails> baseRepositoryContext)
        {
            _baseRepository = baseRepository;
            _baseRepositoryContext = baseRepositoryContext;
        }


        public async Task<IEnumerable<CaseIntakeLeadership>> GetLeadershipDetailsByCaseCodeOrOpportunityId(string oldCaseCode, string opportunityId, string planningCardId)
        {
            var leadershipDetails = await _baseRepository.GetAllAsync(
                new { oldCaseCode, opportunityId, planningCardId },
                StoredProcedureMap.GetCaseIntakeLeadershipbyOldCaseCodeOrOpportunityId
            );

            return leadershipDetails;

        }

        public async Task<CaseIntakeDetail> GetCaseIntakeDetailsByCaseCodeOrOpportunityId(string oldCaseCode, string opportunityId, string planningCardId)
        {
            var leadershipDetails = await _baseRepository.Context.Connection.QueryAsync<CaseIntakeDetail>(
                StoredProcedureMap.GetCaseIntakeDetailbyOldCaseCodeOrOpportunityId,
                new { oldCaseCode, opportunityId, planningCardId },
                commandType: CommandType.StoredProcedure
            );

            return leadershipDetails.FirstOrDefault();
        }

        public async Task<IEnumerable<CaseIntakeLeadership>> UpsertLeadershipDetails(DataTable leadershipDataTable)
        {

            var upsertedData = await _baseRepository.Context.Connection.QueryAsync<CaseIntakeLeadership>(
                StoredProcedureMap.UpsertCaseIntakeLeadership,
                new
                {
                    leadershipDetails =
                        leadershipDataTable.AsTableValuedParameter(
                            "[dbo].[leadershipDetailsTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return upsertedData;
        }


        public async Task<CaseIntakeDetail> UpsertCaseIntakeDetails(CaseIntakeDetail caseIntakeDetails)
        {
            var upsertedData = await _baseRepository.Context.Connection.QueryFirstOrDefaultAsync<CaseIntakeDetail>(
                StoredProcedureMap.UpsertCaseIntakeDetail,
                new
                {
                    caseIntakeDetails.OfficeCodes,
                    caseIntakeDetails.ClientEngagementModel,
                    caseIntakeDetails.ClientEngagementModelCodes,
                    caseIntakeDetails.CaseDescription,
                    caseIntakeDetails.ExpertiseRequirement,
                    caseIntakeDetails.Languages,
                    caseIntakeDetails.ReadyToStaffNotes,
                    caseIntakeDetails.BackgroundCheckNotes,
                    caseIntakeDetails.IndustryPracticeAreaCodes,
                    caseIntakeDetails.CapabilityPracticeAreaCodes,
                    caseIntakeDetails.OldCaseCode,
                    caseIntakeDetails.OpportunityId,
                    caseIntakeDetails.PlanningCardId,
                    caseIntakeDetails.LastUpdatedBy
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180);

            return upsertedData;
        }

        public async Task<CaseIntakeRoleAndWorkstream> GetRoleAndWorkstreamByCaseCodeOrOpportunityId(string oldCaseCode, string opportunityId, string planningCardId)
        {
            
            var result = await _baseRepository.Context.Connection.QueryMultipleAsync(
              StoredProcedureMap.GetCaseIntakeWorkstreamRolesByCaseOrOppId,
              new
              {
                  oldCaseCode,
                  opportunityId,
                  planningCardId
              },
              commandType: CommandType.StoredProcedure,
              commandTimeout: _baseRepository.Context.TimeoutPeriod);

            var roleDetails = result.Read<CaseIntakeRoleDetails>().ToList();
            var workstreamDetails = result.Read<CaseIntakeWorkstreamDetails>().ToList();
            var lastUpdatedInfo = await result.ReadFirstOrDefaultAsync<(DateTime lastUpdated, string lastUpdatedBy)>();

            return new CaseIntakeRoleAndWorkstream
            {
                roleDetails = roleDetails,
                workStreamDetails = workstreamDetails,
                lastUpdated = lastUpdatedInfo.lastUpdated,
                lastUpdatedBy = lastUpdatedInfo.lastUpdatedBy,
            };

        }


        public async Task<IEnumerable<CaseIntakeRoleDetails>> UpsertRoleDetailsByCaseCodeOrOpportunityId(DataTable roleDetailsDataTable, string oldCaseCode, Guid? opportunityId, Guid? planningCardId)
        {
            var upsertedRoleDetails = await _baseRepository.Context.Connection.QueryAsync<CaseIntakeRoleDetails>(
                StoredProcedureMap.UpsertCaseIntakeRoleByCaseOrOppId,
                new
                {
                    roleDetails =
                        roleDetailsDataTable.AsTableValuedParameter(
                            "[dbo].[caseIntakeWorkstreamRoleTableType]"),
                    OldCaseCode = oldCaseCode,
                    OpportunityId = opportunityId,
                    PlanningCardId = planningCardId
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return upsertedRoleDetails;

        }
        public async Task<CaseIntakeRoleAndWorkstream> UpsertRoleAndWorkStreamDetails(DataTable workstreamDetails, DataTable roleDetails)
        {
           
            var result = await _baseRepository.Context.Connection.QueryMultipleAsync(
             StoredProcedureMap.UpsertCaseIntakeRoleAndWorkstream, 
                new
                {
                    WorkstreamDetails = workstreamDetails.AsTableValuedParameter("[dbo].[caseIntakeWorkstreamTableType]"), 
                    RoleDetails = roleDetails.AsTableValuedParameter("[dbo].[caseIntakeWorkstreamRoleTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180); 
    
                    var workstreamDetailsResult = result.Read<CaseIntakeWorkstreamDetails>().ToList();
                    var roleDetailsResult = result.Read<CaseIntakeRoleDetails>().ToList();

            return new CaseIntakeRoleAndWorkstream
            {
                roleDetails = roleDetailsResult,
                workStreamDetails = workstreamDetailsResult
            };
        }

        public async Task DeleteWorkStreamByIds(string ids, string lastUpdatedBy)
        {
            await _baseRepository.DeleteAsync(new { ids, lastUpdatedBy }, StoredProcedureMap.DeleteWorkStreamByIds);
        }

        public async Task DeleteRoleByIds(string ids, string lastUpdatedBy)
        {
            await _baseRepository.DeleteAsync(new { ids, lastUpdatedBy }, StoredProcedureMap.DeleteRoleByIds);
        }

        public async Task DeleteLeadershipById(string id, string lastUpdatedBy)
        {
            await _baseRepository.DeleteAsync(new { id, lastUpdatedBy }, StoredProcedureMap.DeleteLeadershipById);
        }

        public async Task DeleteLeadershipByCaseRoleCode(string caseRoleCode, string oldCaseCode, Guid? opportunityId, Guid? planningCardId, string lastUpdatedBy)
        {
            await _baseRepository.DeleteAsync(new { caseRoleCode, oldCaseCode, opportunityId, planningCardId, lastUpdatedBy}, StoredProcedureMap.DeleteLeadershipByCaseRoleCode);
        }

       
        public async Task<LastUpdates> GetMostRecentUpdateInCaseIntake(string oldCaseCode, string opportunityId, string planningCardId)
        {
            var lastUpdatedDetails = await _baseRepository.Context.Connection.QueryAsync<LastUpdates>(
                StoredProcedureMap.GetMostRecentUpdateInCaseIntake,
                new { oldCaseCode, opportunityId, planningCardId },
                commandType: CommandType.StoredProcedure
            );

            return lastUpdatedDetails.FirstOrDefault();
        }

        public async Task<IEnumerable<CaseIntakeExpertise>> GetExpertiseRequirementList()
        {
            var list = await Task.Run(() => _baseRepository.Context.Connection.Query<CaseIntakeExpertise>(
                StoredProcedureMap.GetExpertiseRequirementList,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return list;
        }

        public async Task<CaseIntakeExpertise> UpsertExpertiseRequirementList(CaseIntakeExpertise expertise)
        {
            var upsertedData = await _baseRepository.Context.Connection.QueryFirstOrDefaultAsync<CaseIntakeExpertise>(
                StoredProcedureMap.UpsertExpertiseRequirementList,
                new
                {
                    expertise.ExpertiseAreaCode,
                    expertise.ExpertiseAreaName,
                    expertise.LastUpdatedBy
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180);

            return upsertedData;
        }

        public async Task<bool> UpsertCaseIntakeAlertsForEmployees(string employeeCodes, string lastUpdatedBy, string oldCaseCode, Guid? opportunityId, Guid? planningCardId, string demandName)
        {
            if (string.IsNullOrEmpty(employeeCodes) || string.IsNullOrEmpty(lastUpdatedBy))
                throw new ArgumentException("Employee codes and lastUpdatedBy cannot be null or empty.");

            // Execute the stored procedure using Dapper
            await _baseRepository.Context.Connection.ExecuteAsync(
                StoredProcedureMap.UpsertCaseIntakeAlertsForEmployees,  // Make sure this is correctly mapped to your stored procedure
                new
                {
                    EmployeeCodes = employeeCodes,   // Comma-separated employee codes
                    LastUpdatedBy = lastUpdatedBy,   // User who updated the record
                    OldCaseCode = oldCaseCode,
                    OpportunityId = opportunityId,
                    PlanningCardId = planningCardId,
                    DemandName = demandName
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod
            );

            return true;  // Indicate that the upsert was successful
        }

        public async Task<IEnumerable<CaseIntakeAlertViewModel>> GetCaseIntakeAlert(string employeeCode)
        {
            var notesAlert = await _baseRepository.Context.Connection.QueryAsync<CaseIntakeAlertViewModel>(
                StoredProcedureMap.GetCaseIntakeAlert,
                new
                {
                    employeeCode
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return notesAlert;
        }

        public async Task<string> GetEmployeesForCaseIntakeAlert(string demandViewOfficeCode, string demandType, string lastUpdatedBy)
        {
            if (string.IsNullOrEmpty(demandViewOfficeCode) || string.IsNullOrEmpty(demandType))
                throw new ArgumentException("demandViewOfficeCode and demandType cannot be null or empty.");

            // Execute the stored procedure using Dapper
            var matchingEmployees = await _baseRepository.Context.Connection.QuerySingleOrDefaultAsync<string>(
                StoredProcedureMap.GetEmployeesForCaseIntakeAlert,  // Ensure this is mapped to the correct stored procedure
                new
                {
                    demandViewOfficeCode,
                    demandType,
                    lastUpdatedBy
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod
            );

            return matchingEmployees;  // Return the comma-separated employee list
        }

        public async Task<IEnumerable<GeoLocation>> GetPlacesForTypeAhead(string searchstring)
        {
            var geolocations = await _baseRepository.Context.Connection.QueryAsync<GeoLocation>(
                StoredProcedureMap.GetPlacesForTypeAhead,
                new
                {
                    search_term = searchstring
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return geolocations;

        }

    }
}
