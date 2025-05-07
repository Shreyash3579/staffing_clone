import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import * as ProjectAllocationsActions from "../actions/project-allocations.action";
import { map, mergeMap, switchMap } from "rxjs/operators";
import { OverlayService } from "src/app/overlay/overlay.service";
import { NotificationService } from "src/app/shared/notification.service";
import { SharedService } from "src/app/shared/shared.service";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { ResourceAssignmentService } from "src/app/overlay/behavioralSubjectService/resourceAssignment.service";
import { OverlayMessageService } from "src/app/overlay/behavioralSubjectService/overlayMessage.service";
import { HomeService } from "src/app/home/home.service";
import { CoreService } from "src/app/core/core.service";
import { ProjectViewModel } from "src/app/shared/interfaces/projectViewModel.interface";
import * as StaffingDemandActions from 'src/app/home-copy/state/actions/staffing-demand.action';

@Injectable()
export class ProjectAllocationsEffects {
    constructor(private actions$: Actions,
        private notifyService: NotificationService,
        private sharedService: SharedService,
        private resourceAssignmentService: ResourceAssignmentService,
        private overlayService: OverlayService,
        private homeService: HomeService,
        private coreService: CoreService) {}
    
    
    upsertResourceAllocations$ = createEffect(() => this.actions$.pipe(
      ofType(ProjectAllocationsActions.ProjectAllocationsActionTypes.UpsertResourceAllocations),
      map((action: ProjectAllocationsActions.UpsertResourceAllocations) => action.payload),
      switchMap((payload: any) =>
      this.overlayService.upsertResourceAllocations(payload.resourceAllocation, null).pipe(
        mergeMap((updatedData: any) => {
          // made different properties for updatedData as we want to add past/future allocations in projects 
          // but it should not impact availability
          payload.demandUpdatedData = updatedData;         
          payload.supplyUpdatedData = this.coreService.filterUpdatedDataByStaffingSettings(updatedData);

          if (payload.splitSuccessMessage) {
            this.notifyService.showStickySuccess(payload.splitSuccessMessage);
          } else if (payload.showMoreThanYearWarning) {
            this.notifyService.showWarning('Assignment Saved for one month.');
          } else {
            this.notifyService.showSuccess('Assignment Saved');
          }

          if (payload.allocationDataBeforeSplitting) {
            let allocationData = payload.allocationDataBeforeSplitting;
            if (allocationData.every((r)=> r.oldCaseCode)) {
                this.sharedService.checkPegRingfenceAllocationAndInsertDownDayCommitments(allocationData).subscribe(commitments => {
                    if(commitments?.length > 0) {
                        this.notifyService.showSuccess(ConstantsMaster.Messages.DownDaySaved);
                    }
                });
            }}
            this.resourceAssignmentService.upsertPlaygroundAllocationsForCasePlanningMetrics(payload.resourceAllocation); 
          return (
          [
            new ProjectAllocationsActions.UpsertResourceAllocationsSuccess(payload)
          ]
      );        
      })))
    ))


    
    loadHistoricalProjectsBySelectedFilters$ = createEffect(() => this.actions$.pipe(
      ofType(ProjectAllocationsActions.ProjectAllocationsActionTypes.LoadHistoricalProjectsBySelectedFilters),
      map((action: ProjectAllocationsActions.LoadHistoricalProjectsBySelectedFilters) => action.payload),
      switchMap((payload: any) =>
        this.sharedService.getOngoingCasesBySelectedValues(payload.demandFilterCriteriaObj).pipe(
            mergeMap((projects: ProjectViewModel) => {
              return (
                [

                  new ProjectAllocationsActions.LoadHistoricalProjectsIdBySelectedFiltersSuccess(projects),
                  new StaffingDemandActions.LoadHistoricalProjectsBySelectedFiltersSuccess(projects)
                ]
              );
            })))));

