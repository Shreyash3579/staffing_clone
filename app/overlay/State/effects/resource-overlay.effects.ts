import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import * as ResourceOverlayActions from "../actions/resource-overlay.actions";
import { map, mergeMap, switchMap } from "rxjs/operators";
import { OverlayService } from "src/app/overlay/overlay.service";
import { NotificationService } from "src/app/shared/notification.service";
import { EmployeeStaffingInfo } from "src/app/shared/interfaces/employeeStaffingInfo";

@Injectable()
export class ResourceOverlayEffects {
    constructor(private actions$: Actions,
        private overlayService:OverlayService, private notifyService: NotificationService) { }
   

    
    upsertResourceStaffingInfo$ = createEffect(() => this.actions$.pipe(
      ofType(ResourceOverlayActions.ResourceOverlayActionTypes.UpsertResourceStaffingInfo),
      map((action: ResourceOverlayActions.UpsertResourceStaffingInfo) => action.payload),
      switchMap((payload: EmployeeStaffingInfo[]) =>
      this.overlayService.upsertResourceStaffingResponsibleData(payload).pipe(
            mergeMap((resourceStaffingInfo: EmployeeStaffingInfo[]) => {
              this.notifyService.showSuccess('Staffing Responsible/ PD Lead / Notify Upon Staffing updated successfully');
              return (
                [
                  new ResourceOverlayActions.UpsertResourceStaffingInfoSucess(resourceStaffingInfo)
                ]
              );
            })))));

    
    getResourceStaffingInfo$ = createEffect(() => this.actions$.pipe(
      ofType(ResourceOverlayActions.ResourceOverlayActionTypes.GetResourceStaffingInfo),
      map((action: ResourceOverlayActions.GetResourceStaffingInfo) => action.payload),
      switchMap((payload: any) =>
      this.overlayService.getResourceStaffingResponsibleDataByEmployeeCode(payload).pipe(
            mergeMap((resourceStaffingInfo: EmployeeStaffingInfo) => {
              return (
                [
                  new ResourceOverlayActions.GetResourceStaffingInfoSuccess(resourceStaffingInfo)
                ]
              );
            })))));
}
