using System;

namespace Staffing.HttpAggregator.Models
{
    public class NotesAlertViewModel
    {
            public Guid Id { get; set; }
            public Guid NoteID { get; set; }
            public Guid? PipelineId { get; set; }
            public Guid? PlanningCardId { get; set; }
            public string OldCaseCode { get; set; }
            public string PlanningCardName { get; set; }
            public string CaseName { get; set; }
            public string OppName { get; set; }
            public string EmployeeCode { get; set; }
            public string EmployeeName { get; set; }
            public string NoteForEmployeeCode { get; set; }
            public string NoteForEmployeeName { get; set; }
            public char AlertStatus { get; set; }
            public DateTime? LastUpdated { get; set; }
            public string LastUpdatedBy { get; set; }
            public string CreatedBy { get; set; }
            public string CreatedByEmployeeName { get; set; }
            public string Note { get; set; }
            public string NoteTypeCode { get; set; }
        }
}
