export interface EmployeeStaffingPreferences {
    employeeCode: string;
    preferenceType: string;
    staffingPreferences: staffingPreference[];
    lastUpdatedBy: string;
}

export interface staffingPreference {
    code: string;
    name: string;
    interest: boolean;
    noInterest: boolean;
}