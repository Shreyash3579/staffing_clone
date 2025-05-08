import { Action } from "@ngrx/store";

export enum ResourceCommitmentActionTypes {
    InsertResourceCommitment = '[Insert Resource Commitment] Insert Resource Commitment',
    InsertResourceCommitmentSuccess = '[Insert Resource Commitment] Insert Resource Commitment Success',
    ResetRefreshResourcesSuccess = '[Resource Commitment] Reset Refresh Resources Success',
    InsertCaseOppCommitments = '[Resource Commitment] Insert Case Opp Resource Commitments',
    InsertCaseOppCommitmentsSuccess = '[Resource Commitment] Insert Case Opp Resource Commitments Success',
    DeleteCaseOppCommitments = '[Resource Commitment] Delete Case Opp Commitments',
    DeleteCaseOppCommitmentsSuccess = '[Resource Commitment] Delete Case Opp Commitments success'
  }

  export class InsertResourceCommitment implements Action {
    readonly type = ResourceCommitmentActionTypes.InsertResourceCommitment;
    constructor(public payload: any) { }
  }

  export class InsertResourceCommitmentSuccesss implements Action {
    readonly type = ResourceCommitmentActionTypes.InsertResourceCommitmentSuccess;
    constructor(public payload: any) { }
  }

  export class DeleteCaseOppCommitments implements Action {
    readonly type = ResourceCommitmentActionTypes.DeleteCaseOppCommitments;
    constructor(public payload: any) { }
  }

  export class DeleteCaseOppCommitmentsSuccess implements Action {
    readonly type = ResourceCommitmentActionTypes.DeleteCaseOppCommitmentsSuccess;
    constructor(public payload: any) { }
  }
  export class InsertCaseOppCommitments implements Action {
    readonly type = ResourceCommitmentActionTypes.InsertCaseOppCommitments;
    constructor(public payload: any) { }
  }

  export class InsertCaseOppCommitmentsSuccess implements Action {
    readonly type = ResourceCommitmentActionTypes.InsertCaseOppCommitmentsSuccess;
    constructor(public payload: any) { }
  }




  export class ResetRefreshResourcesSuccess implements Action {
    readonly type = ResourceCommitmentActionTypes.ResetRefreshResourcesSuccess;
    constructor(public payload: any) { }
  }

  export type ResourceCommitmentActions =
  InsertResourceCommitment
  | InsertResourceCommitmentSuccesss
  | ResetRefreshResourcesSuccess
  | DeleteCaseOppCommitments
  | DeleteCaseOppCommitmentsSuccess
  | InsertCaseOppCommitments
  | InsertCaseOppCommitmentsSuccess;