      loadProjectsBySelectedFilters$ = createEffect(() => this.actions$.pipe(
        ofType(ProjectAllocationsActions.ProjectAllocationsActionTypes.LoadProjectsBySelectedFilters),
        map((action: ProjectAllocationsActions.LoadProjectsBySelectedFilters) => action.payload),
        switchMap((payload: any) =>
          this.sharedService.getProjectsFilteredBySelectedValues(payload.demandFilterCriteriaObj).pipe(
              mergeMap((projects: ProjectViewModel) => {
                return (
                  [
                    new ProjectAllocationsActions.LoadProjectsBySelectedFiltersSuccess(projects),
                    new StaffingDemandActions.LoadProjectsBySelectedFiltersSuccess(projects)
                  ]
                );
              })))));

    
    upsertPlaceholderAllocations$ = createEffect(() => this.actions$.pipe(
      ofType(ProjectAllocationsActions.ProjectAllocationsActionTypes.UpsertPlaceholderAllocations),
      map((action: ProjectAllocationsActions.UpsertPlaceholderAllocations) => action.payload),
      switchMap((payload: any) =>
        this.homeService.upsertPlaceholderAllocations(payload.placeholderAllocations).pipe(
            mergeMap((updatedData: any) => {
              payload.updatedData = updatedData
              // made different properties for updatedData as we want to add past/future allocations in projects 
              // but it should not impact availability
              payload.demandUpdatedData = updatedData;
              payload.supplyUpdatedData = this.coreService.filterUpdatedDataByStaffingSettings(updatedData);

          if(payload?.isMergeFromPlanningCard && payload?.isCopyAndMerge) {
                this.notifyService.showSuccess('Planning Card has been copied and merged');
              } else if (updatedData[0].employeeCode && updatedData[0].id) {
                this.notifyService.showSuccess(`Placeholder assignment for ${updatedData[0].employeeName} is created/updated`);
                payload.placeholderAllocations.id = updatedData[0].id;
              } else if (!updatedData[0].employeeCode && updatedData[0].id && updatedData[0].operatingOfficeCode) {
                this.notifyService.showSuccess(`Guessed Placeholder assignment is created/updated`);
                payload.placeholderAllocations.id = updatedData[0].id;
              } else if (updatedData[0].employeeCode === null && payload.placeholderAllocations.id !== null && payload.placeholderAllocations.id !== undefined) {
                this.notifyService.showSuccess(`Placeholder assignment is removed`);
              } else if (payload.placeholderAllocations.id === null || payload.placeholderAllocations.id === undefined) {
                this.notifyService.showSuccess('Placeholder Created');
              }  
              return (
              [
                  updatedData[0].planningCardId ? new ProjectAllocationsActions.UpsertPlanningCardAllocationsSuccess(payload) :
                  new ProjectAllocationsActions.UpsertPlaceholderAllocationsSuccess(payload)
              ]
              );
            })))
    ))

    
    deleteResourceAllocationCommitment$ = createEffect(() => this.actions$.pipe(
      ofType(ProjectAllocationsActions.ProjectAllocationsActionTypes.DeleteResourceAllocationCommitment),
      map((action: ProjectAllocationsActions.DeleteResourceAllocationCommitment) => action.payload),
      switchMap((payload: any) =>
      this.overlayService.deleteResourcesAllocationsCommitments(payload).pipe(
        mergeMap((deletedData: any) => {
          this.notifyService.showSuccess('Assignment Deleted');
          return (
          [
            new ProjectAllocationsActions.DeleteResourceAllocationCommitmentSuccess(payload)
          ]
        );
      })))
    ))

    
    deletePlaceholderAllocations$ = createEffect(() => this.actions$.pipe(
      ofType(ProjectAllocationsActions.ProjectAllocationsActionTypes.DeletePlaceholderAllocations),
      map((action: ProjectAllocationsActions.DeletePlaceholderAllocations) => action.payload),
      mergeMap((payload: any) =>
        this.homeService.deletePlaceholdersByIds(payload.placeholderIds).pipe(
            mergeMap(() => {
              if(payload.notifyMessage) {
                this.notifyService.showSuccess(payload.notifyMessage);
              } 
              else{
                this.notifyService.showSuccess('Assignment Deleted');
              }
              return (
                [
                  new ProjectAllocationsActions.DeletePlaceholderAllocationsSuccess(payload)
                ]
              );
            })))
    ));

}
