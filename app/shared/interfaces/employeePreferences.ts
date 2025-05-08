export interface EmployeePreferences {
    employeeCode: string;
    preferenceTypeCode: string;
    preferenceTypeName: string;
    staffingPreference: string;
    priority: number;
    lastUpdatedBy: string | null;
    interest: boolean;
    noInterest: boolean;
  }