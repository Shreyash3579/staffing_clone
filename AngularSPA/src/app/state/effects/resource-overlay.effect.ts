import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { map, mergeMap, switchMap } from 'rxjs/operators';
import * as ResourceOverlayActions from '../actions/resource-overlay.action';
import { OverlayService } from 'src/app/overlay/overlay.service';
import { NotificationService } from 'src/app/shared/notification.service';

@Injectable()
export class ResourceOverlayEffects {
    constructor(
        private actions$: Actions,
        private overlayService: OverlayService,
        private notifyService: NotificationService) {}

    
    upsertStaffableAs$ = createEffect(() => this.actions$.pipe(
        ofType(ResourceOverlayActions.ResourceOverlayActionTypes.UpsertResourceStaffableAs),
            map((action: ResourceOverlayActions.UpsertResourceStaffableAs) => action.payload),
            switchMap((payload: any) => 
                this.overlayService.upsertResourceStaffableAs(payload.staffableRoles).pipe(
                    mergeMap((updatedData: any) => {
                        payload.updatedData = updatedData;
                        this.notifyService.showSuccess('Staffable As role updated', 'Success');
                    return (
                    [
                      new ResourceOverlayActions.UpsertResourceStaffableAsSuccess(payload)
                    ]
                );
              })))
            ))

        
        deleteStaffableAs$ = createEffect(() => this.actions$.pipe(
            ofType(ResourceOverlayActions.ResourceOverlayActionTypes.DeleteResourceStaffableAs),
                map((action: ResourceOverlayActions.DeleteResourceStaffableAs) => action.payload),
                switchMap((payload: any) => 
                    this.overlayService.deleteResourceStaffableAsById(payload.staffableRoleToBeDeleted).pipe(
                        mergeMap((updatedData: any) => {
                            payload.updatedData = updatedData;
                            this.notifyService.showSuccess('Staffable As role deleted', 'Success');
                        return (
                        [
                          new ResourceOverlayActions.DeleteResourceStaffableAsSuccess(payload)
                        ]
                    );
                  })))
                ))
}
