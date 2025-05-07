import { Action } from "@ngrx/store";
import { EmployeeStaffingInfo } from "src/app/shared/interfaces/employeeStaffingInfo";

export enum ResourceOverlayActionTypes {
    ClearResourceOverlayState = '[Resource Overlay] Clear Resource Overlay State',
    UpsertResourceStaffingInfo = '[Resource Staffing Info] Upsert Resource Staffing Info',
    UpsertResourceStaffingInfoSucess = '[Resource Staffing Info] Upsert Resource Staffing Info Success',
    GetResourceStaffingInfo = '[Resource Staffing Info] Get Resource Staffing Info',
    GetResourceStaffingInfoSuccess = '[Resource Staffing Info] Get Resource Staffing Info Success'
  }

  export class ClearResourceOverlayState implements Action {
    readonly type = ResourceOverlayActionTypes.ClearResourceOverlayState;
  }

  export class UpsertResourceStaffingInfo implements Action {
    readonly type = ResourceOverlayActionTypes.UpsertResourceStaffingInfo;
    constructor(public payload: EmployeeStaffingInfo[]) { }
  }

  export class UpsertResourceStaffingInfoSucess implements Action {
    readonly type = ResourceOverlayActionTypes.UpsertResourceStaffingInfoSucess;
    constructor(public payload: EmployeeStaffingInfo[]) { }
  }

  export class GetResourceStaffingInfo implements Action {
    readonly type = ResourceOverlayActionTypes.GetResourceStaffingInfo;
    constructor(public payload: any) { }
  }

  export class GetResourceStaffingInfoSuccess implements Action {
    readonly type = ResourceOverlayActionTypes.GetResourceStaffingInfoSuccess;
    constructor(public payload: EmployeeStaffingInfo) { }
  }

  export type ResourceOverlayActions =
  ClearResourceOverlayState
  | UpsertResourceStaffingInfo
  | UpsertResourceStaffingInfoSucess
  | GetResourceStaffingInfo
  | GetResourceStaffingInfoSuccess
