export interface PlanningCardModel {
    id?: string;
    name?: string;
    projectName?: string;
    startDate?: Date;
    endDate?: Date;
    office?: number;
    isShared?: boolean;
    sharedOfficeCodes?: string;
    sharedStaffingTags?: string;
    sharedOfficeNames?: string;
    sharedStaffingTagNames?: string;
    status?: string;
    includeInCapacityReporting?: boolean;
}
