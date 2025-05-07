import { UserPreferences } from "./userPreferences.interface";
import { UserPreferenceSupplyGroupViewModel } from "./userPreferenceSupplyGroupViewModel";

export interface CombinedUserPreferences {
  userPreferences: UserPreferences;
  userPreferenceSupplyGroups: UserPreferenceSupplyGroupViewModel[];
}
