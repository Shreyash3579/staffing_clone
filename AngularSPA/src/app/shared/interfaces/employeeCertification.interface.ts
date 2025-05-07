export interface EmployeeCertification {
    employeeCode: string;
    certifications: Certification[];
}

export interface Certification {
    name: string;
    issuedDate: string;
}
