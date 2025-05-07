import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { map, mergeMap } from 'rxjs/operators';
import { ResourceAssignmentService } from 'src/app/overlay/behavioralSubjectService/resourceAssignment.service';
import { Commitment } from 'src/app/shared/interfaces/commitment.interface';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
import { StaffableAsRole } from 'src/app/shared/interfaces/staffableAsRole.interface';
import { NotificationService } from 'src/app/shared/notification.service';
import { CasePlanningService } from '../case-planning.service';
import * as casePlanningActions from './case-planning.actions';
import { ResourceOrCasePlanningViewNote } from 'src/app/shared/interfaces/resource-or-case-planning-view-note.interface';
import * as UserSpecificDetailsActions from "src/app/state/actions/user-specific-details.action";
import { PlaceholderAssignmentService } from 'src/app/overlay/behavioralSubjectService/placeholderAssignment.service';
import { IncludeInDemandProjectPreference } from 'src/app/shared/interfaces/includeInDemandProjectPreference';

@Injectable()
export class CasePlanningEffects {

    constructor(
        private actions$: Actions,
        private notifyService: NotificationService,
        private casePlanningService: CasePlanningService,
        private placeholderAssignmentService: PlaceholderAssignmentService
    ) { }

    
    loadActiveProjects$ = createEffect(() => this.actions$.pipe(
        ofType(casePlanningActions.CasePlanningActionTypes.LoadProjects),
        map((action: casePlanningActions.LoadProjects) => action.payload),
        mergeMap((payload: any) =>
            this.casePlanningService.getProjectsFilteredBySelectedValues(payload.demandFilterCriteriaObj)
                .pipe(
                    mergeMap((projects: Project[]) => {
                        return (
                            [
                                new casePlanningActions.CasePlanningLoader(false)
                                ,new casePlanningActions.LoadProjectsSuccess(projects)
                            ]
                        );
                    })))));

    
    loadActivePlanningCards$ = createEffect(() => this.actions$.pipe(
        ofType(casePlanningActions.CasePlanningActionTypes.LoadPlanningCards),
        map((action: casePlanningActions.LoadPlanningCards) => action.payload),
        mergeMap((payload: any) =>
            this.casePlanningService.getPlanningCardsBySelectedValues(payload.demandFilterCriteriaObj)
                .pipe(
                    mergeMap((planningCards: PlanningCard[]) => {
                        const combinedObject = {demandFilterCriteriaObj: payload.demandFilterCriteriaObj, planningCards: planningCards};
                        return (
                            [
                                new casePlanningActions.CasePlanningLoader(false)
                                , new casePlanningActions.LoadPlanningCardsSuccess(combinedObject)
                            ]
                        );
                    })))));

    loadAvailabilityMetrics$ = createEffect(() => this.actions$.pipe(
    ofType(casePlanningActions.CasePlanningActionTypes.LoadAvailabilityMetrics),
    map((action: casePlanningActions.LoadAvailabilityMetrics) => action.payload),
    mergeMap((payload: any) =>
        this.casePlanningService.getAvailabilityMetricsByFilterValues(payload.supplyFilterCriteriaObj)
            .pipe(
                mergeMap((metrics: any) => {
                    return (
                        [
                            new casePlanningActions.CasePlanningLoader(false)
                            , new casePlanningActions.LoadAvailabilityMetricsSuccess(metrics)
                        ]
                    );
                })))));

    loadMetricsDemandData$ = createEffect(() => this.actions$.pipe(
    ofType(casePlanningActions.CasePlanningActionTypes.LoadMetricsDemandData),
    map((action: casePlanningActions.LoadMetricsDemandData) => action.payload),
    mergeMap((payload: any) =>
        this.casePlanningService.getCasePlanningBoardDataBySelectedValues(payload.demandFilterCriteriaObj)
            .pipe(
                mergeMap((data: any) => {
                    return (
                        [
                            new casePlanningActions.CasePlanningLoader(false)
                            , new casePlanningActions.LoadMetricsDemandDataSuccess(data)
                        ]
                    );
                })))));

    loadProjectsBySearchString$ = createEffect(() => this.actions$.pipe(
        ofType(casePlanningActions.CasePlanningActionTypes.LoadProjectsBySearchString),
        map((action: casePlanningActions.LoadProjectsBySearchString) => action.payload),
        mergeMap((payload: any) =>
            this.casePlanningService.getProjectsBySearchString(payload.searchString).pipe(
                mergeMap((projects: Project[]) => {
                    return (
                        [
                            new casePlanningActions.CasePlanningLoader(false)
                            , new casePlanningActions.LoadProjectsBySearchStringSuccess(projects)
                        ])
                })
            ))
    ));

    upsertCasePlanningViewNote$ = createEffect(() => this.actions$.pipe(
      ofType(casePlanningActions.CasePlanningActionTypes.UpsertCasePlanningViewNote),
      map((action: casePlanningActions.UpsertCasePlanningViewNote) => action.payload),
      mergeMap((payload: ResourceOrCasePlanningViewNote) =>
        this.casePlanningService.upsertCasePlanningViewNote(payload).pipe(
          mergeMap((result: ResourceOrCasePlanningViewNote) => {
            if (payload.id)
              this.notifyService.showSuccess(`Note Updated Successfully`);
            else
              this.notifyService.showSuccess(`Note Added Successfully`);
            return (
              [
                new casePlanningActions.UpsertCasePlanningViewNoteSuccess(result),
                new casePlanningActions.CasePlanningLoader(false),
                new UserSpecificDetailsActions.GetMostRecentSharedWithEmployeeGroups()
              ]
            );
          })
        ))
    ));

        
    deleteCasePlanningViewNotes$ = createEffect(() => this.actions$.pipe(
        ofType(casePlanningActions.CasePlanningActionTypes.DeleteCasePlanningViewNotes),
        map((action: casePlanningActions.DeleteCasePlanningViewNotes) => action.payload),
        mergeMap((payload: string) =>
          this.casePlanningService.deleteCasePlanningNotes(payload).pipe(
            mergeMap((result: string[]) => {
              if (result.length === 1)
                this.notifyService.showSuccess(`Note Deleted Successfully`);
              else if (result.length > 1)
                this.notifyService.showSuccess(`Notes Deleted Successfully`);
              return (
                [
                  new casePlanningActions.DeleteCasePlanningViewNotesSuccess(result),
                  new casePlanningActions.CasePlanningLoader(false),
                  new UserSpecificDetailsActions.GetMostRecentSharedWithEmployeeGroups()
                ]
              );
            })
          ))
      ));

      upsertCasePlanningProjectDetails$ = createEffect(() => this.actions$.pipe(
        ofType(casePlanningActions.CasePlanningActionTypes.UpsertCasePlanningProjectDetails),
        map((action: casePlanningActions.UpsertCasePlanningProjectDetails) => action.payload),
        mergeMap((payload: any) =>
          this.casePlanningService.upsertCasePlanningProjectDetails(payload.projDetails).pipe(
            mergeMap((result: any) => {
                this.notifyService.showSuccess(`Include In Demand Updated Successfully`);
              return (
                [
                  new casePlanningActions.UpsertCasePlanningProjectDetailsSuccess(payload),
                ]
              );
            })
          ))
      ));

 
}
