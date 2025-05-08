export interface CasePlanningBoardStaffableTeamViewModel{
  officeName: string;
  officeCode: number,
  gcTeamCount: number,
  pegTeamCount: number,
  lastUpdatedBy: string,
  staffableTeamChildren : CasePlanningBoardStaffableTeamViewModel[];
}