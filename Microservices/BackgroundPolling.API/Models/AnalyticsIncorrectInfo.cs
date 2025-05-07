using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BackgroundPolling.API.Models
{
    public class AnalyticsIncorrectInfo
    {
        public IList<WorkdayStaffingTransactionMismatch> StaffingTransactionMismatch { get; set; }
        public IList<WorkdayLoATransactionMismatch> LoATransactionMismatch { get; set; }
        public IList<IncorrectInfo> IncorrectAnlayticsData { get; set; }
        public IList<CcmBillRateMismatch> BillRateMismatch { get; set; }

    }

    public class WorkdayStaffingTransactionMismatch
    {
        public string Remarks { get; set; }
        public Guid Id { get; set; }
        public Guid ScheduleId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string OperatingOfficeAbbreviation { get; set; }
        public string WDOperatingOfficeAbbreviation { get; set; }
        public string OfficeName { get; set; }
        public string CurrentLevelGrade { get; set; }
        public string WDCurrentLevelGrade { get; set; }
        public string ServiceLineCode { get; set; }
        public string WDServiceLineCode { get; set; }
        public string ServiceLineName { get; set; }
        public string WDServiceLineName { get; set; }
        public string PositionGroupName { get; set; }
        public string WDPositionGroupName { get; set; }
        public string Fte { get; set; }
        public string WDFte { get; set; }
        public string BillCode { get; set; }
        public string WDBillCode { get; set; }
        public string EmployeeStatusCode { get; set; }
        public string WDEmployeeStatusCode { get; set; }

        [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
        public DateTime? EffectiveDate { get; set; }

        [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
        public DateTime? TerminationEffectiveDate { get; set; }

        [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
        public DateTime? TransitionStartDate { get; set; }

        [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
        public DateTime? TransitionEndDate { get; set; }

        [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
        public DateTime Date { get; set; }
    }
    public class WorkdayLoATransactionMismatch
    {
        public string Remarks { get; set; }
        public Guid Id { get; set; }
        public Guid ScheduleId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public int EmployeeStatusCode { get; set; }

        [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
        public DateTime? WDlastDayOfWork { get; set; }

        [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
        public DateTime? WDfirstDayOfLeave { get; set; }

        [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
        public DateTime? WDactualLastDayOfLeave { get; set; }

        [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
        public DateTime? WDestimatedLastDayOfleave { get; set; }

        [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
        public DateTime Date { get; set; }
    }
    public class IncorrectInfo
    {
        public string Remarks { get; set; }
        public Guid Id { get; set; }
        public Guid ScheduleId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string OperatingOfficeAbbreviation { get; set; }
        public string OperatingOfficeName { get; set; }
        public string CurrentLevelGrade { get; set; }

        [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
        public DateTime Date { get; set; }
    }
    public class CcmBillRateMismatch
    {
        public string Remarks { get; set; }
        public Guid Id { get; set; }
        public Guid ScheduleId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string OperatingOfficeAbbreviation { get; set; }
        public string OperatingOfficeCode { get; set; }
        public string BRofficeCode { get; set; }
        public string CurrentLevelGrade { get; set; }
        public string BRlevelGrade { get; set; }
        public string Fte { get; set; }
        public string BillCode { get; set; }
        public string BRbillCode { get; set; }
        public int? Allocation { get; set; }
        public int? Availability { get; set; }

        [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
        public DateTime Date { get; set; }
        public int WorkingDays { get; set; }
        public string ActualCost { get; set; }
        public string ExpectedActualCost { get; set; }
        public string OpportunityCost { get; set; }
        public string ExpectedOpportunityCost { get; set; }
        public string BillRate { get; set; }
        public string ExpectedBillRate { get; set; }
        public string BRbillRate { get; set; }

        [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
        public DateTime BRstartDate { get; set; }

        [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
        public DateTime? BRendDate { get; set; }

    }
}
