import * as fromRoot from '../app.state';
import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ProjectAllocationsActionTypes } from '../actions/project-allocations.action';
import { ProjectOverlayActionTypes } from '../actions/project-overlay.action';
import { ResourceCommitmentActionTypes } from '../actions/resource-commitment.action';
import { Project } from 'src/app/shared/interfaces/project.interface';

export interface ProjectAllocationsState {
    refreshCasesAndOpportunities: boolean;
    projectIdentifiers: string[];
    historicalProjectIdentifiers: string[];
  }
  
  export interface State extends fromRoot.State {
    refreshCasesAndOpportunities: ProjectAllocationsState;
    projectIdentifiers: string[];
    historicalProjectIdentifiers: string[];
  }
  
  const initialState = {
    refreshCasesAndOpportunities: false,
    projectIdentifiers: null as string[],
    historicalProjectIdentifiers: null as string[],
  };

// Selector Functions
const getProjectAllocationsState = createFeatureSelector<ProjectAllocationsState>(
  'projectAllocation'
);


export const getRefreshCasesAndOpoortunities = createSelector(
  getProjectAllocationsState,
    (state) => state.refreshCasesAndOpportunities
)
export const getStaffingProjectsOldCaseCodes= createSelector(
  getProjectAllocationsState,
  (state) => state.projectIdentifiers
);

export const getStaffingHistoricalProjectsOldCaseCodes = createSelector(
  getProjectAllocationsState,
  (state) => state.historicalProjectIdentifiers
);


export function projectAllocationsReducer(state = initialState, action: any): ProjectAllocationsState {
    let refresh;
    let projectsToDisplay: Project[] = JSON.parse(JSON.stringify(state.projectIdentifiers));
    let historicalProjectsToDisplay: Project[] = JSON.parse(JSON.stringify(state.historicalProjectIdentifiers));
    

    switch (action.type) {
      case ProjectAllocationsActionTypes.UpsertResourceAllocationsSuccess:
      case ProjectAllocationsActionTypes.UpsertPlaceholderAllocationsSuccess:
      case ProjectAllocationsActionTypes.UpsertPlanningCardAllocationsSuccess:
      case ProjectAllocationsActionTypes.DeleteResourceAllocationCommitmentSuccess:
      case ProjectAllocationsActionTypes.DeletePlaceholderAllocationsSuccess:
      case ProjectOverlayActionTypes.UpdateProjectSuccess:
      case ProjectOverlayActionTypes.UpsertCaseRollAndAllocationsSuccess:
      case ProjectOverlayActionTypes.UpsertCaseRollAndPlaceholderAllocationsSuccess:
      case ProjectOverlayActionTypes.RevertCaseRollAndAllocationsSuccess:
      case ProjectOverlayActionTypes.InsertSKUCaseTermsSuccess:
      case ProjectOverlayActionTypes.DeleteSKUCaseTermsSuccess:
      case ProjectOverlayActionTypes.UpdateSKUCaseTermsSuccess:
      case ResourceCommitmentActionTypes.DeleteCaseOppCommitmentsSuccess:
      case ResourceCommitmentActionTypes.InsertCaseOppCommitmentsSuccess:
      case ResourceCommitmentActionTypes.InsertResourceCommitmentSuccess:

        refresh = true;
        if(action.payload.projectDialogRef) {
          refreshOverlays(action.payload.projectDialogRef);
        }
        if(action.payload.planningCardDialogRef){
          refreshPlanningCardOverlays(action.payload.planningCardDialogRef);
        }

        return {
          ...state,
          refreshCasesAndOpportunities : refresh
        };
      
      case ProjectAllocationsActionTypes.ResetRefreshCasesAndOpportunitiesSuccess:    
       return {
         ...state,
         refreshCasesAndOpportunities : action.payload
       }; 

       case ProjectAllocationsActionTypes.LoadProjectsBySelectedFiltersSuccess:
        let projectsData = action.payload;
        projectsToDisplay = RestructureProjectsInDisplayFormat(projectsData.projects, projectsData.pinnedProjects, projectsData.hiddenProjects);
        let projectsOldCaseCodes = projectsToDisplay.map((project: Project) => project.oldCaseCode || project.pipelineId); 
        return {
          ...state,
          projectIdentifiers : projectsOldCaseCodes
        };

      case ProjectAllocationsActionTypes.LoadHistoricalProjectsBySelectedFiltersSuccess:
        let historicalProjectsData = action.payload;
        historicalProjectsToDisplay = RestructureProjectsInDisplayFormat(historicalProjectsData.projects, historicalProjectsData.pinnedProjects, historicalProjectsData.hiddenProjects);
        let historicalProjectOldCaseCodes = historicalProjectsToDisplay.map((project: Project) => project.oldCaseCode || project.pipelineId); 
        return {
          ...state,
          historicalProjectIdentifiers : historicalProjectOldCaseCodes
        };

      default: 
        return state;
    }
}

function refreshOverlays(projectDialogRef: any) {
  if (projectDialogRef && projectDialogRef.componentInstance) {
    const projectData = projectDialogRef.componentInstance.project.projectDetails;
    projectDialogRef.componentInstance.getProjectDetails(projectData);
  }

}

function refreshPlanningCardOverlays(planningCardDialogRef: any) {
  if (planningCardDialogRef && planningCardDialogRef.componentInstance) {
    const planningCardId = planningCardDialogRef.componentInstance.planningCard.id;
    planningCardDialogRef.componentInstance.getPlanningCardDetails(planningCardId);
  }
}

function RestructureProjectsInDisplayFormat(projects, pinnedProjects, hiddenProjects) {
  let updatedProjects = Object.assign([], projects);

  pinnedProjects?.forEach(pinnedProject => {
    let existingProject = findProjectToUpdate(pinnedProject, updatedProjects, null);
    if (existingProject) {
      existingProject.isProjectPinned = true;
    } else {
      pinnedProject.isProjectPinned = true;
      updatedProjects.push(pinnedProject);
    }
  });

  hiddenProjects?.forEach(hiddenProject => {
    if(hiddenProject.pipelineId) {
      updatedProjects = updatedProjects.filter(x => x.pipelineId !== hiddenProject.pipelineId);
    } else if(hiddenProject.oldCaseCode) {
      updatedProjects = updatedProjects.filter(x => x.oldCaseCode !== hiddenProject.oldCaseCode);
    }
  });

  return updatedProjects;
}


  function findProjectToUpdate(updatedData, projects, planningCards) {
      // find the project that the placeholder allocation belongs to
      const projectToUpdate = updatedData?.oldCaseCode
      ? projects?.find(x => x.oldCaseCode === updatedData.oldCaseCode)
      : (updatedData?.pipelineId 
          ? projects?.find(x => x.pipelineId === updatedData.pipelineId)
          : planningCards?.find(x => x.id === updatedData.planningCardId));
      return projectToUpdate;
  }
