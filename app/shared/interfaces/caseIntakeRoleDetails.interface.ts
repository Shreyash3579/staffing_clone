import { Language } from "./language";
import { Office } from "./office.interface";
import { PracticeArea } from "./practiceArea.interface";
import { PositionGroup } from "./position-group.interface";
import { ServiceLine } from "./serviceLine.interface";
import { CaseIntakeExpertise } from "./caseIntakeExpertise.interface";

export interface CaseIntakeRoleDetails {
    id: string;
    name: string;
    positionCode: string;
    expertiseRequirementCodes: string;
    serviceLineCode: string;
    officeCodes: string;
    officeNames: string;
    selectedLocation: Office[];
    selectedLanguages: Language[];
    selectedExpertise: CaseIntakeExpertise[];
    mustHaveExpertiseCodes: string;
    niceToHaveExpertiseCodes: string;
    selectedPositionGroup: PositionGroup;
    selectedServiceLine: ServiceLine;
    languageCodes: string;
    mustHaveLanguageCodes: string;
    niceToHaveLanguageCodes: string;
    clientEngagementModel: string;
    clientEngagementModelCodes: string;
    isLead: boolean;
    isAssignedInWorkstream: boolean;
    roleDescription: string;
    oldCaseCode: string;
    planningCardId?: string; // Use string for Guid, as TypeScript doesn't have a Guid type
    opportunityId?: string; // Use string for Guid, as TypeScript doesn't have a Guid type
    workstreamId?: string; // Use string for Guid, as TypeScript doesn't have a Guid type
    lastUpdated: Date;
    lastUpdatedBy: string;
    lastUpdatedByName: string;
  }
  