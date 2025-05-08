export interface ResourceAllocation {
  id?: string;
  oldCaseCode: string;
  caseName: string;
  caseTypeCode?: number;
  clientName: string;
  pipelineId: string;
  planningCardId?: string;
  planningCardTitle?: string;
  isPlanningCardShared?: boolean;
  opportunityName: string;
  employeeCode: string;
  employeeName: string;
  internetAddress?: string;
  operatingOfficeCode: number;
  operatingOfficeAbbreviation: string;
  currentLevelGrade: string;
  serviceLineCode: string;
  serviceLineName: string;
  allocation: number;
  startDate: string;
  endDate: string;
  previousStartDate?: string;
  previousEndDate?: string;
  previousAllocation?: number;
  previousInvestmentCode?:number;
  investmentCode: number;
  investmentName: string;
  caseRoleCode: string;
  lastUpdatedBy: string;
  caseStartDate?: string;
  caseEndDate?: string;
  opportunityStartDate?: string;
  opportunityEndDate?: string;
  notes?: string;
  caseRoleName?: string;
  joiningDate?: string;
  terminationDate?: string;
  isPlaceholderAllocation?: boolean;
  commitmentTypeName?: string,
  commitmentTypeCode?: string,
  commitmentTypeReasonCode?:string,
  commitmentTypeReasonName?:string,
  probabilityPercent?: number;
  positionGroupCode?: string;
  caseManagerName?: string;
  includeInCapacityReporting?: boolean;
}
