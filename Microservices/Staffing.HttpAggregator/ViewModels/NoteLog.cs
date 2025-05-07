using System;

namespace Staffing.HttpAggregator.ViewModels
{
    public class NoteLog
    {        
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; }
        public string oldCaseCode { get; set; }
        public string NoteDescription { get; set; }
        public string AddedBy { get; set; }
        public string SharedWith { get; set; }
        public bool IsPrivate { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedByName { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
