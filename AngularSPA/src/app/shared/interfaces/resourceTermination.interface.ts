import { Office } from './office.interface';
import { ServiceLine } from './serviceLine.interface';

export interface ResourceTermination {
  employeeCode: string;
  employeeName: string;
  endDate: string;
  levelGrade: string;
  billCode: number;
  fte: number;
  operatingOffice: Office;
  position: string;
  serviceLine: ServiceLine;
  lastUpdated: string;
  type: string;
}
