import { CaseIntakeRoleDetails } from "./caseIntakeRoleDetails.interface";

export interface CaseIntakeWorkstreamDetails {
    id?: string; // Use string for Guid, as TypeScript doesn't have a Guid type
    name: string;
    skuSize: string;
    oldCaseCode: string;
    opportunityId?: string; // Use string for Guid, as TypeScript doesn't have a Guid type
    planningCardId?: string; // Use string for Guid, as TypeScript doesn't have a Guid type
    roles?: CaseIntakeRoleDetails[];
    lastUpdated?: Date;
    lastUpdatedBy: string;
    LastUpdatedByName?: string; 
  }
  