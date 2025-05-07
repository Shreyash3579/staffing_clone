using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models.Security;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Core.Repository;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class NoteService : INoteService
    {
        private readonly INoteRepository _noteRepository;
        public NoteService(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public async Task<IEnumerable<ResourceViewNote>> GetResourceViewNotes(string employeeCodes, string loggedInUser, string noteTypeCode)
        {
            if (string.IsNullOrEmpty(employeeCodes) || string.IsNullOrEmpty(loggedInUser))
            {
                return Enumerable.Empty<ResourceViewNote>();
            }
            var resourceNotes = await _noteRepository.GetResourceViewNotes(employeeCodes, loggedInUser, noteTypeCode);
            return resourceNotes;
        }

        public async Task<ResourceViewNote> UpsertResourceViewNote(ResourceViewNote resourceViewNote)
        {
            if (string.IsNullOrEmpty(resourceViewNote.EmployeeCode) || string.IsNullOrEmpty(resourceViewNote.Note)
                || string.IsNullOrEmpty(resourceViewNote.LastUpdatedBy))
            {
                return new ResourceViewNote();
            }
            var insertedResourceNote = await _noteRepository.UpsertResourceViewNote(resourceViewNote);
            return insertedResourceNote;
        }

        public async Task<IEnumerable<Guid>> DeleteResourceViewNotes(string idsToDelete, string lastUpdatedBy)
        {
            if (string.IsNullOrEmpty(idsToDelete) || string.IsNullOrEmpty(lastUpdatedBy))
            {
                return Enumerable.Empty<Guid>();
            }
            var deletedResourceNote = await _noteRepository.DeleteResourceViewNotes(idsToDelete, lastUpdatedBy);
            return deletedResourceNote;
        }

        public async Task<IEnumerable<Guid>> DeleteResourceViewCD(string idsToDelete, string lastUpdatedBy)
        {
            if (string.IsNullOrEmpty(idsToDelete) || string.IsNullOrEmpty(lastUpdatedBy))
            {
                return Enumerable.Empty<Guid>();
            }
            var deletedResourceCD = await _noteRepository.DeleteResourceViewCD(idsToDelete, lastUpdatedBy);
            return deletedResourceCD;
        }

        public async Task<IEnumerable<Guid>> DeleteResourceCommercialModel(string idsToDelete, string lastUpdatedBy)
        {
            if (string.IsNullOrEmpty(idsToDelete) || string.IsNullOrEmpty(lastUpdatedBy))
            {
                return Enumerable.Empty<Guid>();
            }
            var deletedResourceCommercialModel= await _noteRepository.DeleteResourceCommercialModel(idsToDelete, lastUpdatedBy);
            return deletedResourceCommercialModel;
        }

        public async Task<IEnumerable<CaseViewNote>> GetCaseViewNotes(string oldCaseCodes, string pipelineIds, string planningCardIds, string loggedInUser)
        {
            if ((string.IsNullOrEmpty(oldCaseCodes) && string.IsNullOrEmpty(pipelineIds) && string.IsNullOrEmpty(planningCardIds)) || string.IsNullOrEmpty(loggedInUser))
            {
                return Enumerable.Empty<CaseViewNote>();
            }
            var caseNotes = await _noteRepository.GetCaseViewNotes(oldCaseCodes, pipelineIds, planningCardIds, loggedInUser);
            return caseNotes;
        }

        public async Task<IEnumerable<CaseViewNote>> GetLatestCaseViewNotes(string oldCaseCodes, string pipelineIds, string planningCardIds, string loggedInUser)
        {
            if ((string.IsNullOrEmpty(oldCaseCodes) && string.IsNullOrEmpty(pipelineIds) && string.IsNullOrEmpty(planningCardIds)) || string.IsNullOrEmpty(loggedInUser))
            {
                return Enumerable.Empty<CaseViewNote>();
            }
            var caseNotes = await _noteRepository.GetCaseViewNotes(oldCaseCodes, pipelineIds, planningCardIds, loggedInUser);
           
            var latestNote = caseNotes.GroupBy(obj => new{obj.PipelineId,obj.PlanningCardId,obj.OldCaseCode }).Select(x=>x.FirstOrDefault());

            return latestNote;
        }

        public async Task<CaseViewNote> UpsertCaseViewNote(CaseViewNote caseViewNote)
        {
            if (!isCaseViewNoteValid(caseViewNote)
                || string.IsNullOrEmpty(caseViewNote.LastUpdatedBy))
            {
                throw new ArgumentException("caseViewNote or LastUpdatedBy cannot be null or empty");
            }
            var upsertedCaseNote = await _noteRepository.UpsertCaseViewNote(caseViewNote);
            return upsertedCaseNote;
        }

        public async Task<IEnumerable<Guid>> DeleteCaseViewNotes(string idsToDelete, string lastUpdatedBy)
        {
            if (string.IsNullOrEmpty(idsToDelete) || string.IsNullOrEmpty(lastUpdatedBy))
            {
                throw new ArgumentException("idsToeDeleted or LastUpdatedBy cannot be null or empty");
            }
            var deletedCaseNotes = await _noteRepository.DeleteCaseViewNotes(idsToDelete, lastUpdatedBy);
            return deletedCaseNotes;
        }

        public async Task<IEnumerable<NoteAlert>>  GetNotesAlert(string employeeCode)
        {
            var notesAlert = await _noteRepository.GetNotesAlert(employeeCode);
            return notesAlert;
        }

        public async Task<IEnumerable<NoteSharedWithGroup>> GetMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode(string employeeCode)
        {
            var recentNoteSharedWithGroups = await _noteRepository.GetMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode(employeeCode);
            return recentNoteSharedWithGroups;
        }

        public async Task UpdateNotesAlertStaus(string employeeCode)
        {
           await _noteRepository.UpdateNotesAlertStaus(employeeCode);
            return;
        }

        public async Task<IEnumerable<ResourceCommercialModel>> GetCommercialModelList()
        {
            var commercialModelList = await _noteRepository.GetCommercialModelList();
            return commercialModelList;
        }

        public async Task<IEnumerable<ResourceCommercialModel>> GetResourceCommercialModel(string employeeCodes)
        {
            var resourceCommercialModel = await _noteRepository.GetResourceCommercialModel(employeeCodes);
            return resourceCommercialModel;
        }

        public async Task<IEnumerable<ResourceCD>> GetResourceRecentCD(string employeeCodes)
        {
            var resourceSkills = await _noteRepository.GetResourceRecentCD(employeeCodes);
            return resourceSkills;
        }
        public async Task<IEnumerable<ResourceCD>> GetRecentCDList()
        {
            var resourceSkills = await _noteRepository.GetRecentCDList();
            return resourceSkills;
        }

        public async Task<ResourceCommercialModel> UpsertResourceCommercialModel (ResourceCommercialModel resourceCommercialModel)
        {
            var upsertCommercialModel = await _noteRepository.UpsertResourceCommercialModel(resourceCommercialModel);

            return upsertCommercialModel;
        }

        public async Task<ResourceCD> UpsertResourceRecentCD(ResourceCD resourceSkills)
        {
            var upsertedResource = await _noteRepository.UpsertResourceRecentCD(resourceSkills);
            return upsertedResource;
        }

        public async Task<IEnumerable<ResourceViewNote>> GetResourceNotesByLastUpdatedDate(DateTime lastupdatedAfter, string noteTypeCode)
        {
            if (lastupdatedAfter == null || lastupdatedAfter.Date == DateTime.MinValue)
            {
                return Enumerable.Empty<ResourceViewNote>();
            }
            var resourceNotes = await _noteRepository.GetResourceNotesByLastUpdatedDate(lastupdatedAfter, noteTypeCode);
            return resourceNotes;
        }


        #region Private Methods
        private bool isCaseViewNoteValid(CaseViewNote caseViewNote)
        {
            if (caseViewNote == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(caseViewNote.OldCaseCode)
                && IsGuidNullOrEmpty(caseViewNote.PipelineId)
                && IsGuidNullOrEmpty(caseViewNote.PlanningCardId))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private bool IsGuidNullOrEmpty(Guid? value)
        {
            return value == null || value.Equals(Guid.Empty);
        }
          
        #endregion
    }
}
