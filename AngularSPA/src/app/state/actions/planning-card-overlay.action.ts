import { Action } from "@ngrx/store";
import { PlanningCard } from "src/app/shared/interfaces/planningCard.interface";

export enum PlanningCardOverlayActionTypes {
    UpsertPlanningCard = '[Planning Card Overlay] Planning Cards Upsert',
    UpsertPlanningCardSuccess = '[Planning Card Overlay] Planning Cards Upsert Success',
    RefreshPlanningCardOverlaySuccess = '[Planning Card Overlay] Refresh Planning Card Overlay Success',
    DeletePlanningCard = '[Planning Card Overlay] Planning Cards Delete',
    DeletePlanningCardSuccess = '[Planning Card Overlay] Planning Cards Delete Success',
    MergePlanningCards = '[Planning Card Overlay] Merge Planning Cards',
    MergePlanningCardsSuccess = '[Planning Card Overlay] Merge Planning Cards Success',
    LoadPlanningCardsBySelectedFilters = '[Global Staffing Filter Planning Cards] Planning Cards Load By Selected Filters',
    LoadPlanningCardsBySelectedFiltersSuccess = '[Global Staffing Filter Planning Cards] Planning Cards Load By Selected Filters Success',

  }

  export class UpsertPlanningCard implements Action {
    readonly type = PlanningCardOverlayActionTypes.UpsertPlanningCard;  
    constructor(public payload: any) { }
  }

  export class UpsertPlanningCardSuccess implements Action {
    readonly type = PlanningCardOverlayActionTypes.UpsertPlanningCardSuccess;
    constructor(public payload: any) { }
  }

  export class RefreshPlanningCardOverlaySuccess implements Action {
    readonly type = PlanningCardOverlayActionTypes.RefreshPlanningCardOverlaySuccess;
    constructor(public payload: any) { }
  }

  export class DeletePlanningCard implements Action {
    readonly type = PlanningCardOverlayActionTypes.DeletePlanningCard;
    constructor(public payload: any) { }
  }

  export class DeletePlanningCardSuccess implements Action {
    readonly type = PlanningCardOverlayActionTypes.DeletePlanningCardSuccess;
    constructor(public payload: any) { }
  }

  export class MergePlanningCards implements Action {
    readonly type = PlanningCardOverlayActionTypes.MergePlanningCards;
    constructor(public payload: any) { }
  }

  export class MergePlanningCardsSuccess implements Action {
    readonly type = PlanningCardOverlayActionTypes.MergePlanningCardsSuccess;
    constructor(public payload: any) { }
  }

  export class LoadPlanningCardsBySelectedFilters implements Action {
    readonly type = PlanningCardOverlayActionTypes.LoadPlanningCardsBySelectedFilters;  
    constructor(public payload: any) { }
  }

  export class LoadPlanningCardsBySelectedFiltersSuccess implements Action {
    readonly type = PlanningCardOverlayActionTypes.LoadPlanningCardsBySelectedFiltersSuccess;
    constructor(public payload: PlanningCard[]) { }
  }


  export type PlanningCardOverlayActions =
  UpsertPlanningCard
  | UpsertPlanningCardSuccess
  | RefreshPlanningCardOverlaySuccess
  | DeletePlanningCard
  | DeletePlanningCardSuccess
  | MergePlanningCards
  | MergePlanningCardsSuccess
  | LoadPlanningCardsBySelectedFilters
  | LoadPlanningCardsBySelectedFiltersSuccess

