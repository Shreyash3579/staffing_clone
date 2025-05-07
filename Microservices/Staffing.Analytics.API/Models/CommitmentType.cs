namespace Staffing.Analytics.API.Models
{
    public class CommitmentType
    {
        public string CommitmentTypeCode { get; set; }
        public string CommitmentTypeName { get; set; }
        public int Precedence { get; set; }
        public int ReportingPrecedence { get; set; }
        public bool IsStaffingTag { get; set; }
    }
}
