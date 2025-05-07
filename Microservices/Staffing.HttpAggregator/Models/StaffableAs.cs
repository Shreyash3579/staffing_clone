using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Models
{
    public class StaffableAs
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public string LevelGrade { get; set; }
        public DateTime EffectiveDate { get; set; }
        public short StaffableAsTypeCode { get; set; }
        public string StaffableAsTypeName { get; set; }
        public bool isActive { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
