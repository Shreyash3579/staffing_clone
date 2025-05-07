import { Action } from '@ngrx/store';

export enum UserSpecificDetailsActionTypes {
    GetMostRecentSharedWithEmployeeGroups = '[User Specific Details] Get Most Recent Shared With Employee Groups',
    GetMostRecentSharedWithEmployeeGroupsSuccess = '[User Specific Details] Get Most Recent Shared With Employee Groups Success',
    GetMostRecentSharedWithEmployeeGroupsFailure = '[User Specific Details] Get Most Recent Shared With Employee Groups Failure',
}

export class GetMostRecentSharedWithEmployeeGroups implements Action {
    readonly type = UserSpecificDetailsActionTypes.GetMostRecentSharedWithEmployeeGroups;
    constructor() {}
}

export class GetMostRecentSharedWithEmployeeGroupsSuccess implements Action {
    readonly type = UserSpecificDetailsActionTypes.GetMostRecentSharedWithEmployeeGroupsSuccess;
    constructor(public payload: any) {}
}

export type UserSpecificDetailsActions =
    | GetMostRecentSharedWithEmployeeGroups
    | GetMostRecentSharedWithEmployeeGroupsSuccess
