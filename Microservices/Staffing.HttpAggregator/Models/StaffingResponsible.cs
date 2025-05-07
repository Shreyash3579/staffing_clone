using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Staffing.HttpAggregator.Models
{
    public class StaffingResponsible
    {
        public Guid? Id { get; set; }
        public string EmployeeCode { get; set; }
        public string ResponsibleForStaffingCodes { get; set; }
        public string pdLeadCodes { get; set; }

        public string notifyUponStaffingCodes { get; set; }
        public List<EmployeeDetailsViewModel> responsibleForStaffingDetails { get; set; }
        public List<EmployeeDetailsViewModel> pdLeadDetails { get; set; }

        public List<EmployeeDetailsViewModel> notifyUponStaffingDetails { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}