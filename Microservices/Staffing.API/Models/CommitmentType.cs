namespace Staffing.API.Models
{
    public class CommitmentType
    {
        public string CommitmentTypeCode { get; set; }
        public string CommitmentTypeName { get; set; }
        public int Precedence { get; set; }
        public int ReportingPrecedence { get; set; }
        public bool? IsStaffingTag { get; set; }
        public bool? IsHidden { get; set; }
        public bool? AllowsStaffingInAmericas { get; set; }
        public bool? AllowsStaffingInEMEA { get; set; }
        public bool? AllowsStaffingInAPAC { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
