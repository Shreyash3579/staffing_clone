export interface CaseOppCortexTeamSize {
  oldCaseCode: string;
  pipelineId: string;
  cortexOpportunityId?: string;
  estimatedTeamSize: string;
  isPlaceholderCreatedFromCortex?: boolean;
  lastUpdatedBy: string;
}
