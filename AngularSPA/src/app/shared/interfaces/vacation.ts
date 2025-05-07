export interface Vacation {
  employeeCode: string;
  startDate: string;
  endDate: string;
  description: string;
  status?: string;
  type: string;
  approvedBy?: string
}
