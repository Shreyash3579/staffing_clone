using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class NoteLogService : INoteLogService
    {
        private readonly IResourceApiClient _resourceApiClient;
        private readonly IStaffingApiClient _staffingApiClient;

        public NoteLogService(IResourceApiClient resourceApiClient, IStaffingApiClient staffingApiClient)
        {
            _resourceApiClient = resourceApiClient;
            _staffingApiClient = staffingApiClient;
        }
        public async Task<IEnumerable<ResourceViewNoteViewModel>> GetResourceNotes(string employeeCode, string loggedInEmployeeCode, string noteTypeCode)
        {
            var resourceViewNotesTask = _staffingApiClient.GetResourceViewNotes(employeeCode, loggedInEmployeeCode, noteTypeCode);

            var resourcesDataTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(resourceViewNotesTask, resourcesDataTask);

            var resourceViewNotes = resourceViewNotesTask.Result;
            var resources = resourcesDataTask.Result;

            if(resourceViewNotes.Count() <= 0)
            {
                return Enumerable.Empty<ResourceViewNoteViewModel>();
            }

            return ConvertToResourceViewNotesViewModel(resourceViewNotes, resources);

        }

        private IEnumerable<ResourceViewNoteViewModel> ConvertToResourceViewNotesViewModel(IEnumerable<ResourceViewNote> resourceViewNotes, List<Resource> resources)
        {
            var est = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            IEnumerable<ResourceViewNoteViewModel> resourceViewNotesModel = resourceViewNotes.Select(note => new ResourceViewNoteViewModel
            {
                Id = note.Id,
                EmployeeCode = note.EmployeeCode,
                Note = note.Note ?? "",
                IsPrivate = note.IsPrivate,
                SharedWith = note.SharedWith,
                SharedWithDetails = !string.IsNullOrEmpty(note.SharedWith) ? resources?.Where(x => note.SharedWith.Split(',').Contains(x.EmployeeCode)).ToList() : null,
                CreatedBy = note.CreatedBy,
                CreatedByName = resources.FirstOrDefault(x => x.EmployeeCode == note.CreatedBy).FullName,
                NoteTypeCode = note.NoteTypeCode,
                LastUpdatedBy = note.LastUpdatedBy,
                LastUpdated = TimeZoneInfo.ConvertTimeToUtc(note.LastUpdated, est)
            });

            return resourceViewNotesModel;
        }
    }
}
