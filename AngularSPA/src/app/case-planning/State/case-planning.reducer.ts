import * as fromRoot from '../../../app/state/app.state';
import { CasePlanningActions, CasePlanningActionTypes } from './case-planning.actions';
import { createSelector, createFeatureSelector } from '@ngrx/store';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';

// State for this feature
export interface CasePlanningState {
  projects: Project[];
  planningCards: PlanningCard[];
  searchedProjects: Project[];
  refreshCaseAndResourceOverlay: boolean;
  casePlanningLoader: boolean;
}

export interface State extends fromRoot.State {
  projects: CasePlanningState;
  planningCards: CasePlanningState;
  searchedProjects: CasePlanningState;
  refreshCaseAndResourceOverlay: boolean;
  casePlanningLoader: boolean;
}

const initialState = {
  projects: [] as Project[],
  planningCards: [] as PlanningCard[],
  searchedProjects: [] as Project[],
  refreshCaseAndResourceOverlay: false,
  casePlanningLoader: false,
};

// Selector Functions
const getProjectsFeatureState = createFeatureSelector<CasePlanningState>(
  'casePlanning'
);

export const getProjects = createSelector(
  getProjectsFeatureState,
  (state) => state.projects
);

export const getPlanningCards = createSelector(
  getProjectsFeatureState,
  (state) => state.planningCards
);

export const getSearchedProjects = createSelector(
  getProjectsFeatureState,
  (state) => state.searchedProjects
);

export const refreshCaseAndResourceOverlay = createSelector(
  getProjectsFeatureState,
  (state) => state.refreshCaseAndResourceOverlay
);

export const casePlanningLoader = createSelector(
  getProjectsFeatureState,
  (state) => state.casePlanningLoader
);


