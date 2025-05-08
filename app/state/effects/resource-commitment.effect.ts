import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import * as ResourceCommitmentActions from '../actions/resource-commitment.action';
import { map, mergeMap, switchMap } from "rxjs/operators";
import { Commitment } from "src/app/shared/interfaces/commitment.interface";
import { OverlayService } from "src/app/overlay/overlay.service";
import { NotificationService } from "src/app/shared/notification.service";
import { CoreService } from "src/app/core/core.service";
import { CommitmentWithCaseOppInfo } from "src/app/shared/interfaces/commitmentView";

@Injectable()
export class ResourceCommitmentEffects {
    constructor(private actions$: Actions,
         private overlayService:OverlayService, 
         private notifyService: NotificationService,        
         private coreService: CoreService) { }

    
    insertResourcesCommitments$ = createEffect(() => this.actions$.pipe(
      ofType(ResourceCommitmentActions.ResourceCommitmentActionTypes.InsertResourceCommitment),
      map((action: ResourceCommitmentActions.InsertResourceCommitment) => action.payload),
      switchMap((payload: any) =>
      this.overlayService.insertResourcesCommitments(payload.commitments).pipe(
            mergeMap((commitments: Commitment[]) => {
              payload.commitments = commitments;
              this.notifyService.showSuccess('Commitment Added Successfully');
              payload.commitments = this.coreService.filterUpdatedDataByStaffingSettings(payload.commitments);
              return (
                [
                  new ResourceCommitmentActions.InsertResourceCommitmentSuccesss(payload)
                ]
              );
            })))
      )); 
      

      insertCaseOppCommitments$ = createEffect(() => this.actions$.pipe(
        ofType(ResourceCommitmentActions.ResourceCommitmentActionTypes.InsertCaseOppCommitments),
        map((action: ResourceCommitmentActions.InsertCaseOppCommitments) => action.payload),
        switchMap((payload: any) =>
        this.overlayService.insertCaseOppCommitments(payload.commitments).pipe(
              mergeMap((commitments: CommitmentWithCaseOppInfo[]) => {
                const updatedPayload = {
                  ...payload,
                  commitments: commitments
                };
                this.notifyService.showSuccess('Commitment Added Successfully');
                return (
                  [
                    new ResourceCommitmentActions.InsertResourceCommitmentSuccesss(updatedPayload),
                    new ResourceCommitmentActions.InsertCaseOppCommitmentsSuccess(updatedPayload)
                  ]
                );
              })))
        ));
      
      deleteCaseOppCommitments$ = createEffect(() => this.actions$.pipe(
          ofType(ResourceCommitmentActions.ResourceCommitmentActionTypes.DeleteCaseOppCommitments),
          map((action: ResourceCommitmentActions.DeleteCaseOppCommitments) => action.payload),
          mergeMap((payload: any) =>
            this.overlayService.deleteCaseOppCommitments(payload.commitmentIds).pipe(
                mergeMap(() => {
                  if(payload.notifyMessage) {
                    this.notifyService.showSuccess(payload.notifyMessage);
                  } 
                  else{
                    this.notifyService.showSuccess('Assignment Deleted');
                  }
                  return (
                    [
                      new ResourceCommitmentActions.DeleteCaseOppCommitmentsSuccess(payload)
                    ]
                  );
                })))
        ));

}
