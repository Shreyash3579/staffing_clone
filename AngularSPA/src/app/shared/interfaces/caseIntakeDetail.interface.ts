export interface CaseIntakeDetail {
    officeCodes: string;
    officeNames: string;
    clientEngagementModel: string;
    clientEngagementModelCodes: string;
    caseDescription: string;
    expertiseRequirement: string;
    languages: string;
    readyToStaffNotes?: string;
    backgroundCheckNotes?: string;
    capabilityPracticeAreaCodes?: string;
    industryPracticeAreaCodes?: string;
    oldCaseCode: string;
    opportunityId?: string; // Use string for Guid, as TypeScript doesn't have a Guid type
    planningCardId?: string; // Use string for Guid, as TypeScript doesn't have a Guid type
    lastUpdated: Date;
    lastUpdatedBy: string;
    lastUpdatedByName?: string;
  }
  