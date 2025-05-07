using System;

namespace Staffing.API.Models
{
    public class StaffingResponsible
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public string ResponsibleForStaffingCodes { get; set; }
        public string pdLeadCodes { get; set; }
        public string notifyUponStaffingCodes { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
