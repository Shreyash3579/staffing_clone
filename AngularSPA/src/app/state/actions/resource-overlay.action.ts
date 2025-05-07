import { Action } from '@ngrx/store';

export enum ResourceOverlayActionTypes {
    UpsertResourceStaffableAs = '[Resource Overlay] Upsert Staffable As',  
    UpsertResourceStaffableAsSuccess = '[Resource Overlay] Upsert Staffable As Success',
    DeleteResourceStaffableAs = '[Resource Overlay] Delete Staffable As',
    DeleteResourceStaffableAsSuccess = '[Resource Overlay] Delete Staffable As Success',
}

export class UpsertResourceStaffableAs implements Action {
    readonly type = ResourceOverlayActionTypes.UpsertResourceStaffableAs;
    constructor(public payload: any) {}
}

export class UpsertResourceStaffableAsSuccess implements Action {
    readonly type = ResourceOverlayActionTypes.UpsertResourceStaffableAsSuccess;
    constructor(public payload: any) {}
}

export class DeleteResourceStaffableAs implements Action {
    readonly type = ResourceOverlayActionTypes.DeleteResourceStaffableAs;
    constructor(public payload: any) {}
}

export class DeleteResourceStaffableAsSuccess implements Action {
    readonly type = ResourceOverlayActionTypes.DeleteResourceStaffableAsSuccess;
    constructor(public payload: any) {}
}

export type ResourceOverlayActions = 
UpsertResourceStaffableAs
| UpsertResourceStaffableAsSuccess
| DeleteResourceStaffableAs
| DeleteResourceStaffableAsSuccess;
