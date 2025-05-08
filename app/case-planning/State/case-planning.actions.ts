import { Action } from '@ngrx/store';
import { Commitment } from 'src/app/shared/interfaces/commitment.interface';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { ResourceOrCasePlanningViewNote } from 'src/app/shared/interfaces/resource-or-case-planning-view-note.interface';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';

export enum CasePlanningActionTypes {
  LoadProjects = '[Case Planning List Page] Projects Load',
  LoadProjectsSuccess = '[Projects API] Projects Load Success',
  LoadProjectsFail = '[Projects API] Projects Load Fail',
  CasePlanningLoader = '[Case Planning Loader] Loader',
  LoadPlanningCards = '[Case Planning List Page] Planning Cards Load',
  LoadPlanningCardsSuccess = '[Planning Cards API] Planning Cards Load Success',
  LoadPlanningCardsFail = '[Planning Cards API] Planning Cards Load Fail',
  LoadCasesOnPageScroll = '[Case Planning List Page] Load On Page Scroll',
  LoadCasesOnPageScrollSuccess = '[Projects API] Load On Page Scroll Success',
  LoadProjectsBySearchString = '[Case Planning List Page] Projects Load Projects By Search String',
  LoadProjectsBySearchStringSuccess = '[Projects API] Projects Load Projects By Search String Success',
  LoadProjectsBySearchStringFail = '[Projects API] Projects Load Projects By Search String Fail',
  ClearSearchData = '[Case Planning List Page] Remove searched projects data',
  UpsertResourceStaffing = '[Case Planning List Page] Upsert Current Resource Staffing',
  UpsertResourceStaffingSuccess = '[Case Planning List Page] Upsert Current Resource Staffing Success',
  UpsertResourceStaffingFail = '[Case Planning List Page] Upsert Current Resource Staffing Fail',
  RefreshCaseAndResourceOverlay = '[Case Planning Case And Resource Overlay] Refresh',
  AddResourceCommitment = '[Case Planning List Page] Add Current Resource Commitment',
  AddResourceCommitmentSuccess = '[Case Planning API] Add Current Resource Commitment Success',
  UpdateResourceCommitment = '[Case Planning List Page] Update Current Resource Commitment',
  UpdateResourceCommitmentSuccess = '[Case Planning API] Update Current Resource Commitment Success',
  DeleteResourceCommitment = '[Case Planning List Page] Delete Current Resource Commitment',
  DeleteResourceCommitmentSuccess = '[Case Planning API] Delete Current Resource Commitment Success',
  DeleteResourceStaffing = '[Case Planning List Page] Delete Current Resource Staffing',
  DeleteResourceStaffingSuccess = '[Case Planning API] Delete Current Resource Staffing Success',
  DeleteResourceStaffingFail = '[Case Planning API] Delete Current Resource Staffing Fail',
  DeleteResourcesStaffing = '[Case Planning List Page] Delete Selected Resources Staffing',
  DeleteAllocationsCommitmentsStaffing = '[Case Planning List Page] Delete Selected Allocations Commitments Staffing',
  DeleteAllocationsCommitmentsStaffingSuccess = '[Case Planning API] Delete Current Allocations Commitments Staffing Success',
  UpsertResourceStaffableAsRole = '[Case Planning List Page] Upsert Resource Staffable As Role',
  UpsertResourceStaffableAsRoleSuccess = '[Case Planning List Page] Upsert Resource Staffable As Role Success',
  DeleteResourceStaffableAsRole = '[Case Planning List Page] Delete Resource Staffable As Role',
  DeleteResourceStaffableAsRoleSuccess = '[Case Planning List Page] Delete Resource Staffable As Role Success',
  UpsertCasePlanningViewNote = '[Case Planning List Page] Upsert Case Planning View Note',
  UpsertCasePlanningViewNoteSuccess = '[Case Planning List Page] Upsert Case Planning View Note Success',
  UpsertCasePlanningViewNoteFail = '[Case Planning List Page] Upsert Case Planning View Note Fail',
  DeleteCasePlanningViewNotes = '[Case Planning List Page] Delete Case Planning View Note',
  DeleteCasePlanningViewNotesSuccess = '[Case Planning List Page] Delete Case Planning View Note Success',
  DeleteCasePlanningViewNotesFail = '[Case Planning List Page] Delete Case Planning View Note Fail'
}

export class LoadProjects implements Action {
  readonly type = CasePlanningActionTypes.LoadProjects;

