import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import * as PlanningCardOverlayActions from '../actions/planning-card-overlay.action';
import { map, mergeMap, switchMap } from "rxjs/operators";
import { HomeService } from "src/app/home-copy/home.service";
import { NotificationService } from "src/app/shared/notification.service";
import { PlanningCard } from "src/app/shared/interfaces/planningCard.interface";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { PlanningCardService } from "src/app/core/services/planning-card.service";
import { ResourceAssignmentService } from "src/app/overlay/behavioralSubjectService/resourceAssignment.service";
import { SharedService } from "src/app/shared/shared.service";
import * as StaffingDemandActions from 'src/app/home-copy/state/actions/staffing-demand.action';


@Injectable()
export class PlanningCardOverlayEffects {
    constructor(private actions$: Actions,
        private homeCopyService: HomeService,
        private notifyService: NotificationService,
        private planningCardService: PlanningCardService,
        private sharedService: SharedService,
        private resourceAssignmentService: ResourceAssignmentService,) { }
  
    
    upsertPlanningCard$ = createEffect(() => this.actions$.pipe(
      ofType(PlanningCardOverlayActions.PlanningCardOverlayActionTypes.UpsertPlanningCard),
      map((action: PlanningCardOverlayActions.UpsertPlanningCard) => action.payload),
      switchMap((payload: any) =>
        this.homeCopyService.upsertPlanningCard(payload.planningCard).pipe(
            mergeMap((upsertedData: any) => {
              payload.upsertedData = upsertedData;
              payload.upsertedData.tempid = payload.planningCard.id;
              payload.demandFilterCriteria = JSON.parse(sessionStorage.getItem('demandFilterCriteriaObj'));
              this.notifyService.showSuccess(`Planning Card updated successfully!!`);
              return (
                [
                  new PlanningCardOverlayActions.UpsertPlanningCardSuccess(payload)
                ]
              );
            })))));
    
    
    deletePlanningCard$ = createEffect(() => this.actions$.pipe(
      ofType(PlanningCardOverlayActions.PlanningCardOverlayActionTypes.DeletePlanningCard),
      map((action: PlanningCardOverlayActions.DeletePlanningCard) => action.payload),
      mergeMap((payload: any) =>
        this.homeCopyService.deletePlanningCardAndItsAllocations(payload.planningCardId).pipe(
            mergeMap(() => {
              this.notifyService.showSuccess(`Planning Card deleted successfully!!`);
              return (
                [
                  new PlanningCardOverlayActions.DeletePlanningCardSuccess(payload)
                ]
              );
            })))));

    
    mergePlanningCards$ = createEffect(() => this.actions$.pipe(
      ofType(PlanningCardOverlayActions.PlanningCardOverlayActionTypes.MergePlanningCards),
      map((action: PlanningCardOverlayActions.MergePlanningCards) => action.payload),
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
                  new PlanningCardOverlayActions.MergePlanningCardsSuccess(payload)
                ]
              );
            })))));

    loadPlanningCardsBySelectedFilters$ = createEffect(() => this.actions$.pipe(
      ofType(PlanningCardOverlayActions.PlanningCardOverlayActionTypes.LoadPlanningCardsBySelectedFilters),
      map((action: PlanningCardOverlayActions.LoadPlanningCardsBySelectedFilters) => action.payload),
      switchMap((payload: any) =>
        this.sharedService.getPlanningCardsBySelectedValues(payload.demandFilterCriteriaObj).pipe(
            mergeMap((planningCards: PlanningCard[]) => {
              payload.demandFilterCriteria = JSON.parse(sessionStorage.getItem('demandFilterCriteriaObj'));
              payload.planningCards = planningCards;
              return (
                [
                  // new StaffingDemandActions.LoadPlanningCardsBySelectedFiltersSuccess(payload),
                  new PlanningCardOverlayActions.LoadPlanningCardsBySelectedFiltersSuccess(payload),
                  new StaffingDemandActions.LoadPlanningCardsBySelectedFiltersSuccess(payload)
                ]
              );
            })))));

}
