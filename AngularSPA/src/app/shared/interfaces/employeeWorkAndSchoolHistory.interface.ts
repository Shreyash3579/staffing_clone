import { EmployeeSchoolHistory } from "./employeeSchoolHistory";
import { EmployeeWorkHistory } from "./employeeWorkHistory";

export interface EmployeeWorkAndSchoolHistory {
    ecode: string;
    educationHistory: EmployeeSchoolHistory[];
    employmentHistory: EmployeeWorkHistory[];
}
