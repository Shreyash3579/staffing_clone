import { Employee } from "./employee.interface";

export interface ResourceOrCasePlanningViewNote {
  id?: string;
  employeeCode?: string;
  oldCaseCode?: string;
  pipelineId?: string;
  planningCardId?: string;
  noteTypeCode?: string;
  note: string;
  isPrivate: boolean;
  sharedWith: string;
  sharedWithDetails?: Employee[];
  createdBy: string;
  createdByName?: string;
  lastUpdated?: Date;
  lastUpdatedBy: string;
}