export function reducer(state = initialState, action: CasePlanningActions): CasePlanningState {
  switch (action.type) {
    case CasePlanningActionTypes.CasePlanningLoader:
      return {
        ...state,
        casePlanningLoader: action.payload
      };

    case CasePlanningActionTypes.RefreshCaseAndResourceOverlay:
      return {
        ...state,
        refreshCaseAndResourceOverlay: action.payload
      };

    case CasePlanningActionTypes.LoadProjectsSuccess:
      // Filter out all the opps that do not have either start or end date
      const projectsWithDates = action.payload?.filter(x => x.startDate && x.endDate);
      projectsWithDates.forEach(x => x.trackById = Date.now());
      return {
        ...state,
        projects: projectsWithDates
      };

    case CasePlanningActionTypes.LoadPlanningCardsSuccess:
      // Filter out all the planning cards that do not have either start or end date
      let planningCardsWithDates = action.payload.planningCards?.filter(x => x.startDate && x.endDate);
      
      // filter out the planning cards based on the probability percentage from action.payload.demandfilterCriteria
      if (action.payload.demandFilterCriteriaObj.minOpportunityProbability !== undefined) {
        const minProbabilityPercent = action.payload.demandFilterCriteriaObj.minOpportunityProbability;
        planningCardsWithDates = filterByMinProbabilityPercent(planningCardsWithDates, minProbabilityPercent);
      }

      planningCardsWithDates.forEach(planningCard => {
        planningCard.trackById = Date.now();
        planningCard.placeholderAllocations = planningCard.allocations.filter(x => x.isPlaceholderAllocation);
        planningCard.regularAllocations = planningCard.allocations.filter(x => !x.isPlaceholderAllocation);
      });

      return {
        ...state,
        planningCards: planningCardsWithDates
      };

    case CasePlanningActionTypes.LoadCasesOnPageScrollSuccess:
      const existingProjects: Project[] =
        JSON.parse(JSON.stringify(state.projects));
      action.payload.forEach((project: Project) => {
        project.trackById = Date.now();
        existingProjects.push(project);
      });
      return {
        ...state,
        projects: existingProjects
      };
    case CasePlanningActionTypes.ClearSearchData:
      return {
        ...state,
        searchedProjects: []
      };
    case CasePlanningActionTypes.LoadProjectsBySearchStringSuccess:
      return {
        ...state,
        searchedProjects: action.payload
      };

    case CasePlanningActionTypes.UpsertResourceStaffingSuccess:
      const projectsWithUpsertedAssignments: Project[] =
        JSON.parse(JSON.stringify(state.projects));

      action.payload.forEach(r => {
        const index = projectsWithUpsertedAssignments.findIndex(p =>
          p.allocatedResources.find(x => x.id == r.id));
        if (index > -1) {
          let projectWithUpsertedAssignments = projectsWithUpsertedAssignments.find(p =>
            p.allocatedResources.find(x => x.id == r.id));
          projectWithUpsertedAssignments.allocatedResources.splice(index, 1, r);
          projectWithUpsertedAssignments.trackById = Date.now();
        } else {
          let projectWithUpsertedAssignments = projectsWithUpsertedAssignments.find(p =>
            p.allocatedResources.find(x => x.oldCaseCode == x.oldCaseCode || x.pipelineId == r.pipelineId))
          projectWithUpsertedAssignments.allocatedResources.push(r);
          projectWithUpsertedAssignments.trackById = Date.now();
        }
      });

      return {
        ...state,
        projects: projectsWithUpsertedAssignments
      };

    case CasePlanningActionTypes.DeleteResourceStaffingSuccess:
      let updatedProjectsWithDeletedAssignment: Project[] =
        JSON.parse(JSON.stringify(state.projects));
      const projectWhoseAllocationToDelete = updatedProjectsWithDeletedAssignment.find(x =>
        !!x.allocatedResources.find(y => y.id === action.payload)
      );

      if (projectWhoseAllocationToDelete) {
        updatedProjectsWithDeletedAssignment.map(x => {
          x.allocatedResources = x.allocatedResources.filter(y => y.id !== action.payload);
          x.trackById = Date.now();
          return x;
        });
      }

      return {
        ...state,
        projects: updatedProjectsWithDeletedAssignment
      };

    case CasePlanningActionTypes.UpsertCasePlanningViewNoteSuccess:
      let updatedProjectsWithUpsertedNote: Project[] =
        JSON.parse(JSON.stringify(state.projects));
      let updatedPlanningCardsWithUpsertedNote: PlanningCard[] =
        JSON.parse(JSON.stringify(state.planningCards));
      if (!!action.payload.oldCaseCode) {
        const projectWhoseNoteToUpsert = updatedProjectsWithUpsertedNote.find(x => x.oldCaseCode === action.payload.oldCaseCode);
        const index = projectWhoseNoteToUpsert.casePlanningViewNotes.findIndex(y => y.id === action.payload.id || !y.id);
        if (index > -1) {
          projectWhoseNoteToUpsert.casePlanningViewNotes.splice(index, 1, action.payload);
        }
        else {
          projectWhoseNoteToUpsert.casePlanningViewNotes.unshift(action.payload);
        }
        projectWhoseNoteToUpsert.trackById = Date.now();
      }
      else if (!!action.payload.pipelineId) {
        const projectWhoseNoteToUpsert = updatedProjectsWithUpsertedNote.find(x => x.pipelineId === action.payload.pipelineId);
        const index = projectWhoseNoteToUpsert.casePlanningViewNotes.findIndex(y => y.id === action.payload.id || !y.id);
        if (index > -1) {
          projectWhoseNoteToUpsert.casePlanningViewNotes.splice(index, 1, action.payload);
        }
        else {
          projectWhoseNoteToUpsert.casePlanningViewNotes.unshift(action.payload);
        }
        projectWhoseNoteToUpsert.trackById = Date.now();
      }
      else if (!!action.payload.planningCardId) {
        const projectWhoseNoteToUpsert = updatedPlanningCardsWithUpsertedNote.find(x => x.id === action.payload.planningCardId);
        const index = projectWhoseNoteToUpsert.casePlanningViewNotes.findIndex(y => y.id === action.payload.id || !y.id);
        if (index > -1) {
          projectWhoseNoteToUpsert.casePlanningViewNotes.splice(index, 1, action.payload);
        }
        else {
          projectWhoseNoteToUpsert.casePlanningViewNotes.unshift(action.payload);
        }
        projectWhoseNoteToUpsert.trackById = Date.now();
      }

      return {
        ...state,
        projects: updatedProjectsWithUpsertedNote,
        planningCards: updatedPlanningCardsWithUpsertedNote
      };

    case CasePlanningActionTypes.DeleteCasePlanningViewNotesSuccess:
      let updatedProjectsWithDeletedNote: Project[] =
        JSON.parse(JSON.stringify(state.projects));
      let updatedPlanningCardsWithDeletedNote: PlanningCard[] =
        JSON.parse(JSON.stringify(state.planningCards));

      updatedProjectsWithDeletedNote.forEach(r => {
        r.trackById = Date.now();
        r.casePlanningViewNotes = r.casePlanningViewNotes.filter(x => !action.payload.includes(x.id))
      })

      updatedPlanningCardsWithDeletedNote.forEach(r => {
        r.trackById = Date.now();
        r.casePlanningViewNotes = r.casePlanningViewNotes.filter(x => !action.payload.includes(x.id))
      })

      return {
        ...state,
        projects: updatedProjectsWithDeletedNote,
        planningCards: updatedPlanningCardsWithDeletedNote
      };

    default:
      return state;
  }
}

function filterByMinProbabilityPercent(planningCards, minProbabilityPercent) {
  return planningCards.filter(planningCard => planningCard.probabilityPercent >= minProbabilityPercent);
}

