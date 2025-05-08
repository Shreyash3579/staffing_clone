import { Office } from './office.interface';

export interface StaffingCommitment {
    planningCardId?: string,
    planningCardName?: string;
    id?: string;
    placeholderId?: string,
    oldCaseCode: string;
    caseName: string;
    caseTypeCode?: number;
    clientName: string;
    pipelineId: string;
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
    investmentCode: number;
    investmentName: string;
    caseRoleCode: string;
    lastUpdatedBy: string;
    caseStartDate?: string;
    caseEndDate?: string;
    opportunityStartDate?: string;
    opportunityEndDate?: string;
    notes?: string;
    description: string;
    status?: string;
    type: string;
    role?: string;
    trainingName?: string;
    commitmentTypeCode: string;
    commitmentTypeName: string;
    commitmentTypeReasonCode: string;
    commitmentTypeReasonName: string;
    billCode: number;
    fte: number;
    operatingOffice: Office;
    position: string;
    operatingOfficeCurrent: Office;
    operatingOfficeProposed: Office;
    lastUpdated: string;
    // commitmentType: string;
    isEditable?: boolean;
    caseRoleName?: string;
    probabilityPercent?: number;
    isPlaceholderAllocation?: boolean;
    approvedBy?:string;
    includeInCapacityReporting?: boolean;
}
