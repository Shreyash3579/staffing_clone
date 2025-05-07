using Dapper;
using Microsoft.Graph.Models.Security;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class NoteRepository : INoteRepository
    {

        private readonly IBaseRepository<ResourceViewNote> _baseRepository;

        public NoteRepository(IBaseRepository<ResourceViewNote> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<ResourceViewNote>> GetResourceViewNotes(string employeeCodes, string loggedInUser, string noteTypeCode)
        {
            var resourceViewNotes = await _baseRepository.GetAllAsync(
                new
                {
                    employeeCodes,
                    loggedInUser,
                    noteTypeCode
                },
                StoredProcedureMap.GetResourceViewNotes
            );

            return resourceViewNotes;
        }

        public async Task<ResourceViewNote> UpsertResourceViewNote(ResourceViewNote resourceViewNote)
        {
            var upsertedResourceViewNote = await _baseRepository.UpdateAsync(
                    new
                    {
                        resourceViewNote.Id,
                        resourceViewNote.EmployeeCode,
                        resourceViewNote.Note,
                        resourceViewNote.IsPrivate,
                        resourceViewNote.SharedWith,
                        resourceViewNote.CreatedBy,
                        resourceViewNote.NoteTypeCode,
                        resourceViewNote.LastUpdatedBy
                    },
                    StoredProcedureMap.UpsertResourceViewNote
                );
           
            return upsertedResourceViewNote;
        }

        public async Task<IEnumerable<Guid>> DeleteResourceViewNotes(string idsToDelete, string lastUpdatedBy)
        {
            var deletedResourceViewNote = await _baseRepository.Context.Connection.QueryAsync<Guid>(
                StoredProcedureMap.DeleteResourceViewNotes,
                new
                {
                    idsToDelete,
                    lastUpdatedBy
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return deletedResourceViewNote;
        }

        public async Task<IEnumerable<Guid>> DeleteResourceViewCD(string idsToDelete, string lastUpdatedBy)
        {
            var deletedResourceViewNote = await _baseRepository.Context.Connection.QueryAsync<Guid>(
                StoredProcedureMap.DeleteResourceViewCD,
                new
                {
                    idsToDelete,
                    lastUpdatedBy
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return deletedResourceViewNote;
        }

        public async Task<IEnumerable<Guid>> DeleteResourceCommercialModel(string idsToDelete, string lastUpdatedBy)
        {
            var deletedResourceViewCommercialModel = await _baseRepository.Context.Connection.QueryAsync<Guid>(
                StoredProcedureMap.DeleteResourceViewCommercialModel,
                new
                {
                    idsToDelete,
                    lastUpdatedBy
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return deletedResourceViewCommercialModel;
        }

        public async Task<IEnumerable<CaseViewNote>> GetCaseViewNotes(string oldCaseCodes, string pipelineIds, string planningCardIds, string loggedInUser)
        {
            var caseViewNotes = await _baseRepository.Context.Connection.QueryAsync<CaseViewNote>(
                StoredProcedureMap.GetCasePlanningViewNotes,
                new
                {
                    oldCaseCodes,
                    pipelineIds,
                    planningCardIds,
                    loggedInUser
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return caseViewNotes;
        }

        public async Task<CaseViewNote> UpsertCaseViewNote(CaseViewNote caseViewNote)
        {
            var upsertedCaseViewNote = caseViewNote != null
                ? await _baseRepository.Context.Connection.QuerySingleAsync<CaseViewNote>(
                    StoredProcedureMap.UpsertCasePlanningViewNote,
                    new
                    {
                        caseViewNote.Id,
                        caseViewNote.OldCaseCode,
                        caseViewNote.PipelineId,
                        caseViewNote.PlanningCardId,
                        caseViewNote.Note,
                        caseViewNote.IsPrivate,
                        caseViewNote.SharedWith,
                        caseViewNote.CreatedBy,
                        caseViewNote.LastUpdatedBy
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: _baseRepository.Context.TimeoutPeriod)
                : throw new ArgumentNullException(nameof(caseViewNote), "The 'caseViewNote' parameter is null.");

            return upsertedCaseViewNote;
        }

        public async Task<IEnumerable<Guid>> DeleteCaseViewNotes(string idsToDelete, string lastUpdatedBy)
        {
            var deletedCaseViewNote = await _baseRepository.Context.Connection.QueryAsync<Guid>(
                StoredProcedureMap.DeleteCasePlanningViewNotes,
                new
                {
                    idsToDelete,
                    lastUpdatedBy
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return deletedCaseViewNote;
        }

        public async Task<IEnumerable<NoteAlert>> GetNotesAlert(string employeeCode)
        {
            var notesAlert = await _baseRepository.Context.Connection.QueryAsync<NoteAlert>(
                StoredProcedureMap.GetNotesAlert,
                new
                {
                    employeeCode
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return notesAlert;
        }

        public async Task<IEnumerable<NoteSharedWithGroup>> GetMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode(string employeeCode)
        {
            var recentNoteSharedWithGroups = await _baseRepository.Context.Connection.QueryAsync<NoteSharedWithGroup>(
                StoredProcedureMap.GetMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode,
                new
                {
                    employeeCode
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return recentNoteSharedWithGroups;
        }


        public async Task UpdateNotesAlertStaus(string employeeCode)
        {

            await _baseRepository.Context.Connection.ExecuteAsync(
                StoredProcedureMap.UpdateNotesAlertStatus,
                new {employeeCode },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return;
        }

        public async Task<IEnumerable<ResourceCommercialModel>> GetCommercialModelList()
        {
            var commercialModelList = await Task.Run(() => _baseRepository.Context.Connection.Query<ResourceCommercialModel>(
                StoredProcedureMap.GetCommercialModelList,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return commercialModelList;
        }

        public async Task<IEnumerable<ResourceCommercialModel>> GetResourceCommercialModel(string employeeCodes)
        {
            var resourceCommercialModel = await _baseRepository.Context.Connection.QueryAsync<ResourceCommercialModel>(
                StoredProcedureMap.GetResourceCommercialModel,
                new
                {
                    employeeCodes
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return resourceCommercialModel;
        }

        public async Task<IEnumerable<ResourceCD>> GetResourceRecentCD(string employeeCodes)
        {
            var resourceSkills = await _baseRepository.Context.Connection.QueryAsync<ResourceCD>(
                StoredProcedureMap.GetResourceRecentCD,
                new
                {
                    employeeCodes
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return resourceSkills;
        }

        public async Task<IEnumerable<ResourceCD>> GetRecentCDList()
        {
            var staffableAsTypes = await Task.Run(() => _baseRepository.Context.Connection.Query<ResourceCD>(
                StoredProcedureMap.GetRecentCDList,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return staffableAsTypes;
        }

        public async Task<ResourceCD> UpsertResourceRecentCD(ResourceCD resourceCD)
        {
            var upsertedCaseViewNote = resourceCD != null
                ? await _baseRepository.Context.Connection.QuerySingleAsync<ResourceCD>(
                    StoredProcedureMap.UpsertResourceRecentCD,
                    new
                    {
                        resourceCD.Id,
                        resourceCD.RecentCD,
                        resourceCD.LastUpdatedBy,
                        resourceCD.EmployeeCode,
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: _baseRepository.Context.TimeoutPeriod)
                : throw new ArgumentNullException(nameof(resourceCD), "The 'caseViewNote' parameter is null.");

            return upsertedCaseViewNote;
        }

        public async Task<ResourceCommercialModel> UpsertResourceCommercialModel(ResourceCommercialModel resourceCommercialModel)
        {
            var upsertedResourceViewCommercialModel = resourceCommercialModel != null
                ? await _baseRepository.Context.Connection.QueryFirstOrDefaultAsync<ResourceCommercialModel>(
                    StoredProcedureMap.UpsertResourceCommercialModel,
                    new
                    {
                        resourceCommercialModel.Id,
                        resourceCommercialModel.CommercialModel,
                        resourceCommercialModel.LastUpdatedBy,
                        resourceCommercialModel.EmployeeCode,
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: _baseRepository.Context.TimeoutPeriod)
                : throw new ArgumentNullException(nameof(resourceCommercialModel), "The 'resourceCommercialModel' parameter is null.");

            return upsertedResourceViewCommercialModel;
        }

        public async Task<IEnumerable<ResourceViewNote>> GetResourceNotesByLastUpdatedDate(DateTime lastupdatedAfter, string noteTypeCode)
        {
            var resourceViewNotes = await _baseRepository.GetAllAsync(
               new
               {
                   lastupdatedAfter,
                   noteTypeCode
               },
               StoredProcedureMap.GetResourceNotesByLastUpdatedDate
           );

            return resourceViewNotes;
        }


    }
}
