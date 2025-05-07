import { Office } from './office.interface';
import { ServiceLine } from './serviceLine.interface';
import { Position } from './position.interface';

export interface EmployeeBasic {
  employeeCode: string;
  firstName: string;
  lastName: string;
  fullName: string;
  profileImageUrl?: string;
}

export interface Employee extends EmployeeBasic {
  levelName: string;
  levelGrade: string;
  fte: number;
  activeStatus: string;
  internetAddress: string;
  startDate: string;
  terminationDate: string;
  isTerminated: boolean;
  office: Office;
  serviceLine: ServiceLine;
  position: Position;
  token: string;
  isAdmin: boolean;
  override: boolean;
  schedulingOffice: Office;
  mentorEcode: string;
  mentorName: string;
  hcpdOfficeCodes: number[];
  hasAccessToAISearch: boolean;
  hasAccessToStaffingInsightsTool: boolean;
  hasAccessToRetiredStaffingTab: boolean;
}
