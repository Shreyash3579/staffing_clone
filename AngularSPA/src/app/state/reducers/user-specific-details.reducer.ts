import { createFeatureSelector, createSelector } from '@ngrx/store';
import  {UserSpecificDetailsActionTypes} from '../actions/user-specific-details.action';
import * as fromRoot from '../app.state';


export interface UserSpecificDetailsState {
    mostRecentSharedWithEmployeeGroups: any[]; 
}

export interface State extends fromRoot.State {
    mostRecentSharedWithEmployeeGroups: UserSpecificDetailsState;
  }

const initialState = {
    mostRecentSharedWithEmployeeGroups: [],
};


const getUserSpecificFeatureState = createFeatureSelector<UserSpecificDetailsState>(
    'userSpecificDetails'
  );



export const getMostRecentSharedWithEmployeeGroups = createSelector(
    getUserSpecificFeatureState,
  (state) => state.mostRecentSharedWithEmployeeGroups
);

export function userSpecificDetailsReducer(state = initialState, action: any): UserSpecificDetailsState {

    switch (action.type) {
       case UserSpecificDetailsActionTypes.GetMostRecentSharedWithEmployeeGroupsSuccess:
            return {
              ...state,
              mostRecentSharedWithEmployeeGroups: action.payload 
            };

      default: 
        return state;
    }
}
