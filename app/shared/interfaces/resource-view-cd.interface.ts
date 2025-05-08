import { Employee } from "./employee.interface";

export interface ResourceViewCD {
  id?: string;
  employeeCode?: string;
  recentCD: string;
  createdBy: string;
  createdByName?: string;
  lastUpdated?: Date;
  lastUpdatedBy: string;
}