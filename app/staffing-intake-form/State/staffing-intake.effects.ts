import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import * as StaffingIntakeActions from "./staffing-intake.actions";
import { map, mergeMap, switchMap } from "rxjs/operators";
import { NotificationService } from "src/app/shared/notification.service";
import { StaffingIntakeFormService } from "../staffing-intake-form.service";
import { CaseIntakeLeadership } from "src/app/shared/interfaces/caseIntakeLeadership.interface";
import { CaseIntakeDetail } from "src/app/shared/interfaces/caseIntakeDetail.interface";
import { ProjectDetails } from "src/app/shared/interfaces/projectDetails.interface";
import { CaseIntakeRoleDetails } from "src/app/shared/interfaces/caseIntakeRoleDetails.interface";
import { CoreService } from "src/app/core/core.service";
import { PlanningCard } from "src/app/shared/interfaces/planningCard.interface";

@Injectable()
export class StaffingIntakeEffects {
    constructor(private actions$: Actions,
        private staffingIntakeFormService:StaffingIntakeFormService, 
        private notifyService: NotificationService,
        private coreService: CoreService) { }

    
    getLeaderShipData$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingIntakeActions.StaffingIntakeActionTypes.LoadLeadershipDetails),
      map((action: StaffingIntakeActions.LoadLeadershipDetails) => action.payload),
      switchMap((payload: any) =>
      this.staffingIntakeFormService.getLeaderShipDetails(payload).pipe(
            mergeMap((leadershipDetails: CaseIntakeLeadership[]) => {
              return (
                [
                  new StaffingIntakeActions.LoadLeadershipDetailsSuccess(leadershipDetails)
                ]
              );
            })))));

    getCaseIntakeDetails$ = createEffect(() => this.actions$.pipe(  
      ofType(StaffingIntakeActions.StaffingIntakeActionTypes.LoadCaseIntakeDetails),
      map((action: StaffingIntakeActions.LoadCaseIntakeDetails) => action.payload),
      switchMap((payload: any) =>
      this.staffingIntakeFormService.getCaseIntakeDetail(payload).pipe(
            mergeMap((caseIntakeDetails: CaseIntakeDetail) => {
              return (
                [
                  new StaffingIntakeActions.LoadCaseIntakeDetailsSuccess(caseIntakeDetails)
                ]
              );
            })))));

    getCaseBasicDetails$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingIntakeActions.StaffingIntakeActionTypes.LoadCaseBasicDetails),
      map((action: StaffingIntakeActions.LoadCaseBasicDetails) => action.payload),
      switchMap((payload: any) =>
      this.staffingIntakeFormService.getCaseDetails(payload).pipe(
            mergeMap((caseBasicDetails: ProjectDetails) => {
              return (
                [
                  new StaffingIntakeActions.LoadCaseBasicDetailsSuccess(caseBasicDetails)
                ]
              );
            })))));

    getOpportunityBasicDetails$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingIntakeActions.StaffingIntakeActionTypes.LoadOpportunityDetails),
      map((action: StaffingIntakeActions.LoadOpportunityDetails) => action.payload),
      switchMap((payload: any) =>
      this.staffingIntakeFormService.getOpportunityDetails(payload).pipe(
            mergeMap((opportunityDetails: ProjectDetails) => {
              return (
                [
                  new StaffingIntakeActions.LoadOpportunityDetailsSuccess(opportunityDetails)
                ]
              );
            })))));

    getPlanningCardDetails$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingIntakeActions.StaffingIntakeActionTypes.LoadPlanningCardDetails),
      map((action: StaffingIntakeActions.LoadPlanningCardDetails) => action.payload),
      switchMap((payload: any) =>
      this.staffingIntakeFormService.getPlanningCardDetails(payload).pipe(
            mergeMap((planningCardDetails: PlanningCard) => {
              if (planningCardDetails[0]) {
                return (
                  [
                    new StaffingIntakeActions.LoadPlanningCardDetailsSuccess(planningCardDetails)
                  ]
                )
              }
              else {
                // If no planningCardDetails found, dispatch failure action
                return [
                  new StaffingIntakeActions.LoadPlanningCardDetailsNotFound('Planning Card not found')
                ];
              }
            })))));

            getRoleAndWorkstreamDetails$ = createEffect(() => this.actions$.pipe(
              ofType(StaffingIntakeActions.StaffingIntakeActionTypes.LoadRoleAndWorkstreamDetails),
              map((action: StaffingIntakeActions.LoadRoleAndWorkstreamDetails) => action.payload),
              switchMap((payload) =>
                this.staffingIntakeFormService.getRoleAndWorkstreamDetails(payload.demandId).pipe(
                  map((roleAndWorkstreamDetails: any) => {
                    
                    return new StaffingIntakeActions.LoadRoleAndWorkstreamDetailsSuccess(roleAndWorkstreamDetails);
                    
                  }),
                )
              )
            ));
            

    upsertRoleDetails$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingIntakeActions.StaffingIntakeActionTypes.UpsertRoleDetails),
      map((action: StaffingIntakeActions.UpsertRoleDetails) => action.payload),
      switchMap((payload: any) =>
      this.staffingIntakeFormService.upsertRoleDetails(payload).pipe(
            mergeMap((roleDetails: CaseIntakeRoleDetails[]) => {
              if(roleDetails)
              {
                roleDetails[0].lastUpdatedByName = this.coreService.loggedInUser.fullName;              
              }
              this.notifyService.showSuccess('Role Details Saved');
              return (
                [
                  new StaffingIntakeActions.UpsertRoleDetailsSuccess(roleDetails)
                ]
              );
            })))));

      upsertRolesAndWorkstreamDetails$ = createEffect(() => this.actions$.pipe(
        ofType(StaffingIntakeActions.StaffingIntakeActionTypes.UpsertRolesAndWorkstreamDetails),
        map((action: StaffingIntakeActions.UpsertRolesAndWorkstreamDetails) => action.payload),
        switchMap((payload: any) =>
            this.staffingIntakeFormService.upsertRoleAndWorkstreamDetails(payload).pipe(
                mergeMap((result: any) => {
                    result.lastUpdatedByName = this.coreService.loggedInUser.fullName;
                    this.notifyService.showSuccess('Workstream Details Saved');
                    return [
                        new StaffingIntakeActions.UpsertRolesAndWorkstreamDetailsSuccess(result)
                    ];  
                })
            )
        )
    ));
          
    deleteWorkstreamById$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingIntakeActions.StaffingIntakeActionTypes.DeleteWorstreamsById),
      map((action: StaffingIntakeActions.DeleteWorstreamsById) => action.payload),
      switchMap((payload: any) =>
      this.staffingIntakeFormService.deleteWorstreamsById(payload).pipe(  
          mergeMap((response: any) => {
            this.notifyService.showSuccess('Workstream Details Deleted Successfully');
            let deletedData = {
              deletedWorkstreamIds : payload.id, 
              lastUpdatedByName : this.coreService.loggedInUser.fullName
            };
            
              return (
                [
                  new StaffingIntakeActions.DeleteWorstreamsByIdSuccess(deletedData)
                ]
              );
            })))));

    deleteRolesById$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingIntakeActions.StaffingIntakeActionTypes.DeleteRolesById),
      map((action: StaffingIntakeActions.DeleteRolesById) => action.payload),
      switchMap((payload: any) =>
      this.staffingIntakeFormService.deleteRolesById(payload).pipe(  
          mergeMap((response: any) => {
            this.notifyService.showSuccess('Role Details Deleted Successfully');
            let deletedData = {
              deletedRoleIds :payload.id, 
              lastUpdatedByName : this.coreService.loggedInUser.fullName
            };
              return (
                [
                  new StaffingIntakeActions.DeleteRolesByIdSuccess(deletedData)
                ]
              );
            })))));
    
    upsertCaseIntakeDetails$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingIntakeActions.StaffingIntakeActionTypes.UpsertCaseIntakeDetails),
      map((action: StaffingIntakeActions.UpsertCaseIntakeDetails) => action.payload),
      switchMap((payload: any) =>
      this.staffingIntakeFormService.upsertCaseIntakeDetail(payload).pipe(
            mergeMap((caseIntakeDetails: CaseIntakeDetail) => {
              caseIntakeDetails.lastUpdatedByName = this.coreService.loggedInUser.fullName;
              this.notifyService.showSuccess('Details Saved');
              return (
                [
                  new StaffingIntakeActions.UpsertCaseIntakeDetailsSuccess(caseIntakeDetails)
                ]
              );
            })))));

    upsertLeadershipDetails$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingIntakeActions.StaffingIntakeActionTypes.UpsertLeadershipDetails),
      map((action: StaffingIntakeActions.UpsertLeadershipDetails) => action.payload),
      mergeMap((payload: any) => //using mergeMap to keep all upsert calls since there can be many due to auto-save
      this.staffingIntakeFormService.upsertLeadershipDetails(payload).pipe(
            mergeMap((leadershipDetails: CaseIntakeLeadership[]) => {
              leadershipDetails.forEach(leader => {
                leader.lastUpdatedByName = this.coreService.loggedInUser.fullName;
              });
              this.notifyService.showSuccess('LeaderShip Details Saved');
              return (
                [
                  new StaffingIntakeActions.UpsertLeadershipDetailsSuccess(leadershipDetails)
                ]
              );
            })))));

    deleteLeadershipDetail$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingIntakeActions.StaffingIntakeActionTypes.DeleteLeadershipDetail),
      map((action: StaffingIntakeActions.DeleteLeadershipDetail) => action.payload),
      switchMap((payload: any) =>
      this.staffingIntakeFormService.deleteLeadershipDetail(payload).pipe(
            mergeMap((response: any) => {
              payload.lastUpdatedByName = this.coreService.loggedInUser.fullName;
              this.notifyService.showSuccess('LeaderShip Detail Removed');
              return (
                [
                  new StaffingIntakeActions.DeleteLeadershipDetailSuccess(payload)
                ]
              );
            })))));
    
      getLastUpdatedInfo$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingIntakeActions.StaffingIntakeActionTypes.LoadLastUpdatedByChanges),
      map((action: StaffingIntakeActions.LoadLastUpdatedByChanges) => action.payload),
      switchMap((payload: any) =>
      this.staffingIntakeFormService.getMostRecentUpdateInCaseIntake(payload).pipe(
            mergeMap((response: any) => {
              return (
                [
                  new StaffingIntakeActions.LoadLastUpdatedByChangesSuccess(response)
                ]
              );
            })))));

      getExpertiseRequirementList$ = createEffect(() =>
        this.actions$.pipe(
          ofType(StaffingIntakeActions.StaffingIntakeActionTypes.GetExpertiseRequirementList),
          switchMap(() =>
            this.staffingIntakeFormService.getExpertiseRequirementList().pipe(
              map((expertiseRequirementList: any) =>
                new StaffingIntakeActions.GetExpertiseRequirementListSuccess(expertiseRequirementList)
              )
            )
          )
        )
      );

      upsertExpertiseRequirementList$ = createEffect(() => this.actions$.pipe(
      ofType(StaffingIntakeActions.StaffingIntakeActionTypes.UpsertExpertiseRequirementList),
      map((action: StaffingIntakeActions.UpsertExpertiseRequirementList) => action.payload),
      switchMap((payload: any) => this.staffingIntakeFormService.upsertExpersiteRequirement(payload).pipe(
            mergeMap((response: any) => {
              return (
                [
                  new StaffingIntakeActions.UpsertExpertiseRequirementListSuccess(response)
                ]
              );
            })))));

}
