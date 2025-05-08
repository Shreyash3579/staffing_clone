import * as fromRoot from '../../../../app/state/app.state';
import { ResourceCommitment } from 'src/app/shared/interfaces/resourceCommitment';
import { StaffingSupplyActions, StaffingSupplyActionTypes } from '../actions/staffing-supply.action';
import { createFeatureSelector, createSelector } from '@ngrx/store';
import { StaffingDemandActionTypes } from '../actions/staffing-demand.action';
import { CommitmentView } from 'src/app/shared/interfaces/commitmentView';
import { ResourceCommitmentActionTypes } from 'src/app/state/actions/resource-commitment.action';
import { ProjectAllocationsActionTypes } from 'src/app/state/actions/project-allocations.action';
import { ResourceOverlayActionTypes } from 'src/app/state/actions/resource-overlay.action';
import { ProjectOverlayActionTypes } from 'src/app/state/actions/project-overlay.action';
import { ResourceCommitmentActions } from 'src/app/state/actions/resource-commitment.action';
import { BossSearchResult } from 'src/app/shared/interfaces/azureSearchCriteria.interface';
import { PlanningCardOverlayActionTypes } from 'src/app/state/actions/planning-card-overlay.action';
import { DateService } from 'src/app/shared/dateService';

export interface StaffingSupplyState {
    resources: ResourceCommitment;
    searchQueryWithResults: BossSearchResult;
}
  
export interface State extends fromRoot.State {
  staffingResources: StaffingSupplyState;
}

const initialState = {
  resources: null as ResourceCommitment, //set as null as  [] is valid dataset for initial state when no resources as returned by API
  searchQueryWithResults: null as BossSearchResult
};

// Selector Functions
const getSupplyFeatureState = createFeatureSelector<StaffingSupplyState>(
  'staffingResources'
);

export const getStaffingResources = createSelector(
  getSupplyFeatureState,
  (state) => state.resources    
);

export const getSearchQueryWithResults = createSelector(
  getSupplyFeatureState,
  (state) => state.searchQueryWithResults
);

export const getFilteredStaffingResources = createSelector(
  getStaffingResources,
  getSearchQueryWithResults,
  (resourcesState, searchQueryState) => ( {
      availableResources: resourcesState,
      searchQueryWithResults: searchQueryState
  })
);


