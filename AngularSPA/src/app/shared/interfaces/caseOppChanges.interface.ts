export interface CaseOppChanges {
  pipelineId: string;
  oldCaseCode: string
  startDate: string;
  endDate: string;
  probabilityPercent?: number;
  notes: string;
  staffingOfficeCode?: number;
  lastUpdatedBy?: string;
  caseServedByRingfence?: boolean;
}
