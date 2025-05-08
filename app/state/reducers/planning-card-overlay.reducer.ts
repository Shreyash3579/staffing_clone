import * as fromRoot from '../app.state';
import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ProjectAllocationsActionTypes } from '../actions/project-allocations.action';
import { ProjectOverlayActionTypes } from '../actions/project-overlay.action';
import { PlanningCardOverlayActionTypes } from '../actions/planning-card-overlay.action';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';


export interface PlanningCardOverlayState {
    refreshPlanningCard: boolean;
    newlyCreatedPlanningCard: PlanningCard;
    planningCardsId: string[];
  }
  
  export interface State extends fromRoot.State {
    refreshPlanningCard: PlanningCardOverlayState;
    newlyCreatedPlanningCard: PlanningCard;
    planningCardsId: string[];
  }
  
  const initialState = {
    refreshPlanningCard: false,
    newlyCreatedPlanningCard: null as PlanningCard,
    planningCardsId: null as string[]
  };

// Selector Functions
const getProjectAllocationsState = createFeatureSelector<PlanningCardOverlayState>(
  'planningCardOverlay'
);

export const getNewlyCreatedPlanningCard = createSelector(
  getProjectAllocationsState,
  (state) => state.newlyCreatedPlanningCard
);


export const getRefreshCasesAndOpoortunities = createSelector(
  getProjectAllocationsState,
    (state) => state.refreshPlanningCard
)
export const getStaffingPlanningCardsId = createSelector(
  getProjectAllocationsState,
  (state) => state.planningCardsId
);

export function planningCardOverlayReducer(state = initialState, action: any): PlanningCardOverlayState {
    let refresh;
    let newlyCreatedPlanningCard = null;
    let planningCards: PlanningCard[] = JSON.parse(JSON.stringify(state.planningCardsId));

    switch (action.type) {
      case PlanningCardOverlayActionTypes.MergePlanningCardsSuccess:
      case PlanningCardOverlayActionTypes.RefreshPlanningCardOverlaySuccess:
      case PlanningCardOverlayActionTypes.DeletePlanningCardSuccess:
      
        refresh = true;
        if(action.payload.planningCardDialogRef) {
          refreshPlanningCardOverlays(action.payload.planningCardDialogRef);
        }

        return {
          ...state,
          refreshPlanningCard : refresh
        };

      case PlanningCardOverlayActionTypes.UpsertPlanningCardSuccess:
        refresh = true;
        if(action.payload.planningCardDialogRef) {
          refreshPlanningCardOverlays(action.payload.planningCardDialogRef);
        }


        newlyCreatedPlanningCard = action.payload.upsertedData;
        return {
          ...state,
          refreshPlanningCard : refresh,
          newlyCreatedPlanningCard: newlyCreatedPlanningCard
        };

        case PlanningCardOverlayActionTypes.LoadPlanningCardsBySelectedFiltersSuccess:
          planningCards = action.payload.planningCards;
            
          if (!action.payload.demandFilterCriteria?.demandTypes?.includes('PlanningCards')) {
            planningCards = [];
          } 
          else if (action.payload.demandFilterCriteria.minOpportunityProbability !== undefined) {
            const minProbabilityPercent = action.payload.demandFilterCriteria.minOpportunityProbability;
            planningCards = filterByMinProbabilityPercent(planningCards, minProbabilityPercent); 
          }
          let planningCardsId = planningCards.map((planningCard: PlanningCard) => planningCard.id); 
          
          return {
            ...state,
            planningCardsId: planningCardsId
          };


      default: 
        return state;
    }
}

function filterByMinProbabilityPercent(planningCards, minProbabilityPercent) {
  return planningCards.filter(planningCard => planningCard.probabilityPercent >= minProbabilityPercent);
}

function refreshPlanningCardOverlays(planningCardDialogRef: any) {
  if (planningCardDialogRef && planningCardDialogRef.componentInstance) {
    const planningCardId = planningCardDialogRef.componentInstance.planningCard.id;
    planningCardDialogRef.componentInstance.getPlanningCardDetails(planningCardId);
  }
}
