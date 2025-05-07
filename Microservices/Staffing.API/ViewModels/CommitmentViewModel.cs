using System;

namespace Staffing.API.ViewModels
{
    public class CommitmentViewModel
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? Allocation { get; set; }
        public bool? IsSourceStaffing { get; set; }
        public bool? IsOverridenInSource { get; set; }
        public string Description { get; set; }
        public string CommitmentTypeCode { get; set; }
        public string CommitmentTypeName { get; set; }
        public string CommitmentTypeReasonCode { get; set; }
        public string CommitmentTypeReasonName { get; set; }
        public int Precdence { get; set; }
        public int ReportingPrecdence { get; set; }
        public bool? IsStaffingTag { get; set; }
    }
}
