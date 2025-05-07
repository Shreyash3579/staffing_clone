using System;

namespace BackgroundPolling.API.Models
{
    public class StaffableAs
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public string LevelGrade { get; set; }
        public DateTime EffectiveDate { get; set; }
        public short StaffableAsTypeCode { get; set; }
        public bool isActive { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
