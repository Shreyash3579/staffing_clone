export interface EmployeeLanguage {
    employeeCode: string;
    languages: Language[];
}

export interface Language {
    name: string;
    proficiencyCode: number;
    proficiencyName: string;
}

