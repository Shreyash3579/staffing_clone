import { Action } from '@ngrx/store';
import { CaseIntakeDetail } from 'src/app/shared/interfaces/caseIntakeDetail.interface';
import { CaseIntakeLeadership } from 'src/app/shared/interfaces/caseIntakeLeadership.interface';
import { CaseIntakeRoleDetails } from 'src/app/shared/interfaces/caseIntakeRoleDetails.interface';
import { CaseIntakeWorkstreamDetails } from 'src/app/shared/interfaces/caseIntakeWorkstreamDetails.interface';
import { demandId } from 'src/app/shared/interfaces/demandId';

export enum StaffingIntakeActionTypes {
  ClearStaffingIntakeState = '[Staffing Intake] Clear Staffing Intake State',
  LoadLeadershipDetails = '[Staffing Intake] Get Leadership Details',
  LoadLeadershipDetailsSuccess = '[Staffing Intake] Get Leadership Details Success',
  LoadCaseIntakeDetails = '[Staffing Intake] Get Case Intake Details',
  LoadCaseIntakeDetailsSuccess = '[Staffing Intake] Get Case Intake Details Success',
  LoadCaseBasicDetails = '[Staffing Intake] Get Case Basic Details',
  LoadCaseBasicDetailsSuccess = '[Staffing Intake] Get Case Basic Details Success',
  LoadOpportunityDetails = '[Staffing Intake] Get Opportunity Details',
  LoadOpportunityDetailsSuccess = '[Staffing Intake] Get Opportunity Details Success',
  LoadRoleAndWorkstreamDetails = '[Staffing Intake] Get Role and Workstream Details',
  LoadRoleAndWorkstreamDetailsSuccess = '[Staffing Intake] Get Role and Workstream Details Success',
  UpsertRoleDetails = '[Staffing Intake] Upsert Role Details',
  UpsertRoleDetailsSuccess = '[Staffing Intake] Upsert Role Details Success',
  UpsertRolesAndWorkstreamDetails = '[Staffing Intake] Upsert Roles and Workstream Details',
  UpsertRolesAndWorkstreamDetailsSuccess = '[Staffing Intake] Upsert Roles and Workstream Details Success',
  DeleteWorstreamsById = '[Staffing Intake] Delete Workstreams By Id',
  DeleteWorstreamsByIdSuccess = '[Staffing Intake] Delete Workstreams By Id Success',
  DeleteRolesById = '[Staffing Intake] Delete Roles By Id',
  DeleteRolesByIdSuccess = '[Staffing Intake] Delete Roles By Id Success',
  UpsertCaseIntakeDetails = '[Staffing Intake] Upsert Case Intake Details',
  UpsertCaseIntakeDetailsSuccess = '[Staffing Intake] Upsert Case Intake Details Success',
  UpsertLeadershipDetails = '[Staffing Intake] Upsert Leadership Details',
  UpsertLeadershipDetailsSuccess = '[Staffing Intake] Upsert Leadership Details Success',
  DeleteLeadershipDetail = '[Staffing Intake] Delete Leadership Detail',
  DeleteLeadershipDetailSuccess = '[Staffing Intake] Delete Leadership Detail Success',
  LoadPlanningCardDetails = '[Staffing Intake] Get Planning Card Details',
  LoadPlanningCardDetailsSuccess = '[Staffing Intake] Get Planning Card Details Success',
  LoadPlanningCardDetailsNotFound = '[Staffing Intake] Get Planning Card Details Failure',
  LoadLastUpdatedByChanges = '[Staffing Intake] Get Last Updated By Changes',
  LoadLastUpdatedByChangesSuccess = '[Staffing Intake] Get Last Updated By Changes Success',
  GetExpertiseRequirementList = '[Staffing Intake] Get Expertise list',
  GetExpertiseRequirementListSuccess = '[Staffing Intake] Get Expertise list Success',
  UpsertExpertiseRequirementList = '[Staffing Intake] Upsert Expertise list',
  UpsertExpertiseRequirementListSuccess = '[Staffing Intake] Upsert Expertise list Success',
}