export function supplyReducer(state = initialState, action: any): StaffingSupplyState {
 
  let resources: ResourceCommitment = JSON.parse(JSON.stringify(state.resources));

    switch (action.type) {
      case StaffingSupplyActionTypes.ClearSupplyState:
        resources = {
          resources: [],
          allocations: [],
          placeholderAllocations: [],
          loAs: [],
          vacations: [],
          trainings: [],
          commitments: [],
          transitions: [],
          transfers: [],
          terminations: [],
          timeOffs: [],
          placeholderAndPlanningCardAllocations: [],
          staffableAsRoles: [],
          resourceViewNotes: [],
          holidays: []
        };
        return {
          ...state,
          resources: resources,
          searchQueryWithResults: null
        };

      case StaffingSupplyActionTypes.LoadResourcesBySelectedGroupSuccess:
        return {
          ...state,
          resources: action.payload,
          searchQueryWithResults: null
        };

      case StaffingSupplyActionTypes.LoadResourcesBySelectedFiltersSuccess:
        return {
          ...state,
          resources: action.payload,
          searchQueryWithResults: null
        };

      case StaffingDemandActionTypes.MergePlanningCardsSuccess:
      case PlanningCardOverlayActionTypes.MergePlanningCardsSuccess:
        resources.allocations = resources.allocations.concat(action.payload.resourceAllocations);
        resources.placeholderAllocations = resources.placeholderAllocations.filter((x) => {
          return !action.payload.planningCard.allocations.some((allocation) => allocation.id === x.id);
        });
        return {
          ...state,
          resources: resources
        };

      case StaffingDemandActionTypes.UpsertResourceAllocationsSuccess:
      case ProjectAllocationsActionTypes.UpsertResourceAllocationsSuccess:
        if (action.payload.supplyUpdatedData.length > 0) {
          action.payload.supplyUpdatedData.forEach((allocation) => {
            resources.allocations = resources.allocations.filter((x) => x.id !== allocation.id);
            resources.allocations.push(allocation);
          });
        }
        return {
          ...state,
          resources: resources
        };

      case StaffingDemandActionTypes.DeletePlanningCardSuccess:
      case PlanningCardOverlayActionTypes.DeletePlanningCardSuccess:
        
        resources.allocations = resources.allocations.filter(x => x.planningCardId != action.payload.planningCardId);
        resources.placeholderAllocations = resources.placeholderAllocations.filter(x => x.planningCardId != action.payload.planningCardId);
        
        return {

          ...state,
          resources: resources
        };

        
      case StaffingDemandActionTypes.DeleteResourceAllocationsSuccess:
      case ProjectAllocationsActionTypes.DeleteResourceAllocationCommitmentSuccess:
        if (action.payload.allocationIds.length > 0) {
          action.payload.allocation.forEach((allocation) => {
            resources.allocations = resources.allocations.filter((x) => x.id !== allocation.id);
          });
        } if (action.payload.commitmentIds.length > 0) {
          action.payload.allocation.forEach((commitment) => {
            resources.commitments = resources.commitments.filter((x) => x.id !== commitment.id);
          });
        }
        return {
          ...state,
          resources: resources
        };

      case StaffingDemandActionTypes.UpsertPlaceholderAllocationsSuccess:
      case ProjectAllocationsActionTypes.UpsertPlaceholderAllocationsSuccess:
        //update/insert both scenarios handled if placeholder allocation exists or if new is added
        if (action.payload.placeholderAllocations.length > 0 && action.payload.supplyUpdatedData.length > 0) {
         
          action.payload.supplyUpdatedData.forEach((allocation) => {
            resources.placeholderAllocations = resources.placeholderAllocations.filter((x) => x.id !== allocation.id);
            resources.placeholderAllocations.push(allocation);
          })
        }
        return {
          ...state,
          resources: resources
        };

      case StaffingDemandActionTypes.UpsertPlanningCardAllocationsSuccess:
      case ProjectAllocationsActionTypes.UpsertPlanningCardAllocationsSuccess:
        //update/insert both scenarios handled if placeholder allocation exists or if new is added
        if (action.payload.placeholderAllocations.length > 0 && action.payload.supplyUpdatedData.length > 0) {
         
          action.payload.supplyUpdatedData.forEach((allocation) => {
            resources.placeholderAllocations = resources.placeholderAllocations.filter((x) => x.id !== allocation.id);
            resources.placeholderAllocations.push(allocation);
          })
        }

        return {
          ...state,
          resources: resources
        };

      case StaffingDemandActionTypes.UpsertPlanningCardSuccess:  
      case PlanningCardOverlayActionTypes.UpsertPlanningCardSuccess:
        if (action.payload.upsertedData) {
          //Todo : replace the whole object once we have api call in aggregator
          resources.placeholderAllocations.forEach((allocation, index) => {
            if (allocation.planningCardId === action.payload.upsertedData.id) {
              const updatedAllocation = action.payload.upsertedData.allocations.find(x => x.id === allocation.id);
              if (updatedAllocation) {
                resources.placeholderAllocations[index] = updatedAllocation;
              }
            }
          });
        }
        return {
          ...state,
          resources: resources
        };

      case StaffingDemandActionTypes.UpdatePegPlanningCardSuccess:
      case PlanningCardOverlayActionTypes.RefreshPlanningCardOverlaySuccess:
        if (action.payload.updatedPegPlanningCardData) {
          //Todo : replace the whole object once we have api call in aggregator
          resources.placeholderAllocations.forEach((allocation, index) => {
            if (allocation.planningCardId === action.payload.updatedPegPlanningCardData.planningCardId) {
              allocation.startDate = DateService.convertDateInBainFormat(action.payload.updatedPegPlanningCardData.startDate);
              allocation.endDate = DateService.convertDateInBainFormat(action.payload.updatedPegPlanningCardData.endDate);
            }
          });
        }
        return {
          ...state,
          resources: resources
        };

      case StaffingDemandActionTypes.DeletePlaceholderAllocationsSuccess:
      case ProjectAllocationsActionTypes.DeletePlaceholderAllocationsSuccess:
        action.payload.placeholderAllocation.forEach((placeholderAllocation) => {
          resources.placeholderAllocations = resources.placeholderAllocations.filter(
            (x) => x.id !== placeholderAllocation.id
          );
        });
        return {
          ...state,
          resources: resources
        };

      case ProjectOverlayActionTypes.UpsertCaseRollAndAllocationsSuccess:
      case ProjectOverlayActionTypes.RevertCaseRollAndAllocationsSuccess:
        action.payload.updatedData.forEach((allocation) => {
          resources.allocations = resources.allocations.filter((x) => x.id !== allocation.id);
          resources.allocations.push(allocation);
        });
        return {
          ...state,
          resources: resources
        };
      
        case ProjectOverlayActionTypes.UpsertCaseRollAndPlaceholderAllocationsSuccess:
          action.payload.updatedData.filter(allocation => allocation.planningCardId != null).forEach((allocation) => {
            resources.placeholderAllocations = resources.placeholderAllocations.filter((x) => x.id !== allocation.id);
            resources.placeholderAllocations.push(allocation);
          });
          action.payload.updatedData.filter(allocation => allocation.oldCaseCode != null).forEach((allocation) => {
            resources.allocations = resources.allocations.filter((x) => x.id !== allocation.id);
            resources.allocations.push(allocation);
          });

          return {
            ...state,
            resources: resources
          };

      case ResourceCommitmentActionTypes.InsertResourceCommitmentSuccess:
        if (action.payload.commitments?.length > 0) {
          resources.commitments = filterNonUpdatedCommitments(resources.commitments, action.payload.commitments[0]);
          const newCommitments: CommitmentView[] = action.payload.commitments.map((commitment) => ({
            id: commitment.id,
            employeeCode: commitment.employeeCode,
            commitmentTypeCode: commitment.commitmentType.commitmentTypeCode,
            commitmentTypeName: commitment.commitmentType.commitmentTypeName,
            commitmentTypeReasonCode: commitment.commitmentTypeReasonCode,
            startDate: commitment.startDate,
            endDate: commitment.endDate,
            allocation: commitment.allocation,
            description: commitment.notes,
            isSourceStaffing: commitment.isSourceStaffing
          }));
          resources.commitments = resources.commitments.concat(newCommitments);
        }
        return {
          ...state,
          resources: resources
        };


      case ResourceOverlayActionTypes.UpsertResourceStaffableAsSuccess:
        resources.staffableAsRoles = resources.staffableAsRoles.filter(
          (x) => x.id !== action.payload.updatedData[0].id
        );
        action.payload.updatedData[0].staffableAsTypeName = action.payload.staffableRoles[0].staffableAsTypeName;

        resources.staffableAsRoles = resources.staffableAsRoles.concat(action.payload.updatedData[0]);
        return {
          ...state,
          resources: resources
        };

      case ResourceOverlayActionTypes.DeleteResourceStaffableAsSuccess:
        resources.staffableAsRoles = resources.staffableAsRoles.filter(
          (x) => x.id !== action.payload.staffableRoleToBeDeleted
        );
        return {
          ...state,
          resources: resources
        };

      case StaffingSupplyActionTypes.UpsertResourceViewNotesSuccess:
        if (!!action.payload.employeeCode) {
          const resourceWhoseNoteToUpsert = resources.resources.find(
            (x) => x.employeeCode === action.payload.employeeCode
          );
          if (resourceWhoseNoteToUpsert) {
            resourceWhoseNoteToUpsert.resourceViewNotes = resourceWhoseNoteToUpsert.resourceViewNotes.filter(
              (x) => x.id && x.id !== action.payload.id
            );
            resourceWhoseNoteToUpsert.resourceViewNotes.unshift(action.payload);
          }
        }
        return {
          ...state,
          resources: resources
        };

      case StaffingSupplyActionTypes.DeleteResourceViewNotesSuccess:
        const resourceWhoseNoteToUpsert = resources.resources.find(
          (x) => x.employeeCode === action.payload.employeeCode
        );
        if (resourceWhoseNoteToUpsert) {
          resourceWhoseNoteToUpsert.resourceViewNotes = resourceWhoseNoteToUpsert.resourceViewNotes.filter(
            (x) => !action.payload.includes(x.id)
          );
        }
        return {
          ...state,
          resources: resources
        };

      case StaffingSupplyActionTypes.GetResourcesBySearchStringSuccess:
        const searchResultsData = action.payload;
        return {
          ...state,
          searchQueryWithResults: {
            generatedLuceneSearchQuery: searchResultsData.generatedLuceneSearchQuery,
            searchResultsEcodes: searchResultsData.searches?.length > 0 ? searchResultsData.searches.map(x => x.document.employeeCode.toUpperCase()) : null,
            searches: searchResultsData.searches
          }
        };

      case StaffingSupplyActionTypes.ClearSearchString:
        return {
          ...state,
          resources : resources,
          searchQueryWithResults: null
        };

      case ResourceCommitmentActionTypes.DeleteCaseOppCommitmentsSuccess:

      const commitmentIds = action.payload.commitmentIds.split(',') .map((id) => id.trim());

         commitmentIds.forEach((commitmentId) => {
         resources.commitments= resources.commitments.filter(
            (x) => x.id !== commitmentId
          );
        });
        return {
          ...state,
          resources: resources
        };
  
      default:
        return state;
    }


}

function filterNonUpdatedCommitments(commitments, updatedCommitment) {
  const existingCommitment = commitments.filter(commitment => 
    !isNullOrUndefined(commitment.id) && commitment.id != updatedCommitment.id)
  return existingCommitment;
}

function isNullOrUndefined(value) {
  return (value === null || value === undefined);
}

