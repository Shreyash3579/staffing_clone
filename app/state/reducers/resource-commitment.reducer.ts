import * as fromRoot from '../app.state';
import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ResourceCommitmentActionTypes } from '../actions/resource-commitment.action';
import { ProjectAllocationsActionTypes } from '../actions/project-allocations.action';
import { ResourceOverlayActionTypes } from '../actions/resource-overlay.action';


export interface ResourceCommitmentState {
    refreshResources: boolean;
  }
  
  export interface State extends fromRoot.State {
    refreshResources: ResourceCommitmentState;
  }
  
  const initialState = {
    refreshResources: false,
  };

// Selector Functions
const getResourceCommitmentState = createFeatureSelector<ResourceCommitmentState>(
  'resourceCommitment'
);

export const getRefreshResources = createSelector(
  getResourceCommitmentState,
  (state) => state.refreshResources
);

export function resourceCommitmentReducer(state = initialState, action: any): ResourceCommitmentState {
    let refresh;

    switch (action.type) {     
      case ProjectAllocationsActionTypes.UpsertResourceAllocationsSuccess:
      case ProjectAllocationsActionTypes.DeleteResourceAllocationCommitmentSuccess:
      case ProjectAllocationsActionTypes.UpsertPlaceholderAllocationsSuccess:
      case ProjectAllocationsActionTypes.UpsertPlanningCardAllocationsSuccess:
      case ProjectAllocationsActionTypes.DeletePlaceholderAllocationsSuccess:
      case ResourceCommitmentActionTypes.InsertResourceCommitmentSuccess:
      case ResourceOverlayActionTypes.UpsertResourceStaffableAsSuccess:
      case ResourceOverlayActionTypes.DeleteResourceStaffableAsSuccess:

        refresh = true;
        if(action.payload.resourceDialogRef) {
          refreshResourceOverlay(action.payload.resourceDialogRef);
        }

        return {
          ...state,
          refreshResources : refresh
        };

      case ResourceCommitmentActionTypes.ResetRefreshResourcesSuccess:    
       return {
         ...state,
         refreshResources : action.payload
       };

      default: 
        return state;
    }
}

function refreshResourceOverlay(resourceDialogRef: any) {
  if (resourceDialogRef && resourceDialogRef.componentInstance) {
    const employeeCode = resourceDialogRef.componentInstance.data.employeeCode;
    resourceDialogRef.componentInstance.getDetailsForResource(employeeCode);
  }
}
