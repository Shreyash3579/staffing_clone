export interface CaseIntakeBasicDetails {
    id: string;
    caseRoleCode: string;
    oldCaseCode?: string | null; 
    opportunityId?: string | null; 
    planningCardId?: string | null; 
    lastUpdatedBy: string; 
  }
