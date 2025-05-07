import * as fromRoot from '../../../state/app.state';
import { createFeatureSelector, createSelector } from '@ngrx/store';
import { EmployeeStaffingInfo } from 'src/app/shared/interfaces/employeeStaffingInfo';
import { ResourceOverlayActionTypes } from '../actions/resource-overlay.actions';


export interface ResourceOverlayState {
    resourcestaffingInfo: EmployeeStaffingInfo;
  }
  
  export interface State extends fromRoot.State {
    resourcesInfo: ResourceOverlayState;
  }
  
  const initialState = {
    resourcestaffingInfo: null as EmployeeStaffingInfo
  };

// Selector Functions
const getSupplyFeatureState = createFeatureSelector<ResourceOverlayState>(
  'overlay'
);

export const getResourceStaffingInfo = createSelector(
  getSupplyFeatureState,
  (state) => state.resourcestaffingInfo
);

export function resourceOverlayReducer(state = initialState, action: any): ResourceOverlayState {

    switch (action.type) {
      case ResourceOverlayActionTypes.ClearResourceOverlayState:
        return {
          ...state,
          resourcestaffingInfo: null
        };
      
      case ResourceOverlayActionTypes.UpsertResourceStaffingInfoSucess:
        return {
          ...state,
        };
      case ResourceOverlayActionTypes.GetResourceStaffingInfoSuccess:
        const employeeStaffingDetails: EmployeeStaffingInfo = action.payload;
        return {
          ...state,
          resourcestaffingInfo: employeeStaffingDetails
        };

      default: 
        return state;
    }
}
