export interface EmployeeSearchResult {
  id: string;
  score: number;
  employeeCode: string;
  fullName: string;
  levelGrade: string;
  levelName: string;
  serviceLineName: string;
  aggregatedRingfences: string;
  staffingTag: string;
  positionName: string;
  hireDate: Date; 
  startDate: Date; 
  terminationDate: Date | null; 
  departmentName: string | null;
  operatingOfficeAbbreviation: string;
  operatingOfficeName: string;
  certificates: string; 
  clientsWorkedWith: any[] | null; 
  languages: Language[];
  aggregatedLanguages: string;
  dateFirstAvailable: string;
  percentAvailable: number; 
}

export interface Language {
  name: string;
  proficiencyName: string;
}

export interface AvailabilityDate {
  date: string;
  availabilityPercent: number;
}