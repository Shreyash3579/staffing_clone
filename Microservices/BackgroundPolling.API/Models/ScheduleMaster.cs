using System;

namespace BackgroundPolling.API.Models
{
    public class ScheduleMaster
    {
        public Guid? Id { get; set; }
        public int? ClientCode { get; set; }
        public int? CaseCode { get; set; }
        public int? CaseTypeCode { get; set; }
        public string OldCaseCode { get; set; }
        public string EmployeeCode { get; set; }
        public string ServiceLineCode { get; set; }
        public string ServiceLineName { get; set; }
        public short OperatingOfficeCode { get; set; }
        public string CurrentLevelGrade { get; set; }
        public short Allocation { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? PipelineId { get; set; }
        public short? InvestmentCode { get; set; }
        public string CaseRoleCode { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Notes { get; set; }
        //This does a shallow copy ONLY
        public ScheduleMaster Clone()
        {
            return (ScheduleMaster)this.MemberwiseClone();
        }
    }
}
