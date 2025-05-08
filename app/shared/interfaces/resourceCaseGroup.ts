import { ProjectBasic } from './project.interface';
import { ResourceStaffing } from './resourceStaffing.interface';

export interface ResourceCaseGroup {
  cases: ProjectBasic[];
  resources: ResourceStaffing[]
}