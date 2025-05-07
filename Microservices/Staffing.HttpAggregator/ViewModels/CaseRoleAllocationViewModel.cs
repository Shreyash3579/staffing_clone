namespace Staffing.HttpAggregator.ViewModels
{
    public class CaseRoleAllocationViewModel
    {
        public string OldCaseCode { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public int OfficeCode { get; set; }
        public string OfficeAbbreviation { get; set; }
        public string CaseRoleCode { get; set; }
        public string PipelineId { get; set; }
    }
}
