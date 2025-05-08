import { Resource } from './resource.interface';
import { ResourceAllocation } from './resourceAllocation.interface';
import { ResourceLoA } from './resourceLoA';
import { Vacation } from './vacation';
import { Training } from './training';
import { CommitmentView } from './commitmentView';
import { ResourceTransition } from './resourceTranstion';
import { ResourceTransfer } from './resourceTransfer';
import { ResourceTermination } from './resourceTermination.interface';
import { ResourceTimeOff } from './resourceTimeOff.interface';
import { PlaceholderAllocation } from './placeholderAllocation.interface';
import { StaffableAsRole } from './staffableAsRole.interface';
import { ResourceOrCasePlanningViewNote } from './resource-or-case-planning-view-note.interface';
import { Holiday } from './holiday';

export interface ResourceCommitment {
  resources: Resource[];
  allocations: ResourceAllocation[];
  placeholderAllocations: ResourceAllocation[];
  loAs: ResourceLoA[];
  vacations: Vacation[];
  trainings: Training[];
  commitments: CommitmentView[];
  transitions: ResourceTransition[];
  transfers: ResourceTransfer[];
  terminations: ResourceTermination[];
  timeOffs: ResourceTimeOff[];
  placeholderAndPlanningCardAllocations: PlaceholderAllocation[];
  staffableAsRoles: StaffableAsRole[];
  resourceViewNotes?: ResourceOrCasePlanningViewNote[];
  holidays?: Holiday[];
}
