using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface INoteRepository
    {
        Task<IEnumerable<ResourceViewNote>> GetResourceViewNotes(string employeeCodes, string loggedInUser, string noteTypeCode);
        Task<ResourceViewNote> UpsertResourceViewNote(ResourceViewNote resourceViewNote);
        Task<IEnumerable<Guid>> DeleteResourceViewNotes(string idsToDelete, string lastUpdatedBy);
        Task<IEnumerable<CaseViewNote>> GetCaseViewNotes(string oldCaseCodes, string pipelineIds, string planningCardIds, string loggedInUser);
        Task<CaseViewNote> UpsertCaseViewNote(CaseViewNote caseViewNote);
        Task<IEnumerable<Guid>> DeleteCaseViewNotes(string idsToDelete, string lastUpdatedBy);

        Task<IEnumerable<NoteAlert>> GetNotesAlert(string loggedInEmployee);

        Task<IEnumerable<NoteSharedWithGroup>> GetMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode(string employeeCode);

        Task UpdateNotesAlertStaus(string employeeCode);

        Task<IEnumerable<ResourceCommercialModel>> GetCommercialModelList();
        Task<IEnumerable<ResourceCommercialModel>> GetResourceCommercialModel(string employeeCodes);
        Task<IEnumerable<ResourceCD>> GetResourceRecentCD(string employeeCodes);

        Task<IEnumerable<ResourceCD>> GetRecentCDList();
        Task<ResourceCommercialModel> UpsertResourceCommercialModel(ResourceCommercialModel resourceCommercialModel);
        Task<ResourceCD> UpsertResourceRecentCD(ResourceCD resourceCD);

        Task<IEnumerable<Guid>> DeleteResourceViewCD(string idsToDelete, string lastUpdatedBy);

        Task<IEnumerable<Guid>> DeleteResourceCommercialModel(string idsToDelete, string lastUpdatedBy);

        Task<IEnumerable<ResourceViewNote>> GetResourceNotesByLastUpdatedDate(DateTime lastupdatedAfter, string noteTypeCode);
    }
}
