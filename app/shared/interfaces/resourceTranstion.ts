import { Office } from './office.interface';

export interface ResourceTransition {
  employeeCode: string;
  employeeName: string;
  startDate: string;
  endDate: string;
  levelGrade: string;
  billCode: number;
  fte: number;
  operatingOffice: Office;
  position: string;
  type: string;
}
