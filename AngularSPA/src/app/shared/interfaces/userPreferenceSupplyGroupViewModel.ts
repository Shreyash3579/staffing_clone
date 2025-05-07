import { UserPreferenceGroupFilters } from "./UserPreferenceGroupFilters";
import { UserPreferenceSupplyGroupMember } from "./userPreferenceSupplyGroupMember";
import { UserPreferenceSupplyGroupSharedInfoViewModel } from "./userPrefernceSupplyGroupSharedInfoViewModel";

export interface UserPreferenceSupplyGroupViewModel {
  id?: string;
  name: string;
  description: string;
  createdBy: string;
  isShared: boolean;
  isDefault: boolean;
  isDefaultForResourcesTab: boolean;
  lastUpdatedBy?: string;
  groupMembers: UserPreferenceSupplyGroupMember[];
  sharingOption?: any;
  sharedMembers?: UserPreferenceSupplyGroupMember[];
  sharedWith?: UserPreferenceSupplyGroupSharedInfoViewModel[];
  userPreferences?: any;
  sortRows?: any[];
  sortBy?: string;
  filterBy?: UserPreferenceGroupFilters[];
}
