import { UserPreferenceGroupFilters } from "./UserPreferenceGroupFilters";
import { UserPreferenceSupplyGroupSharedInfoViewModel } from "./userPrefernceSupplyGroupSharedInfoViewModel";

export interface UserPreferenceSupplyGroup {
  id?: string;
  name: string;
  description: string;
  createdBy: string;
  isDefault: boolean;
  isDefaultForResourcesTab: boolean;
  isShared: boolean;
  groupMemberCodes: string;
  lastUpdatedBy?: string;
  sortBy?: string;
  filterBy?: UserPreferenceGroupFilters[];
  sharedWith : UserPreferenceSupplyGroupSharedInfoViewModel[];
}
