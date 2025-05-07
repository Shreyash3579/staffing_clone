export interface RingfenceManagement {
  id?: string;
  officeCode: number;
  officeName: string;
  rfTeamsOwed?: number;
  totalRFResources: number;
  commitmentTypeCode: number;
  commitmentTypeName: string,
  effectiveDate: string;
  lastUpdatedBy: string;
  lastUpdatedByName: string;
}
