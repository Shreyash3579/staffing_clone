export interface CasePlanningBoardBucketPreferences {
  id?: string;
  employeeCode: string;
  bucketId: number;
  includeInDemand: boolean;
  isPartiallyChecked: boolean;
  lastUpdatedBy: string;
}
