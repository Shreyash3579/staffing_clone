import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import * as StaffingSupplyActions from '../actions/staffing-supply.action';
import { map, mergeMap, switchMap } from "rxjs/operators";
import { HomeService } from "src/app/home-copy/home.service";
import { ResourceCommitment } from "src/app/shared/interfaces/resourceCommitment";
import { NotificationService } from "src/app/shared/notification.service";
import { SharedService } from "src/app/shared/shared.service";
import { ResourceOrCasePlanningViewNote } from "src/app/shared/interfaces/resource-or-case-planning-view-note.interface";
import { AzureSearchService } from "../../azureSearch.service";
import * as UserSpecificDetailsActions from "src/app/state/actions/user-specific-details.action";

@Injectable()
export class StaffingSupplyEffects {
    constructor(private actions$: Actions,
        private homeService: HomeService , 
        private notifyService: NotificationService,
        private sharedService: SharedService,
        private azureSearchService: AzureSearchService) { }
   
    loadResourcesBySelectedGroup$ = createEffect(() =>
      this.actions$.pipe(
      ofType(StaffingSupplyActions.StaffingSupplyActionTypes.LoadResourcesBySelectedGroup),
      map((action: StaffingSupplyActions.LoadResourcesBySelectedGroup) => action.payload),
      switchMap((payload: any) =>
        this.homeService.getResourcesFilteredBySelectedGroup(payload.supplyGroupFilterCriteriaObj).pipe(
            mergeMap((resources: ResourceCommitment) => {
              return (
                [
                  new StaffingSupplyActions.LoadResourcesBySelectedGroupSuccess(resources)
                ]
              );               
            })
          )
        )
      )
    );

    loadResourcesBySelectedFilters$ = createEffect(() =>
      this.actions$.pipe(
      ofType(StaffingSupplyActions.StaffingSupplyActionTypes.LoadResourcesBySelectedFilters),
      map((action: StaffingSupplyActions.LoadResourcesBySelectedFilters) => action.payload),
      switchMap((payload: any) =>
        this.homeService.getResourcesFilteredBySelectedValues(payload.supplyFilterCriteriaObj).pipe(
            mergeMap((resources: ResourceCommitment) => {
              return (
                [
                  new StaffingSupplyActions.LoadResourcesBySelectedFiltersSuccess(resources)
                ]
              );
            })
          )
        )
      )
    );
            
    upsertResourceViewNotes$ = createEffect(() =>
    this.actions$.pipe(
      ofType(StaffingSupplyActions.StaffingSupplyActionTypes.UpsertResourceViewNotes),
      map((action: StaffingSupplyActions.UpsertResourceViewNotes) => action.payload),
      switchMap((payload: any) =>
      this.sharedService.upsertResourceViewNote(payload).pipe(
            mergeMap((resourceViewNote: ResourceOrCasePlanningViewNote) => {
              this.notifyService.showSuccess('Note Added Successfully');
              return (
                [
                  new StaffingSupplyActions.UpsertResourceViewNotesSuccess(resourceViewNote),
                  new UserSpecificDetailsActions.GetMostRecentSharedWithEmployeeGroups()
                ]
              );
            })
          )
        )
      )
    );

    deleteResourceViewNotes$ = createEffect(() =>
      this.actions$.pipe(
          ofType(StaffingSupplyActions.StaffingSupplyActionTypes.DeleteResourceViewNotes),
          map((action: StaffingSupplyActions.DeleteResourceViewNotes) => action.payload),
          switchMap((payload: any) =>
              this.sharedService.deleteResourceViewNotes(payload).pipe(
                  mergeMap((idsToDelete: string[]) => {
                      this.notifyService.showSuccess('Note Deleted Successfully');
                      return [
                        new StaffingSupplyActions.DeleteResourceViewNotesSuccess(idsToDelete),
                        new UserSpecificDetailsActions.GetMostRecentSharedWithEmployeeGroups()
                      ];
                  })
              )
            )
        )
    );

    getResourcesBySearchString$ = createEffect(() =>
      this.actions$.pipe(
          ofType(StaffingSupplyActions.StaffingSupplyActionTypes.GetResourcesBySearchString),
          map((action: StaffingSupplyActions.GetResourcesBySearchString) => action.payload),
          switchMap((payload: any) =>
              this.azureSearchService.gesourcesBySearchStringWithinSupply(payload).pipe(
                  mergeMap((searchResultsData: any) => {
                      return [new StaffingSupplyActions.GetResourcesBySearchStringSuccess(searchResultsData)];
                  })
              )
            )
        )
    );
}
