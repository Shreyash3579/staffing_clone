export interface ResourceNotesAlert {
  id: string;
  noteID: string;
  pipelineId?: string;
  planningCardId?: string;
  oldCaseCode?: string;
  planningCardName: string;
  caseName: string;
  oppName: string;
  employeeCode: string;
  employeeName: string;
  noteForEmployeeCode: string;
  noteForEmployeeName: string;
  alertStatus: string;
  lastUpdated: string;
  lastUpdatedBy: string;
  createdBy: string;
  createdByEmployeeName: string;
  note: string;
  noteTypeCode: string;
}