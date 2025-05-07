export interface CommitmentType {
  commitmentTypeCode: string;
  commitmentTypeName: string;
  precedence: number;
  reportingPrecedence?: number;
  isStaffingTag?: boolean;
  allowsStaffingInAmericas?: boolean;
  allowsStaffingInEMEA?: boolean;
  allowsStaffingInAPAC?: boolean;
  lastUpdatedBy?: string;
  children?: CommitmentType[]; 
}