export class ClearStaffingIntakeState implements Action {
  readonly type = StaffingIntakeActionTypes.ClearStaffingIntakeState;
}

export class LoadLeadershipDetails implements Action {
  readonly type = StaffingIntakeActionTypes.LoadLeadershipDetails;
  constructor(public payload: demandId) { }
}

export class LoadCaseIntakeDetails implements Action {
  readonly type = StaffingIntakeActionTypes.LoadCaseIntakeDetails;
  constructor(public payload: demandId) { }
}

export class LoadLeadershipDetailsSuccess implements Action {
  readonly type = StaffingIntakeActionTypes.LoadLeadershipDetailsSuccess;
  constructor(public payload: CaseIntakeLeadership[]) { }
}

export class LoadCaseIntakeDetailsSuccess implements Action {
  readonly type = StaffingIntakeActionTypes.LoadCaseIntakeDetailsSuccess;
  constructor(public payload: CaseIntakeDetail) { }
}

export class LoadCaseBasicDetails implements Action {
  readonly type = StaffingIntakeActionTypes.LoadCaseBasicDetails;
  constructor(public payload: string) { }
}

export class LoadCaseBasicDetailsSuccess implements Action {
  readonly type = StaffingIntakeActionTypes.LoadCaseBasicDetailsSuccess;
  constructor(public payload: any) { }
}

export class LoadOpportunityDetails implements Action {
  readonly type = StaffingIntakeActionTypes.LoadOpportunityDetails;
  constructor(public payload: string) { }
}

export class LoadOpportunityDetailsSuccess implements Action {
  readonly type = StaffingIntakeActionTypes.LoadOpportunityDetailsSuccess;
  constructor(public payload: any) { }
}

export class LoadRoleAndWorkstreamDetails implements Action {
  readonly type = StaffingIntakeActionTypes.LoadRoleAndWorkstreamDetails;
  constructor(public payload: any) { }
}

export class LoadRoleAndWorkstreamDetailsSuccess implements Action {
  readonly type = StaffingIntakeActionTypes.LoadRoleAndWorkstreamDetailsSuccess;
  constructor(public payload: any) { }
}

export class UpsertRoleDetails implements Action {
  readonly type = StaffingIntakeActionTypes.UpsertRoleDetails;
  constructor(public payload: CaseIntakeRoleDetails[]) { }
}

export class UpsertRoleDetailsSuccess implements Action {
  readonly type = StaffingIntakeActionTypes.UpsertRoleDetailsSuccess;
  constructor(public payload: any) { }
}

export class UpsertRolesAndWorkstreamDetails implements Action {
  readonly type = StaffingIntakeActionTypes.UpsertRolesAndWorkstreamDetails;
  constructor(public payload: any) { }
}

export class UpsertRolesAndWorkstreamDetailsSuccess implements Action {
  readonly type = StaffingIntakeActionTypes.UpsertRolesAndWorkstreamDetailsSuccess;
  constructor(public payload: any) { }
}


export class DeleteWorstreamsById implements Action {
  readonly type = StaffingIntakeActionTypes.DeleteWorstreamsById;
  constructor(public payload: any) { }
}

export class DeleteWorstreamsByIdSuccess implements Action {
  readonly type = StaffingIntakeActionTypes.DeleteWorstreamsByIdSuccess;
  constructor(public payload: any) { }
}

export class DeleteRolesById implements Action {
  readonly type = StaffingIntakeActionTypes.DeleteRolesById;
  constructor(public payload: any) { }
}

export class DeleteRolesByIdSuccess implements Action {
  readonly type = StaffingIntakeActionTypes.DeleteRolesByIdSuccess;
  constructor(public payload: any) { }
}

export class UpsertCaseIntakeDetails implements Action {
  readonly type = StaffingIntakeActionTypes.UpsertCaseIntakeDetails;
  constructor(public payload: CaseIntakeDetail) { }
}

export class UpsertCaseIntakeDetailsSuccess implements Action {
  readonly type = StaffingIntakeActionTypes.UpsertCaseIntakeDetailsSuccess;
  constructor(public payload: CaseIntakeDetail) { }
}

