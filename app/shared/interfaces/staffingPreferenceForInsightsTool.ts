export interface StaffingPreferenceForInsightsTool {
  id: string;
  employeeCode: string;
  pdFocusAreas: string;
  pdFocusAreasAdditionalInformation: string;
  firstPriority: string;
  secondPriority: string;
  thirdPriority: string;
  industryCodesHappyToWorkIn: string;
  industryCodesExcitedToWorkIn: string;
  industryCodesNotInterestedToWorkIn: string;
  capabilityCodesHappyToWorkIn: string;
  capabilityCodesExcitedToWorkIn: string;
  capabilityCodesNotInterestedToWorkIn: string;
  preBainExperience: string;
  travelInterest: string;
  travelRegions: string;
  travelDuration: string;
  additionalTravelInfo: string;
  lastUpdated: Date;
  lastUpdatedBy: string;
  lastUpdatedByName: string;
}
