import { ResourceAllocation } from './resourceAllocation.interface';

export interface Case {
  clientCode: number;
  oldCaseCode: string;
  caseCode: number;
  caseName: string;
  clientName: string;
  officeAbbreviation: string;
  startDate: string;
  endDate: string;
  type: string;
  allocatedUsers: ResourceAllocation[];
}
