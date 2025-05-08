import { Action } from "@ngrx/store";
import { PlanningCard } from "src/app/shared/interfaces/planningCard.interface";
import { Project } from "src/app/shared/interfaces/project.interface";
import { PlaceholderAllocation } from "src/app/shared/interfaces/placeholderAllocation.interface";
import { ProjectViewModel } from "src/app/shared/interfaces/projectViewModel.interface";

export enum StaffingDemandActionTypes {
  ClearDemandState = '[Staffing Demand] Clear Demand State',
    LoadProjectsBySelectedFilters = '[Staffing Filter Projects] Projects Load By Selected Filters',
    LoadProjectsBySelectedFiltersSuccess = '[Staffing Filter Projects] Projects Load By Selected Filters Success',
    LoadHistoricalProjectsBySelectedFilters = '[Staffing Filter Historical Projects] Historical Projects Load By Selected Filters',
    LoadHistoricalProjectsBySelectedFiltersSuccess = '[Staffing Filter Historical Projects] Historical Projects Load By Selected Filters Success',
    LoadPlanningCardsBySelectedFilters = '[Staffing Filter Planning Cards] Planning Cards Load By Selected Filters',
    LoadPlanningCardsBySelectedFiltersSuccess = '[Staffing Filter Planning Cards] Planning Cards Load By Selected Filters Success',
    UpsertPlanningCard = '[Staffing Planning Cards] Planning Cards Upsert',
    UpsertPlanningCardSuccess = '[Staffing Planning Cards] Planning Cards Upsert Success',
    DeletePlanningCard = '[Staffing Planning Cards] Planning Cards Delete',
    DeletePlanningCardSuccess = '[Staffing Planning Cards] Planning Cards Delete Success',
    UpsertResourceAllocations = '[Staffing Resource Allocations] Upsert Resource Allocations',
    UpsertResourceAllocationsSuccess = '[Staffing Resource Allocations] Upsert Resource Allocations Success',
    DeleteResourceAllocations = '[Staffing Resource Allocations] Delete Resource Allocations',
    DeleteResourceAllocationsSuccess = '[Staffing Resource Allocations] Delete Resource Allocations Success',
    UpsertPlaceholderAllocations = '[Staffing Placeholder Allocations] Upsert Placeholder Allocations',
    UpsertPlaceholderAllocationsSuccess = '[Staffing Placeholder Allocations] Upsert Placeholder Allocations Success',
    UpsertPlanningCardAllocationsSuccess = '[Staffing Planning Card Allocations] Upsert Planning Card Allocations Success',
    DeletePlaceholderAllocations = '[Staffing Placeholder Allocations] Delete Placeholder Allocations',
    DeletePlaceholderAllocationsSuccess = '[Staffing Placeholder Allocations] Delete Placeholder Allocations Success',
    DeletePlanningCardPlaceholderAllocationSuccess = '[Staffing Planning Card Allocations] Delete Planning Card Placeholder Allocation Success',
    MergePlanningCards = '[Staffing Planning Cards] Merge Planning Cards',
    MergePlanningCardsSuccess = '[Staffing Planning Cards] Merge Planning Cards Success',
    UpsertCaseViewNotes = '[Staffing Demand Cards] Upsert CaseViewNotes',
    UpsertCaseViewNotesSuccess = '[Staffing Demand Cards] Upsert CaseViewNotes Success',
    DeleteCaseViewNotes = '[Staffing Demand Cards] Delete CaseViewNotes',
    DeleteCaseViewNotesSuccess = '[Staffing Demand Cards] Delete CaseViewNotes Sucsess',
    UpdatePegPlanningCardSuccess = '[Staffing Planning Cards] Update Peg Planning Card Success',
  }

  export class ClearDemandState implements Action {
    readonly type = StaffingDemandActionTypes.ClearDemandState;
  }

  export class LoadProjectsBySelectedFilters implements Action {
    readonly type = StaffingDemandActionTypes.LoadProjectsBySelectedFilters;  
    constructor(public payload: any) { }
  }

  export class LoadProjectsBySelectedFiltersSuccess implements Action {
    readonly type = StaffingDemandActionTypes.LoadProjectsBySelectedFiltersSuccess;
    constructor(public payload: ProjectViewModel) { }
  }

  export class LoadHistoricalProjectsBySelectedFilters implements Action {
    readonly type = StaffingDemandActionTypes.LoadHistoricalProjectsBySelectedFilters;  
    constructor(public payload: any) { }
  }

  export class LoadHistoricalProjectsBySelectedFiltersSuccess implements Action {
    readonly type = StaffingDemandActionTypes.LoadHistoricalProjectsBySelectedFiltersSuccess;
    constructor(public payload: ProjectViewModel) { }
  }

  export class LoadPlanningCardsBySelectedFilters implements Action {
    readonly type = StaffingDemandActionTypes.LoadPlanningCardsBySelectedFilters;  
    constructor(public payload: any) { }
  }

  export class LoadPlanningCardsBySelectedFiltersSuccess implements Action {
    readonly type = StaffingDemandActionTypes.LoadPlanningCardsBySelectedFiltersSuccess;
    constructor(public payload: PlanningCard[]) { }
  }

  export class UpsertPlaceholderAllocations implements Action {
    readonly type = StaffingDemandActionTypes.UpsertPlaceholderAllocations;  
    constructor(public payload: any) { }
  }

  export class UpsertPlaceholderAllocationsSuccess implements Action {
    readonly type = StaffingDemandActionTypes.UpsertPlaceholderAllocationsSuccess;
    constructor(public payload: any) { }
  }

  export class UpsertPlanningCardAllocationsSuccess implements Action {
    readonly type = StaffingDemandActionTypes.UpsertPlanningCardAllocationsSuccess;
    constructor(public payload: any) { }
  }

  export class DeletePlaceholderAllocations implements Action {
    readonly type = StaffingDemandActionTypes.DeletePlaceholderAllocations;  
    constructor(public payload: any) { }
  }

  export class DeletePlaceholderAllocationsSuccess implements Action {
    readonly type = StaffingDemandActionTypes.DeletePlaceholderAllocationsSuccess;
    constructor(public payload: any) { }
  }

  export class DeletePlanningCardPlaceholderAllocationSuccess implements Action {
    readonly type = StaffingDemandActionTypes.DeletePlanningCardPlaceholderAllocationSuccess;
    constructor(public payload: any) { }
  }

  export class UpsertResourceAllocations implements Action {
    readonly type = StaffingDemandActionTypes.UpsertResourceAllocations;  
    constructor(public payload: any) { }
  }

  export class UpsertResourceAllocationsSuccess implements Action {
    readonly type = StaffingDemandActionTypes.UpsertResourceAllocationsSuccess;
    constructor(public payload: any) { }
  }

  export class DeleteResourceAllocations implements Action {
    readonly type = StaffingDemandActionTypes.DeleteResourceAllocations;  
    constructor(public payload: any) { }
  }

  export class DeleteResourceAllocationsSuccess implements Action {
    readonly type = StaffingDemandActionTypes.DeleteResourceAllocationsSuccess;
    constructor(public payload: any) { }
  }

  export class MergePlanningCards implements Action {
    readonly type = StaffingDemandActionTypes.MergePlanningCards;  
    constructor(public payload: any) { }
  }

  export class MergePlanningCardsSuccess implements Action {
    readonly type = StaffingDemandActionTypes.MergePlanningCardsSuccess;
    constructor(public payload: any) { }
  }

  export class UpsertPlanningCard implements Action {
    readonly type = StaffingDemandActionTypes.UpsertPlanningCard;  
    constructor(public payload: any) { }
  }

  export class UpsertPlanningCardSuccess implements Action {
    readonly type = StaffingDemandActionTypes.UpsertPlanningCardSuccess;
    constructor(public payload: any) { }
  }

  export class DeletePlanningCard implements Action {
    readonly type = StaffingDemandActionTypes.DeletePlanningCard;  
    constructor(public payload: any) { }
  }

  export class DeletePlanningCardSuccess implements Action {
    readonly type = StaffingDemandActionTypes.DeletePlanningCardSuccess;
    constructor(public payload: any) { }
  }

  export class UpsertCaseViewNotes implements Action {
    readonly type = StaffingDemandActionTypes.UpsertCaseViewNotes;
    constructor(public payload: any) { }
  }

  export class UpsertCaseViewNotesSuccess implements Action {
    readonly type = StaffingDemandActionTypes.UpsertCaseViewNotesSuccess;
    constructor(public payload: any) { }
  }

  export class DeleteCaseViewNotes implements Action {
    readonly type = StaffingDemandActionTypes.DeleteCaseViewNotes;
    constructor(public payload: any) { }
  }

  export class DeleteCaseViewNotesSuccess implements Action {
    readonly type = StaffingDemandActionTypes.DeleteCaseViewNotesSuccess;
    constructor(public payload: any) { }
  }

  export class UpdatePegPlanningCardSuccess implements Action {
    readonly type = StaffingDemandActionTypes.UpdatePegPlanningCardSuccess;
    constructor(public payload: any) { }
  }

  export type StaffingDemandActions =
  ClearDemandState
  | LoadProjectsBySelectedFilters
  | LoadProjectsBySelectedFiltersSuccess
  | LoadHistoricalProjectsBySelectedFilters
  | LoadHistoricalProjectsBySelectedFiltersSuccess
  | LoadPlanningCardsBySelectedFilters
  | LoadPlanningCardsBySelectedFiltersSuccess
  | UpsertPlaceholderAllocations
  | UpsertPlaceholderAllocationsSuccess
  | UpsertPlanningCardAllocationsSuccess
  | DeletePlaceholderAllocations
  | DeletePlaceholderAllocationsSuccess
  | DeletePlanningCardPlaceholderAllocationSuccess
  | UpsertResourceAllocations
  | UpsertResourceAllocationsSuccess
  | DeleteResourceAllocations
  | DeleteResourceAllocationsSuccess
  | MergePlanningCards
  | MergePlanningCardsSuccess
  | DeletePlanningCard
  | DeletePlanningCardSuccess
  | UpsertCaseViewNotes
  | UpsertCaseViewNotesSuccess
  | DeleteCaseViewNotes
  | DeleteCaseViewNotesSuccess
  | UpsertPlanningCard
  | UpsertPlanningCardSuccess
  | UpdatePegPlanningCardSuccess;