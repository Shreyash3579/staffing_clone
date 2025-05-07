using Staffing.API.Models;
using System;

namespace Staffing.API.ViewModels
{
    public class InsertCaseOppCommitmentViewModel : CaseOppCommitment
    {
        public string EmployeeCode { get; set; }
        public string CommitmentTypeCode { get; set; }
        public string CommitmentTypeReasonCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? Allocation { get; set; }
        public string Notes { get; set; }
        //public bool? IsSourceStaffing { get; set; }
    }
}
