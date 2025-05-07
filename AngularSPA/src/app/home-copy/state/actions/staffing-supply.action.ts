import { Action } from "@ngrx/store";
import { BossSearchCriteria } from "src/app/shared/interfaces/azureSearchCriteria.interface";
import { ResourceCommitment } from "src/app/shared/interfaces/resourceCommitment";

export enum StaffingSupplyActionTypes {
    ClearSupplyState = '[Staffing Supply] Clear Supply State',
    LoadResourcesBySelectedGroup = '[Staffing Group Resources] Resources Load By Selected Group',
    LoadResourcesBySelectedGroupSuccess = '[Staffing Group Resources] Resources Load By Selected Group Success',
    LoadResourcesBySelectedFilters = '[Staffing Filter Resources] Resources Load By Selected Filters',
    LoadResourcesBySelectedFiltersSuccess = '[Staffing Filter Resources] Resources Load By Selected Filters Success',
    UpsertResourceViewNotes = '[Resource View Notes] Upsert Resource View Notes',
    UpsertResourceViewNotesSuccess = '[Resource View Notes] Upsert Resource View Notes Success',
    DeleteResourceViewNotes = '[Resource View Notes] Delete Resource View Notes',
    DeleteResourceViewNotesSuccess = '[Resource View Notes] Delete Resource View Notes Success',
    GetResourcesBySearchString = '[Staffing Supply] Get Resources By Search String',
    GetResourcesBySearchStringSuccess = '[Staffing Supply] Get Resources By Search String Succes',
    ClearSearchString = '[Staffing Supply] Clear Search String',
  }

  export class ClearSupplyState implements Action {
    readonly type = StaffingSupplyActionTypes.ClearSupplyState;
  }

  export class LoadResourcesBySelectedGroup implements Action {
    readonly type = StaffingSupplyActionTypes.LoadResourcesBySelectedGroup;  
    constructor(public payload: any) { }
  }

  export class LoadResourcesBySelectedGroupSuccess implements Action {
    readonly type = StaffingSupplyActionTypes.LoadResourcesBySelectedGroupSuccess;
    constructor(public payload: ResourceCommitment) { }
  }

  export class LoadResourcesBySelectedFilters implements Action {
    readonly type = StaffingSupplyActionTypes.LoadResourcesBySelectedFilters;  
    constructor(public payload: any) { }
  }

  export class LoadResourcesBySelectedFiltersSuccess implements Action {
    readonly type = StaffingSupplyActionTypes.LoadResourcesBySelectedFiltersSuccess;
    constructor(public payload: ResourceCommitment) { }
  }

  export class UpsertResourceViewNotes implements Action {
    readonly type = StaffingSupplyActionTypes.UpsertResourceViewNotes;
    constructor(public payload: any) { }
  }

  export class UpsertResourceViewNotesSuccess implements Action {
    readonly type = StaffingSupplyActionTypes.UpsertResourceViewNotesSuccess;
    constructor(public payload: any) { }
  }

  export class DeleteResourceViewNotes implements Action {
    readonly type = StaffingSupplyActionTypes.DeleteResourceViewNotes;
    constructor(public payload: any) { }
  }

  export class DeleteResourceViewNotesSuccess implements Action {
    readonly type = StaffingSupplyActionTypes.DeleteResourceViewNotesSuccess;
    constructor(public payload: any) { }
  }

  export class GetResourcesBySearchString implements Action {
    readonly type = StaffingSupplyActionTypes.GetResourcesBySearchString;
    constructor(public payload: BossSearchCriteria) { }
  }

  export class GetResourcesBySearchStringSuccess implements Action {
    readonly type = StaffingSupplyActionTypes.GetResourcesBySearchStringSuccess;
    constructor(public payload: any) { }
  }

  export class ClearSearchString implements Action {
    readonly type = StaffingSupplyActionTypes.ClearSearchString;
    constructor(public payload: boolean) { }
  }
  
  export type StaffingSupplyActions =
  LoadResourcesBySelectedGroup
  | LoadResourcesBySelectedGroupSuccess
  | LoadResourcesBySelectedFilters
  | LoadResourcesBySelectedFiltersSuccess
  | UpsertResourceViewNotes
  | UpsertResourceViewNotesSuccess
  | DeleteResourceViewNotes
  | DeleteResourceViewNotesSuccess
  | GetResourcesBySearchString
  | GetResourcesBySearchStringSuccess
  | ClearSearchString
  
