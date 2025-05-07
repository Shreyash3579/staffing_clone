export interface SecurityGroup {
  id: string;
  groupName: string;
  roleCodes: string;
  featureCodes: string;
  notes: string;
  lastUpdated: Date;
  lastUpdatedBy: string;
}

export interface SecurityGroupDetails extends SecurityGroup {
  roleNames: string;
  lastUpdatedByName: string;
}
