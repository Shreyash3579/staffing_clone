import { UserPreferenceSupplyGroupMember } from "./userPreferenceSupplyGroupMember";

export interface UserPreferenceSupplyGroupSharedInfoViewModel {
  id?: string;
  sharedWith: string;
  userPreferenceGroupId: string;
  lastUpdatedBy?: string;
  sharedWithMemberDetails: UserPreferenceSupplyGroupMember;
}