  constructor(public payload: any) { }
}

export class LoadProjectsSuccess implements Action {
  readonly type = CasePlanningActionTypes.LoadProjectsSuccess;

  constructor(public payload: Project[]) { }
}

export class LoadProjectsFail implements Action {
  readonly type = CasePlanningActionTypes.LoadProjectsFail;

  constructor(public payload: string) { }
}

export class LoadPlanningCards implements Action {
  readonly type = CasePlanningActionTypes.LoadPlanningCards;

  constructor(public payload: any) { }
}

export class LoadPlanningCardsSuccess implements Action {
  readonly type = CasePlanningActionTypes.LoadPlanningCardsSuccess;

  constructor(public payload: any) { }
}

export class LoadPlanningCardsFail implements Action {
  readonly type = CasePlanningActionTypes.LoadPlanningCardsFail;

  constructor(public payload: string) { }
}

export class CasePlanningLoader implements Action {
  readonly type = CasePlanningActionTypes.CasePlanningLoader;

  constructor(public payload: boolean) { }
}

export class LoadCasesOnPageScroll implements Action {
  readonly type = CasePlanningActionTypes.LoadCasesOnPageScroll;

  constructor(public payload: any) { }
}

export class LoadCasesOnPageScrollSuccess implements Action {
  readonly type = CasePlanningActionTypes.LoadCasesOnPageScrollSuccess;

  constructor(public payload: Project[]) { }
}

export class LoadProjectsBySearchString implements Action {
  readonly type = CasePlanningActionTypes.LoadProjectsBySearchString;

  constructor(public payload: any) { }
}

export class LoadProjectsBySearchStringSuccess implements Action {
  readonly type = CasePlanningActionTypes.LoadProjectsBySearchStringSuccess;

  constructor(public payload: Project[]) { }
}

export class LoadProjectsBySearchStringFail implements Action {
  readonly type = CasePlanningActionTypes.LoadProjectsBySearchStringFail;

  constructor(public payload: string) { }
}

export class ClearSearchData implements Action {
  readonly type = CasePlanningActionTypes.ClearSearchData;

  constructor(public payload: [] = []) { }
}

export class UpsertResourceStaffing implements Action {
  readonly type = CasePlanningActionTypes.UpsertResourceStaffing;

  constructor(public payload: ResourceAllocation[]) { }
}

export class UpsertResourceStaffingSuccess implements Action {
  readonly type = CasePlanningActionTypes.UpsertResourceStaffingSuccess;

  constructor(public payload: ResourceAllocation[]) { }
}

export class UpsertResourceStaffingFail implements Action {
  readonly type = CasePlanningActionTypes.UpsertResourceStaffingFail;

  constructor(public payload: ResourceAllocation[]) { }
}

export class RefreshCaseAndResourceOverlay implements Action {
  readonly type = CasePlanningActionTypes.RefreshCaseAndResourceOverlay;

  constructor(public payload: boolean) { }
}

export class AddResourceCommitment implements Action {
  readonly type = CasePlanningActionTypes.AddResourceCommitment;

  constructor(public payload: Commitment[]) { }
}

export class AddResourceCommitmentSuccess implements Action {
  readonly type = CasePlanningActionTypes.AddResourceCommitmentSuccess;

  constructor(public payload: Commitment[]) { }
}

export class UpdateResourceCommitment implements Action {
  readonly type = CasePlanningActionTypes.UpdateResourceCommitment;

  constructor(public payload: Commitment) { }
}

export class UpdateResourceCommitmentSuccess implements Action {
  readonly type = CasePlanningActionTypes.UpdateResourceCommitmentSuccess;

  constructor(public payload: Commitment[]) { }
}

export class DeleteResourceCommitment implements Action {
  readonly type = CasePlanningActionTypes.DeleteResourceCommitment;

  constructor(public payload: string) { }
}

export class DeleteResourceCommitmentSuccess implements Action {
  readonly type = CasePlanningActionTypes.DeleteResourceCommitmentSuccess;

  constructor(public payload: string) { }
}

export class DeleteResourceStaffing implements Action {
  readonly type = CasePlanningActionTypes.DeleteResourceStaffing;

  constructor(public payload: string) { }
}

export class DeleteResourceStaffingSuccess implements Action {
  readonly type = CasePlanningActionTypes.DeleteResourceStaffingSuccess;

  constructor(public payload: string) { }
}

