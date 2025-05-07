import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { map, mergeMap, switchMap } from 'rxjs/operators';
import * as ProjectOverlayActions from '../actions/project-overlay.action';
import { OverlayService } from 'src/app/overlay/overlay.service';
import { NotificationService } from 'src/app/shared/notification.service';
import { SharedService } from 'src/app/shared/shared.service';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { ResourceAssignmentService } from 'src/app/overlay/behavioralSubjectService/resourceAssignment.service';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { CoreService } from 'src/app/core/core.service';
import { forkJoin } from 'rxjs';

@Injectable()
export class ProjectOverlayEffects {
    constructor(
        private actions$: Actions,
        private overlayService: OverlayService,
        private notifyService: NotificationService,
        private sharedService: SharedService,
        private resourceAssignmentService: ResourceAssignmentService,
        private localStorageService: LocalStorageService,
        private coreService: CoreService) {}

    
    updateOpportunity$ = createEffect(() => this.actions$.pipe(
        ofType(ProjectOverlayActions.ProjectOverlayActionTypes.UpdateOpportunity),
            map((action: ProjectOverlayActions.UpdateOpportunity) => action.payload),
            switchMap((payload: any) => 
                this.overlayService.updateOppChanges(payload.event).pipe(
                    mergeMap((updatedData: any) => {
                        payload.updatedData = updatedData;
                        this.notifyService.showSuccess('Pipeline data updated successfully');
                    return (
                    [
                      new ProjectOverlayActions.UpdateProjectSuccess(payload)
                    ]
                );
              })))
            ))

    
    updateCase$ = createEffect(() => this.actions$.pipe(
        ofType(ProjectOverlayActions.ProjectOverlayActionTypes.UpdateCase),
            map((action: ProjectOverlayActions.UpdateCase) => action.payload),
            switchMap((payload: any) => 
                this.overlayService.updateCaseChanges(payload.event).pipe(
                    mergeMap((updatedData: any) => {
                        payload.updatedData = updatedData;
                        this.notifyService.showSuccess('Case data updated successfully');
                    return (
                    [
                      new ProjectOverlayActions.UpdateProjectSuccess(payload)
                    ]
                );
              })))
            ))

    
    upsertCaseRollAndAllocations$ = createEffect(() => this.actions$.pipe(
        ofType(ProjectOverlayActions.ProjectOverlayActionTypes.UpsertCaseRollAndAllocations),
            map((action: ProjectOverlayActions.UpsertCaseRollAndAllocations) => action.payload),
            switchMap((payload: any) => 
                this.overlayService.upsertCaseRollsAndAllocations(payload.caseRollArray, payload.resourceAllocations).pipe(
                    mergeMap((updatedData: any) => {
                        payload.updatedData = updatedData;
                        this.notifyService.showSuccess('Case Rolled Successfully');

                        if(payload.allocationDataBeforeSplitting) {
                            if (payload.allocationDataBeforeSplitting.every((r)=> r.oldCaseCode))
                            {
                                this.sharedService.checkPegRingfenceAllocationAndInsertDownDayCommitments(payload.allocationDataBeforeSplitting).subscribe(commitments => 
                                {
                                    if(commitments?.length > 0)
                                    {
                                        this.notifyService.showSuccess(ConstantsMaster.Messages.DownDaySaved);
                                    }
                                });
                            }
                        }
                        this.resourceAssignmentService.upsertPlaygroundAllocationsForCasePlanningMetrics(payload.resourceAllocations);
                    return (
                    [
                      new ProjectOverlayActions.UpsertCaseRollAndAllocationsSuccess(payload)
                    ]
                );
              })))
            ))

