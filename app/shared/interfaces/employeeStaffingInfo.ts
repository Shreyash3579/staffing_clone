export interface EmployeeStaffingInfo {
    id?: string;
    employeeCode: string;
    staffingResponsibleName?: string;
    responsibleForStaffingCodes: string;
    responsibleForStaffingDetails?: any[];
    pdLeadCodes: string;
    pdLeadName?: string;
    pdLeadDetails?: any[];
    notifyUponStaffingName?: string;
    notifyUponStaffingCodes: string;
    notifyUponStaffingDetails?: any[];
    lastUpdatedBy: string;
}