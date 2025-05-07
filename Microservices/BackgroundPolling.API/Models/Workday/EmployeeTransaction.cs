using System;

namespace BackgroundPolling.API.Models.Workday
{
    public class EmployeeTransaction
    {
        public string Id { get; set; }
        public string BusinessProcessEvent { get; set; }
        public string BusinessProcessReason { get; set; }
        public string BusinessProcessType { get; set; }
        public string BusinessProcessName { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string EmployeeStatus { get; set; }
        public DateTime? MostRecentCorrectionDate { get; set; }
        public DateTime? TerminationEffectiveDate { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string TransactionStatus { get; set; }
        public EmployeeTransactionProcess Transaction { get; set; }
    }

    public class EmployeeTransactionProcess
    {
        public decimal BillCodeCurrent { get; set; }
        public decimal BillCodeProposed { get; set; }
        public Office CaseDemandOfficeCurrent { get; set; }
        public Office CaseDemandOfficeProposed { get; set; }
        public string CompanyCurrent { get; set; }
        public string CompanyProposed { get; set; }
        public CostCenterModel CostCenterCurrent { get; set; }
        public CostCenterModel CostCenterProposed { get; set; }
        public OrganizationModel CostCenterHierarchyCurrent { get; set; }
        public OrganizationModel CostCenterHierarchyProposed { get; set; }
        public DepartmentModel DepartmentCurrent { get; set; }
        public DepartmentModel DepartmentProposed { get; set; }
        public decimal FteCurrent { get; set; }
        public decimal FteProposed { get; set; }
        public Office HomeOfficeCurrent { get; set; }
        public Office HomeOfficeProposed { get; set; }
        public Office SchedulingOfficeCurrent { get; set; }
        public Office SchedulingOfficeProposed { get; set; }
        public bool JobExemptCurrent { get; set; }
        public bool JobExemptProposed { get; set; }
        public string PdGradeCurrent { get; set; }
        public string PdGradeProposed { get; set; }
        public string PdStepCurrent { get; set; }
        public string PdStepProposed { get; set; }
        public PositionModel PositionCurrent { get; set; }
        public PositionModel PositionProposed { get; set; }
        public string RotationalRoleCurrent { get; set; }
        public string RotationalRoleProposed { get; set; }
        public string TimeTypeCurrent { get; set; }
        public string TimeTypeProposed { get; set; }
        public string ManagerECodeCurrent { get; set; }
        public string ManagerNameCurrent { get; set; }
        public string ManagerECodeProposed { get; set; }
        public string ManagerNameProposed { get; set; }
        public DateTime? TransitionStartDate { get; set; }
        public DateTime? TransitionEndDate { get; set; }
        public ServiceLineModel ServiceLineCurrent { get; set; }
        public ServiceLineModel ServiceLineProposed { get; set; }
    }

    public class DepartmentModel
    {
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
    }

    public class OrganizationModel
    {
        public string OrganizationName { get; set; }
        public string OrganizationId { get; set; }
    }

    public class CostCenterModel
    {
        public string CostCenterName { get; set; }
        public string CostCenterId { get; set; }
    }

    public class PositionModel
    {
        public string PositionCode { get; set; }
        public string PositionName { get; set; }
        public string PositionGroupName { get; set; }
    }

    public class Office
    {
        public string OfficeCode { get; set; }
        public string OfficeName { get; set; }
        public string OfficeAbbreviation { get; set; }
    }
    public class ServiceLineModel
    {
        public string ServiceLineHierarchyId { get; set; }
        public string ServiceLineHierarchyName { get; set; }
        public string ServiceLineId { get; set; }
        public string ServiceLineName { get; set; }
        public int EmployeeCount { get; set; }
    }
}
