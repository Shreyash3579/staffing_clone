import { Office } from './office.interface';

export interface ResourceTransfer{
  employeeCode: string;
  employeeName: string;
  startDate: string;
  levelGrade: string;
  billCode: number;
  fte: number;
  operatingOffice: Office;
  operatingOfficeCurrent: Office;
  operatingOfficeProposed: Office;
  position: string;
  type: string;
}
