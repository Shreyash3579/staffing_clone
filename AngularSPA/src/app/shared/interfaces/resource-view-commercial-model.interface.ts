import { Employee } from "./employee.interface";

export interface ResourceViewCommercialModel {
  id?: string;
  employeeCode?: string;
  commercialModel: string;
  createdBy: string;
  createdByName?: string;
  lastUpdated?: Date;
  lastUpdatedBy: string;
}