            upsertCaseRollAndPlaceholderAllocations$ = createEffect(() => this.actions$.pipe(
              ofType(ProjectOverlayActions.ProjectOverlayActionTypes.UpsertCaseRollAndPlaceholderAllocations),
                  map((action: ProjectOverlayActions.UpsertCaseRollAndAllocations) => action.payload),
                  switchMap((payload: any) => 
                      this.overlayService.upsertCaseRollsAndPlaceholderAllocations(payload.caseRollArray, payload.resourceAllocations).pipe(
                          mergeMap((updatedData: any) => {
                              payload.updatedData = updatedData;
                              this.notifyService.showSuccess('Case Rolled Successfully');
      
                          return (
                          [
                            new ProjectOverlayActions.UpsertCaseRollAndPlaceholderAllocationsSuccess(payload)
                          ]
                      );
                    })))
                  ))

    
    revertCaseRollAndAllocations$ = createEffect(() => this.actions$.pipe(
        ofType(ProjectOverlayActions.ProjectOverlayActionTypes.RevertCaseRollAndAllocations),
            map((action: ProjectOverlayActions.RevertCaseRollAndAllocations) => action.payload),
            switchMap((payload: any) => 
                this.overlayService.revertCaseRollAndAllocations(payload.caseRoll, payload.resourceAllocations).pipe(
                    mergeMap((updatedData: any) => {
                        payload.updatedData = updatedData;
                        this.notifyService.showSuccess('Case Roll reverted Successfully');
                        this.resourceAssignmentService.upsertPlaygroundAllocationsForCasePlanningMetrics(payload.resourceAllocations);
                    return (
                    [
                      new ProjectOverlayActions.RevertCaseRollAndAllocationsSuccess(payload)
                    ]
                );
              })))
            ))

    
    insertSKUCaseTerms$ = createEffect(() => this.actions$.pipe(
        ofType(ProjectOverlayActions.ProjectOverlayActionTypes.InsertSKUCaseTerms),
            map((action: ProjectOverlayActions.InsertSKUCaseTerms) => action.payload),
            switchMap((payload: any) => 
                this.overlayService.insertSKUCaseTerms(payload.skuTab).pipe(
                    mergeMap((updatedData: any) => {
                        payload.updatedData = updatedData;
                        this.notifyService.showSuccess('SKU Term added successfully');
                    return (
                    [
                      new ProjectOverlayActions.InsertSKUCaseTermsSuccess(payload)
                    ]
                );
              })))
            ))

    
    deleteSKUCaseTerms$ = createEffect(() => this.actions$.pipe(
        ofType(ProjectOverlayActions.ProjectOverlayActionTypes.DeleteSKUCaseTerms),
            map((action: ProjectOverlayActions.DeleteSKUCaseTerms) => action.payload),
            switchMap((payload: any) => 
                this.overlayService.deleteSKUCaseTerms(payload.skuTab.id).pipe(
                    mergeMap((updatedData: any) => {
                        payload.updatedData = updatedData;
                        payload.demandFilterCriteria = JSON.parse(sessionStorage.getItem('demandFilterCriteriaObj'));
                        this.notifyService.showSuccess('SKU Term deleted');
                    return (
                    [
                      new ProjectOverlayActions.DeleteSKUCaseTermsSuccess(payload),
                      new ProjectOverlayActions.GetSKUCaseTerms(payload)
                    ]
                );
              })))
            ))

    
    updateSKUCaseTerms$ = createEffect(() => this.actions$.pipe(
        ofType(ProjectOverlayActions.ProjectOverlayActionTypes.UpdateSKUCaseTerms),
            map((action: ProjectOverlayActions.UpdateSKUCaseTerms) => action.payload),
            switchMap((payload: any) => 
                this.overlayService.updateSKUCaseTerms(payload.skuTab).pipe(
                    mergeMap((updatedData: any) => {
                        payload.updatedData = updatedData;
                        payload.demandFilterCriteria = JSON.parse(sessionStorage.getItem('demandFilterCriteriaObj'));
                        this.notifyService.showSuccess('SKU Term updated successfully');
                    return (
                    [
                      new ProjectOverlayActions.UpdateSKUCaseTermsSuccess(payload),
                      new ProjectOverlayActions.GetSKUCaseTerms(payload)
                    ]
                );
              })))
            ))

    
    getSKUCaseTerms$ = createEffect(() => this.actions$.pipe(
        ofType(ProjectOverlayActions.ProjectOverlayActionTypes.GetSKUCaseTerms),
            map((action: ProjectOverlayActions.GetSKUCaseTerms) => action.payload),
            switchMap((payload: any) => 
                this.overlayService.getSKUTermsForCasesOrOpportunitiesForDuration(payload.skuTab, payload.demandFilterCriteria).pipe(
                    mergeMap((responseData: any) => {
                        payload.responseData = responseData;
                        payload.masterSkuTerms = this.localStorageService.get(ConstantsMaster.localStorageKeys.skuTermList);
                    return (
                    [
                      new ProjectOverlayActions.GetSKUCaseTermsSuccess(payload)
                    ]
                );
              })))
            ))

    
    updateUserPreferencesForPinOrHide$ = createEffect(() => this.actions$.pipe(
        ofType(ProjectOverlayActions.ProjectOverlayActionTypes.UpdateUserPreferencesForPinOrHide),
            map((action: ProjectOverlayActions.UpdateUserPreferencesForPinOrHide) => action.payload),
            switchMap((payload: any) =>
                forkJoin([this.coreService.updateUserPreferences(payload.userPreferences), 
                this.overlayService.detailsByCaseCodeOrPipelineId(payload)]).pipe(
                    mergeMap((updatedData: any) => {
                        payload.updatedData = updatedData[0];
                        payload.updatedProject = updatedData[1];
                        this.coreService.setUserPreferences(updatedData[0], false);
                        if (payload.oldCaseCode) {
                          this.notifyService.showSuccess(`Case ${payload.notifyMessage}`);
                        } else if (payload.pipelineId) {
                          this.notifyService.showSuccess(`Opportunity ${payload.notifyMessage}`);
                        }
                    return (
                    [
                      payload.action == 'pin' ?
                         new ProjectOverlayActions.UpdateUserPreferencesForPinSuccess(payload) :
                        new ProjectOverlayActions.UpdateUserPreferencesForHideSuccess(payload)
                    ]
                );
              })))
            ))
}
