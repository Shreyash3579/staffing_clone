using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;

namespace Staffing.HttpAggregator.ViewModels
{
    public class ResourceViewNoteViewModel
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public string Note { get; set; }
        public bool IsPrivate { get; set; }
        public string SharedWith { get; set; }
        public List<Resource> SharedWithDetails { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public string NoteTypeCode { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
