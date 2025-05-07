import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import * as StaffingDemandActions from '../actions/staffing-demand.action';
import { map, mergeMap, switchMap } from "rxjs/operators";
import { HomeService } from "src/app/home-copy/home.service";
import { PlanningCard } from "src/app/shared/interfaces/planningCard.interface";
import { NotificationService } from "src/app/shared/notification.service";
import { PlanningCardService } from "src/app/core/services/planning-card.service";
import { SharedService } from "src/app/shared/shared.service";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { ResourceAssignmentService } from "src/app/overlay/behavioralSubjectService/resourceAssignment.service";
import { OverlayService } from "src/app/overlay/overlay.service";
import { CoreService } from "src/app/core/core.service";
import { ResourceOrCasePlanningViewNote } from "src/app/shared/interfaces/resource-or-case-planning-view-note.interface";
import { ProjectViewModel } from "src/app/shared/interfaces/projectViewModel.interface";
import * as UserSpecificDetailsActions from "src/app/state/actions/user-specific-details.action";

@Injectable()
export class StaffingDemandEffects {
    constructor(private actions$: Actions,
        private homeService: HomeService,
        private notifyService: NotificationService,
        private planningCardService: PlanningCardService,
        private sharedService: SharedService,
        private resourceAssignmentService: ResourceAssignmentService,
        private overlayService: OverlayService,
        private coreService: CoreService) { }
  
    
    loadProjectsBySelectedFilters$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingDemandActions.StaffingDemandActionTypes.LoadProjectsBySelectedFilters),
      map((action: StaffingDemandActions.LoadProjectsBySelectedFilters) => action.payload),
      switchMap((payload: any) =>
        this.homeService.getProjectsFilteredBySelectedValues(payload.demandFilterCriteriaObj).pipe(
            mergeMap((projects: ProjectViewModel) => {
              return (
                [
                  new StaffingDemandActions.LoadProjectsBySelectedFiltersSuccess(projects)
                ]
              );
            })))));

    
    loadHistoricalProjectsBySelectedFilters$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingDemandActions.StaffingDemandActionTypes.LoadHistoricalProjectsBySelectedFilters),
      map((action: StaffingDemandActions.LoadHistoricalProjectsBySelectedFilters) => action.payload),
      switchMap((payload: any) =>
        this.homeService.getOngoingCasesBySelectedValues(payload.demandFilterCriteriaObj).pipe(
            mergeMap((projects: ProjectViewModel) => {
              return (
                [
                  new StaffingDemandActions.LoadHistoricalProjectsBySelectedFiltersSuccess(projects)
                ]
              );
            })))));

    
    loadPlanningCardsBySelectedFilters$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingDemandActions.StaffingDemandActionTypes.LoadPlanningCardsBySelectedFilters),
      map((action: StaffingDemandActions.LoadPlanningCardsBySelectedFilters) => action.payload),
      switchMap((payload: any) =>
        this.homeService.getPlanningCardsBySelectedValues(payload.demandFilterCriteriaObj).pipe(
            mergeMap((planningCards: PlanningCard[]) => {
              payload.demandFilterCriteria = JSON.parse(sessionStorage.getItem('demandFilterCriteriaObj'));
              payload.planningCards = planningCards;
              return (
                [
                  new StaffingDemandActions.LoadPlanningCardsBySelectedFiltersSuccess(payload)
                ]
              );
            })))));

    
    upsertResourceAllocations$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingDemandActions.StaffingDemandActionTypes.UpsertResourceAllocations),
      map((action: StaffingDemandActions.UpsertResourceAllocations) => action.payload),
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
            new StaffingDemandActions.UpsertResourceAllocationsSuccess(payload)
          ]
      );        
      })))
    ))

    
    upsertPlaceholderAllocations$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingDemandActions.StaffingDemandActionTypes.UpsertPlaceholderAllocations),
      map((action: StaffingDemandActions.UpsertPlaceholderAllocations) => action.payload),
      switchMap((payload: any) =>
        this.homeService.upsertPlaceholderAllocations(payload.placeholderAllocations).pipe(
            mergeMap((updatedData: any) => {
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
                  updatedData[0].planningCardId ? new StaffingDemandActions.UpsertPlanningCardAllocationsSuccess(payload) :
                  new StaffingDemandActions.UpsertPlaceholderAllocationsSuccess(payload)
                ]
              );
            })))
    ))

    
    deleteResourceAllocations$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingDemandActions.StaffingDemandActionTypes.DeleteResourceAllocations),
      map((action: StaffingDemandActions.DeleteResourceAllocations) => action.payload),
      switchMap((payload: any) =>
      this.overlayService.deleteResourcesAssignmentsFromProject(payload.allocationIds).pipe(
        mergeMap((deletedData: any) => {
          this.notifyService.showSuccess('Assignment Deleted');
          this.resourceAssignmentService.deletePlaygroundAllocationsForCasePlanningMetrics(payload.allocation);
          return (
          [
            new StaffingDemandActions.DeleteResourceAllocationsSuccess(payload)
          ]
        );
      })))
    ))

    
    deletePlaceholderAllocations$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingDemandActions.StaffingDemandActionTypes.DeletePlaceholderAllocations),
      map((action: StaffingDemandActions.DeletePlaceholderAllocations) => action.payload),
      mergeMap((payload: any) =>
        this.homeService.deletePlaceholdersByIds(payload.placeholderIds).pipe(
            mergeMap(() => {
              if(payload.notifyMessage) {
                this.notifyService.showSuccess(payload.notifyMessage);
              }
              return (
                [
                  new StaffingDemandActions.DeletePlaceholderAllocationsSuccess(payload)
                ]
              );
            })))
    ));

    
    mergePlanningCards$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingDemandActions.StaffingDemandActionTypes.MergePlanningCards),
      map((action: StaffingDemandActions.MergePlanningCards) => action.payload),
      switchMap((payload: any) =>
      this.planningCardService.mergePlanningCardAndAllocations(payload.planningCard, payload.resourceAllocations, payload.placeholderAllocations).pipe(
            mergeMap((updatedData: PlanningCard[]) => {
              if(payload.allocationDataBeforeSplitting) {
                if (payload.allocationDataBeforeSplitting.every((r)=> r.oldCaseCode)) {
                    this.sharedService.checkPegRingfenceAllocationAndInsertDownDayCommitments(payload.allocationDataBeforeSplitting).subscribe(commitments => {
                        if(commitments?.length > 0) {
                            this.notifyService.showSuccess(ConstantsMaster.Messages.DownDaySaved);
                        }
                    });
                }}
                this.resourceAssignmentService.upsertPlaygroundAllocationsForCasePlanningMetrics(payload.resourceAllocations);          
                this.notifyService.showSuccess(`Planning Card merged successfully!!`);
              return (
                [
                  new StaffingDemandActions.MergePlanningCardsSuccess(payload)
                ]
              );
            })))));

    
    deletePlanningCard$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingDemandActions.StaffingDemandActionTypes.DeletePlanningCard),
      map((action: StaffingDemandActions.DeletePlanningCard) => action.payload),
      mergeMap((payload: any) =>
        this.homeService.deletePlanningCardAndItsAllocations(payload.planningCardId).pipe(
            mergeMap(() => {
              this.notifyService.showSuccess(`Planning Card deleted successfully!!`);
              return (
                [
                  new StaffingDemandActions.DeletePlanningCardSuccess(payload)
                ]
              );
            })))));

    
    upsertPlanningCard$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingDemandActions.StaffingDemandActionTypes.UpsertPlanningCard),
      map((action: StaffingDemandActions.UpsertPlanningCard) => action.payload),
      switchMap((payload: any) =>
        this.homeService.upsertPlanningCard(payload.planningCard).pipe(
            mergeMap((upsertedData: any) => {
              payload.upsertedData = upsertedData;
              payload.demandFilterCriteria = JSON.parse(sessionStorage.getItem('demandFilterCriteriaObj'));
              this.notifyService.showSuccess(`Planning Card updated successfully!!`);
              return (
                [
                  new StaffingDemandActions.UpsertPlanningCardSuccess(payload)
                ]
              );
            })))));


    
    upsertCasePlanningViewNote$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingDemandActions.StaffingDemandActionTypes.UpsertCaseViewNotes),
      map((action: StaffingDemandActions.UpsertCaseViewNotes) => action.payload),
      mergeMap((payload: ResourceOrCasePlanningViewNote) =>
        this.sharedService.upsertCasePlanningViewNote(payload).pipe(
          mergeMap((result: ResourceOrCasePlanningViewNote) => {
            if (payload.id)
              this.notifyService.showSuccess(`Note Updated Successfully`);
            else
              this.notifyService.showSuccess(`Note Added Successfully`);

            
            return (
              [
                new StaffingDemandActions.UpsertCaseViewNotesSuccess(result),
                new UserSpecificDetailsActions.GetMostRecentSharedWithEmployeeGroups()
              ]
            );
          })
        ))
    ));

    
    deleteCasePlanningViewNotes$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingDemandActions.StaffingDemandActionTypes.DeleteCaseViewNotes),
      map((action: StaffingDemandActions.DeleteCaseViewNotes) => action.payload),
      mergeMap((payload: string) =>
        this.sharedService.deleteCasePlanningNotes(payload).pipe(
          mergeMap((result: string[]) => {
            if (result.length === 1)
              this.notifyService.showSuccess(`Note Deleted Successfully`);
            else if (result.length > 1)
              this.notifyService.showSuccess(`Notes Deleted Successfully`);
            return (
              [
                new StaffingDemandActions.DeleteCaseViewNotesSuccess(result),
                new UserSpecificDetailsActions.GetMostRecentSharedWithEmployeeGroups()
              ]
            );
          })
        ))
    ));  
    
}
