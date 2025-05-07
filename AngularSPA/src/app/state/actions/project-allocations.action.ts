 import { Action } from "@ngrx/store";

export enum ProjectAllocationsActionTypes {
    UpsertResourceAllocations = '[Resource Allocations] Upsert Resource Allocations',
    UpsertResourceAllocationsSuccess = '[Resource Allocations] Upsert Resource Allocations Success',
    UpsertPlaceholderAllocations = '[Placeholder Allocations] Upsert Placeholder Allocations',
    UpsertPlaceholderAllocationsSuccess = '[Placeholder Allocations] Upsert Placeholder Allocations Success',
    UpsertPlanningCardAllocationsSuccess = '[Planning Card Allocations] Upsert Planning Card Allocations Success',
    DeleteResourceAllocationCommitment = '[Resource Allocations Commitment] Delete Resource Allocations/Commitments',
    DeleteResourceAllocationCommitmentSuccess = '[Resource Allocations Commitment] Delete Resource Allocations/Commitments Success',
    DeletePlaceholderAllocations = '[Placeholder Allocations] Delete Placeholder Allocations',
    DeletePlaceholderAllocationsSuccess = '[Placeholder Allocations] Delete Placeholder Allocations Success',
    ResetRefreshCasesAndOpportunitiesSuccess = '[Project Allocations] Reset Refresh Cases And Opportunities Success',
    LoadProjectsBySelectedFilters = '[Project Allocations] Projects Load By Selected Filters',
    LoadProjectsBySelectedFiltersSuccess = '[Project Allocations] Projects Load By Selected Filters Success',
    LoadHistoricalProjectsBySelectedFilters = '[Project Allocations] Historical Projects Load By Selected Filters',
    LoadHistoricalProjectsBySelectedFiltersSuccess = '[Project Allocations] Historical Projects Load By Selected Filters Success',
  }

  export class UpsertResourceAllocations implements Action {
    readonly type = ProjectAllocationsActionTypes.UpsertResourceAllocations;  
    constructor(public payload: any) { }
  }

  export class UpsertResourceAllocationsSuccess implements Action {
    readonly type = ProjectAllocationsActionTypes.UpsertResourceAllocationsSuccess;
    constructor(public payload: any) { }
  }

  export class UpsertPlaceholderAllocations implements Action {
    readonly type = ProjectAllocationsActionTypes.UpsertPlaceholderAllocations;
    constructor(public payload: any) { }
  }

  export class UpsertPlaceholderAllocationsSuccess implements Action {  
    readonly type = ProjectAllocationsActionTypes.UpsertPlaceholderAllocationsSuccess;
    constructor(public payload: any) { }
  }

  export class UpsertPlanningCardAllocationsSuccess implements Action {
    readonly type = ProjectAllocationsActionTypes.UpsertPlanningCardAllocationsSuccess;
    constructor(public payload: any) { }
  }

  export class DeletePlaceholderAllocations implements Action {
    readonly type = ProjectAllocationsActionTypes.DeletePlaceholderAllocations;
    constructor(public payload: any) { }
  }

  export class DeletePlaceholderAllocationsSuccess implements Action {
    readonly type = ProjectAllocationsActionTypes.DeletePlaceholderAllocationsSuccess;
    constructor(public payload: any) { }
  }

  export class DeleteResourceAllocationCommitment implements Action {
    readonly type = ProjectAllocationsActionTypes.DeleteResourceAllocationCommitment;
    constructor(public payload: any) { }
  }

  export class DeleteResourceAllocationCommitmentSuccess implements Action {
    readonly type = ProjectAllocationsActionTypes.DeleteResourceAllocationCommitmentSuccess;
    constructor(public payload: any) { }
  }

  export class ResetRefreshCasesAndOpportunitiesSuccess implements Action {
    readonly type = ProjectAllocationsActionTypes.ResetRefreshCasesAndOpportunitiesSuccess;
    constructor(public payload: any) { }
  }

  export class LoadProjectsBySelectedFilters implements Action {
    readonly type = ProjectAllocationsActionTypes.LoadProjectsBySelectedFilters;  
    constructor(public payload: any) { }
  }
  export class LoadProjectsBySelectedFiltersSuccess implements Action {
    readonly type = ProjectAllocationsActionTypes.LoadProjectsBySelectedFiltersSuccess;
    constructor(public payload: any) { }
  }

  export class LoadHistoricalProjectsBySelectedFilters implements Action {
    readonly type = ProjectAllocationsActionTypes.LoadHistoricalProjectsBySelectedFilters;  
    constructor(public payload: any) { }
  }

  export class LoadHistoricalProjectsIdBySelectedFiltersSuccess implements Action {
    readonly type = ProjectAllocationsActionTypes.LoadHistoricalProjectsBySelectedFiltersSuccess;
    constructor(public payload: any) { }
  }

  export type ProjectAllocationsActions =
  UpsertResourceAllocations
  | UpsertResourceAllocationsSuccess
  | UpsertPlaceholderAllocations
  | UpsertPlaceholderAllocationsSuccess
  | UpsertPlanningCardAllocationsSuccess
  | DeleteResourceAllocationCommitment
  | DeleteResourceAllocationCommitmentSuccess
  | DeletePlaceholderAllocations
  | DeletePlaceholderAllocationsSuccess
  | ResetRefreshCasesAndOpportunitiesSuccess
  | LoadProjectsBySelectedFilters
  | LoadProjectsBySelectedFiltersSuccess
  | LoadHistoricalProjectsBySelectedFilters
  | LoadHistoricalProjectsIdBySelectedFiltersSuccess;
