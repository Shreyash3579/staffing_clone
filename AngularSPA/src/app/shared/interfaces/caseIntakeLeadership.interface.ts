export interface CaseIntakeLeadership {
    id?: string;
    employeeCode: string;
    employeeName?: string;
    caseRoleCode: string;
    caseRoleName?: string;
    officeAbbreviation?: string;
    allocationPercentage?: number; // Use number for short? as TypeScript doesn't have a short type
    oldCaseCode?: string;
    opportunityId?: string; // Use string for Guid, as TypeScript doesn't have a Guid type
    planningCardId?: string; // Use string for Guid, as TypeScript doesn't have a Guid type
    lastUpdated?: Date;
    lastUpdatedBy: string;
    lastUpdatedByName?: string;
  }
