import { Action } from '@ngrx/store';

export enum ProjectOverlayActionTypes {
    UpdateOpportunity = '[Project Overlay] Update Opportunity',   
    UpdateCase = '[Project Overlay] Update Case',
    UpdateProjectSuccess = '[Project Overlay] Update Project Success',
    UpsertCaseRollAndAllocations = '[Project Overlay] Upsert Case Roll And Allocations',
    UpsertCaseRollAndAllocationsSuccess = '[Project Overlay] Upsert Case Roll And Allocations Success',
    UpsertCaseRollAndPlaceholderAllocations = '[Project Overlay] Upsert Case Roll And Placeholder Allocations',
    UpsertCaseRollAndPlaceholderAllocationsSuccess = '[Project Overlay] Upsert Case Roll And Placeholder Allocations Success',
    RevertCaseRollAndAllocations = '[Project Overlay] Revert Case Roll And Allocations',
    RevertCaseRollAndAllocationsSuccess = '[Project Overlay] Revert Case Roll And Allocations Success',
    InsertSKUCaseTerms = '[Project Overlay] Insert SKU Case Terms',
    InsertSKUCaseTermsSuccess = '[Project Overlay] Insert SKU Case Terms Success',
    DeleteSKUCaseTerms = '[Project Overlay] Delete SKU Case Terms',
    DeleteSKUCaseTermsSuccess = '[Project Overlay] Delete SKU Case Terms Success',
    UpdateSKUCaseTerms = '[Project Overlay] Update SKU Case Terms',
    UpdateSKUCaseTermsSuccess = '[Project Overlay] Update SKU Case Terms Success',
    GetSKUCaseTerms = '[Project Overlay] Get SKU Case Terms',
    GetSKUCaseTermsSuccess = '[Project Overlay] Get SKU Case Terms Success',
    UpdateUserPreferencesForPinOrHide = '[Project Overlay] Update User Preferences For Pin Or Hide',
    UpdateUserPreferencesForPinSuccess = '[Project Overlay] Update User Preferences For Pin Success',
    UpdateUserPreferencesForHideSuccess = '[Project Overlay] Update User Preferences For Hide Success'
}

export class UpdateOpportunity implements Action {
    readonly type = ProjectOverlayActionTypes.UpdateOpportunity;
    constructor(public payload: any) {}
}

export class UpdateCase implements Action {
    readonly type = ProjectOverlayActionTypes.UpdateCase;
    constructor(public payload: any) {}
}

export class UpdateProjectSuccess implements Action {
    readonly type = ProjectOverlayActionTypes.UpdateProjectSuccess;
    constructor(public payload: any) {}
}

export class UpsertCaseRollAndAllocations implements Action {
    readonly type = ProjectOverlayActionTypes.UpsertCaseRollAndAllocations;
    constructor(public payload: any) {}
}

export class UpsertCaseRollAndAllocationsSuccess implements Action {
    readonly type = ProjectOverlayActionTypes.UpsertCaseRollAndAllocationsSuccess;
    constructor(public payload: any) {}
}

export class UpsertCaseRollAndPlaceholderAllocations implements Action {
    readonly type = ProjectOverlayActionTypes.UpsertCaseRollAndPlaceholderAllocations;
    constructor(public payload: any) {}
}

export class UpsertCaseRollAndPlaceholderAllocationsSuccess implements Action {
    readonly type = ProjectOverlayActionTypes.UpsertCaseRollAndPlaceholderAllocationsSuccess;
    constructor(public payload: any) {}
}

export class RevertCaseRollAndAllocations implements Action {
    readonly type = ProjectOverlayActionTypes.RevertCaseRollAndAllocations;
    constructor(public payload: any) {}
}

export class RevertCaseRollAndAllocationsSuccess implements Action {
    readonly type = ProjectOverlayActionTypes.RevertCaseRollAndAllocationsSuccess;
    constructor(public payload: any) {}
}

export class InsertSKUCaseTerms implements Action {
    readonly type = ProjectOverlayActionTypes.InsertSKUCaseTerms;
    constructor(public payload: any) {}
}

export class InsertSKUCaseTermsSuccess implements Action {
    readonly type = ProjectOverlayActionTypes.InsertSKUCaseTermsSuccess;
    constructor(public payload: any) {}
}

export class DeleteSKUCaseTerms implements Action {
    readonly type = ProjectOverlayActionTypes.DeleteSKUCaseTerms;
    constructor(public payload: any) {}
}

export class DeleteSKUCaseTermsSuccess implements Action {
    readonly type = ProjectOverlayActionTypes.DeleteSKUCaseTermsSuccess;
    constructor(public payload: any) {}
}

export class UpdateSKUCaseTerms implements Action {
    readonly type = ProjectOverlayActionTypes.UpdateSKUCaseTerms;
    constructor(public payload: any) {}
}

export class UpdateSKUCaseTermsSuccess implements Action {
    readonly type = ProjectOverlayActionTypes.UpdateSKUCaseTermsSuccess;
    constructor(public payload: any) {}
}

export class GetSKUCaseTerms implements Action {
    readonly type = ProjectOverlayActionTypes.GetSKUCaseTerms;
    constructor(public payload: any) {}
}

export class GetSKUCaseTermsSuccess implements Action {
    readonly type = ProjectOverlayActionTypes.GetSKUCaseTermsSuccess;
    constructor(public payload: any) {}
}

export class UpdateUserPreferencesForPinOrHide implements Action {
    readonly type = ProjectOverlayActionTypes.UpdateUserPreferencesForPinOrHide;
    constructor(public payload: any) {}
}

export class UpdateUserPreferencesForPinSuccess implements Action {
    readonly type = ProjectOverlayActionTypes.UpdateUserPreferencesForPinSuccess;
    constructor(public payload: any) {}
}

export class UpdateUserPreferencesForHideSuccess implements Action {
    readonly type = ProjectOverlayActionTypes.UpdateUserPreferencesForHideSuccess;
    constructor(public payload: any) {}
}

export type ProjectOverlayActions = 
UpdateOpportunity 
| UpdateCase
| UpdateProjectSuccess
| UpsertCaseRollAndAllocations
| UpsertCaseRollAndAllocationsSuccess
| UpsertCaseRollAndPlaceholderAllocations
| UpsertCaseRollAndPlaceholderAllocationsSuccess
| RevertCaseRollAndAllocations
| RevertCaseRollAndAllocationsSuccess
| InsertSKUCaseTerms
| InsertSKUCaseTermsSuccess
| DeleteSKUCaseTerms
| DeleteSKUCaseTermsSuccess
| UpdateSKUCaseTerms
| UpdateSKUCaseTermsSuccess
| GetSKUCaseTerms
| GetSKUCaseTermsSuccess
| UpdateUserPreferencesForPinOrHide
| UpdateUserPreferencesForPinSuccess
| UpdateUserPreferencesForHideSuccess;
