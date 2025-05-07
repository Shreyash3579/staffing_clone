import { UserPreferenceGroupFilters } from "./UserPreferenceGroupFilters";
import { UserPreferenceSupplyGroupSharedInfoViewModel } from "./userPrefernceSupplyGroupSharedInfoViewModel";

export interface ResourceFilter {
    id: string;
    title: string;
    description?: string;
    isDefault: boolean;
    officeCodes: string;
    staffingTags: string;
    levelGrades: string;
    positionCodes: string;
    employeeStatuses: string;
    practiceAreaCodes: string;
    resourcesTabSortBy?: string;
    filterBy?: UserPreferenceGroupFilters[];
    lastUpdatedBy: string;
    staffableAsTypeCodes: string;
    affiliationRoleCodes:string;
    sharedWith : UserPreferenceSupplyGroupSharedInfoViewModel[];
}
