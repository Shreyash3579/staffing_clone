import { PlaceholderAllocation } from './placeholderAllocation.interface';
import { ResourceOrCasePlanningViewNote } from './resource-or-case-planning-view-note.interface';

export interface PlanningCard {
  id?: string;
  tempid?: string; //created for temporary use since we create id in the backend
  name?: string;
  startDate?: Date;
  endDate?: Date;
  isShared?: boolean;
  sharedOfficeCodes?: string;
  sharedOfficeAbbreviations?: string;
  sharedStaffingTags?: string;
  includeInCapacityReporting?: boolean;
  createdBy?: string;
  mergedCaseCode?: string;
  isMerged?: boolean;
  pegOpportunityId?: string;
  includeInDemand?: boolean;
  isFlagged?: boolean;
  probabilityPercent?:number;
  lastUpdatedBy?: string;
  allocations?: PlaceholderAllocation[];
  placeholderAllocations?: PlaceholderAllocation[];
  regularAllocations?: PlaceholderAllocation[];
  casePlanningViewNotes?: ResourceOrCasePlanningViewNote[];
  trackById?: number;
  isSTACommitmentCreated?: boolean;
  combinedSkuTerm?: string;
}
