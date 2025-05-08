import { Action } from '@ngrx/store';
import { Commitment } from 'src/app/shared/interfaces/commitment.interface';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { ResourceOrCasePlanningViewNote } from 'src/app/shared/interfaces/resource-or-case-planning-view-note.interface';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';

export enum CasePlanningActionTypes {
  LoadProjects = '[Case Planning Tab] Projects Load',
  LoadProjectsSuccess = '[Case Planning Tab] Projects Load Success',
  LoadProjectsFail = '[Case Planning Tab] Projects Load Fail',
  CasePlanningLoader = '[Case Planning Tab] Loader',
  LoadPlanningCards = '[Case Planning Tab] Planning Cards Load',
  LoadPlanningCardsSuccess = '[Case Planning Tab] Planning Cards Load Success',
  LoadPlanningCardsFail = '[Case Planning Tab] Planning Cards Load Fail',
  LoadAvailabilityMetrics = '[Case Planning Tab] Availability Metrics Load',
  LoadAvailabilityMetricsSuccess = '[Case Planning Tab] Availability Metrics Load Success',
  LoadAvailabilityMetricsFail = '[Case Planning Tab] Availability Metrics Load Fail',
  LoadMetricsDemandData = '[Case Planning Tab] Metrics Demand Data Load',
  LoadMetricsDemandDataSuccess = '[Case Planning Tab] Metrics Demand Data Load Success',
  LoadMetricsDemandDataFail = '[Case Planning Tab] Metrics Demand Data Load Fail',
  LoadProjectsBySearchString = '[Case Planning Tab] Projects Load Projects By Search String',
  LoadProjectsBySearchStringSuccess = '[Case Planning Tab] Projects Load Projects By Search String Success',
  LoadProjectsBySearchStringFail = '[Case Planning Tab] Projects Load Projects By Search String Fail',
  ClearSearchData = '[Case Planning Tab] Remove searched projects data',
  UpsertCasePlanningViewNote = '[Case Planning Tab] Upsert Case Planning View Note',
  UpsertCasePlanningViewNoteSuccess = '[Case Planning Tab] Upsert Case Planning View Note Success',
  UpsertCasePlanningViewNoteFail = '[Case Planning Tab] Upsert Case Planning View Note Fail',
  DeleteCasePlanningViewNotes = '[Case Planning Tab] Delete Case Planning View Note',
  DeleteCasePlanningViewNotesSuccess = '[Case Planning Tab] Delete Case Planning View Note Success',
  DeleteCasePlanningViewNotesFail = '[Case Planning Tab] Delete Case Planning View Note Fail',
  UpsertCasePlanningProjectDetails = '[Case Planning Tab] Upsert Case Planning Project Details',
  UpsertCasePlanningProjectDetailsSuccess = '[Case Planning Tab] Upsert Case Planning Project Details Success',
  UpsertCasePlanningProjectDetailsFail = '[Case Planning Tab] Upsert Case Planning Project Details Fail',
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

export class LoadAvailabilityMetrics implements Action {
  readonly type = CasePlanningActionTypes.LoadAvailabilityMetrics;

  constructor(public payload: any) { }
}

export class LoadAvailabilityMetricsSuccess implements Action {
  readonly type = CasePlanningActionTypes.LoadAvailabilityMetricsSuccess;

  constructor(public payload: any) { }
}

export class LoadAvailabilityMetricsFail implements Action {
  readonly type = CasePlanningActionTypes.LoadAvailabilityMetricsFail;

  constructor(public payload: string) { }
}

export class LoadMetricsDemandData implements Action {
  readonly type = CasePlanningActionTypes.LoadMetricsDemandData;

  constructor(public payload: any) { }
}

export class LoadMetricsDemandDataSuccess implements Action {
  readonly type = CasePlanningActionTypes.LoadMetricsDemandDataSuccess;

  constructor(public payload: any) { }
}

export class LoadMetricsDemandDataFail implements Action {
  readonly type = CasePlanningActionTypes.LoadMetricsDemandDataFail;

  constructor(public payload: string) { }
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

export class CasePlanningLoader implements Action {
  readonly type = CasePlanningActionTypes.CasePlanningLoader;

  constructor(public payload: boolean) { }
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

export class UpsertCasePlanningProjectDetails implements Action {
  readonly type = CasePlanningActionTypes.UpsertCasePlanningProjectDetails;
  constructor(public payload: any) { }
}

export class UpsertCasePlanningProjectDetailsSuccess implements Action {
  readonly type = CasePlanningActionTypes.UpsertCasePlanningProjectDetailsSuccess;
  constructor(public payload: any) { }
}

export class UpsertCasePlanningProjectDetailsFail implements Action {
  readonly type = CasePlanningActionTypes.UpsertCasePlanningProjectDetailsFail;
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
  | LoadAvailabilityMetrics
  | LoadAvailabilityMetricsSuccess
  | LoadAvailabilityMetricsFail
  | LoadMetricsDemandData
  | LoadMetricsDemandDataSuccess
  | LoadMetricsDemandDataFail
  | LoadProjectsBySearchString
  | LoadProjectsBySearchStringSuccess
  | LoadProjectsBySearchStringFail
  | ClearSearchData
  | UpsertCasePlanningViewNote
  | UpsertCasePlanningViewNoteSuccess
  | UpsertCasePlanningViewNoteFail
  | DeleteCasePlanningViewNotes
  | DeleteCasePlanningViewNotesSuccess
  | DeleteCasePlanningViewNotesFail
  | UpsertCasePlanningProjectDetails
  | UpsertCasePlanningProjectDetailsSuccess
  | UpsertCasePlanningProjectDetailsFail
  ;
