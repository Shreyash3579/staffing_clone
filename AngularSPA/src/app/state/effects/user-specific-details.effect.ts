import { Injectable } from '@angular/core'; 
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { map, mergeMap, catchError, switchMap } from 'rxjs/operators';
import { CoreService } from 'src/app/core/core.service';
import * as userSpecificDetailsActions from '../actions/user-specific-details.action';

@Injectable()
export class UserSpecificDetailsEffects {
    constructor(
        private actions$: Actions,
        private coreService: CoreService
    ) {}


    getMostRecentSharedWithEmployeeGroups$ = createEffect(() =>
        this.actions$.pipe(
            ofType(userSpecificDetailsActions.UserSpecificDetailsActionTypes.GetMostRecentSharedWithEmployeeGroups),
            switchMap(() =>  
                this.coreService.getMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode().pipe(
                    mergeMap((mostRecentSharedWithEmployeeGroups: any) => {
                        return (
                          [
                            new userSpecificDetailsActions.GetMostRecentSharedWithEmployeeGroupsSuccess(mostRecentSharedWithEmployeeGroups)
                          ]
                        );
    })))));


}