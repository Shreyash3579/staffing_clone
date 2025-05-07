export interface CaseOppCommitment {
  scheduleId: string; 
  id: string;
  employeeCode: string;
  commitmentId: string;
  commitmentTypeCode: string;
  commitmentTypeName: string;
  commitmentTypeReasonCode: string;
  startDate: string;
  endDate: string;
  allocation?: number; 
  notes: string;
  oldCaseCode: string;
  planningCardId: string;
  opportunityId: string;
  lastUpdatedBy: string;
}