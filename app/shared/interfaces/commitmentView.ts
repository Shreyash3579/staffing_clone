export interface CommitmentView {
  id?: string;
  employeeCode: string;
  commitmentTypeCode: string;
  commitmentTypeName?: string;
  commitmentTypeReasonCode?: string;
  commitmentTypeReasonName?: string;
  startDate: string;
  endDate: string;
  allocation?: number;
  description: string;
  isSourceStaffing: boolean;
}


export interface CommitmentWithCaseOppInfo extends CommitmentView {
  // Add extra properties here
  oldCaseCode?: string;
  opportunityId?: string;
  plannigCardId?: string;
}