export class DeleteResourceStaffingFail implements Action {
  readonly type = CasePlanningActionTypes.DeleteResourceStaffingFail;

  constructor(public payload: string) { }
}

export class DeleteResourcesStaffing implements Action {
  readonly type = CasePlanningActionTypes.DeleteResourcesStaffing;

  constructor(public payload: string) { }
}

export class DeleteAllocationsCommitmentsStaffing implements Action {
  readonly type = CasePlanningActionTypes.DeleteAllocationsCommitmentsStaffing;

  constructor(public payload: string) { }
}

export class DeleteAllocationsCommitmentsStaffingSuccess implements Action {
  readonly type = CasePlanningActionTypes.DeleteAllocationsCommitmentsStaffingSuccess;

  constructor(public payload: any) { }
}

export class UpsertResourceStaffableAsRole implements Action {
  readonly type = CasePlanningActionTypes.UpsertResourceStaffableAsRole;

  constructor(public payload: any) { }
}

export class UpsertResourceStaffableAsRoleSuccess implements Action {
  readonly type = CasePlanningActionTypes.UpsertResourceStaffableAsRoleSuccess;

  constructor(public payload: any) { }
}

export class DeleteResourceStaffableAsRole implements Action {
  readonly type = CasePlanningActionTypes.DeleteResourceStaffableAsRole;

  constructor(public payload: string) { }
}

export class DeleteResourceStaffableAsRoleSuccess implements Action {
  readonly type = CasePlanningActionTypes.DeleteResourceStaffableAsRoleSuccess;

  constructor(public payload: string) { }
}

export class UpsertCasePlanningViewNote implements Action {
  readonly type = CasePlanningActionTypes.UpsertCasePlanningViewNote;

  constructor(public payload: any) { }
}

export class UpsertCasePlanningViewNoteSuccess implements Action {
  readonly type = CasePlanningActionTypes.UpsertCasePlanningViewNoteSuccess;

  constructor(public payload: ResourceOrCasePlanningViewNote) { }
}

export class UpsertCasePlanningViewNoteFail implements Action {
  readonly type = CasePlanningActionTypes.UpsertCasePlanningViewNoteFail;

  constructor(public payload: any) { }
}

export class DeleteCasePlanningViewNotes implements Action {
  readonly type = CasePlanningActionTypes.DeleteCasePlanningViewNotes;

  constructor(public payload: any) { }
}

export class DeleteCasePlanningViewNotesSuccess implements Action {
  readonly type = CasePlanningActionTypes.DeleteCasePlanningViewNotesSuccess;

  constructor(public payload: string[]) { }
}

export class DeleteCasePlanningViewNotesFail implements Action {
  readonly type = CasePlanningActionTypes.DeleteCasePlanningViewNotesFail;

  constructor(public payload: any) { }
}

export type CasePlanningActions =
  LoadProjects
  | LoadProjectsSuccess
  | LoadProjectsFail
  | CasePlanningLoader
  | LoadPlanningCards
  | LoadPlanningCardsSuccess
  | LoadPlanningCardsFail
  | LoadCasesOnPageScroll
  | LoadCasesOnPageScrollSuccess
  | LoadProjectsBySearchString
  | LoadProjectsBySearchStringSuccess
  | LoadProjectsBySearchStringFail
  | ClearSearchData
  | UpsertResourceStaffing
  | UpsertResourceStaffingSuccess
  | UpsertResourceStaffingFail
  | RefreshCaseAndResourceOverlay
  | AddResourceCommitment
  | AddResourceCommitmentSuccess
  | UpdateResourceCommitment
  | DeleteResourceCommitment
  | DeleteResourceCommitmentSuccess
  | DeleteResourceStaffing
  | DeleteResourceStaffingSuccess
  | DeleteResourceStaffingFail
  | DeleteResourcesStaffing
  | DeleteAllocationsCommitmentsStaffing
  | DeleteAllocationsCommitmentsStaffingSuccess
  | UpsertResourceStaffableAsRole
  | UpsertResourceStaffableAsRoleSuccess
  | DeleteResourceStaffableAsRole
  | DeleteResourceStaffableAsRoleSuccess
  | UpsertCasePlanningViewNote
  | UpsertCasePlanningViewNoteSuccess
  | UpsertCasePlanningViewNoteFail
  | DeleteCasePlanningViewNotes
  | DeleteCasePlanningViewNotesSuccess
  | DeleteCasePlanningViewNotesFail
  ;
