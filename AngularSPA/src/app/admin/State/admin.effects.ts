import { Injectable } from '@angular/core';
import { Actions, createEffect,ofType } from '@ngrx/effects';
import { Observable } from 'rxjs';
import { map, mergeMap } from 'rxjs/operators';
import { CommitmentsService } from 'src/app/shared/commitments.service';
import { CommitmentType } from 'src/app/shared/interfaces/commitmentType.interface';
import { SecurityUserDetail } from 'src/app/shared/interfaces/securityUserDetail';
import { NotificationService } from 'src/app/shared/notification.service';
import { AdminService } from '../admin.service';
import * as adminActions from './admin.actions';
import { SecurityGroup } from 'src/app/shared/interfaces/securityGroup';

@Injectable()
export class AdminEffects {
    constructor(private actions$: Actions,
        private adminService: AdminService,
        private notifyService: NotificationService,
        private commitmentService: CommitmentsService) { }

    loadStaffingUsers$ = createEffect(() => this.actions$.pipe(
        ofType(adminActions.AdminActionTypes.LoadStaffingUsers),
        mergeMap(() => this.adminService.getSecurityUsersDetails()
            .pipe(
                mergeMap((staffingUsers: SecurityUserDetail[]) => {
                    return (
                        [
                            new adminActions.ShowHideStaffingUsersLoader(false),
                            new adminActions.LoadStaffingUsersSuccess(staffingUsers)
                        ]
                    );
                })
            ))
        )
    );

    
    deleteStaffingUsers$ = createEffect(() => this.actions$.pipe(
        ofType(adminActions.AdminActionTypes.DeleteSaffingUser),
        map((action: adminActions.DeleteSaffingUser) => action.payload),
        mergeMap((payload: any) =>
            this.adminService.deleteSecurityUser(payload)
                .pipe(
                    mergeMap(() => {
                        this.notifyService.showSuccess('User has been deleted');
                        return (
                            [
                                new adminActions.ShowHideStaffingUsersLoader(false),
                                new adminActions.DeleteSaffingUserSuccess(payload)
                            ]
                        );
                    })
                )
        )
    ));

    
    upsertSecurityUser$ = createEffect(() => this.actions$.pipe(
        ofType(adminActions.AdminActionTypes.UpsertSecurityUser),
        map((action: adminActions.UpsertSecurityUser) => action.payload),
        mergeMap((payload: any) =>
            this.adminService.upsertSecurityUser(payload)
                .pipe(
                    mergeMap(((staffingUser: SecurityUserDetail) => {
                        this.notifyService.showSuccess('User has been updated');
                        
                        //put upserted properties in response object 
                        const response = { ...payload, lastUpdated: staffingUser.lastUpdated };
                       
                        return (
                            [
                                new adminActions.ShowHideStaffingUsersLoader(false),
                                new adminActions.UpsertSecurityUserSuccess(response)
                            ]
                        );
                    })
                    )
                )
        )
    ));

    loadStaffingGroups$ = createEffect(() => this.actions$.pipe(
        ofType(adminActions.AdminActionTypes.LoadStaffingGroups),
        mergeMap(() => this.adminService.getSecurityGroupsDetails()
            .pipe(
                mergeMap((staffingGroups: SecurityGroup[]) => {
                    return (
                        [
                            new adminActions.ShowHideStaffingUsersLoader(false),
                            new adminActions.LoadStaffingGroupsSuccess(staffingGroups)
                        ]
                    );
                })
            ))
        )
    );

    upsertSecurityGroup$ = createEffect(() => this.actions$.pipe(
        ofType(adminActions.AdminActionTypes.UpsertSecurityGroup),
        map((action: adminActions.UpsertSecurityGroup) => action.payload),
        mergeMap((payload: SecurityGroup) =>
            this.adminService.upsertSecurityGroup(payload)
                .pipe(
                    mergeMap(((response: SecurityGroup) => {
                        this.notifyService.showSuccess('Group has been added/updated');
                        
                        return (
                            [
                                new adminActions.UpsertSecurityGroupSuccess(response)
                            ]
                        );
                    })
                    )
                )
        )
    ));

    deleteStaffingGroups$ = createEffect(() => this.actions$.pipe(
        ofType(adminActions.AdminActionTypes.DeleteSaffingGroup),
        map((action: adminActions.DeleteSaffingUser) => action.payload),
        mergeMap((payload: any) =>
            this.adminService.deleteSecurityGroup(payload)
                .pipe(
                    mergeMap(() => {
                        this.notifyService.showSuccess('Group has been deleted');
                        return (
                            [
                                new adminActions.ShowHideStaffingUsersLoader(false),
                                new adminActions.DeleteSaffingGroupSuccess(payload)
                            ]
                        );
                    })
                )
        )
    ));

    loadPracticeBasedRingfences$ = createEffect(() => this.actions$.pipe(
        ofType(adminActions.AdminActionTypes.LoadPracticeBasedRingfences),
        map((action: adminActions.LoadPracticeBasedRingfences) => action.payload),
        mergeMap((showHidden) => this.commitmentService.getCommitmentTypes(showHidden)
            .pipe(
                mergeMap((allCommitmentTypes: CommitmentType[]) => {
                    return (
                        [
                            new adminActions.LoadPracticeBasedRingfencesSuccess(allCommitmentTypes)
                        ]
                    );
                })
            ))
        )
    );

    
    UpsertPracticeBasedRingfence$ = createEffect(() => this.actions$.pipe(
        ofType(adminActions.AdminActionTypes.UpsertPracticeBasedRingfence),
        map((action: adminActions.UpsertPracticeBasedRingfence) => action.payload),
        mergeMap((payload: any) =>
            this.commitmentService.upsertCommitmentType(payload)
                .pipe(
                    mergeMap(((commitmentType: CommitmentType) => {
                        this.notifyService.showSuccess('Commitment Type has been updated');
                        
                        //put upserted properties in response object 
                        const response = { ...payload };
                       
                        return (
                            [
                                new adminActions.UpsertPracticeBasedRingfenceSuccess(response)
                            ]
                        );
                    })
                    )
                )
        )
    ));
}
