using System;

namespace Staffing.HttpAggregator.Models.Workday
{
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
        public ProfileModel PositionCurrent { get; set; }
        public ProfileModel PositionProposed { get; set; }
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

    }
}