export class UpsertLeadershipDetails implements Action {
  readonly type = StaffingIntakeActionTypes.UpsertLeadershipDetails;
  constructor(public payload: CaseIntakeLeadership[]) { }
}

export class UpsertLeadershipDetailsSuccess implements Action {
  readonly type = StaffingIntakeActionTypes.UpsertLeadershipDetailsSuccess;
  constructor(public payload: CaseIntakeLeadership[]) { }
}

export class DeleteLeadershipDetail implements Action {
  readonly type = StaffingIntakeActionTypes.DeleteLeadershipDetail;
  constructor(public payload: any) { }
}

export class DeleteLeadershipDetailSuccess implements Action {
  readonly type = StaffingIntakeActionTypes.DeleteLeadershipDetailSuccess;
  constructor(public payload: any) { }
}

export class LoadPlanningCardDetails implements Action {
  readonly type = StaffingIntakeActionTypes.LoadPlanningCardDetails;
  constructor(public payload: any) { }
}

export class LoadPlanningCardDetailsSuccess implements Action {
  readonly type = StaffingIntakeActionTypes.LoadPlanningCardDetailsSuccess;
  constructor(public payload: any) { }
}

export class LoadPlanningCardDetailsNotFound implements Action {
  readonly type = StaffingIntakeActionTypes.LoadPlanningCardDetailsNotFound;
  constructor(public payload: any) { }
}

export class LoadLastUpdatedByChanges implements Action {
  readonly type = StaffingIntakeActionTypes.LoadLastUpdatedByChanges;
  constructor(public payload: any) { }
}

export class LoadLastUpdatedByChangesSuccess implements Action {
  readonly type = StaffingIntakeActionTypes.LoadLastUpdatedByChangesSuccess;
  constructor(public payload: any) { }
}

export class GetExpertiseRequirementList implements Action {
  readonly type = StaffingIntakeActionTypes.GetExpertiseRequirementList;
}

export class GetExpertiseRequirementListSuccess implements Action {
  readonly type = StaffingIntakeActionTypes.GetExpertiseRequirementListSuccess;
  constructor(public payload: any) { }
}

export class UpsertExpertiseRequirementList implements Action {
  readonly type = StaffingIntakeActionTypes.UpsertExpertiseRequirementList;
  constructor(public payload: any) { }
}

export class UpsertExpertiseRequirementListSuccess implements Action {
  readonly type = StaffingIntakeActionTypes.UpsertExpertiseRequirementListSuccess;
  constructor(public payload: any) { }
}

export type StaffingIntakeActions =
ClearStaffingIntakeState
| LoadLeadershipDetails 
| LoadCaseIntakeDetails
| LoadLeadershipDetailsSuccess
| LoadCaseIntakeDetailsSuccess
| LoadCaseBasicDetails
| LoadCaseBasicDetailsSuccess
| LoadOpportunityDetails
| LoadOpportunityDetailsSuccess
| LoadRoleAndWorkstreamDetails
| LoadRoleAndWorkstreamDetailsSuccess
| UpsertRoleDetails
| UpsertRoleDetailsSuccess
| UpsertRolesAndWorkstreamDetails
| UpsertRolesAndWorkstreamDetailsSuccess
| DeleteWorstreamsById
| DeleteWorstreamsByIdSuccess
| DeleteRolesById
| DeleteRolesByIdSuccess
| UpsertCaseIntakeDetails
| UpsertCaseIntakeDetailsSuccess
| UpsertLeadershipDetails
| UpsertLeadershipDetailsSuccess
| DeleteLeadershipDetail
| DeleteLeadershipDetailSuccess
| LoadPlanningCardDetails
| LoadPlanningCardDetailsSuccess
| LoadPlanningCardDetailsNotFound
| LoadLastUpdatedByChanges
| LoadLastUpdatedByChangesSuccess
| GetExpertiseRequirementList
| GetExpertiseRequirementListSuccess
| UpsertExpertiseRequirementList
| UpsertExpertiseRequirementListSuccess;

