import { CaseIntakeLeadership } from "./caseIntakeLeadership.interface";

export interface CaseIntakeLeadershipGroup {
  caseRoleName: string;
  caseRoleCode: string;

  leaderships: CaseIntakeLeadership[];
}
