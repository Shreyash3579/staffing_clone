import * as fromRoot from '../../../../app/state/app.state';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { StaffingDemandActions, StaffingDemandActionTypes } from '../actions/staffing-demand.action';
import { createFeatureSelector, createSelector } from '@ngrx/store';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';
import { DateService } from 'src/app/shared/dateService';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { ProjectAllocationsActionTypes } from 'src/app/state/actions/project-allocations.action';
import { ProjectOverlayActionTypes } from 'src/app/state/actions/project-overlay.action';
import { PlanningCardOverlayActionTypes } from 'src/app/state/actions/planning-card-overlay.action';
import { ResourceCommitmentActionTypes } from 'src/app/state/actions/resource-commitment.action';


export interface StaffingDemandState {
    projectsToDisplay: Project[];
    projects: Project[];
    pinnedProjects: Project[];
    hiddenProjects: Project[];
    historicalProjectsToDisplay: Project[];
    historicalProjects: Project[];
    pinnedHistoricalProjects: Project[];
    hiddenHistoricalProjects: Project[];
    planningCards: PlanningCard[];
  }
  
  export interface State extends fromRoot.State {
    projectsToDisplay: Project[];
    projects: Project[];
    pinnedProjects: Project[];
    hiddenProjects: Project[];
    historicalProjectsToDisplay: Project[];
    historicalProjects: Project[];
    pinnedHistoricalProjects: Project[];
    hiddenHistoricalProjects: Project[];
    planningCards: PlanningCard[];
  }
  
  const initialState = {
    projectsToDisplay: null as Project[],
    projects: null as Project[],
    pinnedProjects: null as Project[],
    hiddenProjects: null as Project[],
    historicalProjectsToDisplay: null as Project[],
    historicalProjects: null as Project[],
    pinnedHistoricalProjects: null as Project[],
    hiddenHistoricalProjects: null as Project[],
    planningCards: null as PlanningCard[]
  };


// Selector Functions
const getDemandFeatureState = createFeatureSelector<StaffingDemandState>(
  'projects'
);

export const getStaffingProjects = createSelector(
  getDemandFeatureState,
  (state) => state.projectsToDisplay
);

export const getStaffingHistoricalProjects = createSelector(
  getDemandFeatureState,
  (state) => state.historicalProjectsToDisplay
);

export const getStaffingPlanningCards = createSelector(
  getDemandFeatureState,
  (state) => state.planningCards
);

export function demandReducer(state = initialState, action: any): StaffingDemandState {
  
  let projectsToDisplay: Project[] = JSON.parse(JSON.stringify(state.projectsToDisplay));
  let projects: Project[] = JSON.parse(JSON.stringify(state.projects));
  let pinnedProjects: Project[] = JSON.parse(JSON.stringify(state.pinnedProjects));
  let hiddenProjects: Project[] = JSON.parse(JSON.stringify(state.hiddenProjects));
  let historicalProjectsToDisplay: Project[] = JSON.parse(JSON.stringify(state.historicalProjectsToDisplay));
  let historicalProjects: Project[] = JSON.parse(JSON.stringify(state.historicalProjects));
  let pinnedHistoricalProjects: Project[] = JSON.parse(JSON.stringify(state.pinnedHistoricalProjects));
  let hiddenHistoricalProjects: Project[] = JSON.parse(JSON.stringify(state.hiddenHistoricalProjects));
  let planningCards: PlanningCard[] = JSON.parse(JSON.stringify(state.planningCards));
  let projectToUpdate = null;
  let historicalProjectToUpdate = null;

    switch (action.type) {
      case StaffingDemandActionTypes.ClearDemandState:
        return {
          ...state,
          projectsToDisplay: [],
          historicalProjectsToDisplay: [],
          planningCards: []
        };

      case StaffingDemandActionTypes.LoadProjectsBySelectedFiltersSuccess:
        let projectsData = action.payload;
        projectsToDisplay = RestructureProjectsInDisplayFormat(projectsData.projects, projectsData.pinnedProjects, projectsData.hiddenProjects);
        return {
          ...state,
          projects: projectsData.projects,
          pinnedProjects: projectsData.pinnedProjects,
          hiddenProjects: projectsData.hiddenProjects,
          projectsToDisplay : projectsToDisplay
        };

      case StaffingDemandActionTypes.LoadHistoricalProjectsBySelectedFiltersSuccess:
        let historicalProjectsData = action.payload;
        historicalProjectsToDisplay = RestructureProjectsInDisplayFormat(historicalProjectsData.projects, historicalProjectsData.pinnedProjects, historicalProjectsData.hiddenProjects);
        return {
          ...state,
          historicalProjects: historicalProjectsData.projects,
          pinnedHistoricalProjects: historicalProjectsData.pinnedProjects,
          hiddenHistoricalProjects: historicalProjectsData.hiddenProjects,
          historicalProjectsToDisplay : historicalProjectsToDisplay
        };

        case StaffingDemandActionTypes.LoadPlanningCardsBySelectedFiltersSuccess:
          planningCards = action.payload.planningCards;
          
          if (!action.payload.demandFilterCriteria?.demandTypes?.includes('PlanningCards')) {
            planningCards = [];
          } 
            else if (action.payload.demandFilterCriteria.minOpportunityProbability !== undefined) {
              const minProbabilityPercent = action.payload.demandFilterCriteria.minOpportunityProbability;
              planningCards = filterByMinProbabilityPercent(planningCards, minProbabilityPercent); 
          }
        
          return {
            ...state,
            planningCards: planningCards
          };

        case StaffingDemandActionTypes.UpsertResourceAllocationsSuccess:
        case ProjectAllocationsActionTypes.UpsertResourceAllocationsSuccess:
          upsertResourceAllocations(action.payload.demandUpdatedData, projectsToDisplay, planningCards);
          upsertResourceAllocations(action.payload.demandUpdatedData, historicalProjectsToDisplay, null);
          return {
            ...state,
            projectsToDisplay: projectsToDisplay,
            historicalProjectsToDisplay: historicalProjectsToDisplay
          };

        case StaffingDemandActionTypes.DeleteResourceAllocationsSuccess:
        case ProjectAllocationsActionTypes.DeleteResourceAllocationCommitmentSuccess:
            projectToUpdate = findProjectToUpdate(action.payload.allocation[0], projectsToDisplay, planningCards);
            historicalProjectToUpdate = findProjectToUpdate(action.payload.allocation[0], historicalProjectsToDisplay, null);
            if(projectToUpdate) {
              action.payload.allocation.forEach(allocation => {
                projectToUpdate.allocatedResources = projectToUpdate.allocatedResources.filter(x => x.id != allocation.id);
              })
            }
            if(historicalProjectToUpdate) {
              action.payload.allocation.forEach(allocation => {
                historicalProjectToUpdate.allocatedResources = historicalProjectToUpdate.allocatedResources.filter(x => x.id != allocation.id);
              })
            }
          return {
            ...state,
            projectsToDisplay: projectsToDisplay,
            historicalProjectsToDisplay: historicalProjectsToDisplay
          };

        case StaffingDemandActionTypes.UpsertPlaceholderAllocationsSuccess:
        case ProjectAllocationsActionTypes.UpsertPlaceholderAllocationsSuccess:
          upsertPlaceholderAllocations(action.payload.demandUpdatedData, projectsToDisplay);
          upsertPlaceholderAllocations(action.payload.demandUpdatedData, historicalProjectsToDisplay)
          return {
            ...state,
            projectsToDisplay: projectsToDisplay,
            historicalProjectsToDisplay: historicalProjectsToDisplay
          };

        case StaffingDemandActionTypes.UpsertPlanningCardAllocationsSuccess:
        case ProjectAllocationsActionTypes.UpsertPlanningCardAllocationsSuccess: 
          UpsertPlanningCardAllocations(action.payload.demandUpdatedData, planningCards)
          return {
            ...state,
            planningCards: planningCards
          }

        case StaffingDemandActionTypes.DeletePlaceholderAllocationsSuccess:
        case ProjectAllocationsActionTypes.DeletePlaceholderAllocationsSuccess:
          projectToUpdate = findProjectToUpdate(action.payload.placeholderAllocation[0], projectsToDisplay, planningCards);
          historicalProjectToUpdate = findProjectToUpdate(action.payload.placeholderAllocation[0], historicalProjectsToDisplay, null);

            if(projectToUpdate) {
              action.payload.placeholderAllocation.forEach(placeholder => {
                projectToUpdate.placeholderAllocations = projectToUpdate.placeholderAllocations.filter(x => x.id != placeholder.id);
                projectToUpdate.allocations = projectToUpdate.allocations?.filter(x => x.id != placeholder.id);
                projectToUpdate.allocatedResources = projectToUpdate.allocatedResources?.filter(x => x.id != placeholder.id);
              });
            }
            if(historicalProjectToUpdate) {
              action.payload.placeholderAllocation.forEach(placeholder => {
                historicalProjectToUpdate.placeholderAllocations = historicalProjectToUpdate.placeholderAllocations.filter(x => x.id != placeholder.id);
                historicalProjectToUpdate.allocations = historicalProjectToUpdate.allocations?.filter(x => x.id != placeholder.id);
                historicalProjectToUpdate.allocatedResources = historicalProjectToUpdate.allocatedResources?.filter(x => x.id != placeholder.id);
              });
            }
          return {
            ...state,
            projectsToDisplay: projectsToDisplay,
            historicalProjectsToDisplay: historicalProjectsToDisplay,
            planningCards: planningCards
          };

        case ProjectOverlayActionTypes.UpdateProjectSuccess:
          let updatedData = action.payload.updatedData
          projectToUpdate = findProjectToUpdate(updatedData, projectsToDisplay, null);
          historicalProjectToUpdate = findProjectToUpdate(updatedData, historicalProjectsToDisplay, null);
          if (projectToUpdate) {
            projectToUpdate.caseServedByRingfence = updatedData.caseServedByRingfence;
            projectToUpdate.startDate = DateService.convertDateInBainFormat(updatedData.startDate);
            projectToUpdate.endDate = DateService.convertDateInBainFormat(updatedData.endDate);
            projectToUpdate.probabilityPercent = updatedData.probabilityPercent;
          }
          if (historicalProjectToUpdate) {
            historicalProjectToUpdate.caseServedByRingfence = updatedData.caseServedByRingfence;
            historicalProjectToUpdate.startDate = DateService.convertDateInBainFormat(updatedData.startDate);
            historicalProjectToUpdate.endDate = DateService.convertDateInBainFormat(updatedData.endDate);
            historicalProjectToUpdate.probabilityPercent = updatedData.probabilityPercent;
          }
          return {
            ...state,
            projectsToDisplay: projectsToDisplay,
            historicalProjectsToDisplay: historicalProjectsToDisplay
          };

        case ProjectOverlayActionTypes.UpsertCaseRollAndAllocationsSuccess:
          UpsertCaseRollAndAllocations(action.payload, projectsToDisplay);
          UpsertCaseRollAndAllocations(action.payload, historicalProjectsToDisplay);
          return {
            ...state,
            projectsToDisplay: projectsToDisplay,
            historicalProjectsToDisplay: historicalProjectsToDisplay
          };
          case ProjectOverlayActionTypes.UpsertCaseRollAndPlaceholderAllocationsSuccess:
            UpsertCaseRollAndPlaceholderAllocations(action.payload,projectsToDisplay, planningCards);
            UpsertCaseRollAndPlaceholderAllocations(action.payload, historicalProjectsToDisplay,planningCards);
            return {
              ...state,
              planningCards: planningCards,
              projectsToDisplay: projectsToDisplay,
              historicalProjectsToDisplay: historicalProjectsToDisplay
            };

        case ProjectOverlayActionTypes.RevertCaseRollAndAllocationsSuccess:
          projectToUpdate = findProjectToUpdate(action.payload.project, projectsToDisplay, null);
          historicalProjectToUpdate = findProjectToUpdate(action.payload.project, historicalProjectsToDisplay, null);
          if(projectToUpdate) {
            projectToUpdate.caseRoll = null;
            action.payload.updatedData.forEach(resourceAllocation => {
              projectToUpdate.allocatedResources = projectToUpdate.allocatedResources.filter(x => x.id != resourceAllocation.id);
              projectToUpdate.allocatedResources.push(resourceAllocation);
            });
          }
          if(historicalProjectToUpdate) {
            historicalProjectToUpdate.caseRoll = null;
            action.payload.updatedData.forEach(resourceAllocation => {
              historicalProjectToUpdate.allocatedResources = historicalProjectToUpdate.allocatedResources.filter(x => x.id != resourceAllocation.id);
              historicalProjectToUpdate.allocatedResources.push(resourceAllocation);
            });
          }
          return {
            ...state,
            projectsToDisplay: projectsToDisplay,
            historicalProjectsToDisplay: historicalProjectsToDisplay
          };
        
          case ResourceCommitmentActionTypes.InsertCaseOppCommitmentsSuccess: {
            const commitment = action.payload?.commitments[0];
          
            const projectIndex = projectsToDisplay.findIndex(
              project =>
                (project.oldCaseCode && project.oldCaseCode === commitment.oldCaseCode) ||
                (project.pipelineId && project.pipelineId === commitment.opportunityId)
            );
          
            if (projectIndex !== -1) {
              projectsToDisplay[projectIndex].isSTACommitmentCreated = true;
            }
          
            const historicalProjectIndex = historicalProjectsToDisplay.findIndex(
              project =>
                (project.oldCaseCode && project.oldCaseCode === commitment.oldCaseCode) ||
                (project.pipelineId && project.pipelineId === commitment.opportunityId)
            );
          
            if (historicalProjectIndex !== -1) {
              historicalProjectsToDisplay[historicalProjectIndex].isSTACommitmentCreated = true;
            }
          
            const planningCardIndex = planningCards.findIndex(
              x => x.id && x.id === commitment.planningCardId
            );
          
            if (planningCardIndex !== -1) {
              planningCards[planningCardIndex].isSTACommitmentCreated = true;
            }
          
            return {
              ...state,
              planningCards: planningCards,
              projectsToDisplay: projectsToDisplay,
              historicalProjectsToDisplay: historicalProjectsToDisplay
            };
          }
          

        case ResourceCommitmentActionTypes.DeleteCaseOppCommitmentsSuccess: {


            const projectIndex = projectsToDisplay.findIndex(
              project => (project.oldCaseCode && project.oldCaseCode === action.payload.oldCaseCode) ||
              (project.pipelineId && project.pipelineId === action.payload.opportunityId)
            );
            if (projectIndex !== -1) {
              projectsToDisplay[projectIndex].isSTACommitmentCreated = false;
            };

            const historicalProjectIndex = historicalProjectsToDisplay.findIndex(
              project =>      (project.oldCaseCode && project.oldCaseCode === action.payload.oldCaseCode) ||
              (project.pipelineId && project.pipelineId === action.payload.opportunityId)
            );
            if (historicalProjectIndex !== -1) {
              historicalProjectsToDisplay[historicalProjectIndex].isSTACommitmentCreated = false;
            }
          
            const planningCardIndex = planningCards.findIndex(
              x => x.id && x.id === action.payload.planningCardId
            );
            if (planningCardIndex !== -1) {
              planningCards[planningCardIndex].isSTACommitmentCreated = false;
            }
          
            return {
              ...state,
              planningCards: planningCards,
              projectsToDisplay: projectsToDisplay,
              historicalProjectsToDisplay: historicalProjectsToDisplay
            };


        }
          



        case ProjectOverlayActionTypes.InsertSKUCaseTermsSuccess:
          projectToUpdate = findProjectToUpdate(action.payload.skuTab, projectsToDisplay, null);
          historicalProjectToUpdate = findProjectToUpdate(action.payload.skuTab, historicalProjectsToDisplay, null);
          if(projectToUpdate) {
            projectToUpdate.skuCaseTerms = action.payload.updatedData;
            projectToUpdate.skuCaseTerms.skuTerms = action.payload.skuTerms;
          }
          if(historicalProjectToUpdate) {
            historicalProjectToUpdate.skuCaseTerms = action.payload.updatedData;
            historicalProjectToUpdate.skuCaseTerms.skuTerms = action.payload.skuTerms;
          }
          return {
            ...state,
            projectsToDisplay: projectsToDisplay,
            historicalProjectsToDisplay: historicalProjectsToDisplay
          };

        case ProjectOverlayActionTypes.DeleteSKUCaseTermsSuccess:
          projectToUpdate = findProjectToUpdate(action.payload.skuTab, projectsToDisplay, null);
          historicalProjectToUpdate = findProjectToUpdate(action.payload.skuTab, historicalProjectsToDisplay, null);
          if(projectToUpdate && projectToUpdate.skuCaseTerms?.id == action.payload.skuTab.id) {
            projectToUpdate.skuCaseTerms = null;
          }
          if(historicalProjectToUpdate && historicalProjectToUpdate.skuCaseTerms?.id == action.payload.skuTab.id) {
            historicalProjectToUpdate.skuCaseTerms = null;
          }
          return {
            ...state,
            projectsToDisplay: projectsToDisplay,
            historicalProjectsToDisplay: historicalProjectsToDisplay
          };

        case ProjectOverlayActionTypes.GetSKUCaseTermsSuccess:
          if(action.payload.responseData.length > 0) {
            projectToUpdate = findProjectToUpdate(action.payload.skuTab, projectsToDisplay, null);
            historicalProjectToUpdate = findProjectToUpdate(action.payload.skuTab, historicalProjectsToDisplay, null);
            if(projectToUpdate && projectToUpdate.skuCaseTerms == null) {
              projectToUpdate.skuCaseTerms = action.payload.responseData[0];
              projectToUpdate.skuCaseTerms.skuTerms = action.payload.masterSkuTerms.filter(term => {
                return projectToUpdate.skuCaseTerms.skuTermsCodes.split(',').indexOf(term.code.toString()) > -1 ;
              });
            }
            if(historicalProjectToUpdate && historicalProjectToUpdate.skuCaseTerms == null) {
              historicalProjectToUpdate.skuCaseTerms = action.payload.responseData[0];
              historicalProjectToUpdate.skuCaseTerms.skuTerms = action.payload.masterSkuTerms.filter(term => {
                return historicalProjectToUpdate.skuCaseTerms.skuTermsCodes.split(',').indexOf(term.code.toString()) > -1 ;
              });
            }
          }         
          return {
            ...state,
            projectsToDisplay: projectsToDisplay,
            historicalProjectsToDisplay: historicalProjectsToDisplay
          };

        case ProjectOverlayActionTypes.UpdateSKUCaseTermsSuccess:
          projectToUpdate = findProjectToUpdate(action.payload.skuTab, projectsToDisplay, null);
          historicalProjectToUpdate = findProjectToUpdate(action.payload.skuTab, historicalProjectsToDisplay, null);
          if(projectToUpdate) {
            if(action.payload.skuTerms?.length == 0) {
              projectToUpdate.skuCaseTerms = null;
            } else {
              projectToUpdate.skuCaseTerms = action.payload.updatedData;
              projectToUpdate.skuCaseTerms.skuTerms = action.payload.skuTerms;
            }
          }
          if(historicalProjectToUpdate) {
            if(action.payload.skuTerms?.length == 0) {
              historicalProjectToUpdate.skuCaseTerms = null;
            } else {
              historicalProjectToUpdate.skuCaseTerms = action.payload.updatedData;
              historicalProjectToUpdate.skuCaseTerms.skuTerms = action.payload.skuTerms;
            }
          }
          return {
            ...state,
            projectsToDisplay: projectsToDisplay,
            historicalProjectsToDisplay: historicalProjectsToDisplay
          };

        case ProjectOverlayActionTypes.UpdateUserPreferencesForPinSuccess:
          projectsToDisplay = UpdateUserPreferencesForPin(action.payload, pinnedProjects, projects, hiddenProjects);
          historicalProjectsToDisplay = UpdateUserPreferencesForPin(action.payload, pinnedHistoricalProjects, historicalProjects, hiddenHistoricalProjects);
          return {
            ...state,
            projectsToDisplay: projectsToDisplay,
            pinnedProjects: pinnedProjects,
            historicalProjectsToDisplay: historicalProjectsToDisplay,
            pinnedHistoricalProjects: pinnedHistoricalProjects
          };

        case ProjectOverlayActionTypes.UpdateUserPreferencesForHideSuccess:
          projectsToDisplay = UpdateUserPreferencesForHide(action.payload, hiddenProjects, projects, pinnedProjects);
          historicalProjectsToDisplay = UpdateUserPreferencesForHide(action.payload, hiddenHistoricalProjects, historicalProjects, pinnedHistoricalProjects);
          return {
            ...state,
            projectsToDisplay: projectsToDisplay,
            hiddenProjects: hiddenProjects,
            historicalProjectsToDisplay: historicalProjectsToDisplay,
            hiddenHistoricalProjects: hiddenHistoricalProjects
          };

        case StaffingDemandActionTypes.MergePlanningCardsSuccess:
        case PlanningCardOverlayActionTypes.MergePlanningCardsSuccess:
          MergePlanningCards(action.payload, planningCards, projectsToDisplay);
          MergePlanningCards(action.payload, planningCards, historicalProjectsToDisplay);

          return {
            ...state,
            projectsToDisplay: projectsToDisplay,
            historicalProjectsToDisplay: historicalProjectsToDisplay,
            planningCards: planningCards
          }

          case StaffingDemandActionTypes.UpsertPlanningCardSuccess:
          case PlanningCardOverlayActionTypes.UpsertPlanningCardSuccess:
            if (action.payload.demandFilterCriteria.demandTypes.includes("PlanningCards")) {
                let existingPlanningCardIndex = planningCards.findIndex((x) => x.id === action.payload.upsertedData?.id);
             
                // update
                if (existingPlanningCardIndex !== -1) {
                  planningCards[existingPlanningCardIndex] = action.payload.upsertedData;
                  //ToDo : Discuss and move this to coreservice
                  if (action.payload.upsertedData.isShared != null) {
                    planningCards = updateHomeScreenForSharedPlanningCard(
                      planningCards[existingPlanningCardIndex],
                      planningCards,
                      action.payload.demandFilterCriteria
                    );
                  }
                  // call the method to filter based on percentage
                  if (action.payload.demandFilterCriteria.minOpportunityProbability !== undefined) {
                    const minProbabilityPercent = action.payload.demandFilterCriteria.minOpportunityProbability;
                    planningCards = filterByMinProbabilityPercent(planningCards, minProbabilityPercent);
                  }
                }
                //insert
                else {
                  planningCards?.unshift(action.payload.upsertedData);
                }
              }
              return {
                ...state,
                planningCards: planningCards
              };
          

        case StaffingDemandActionTypes.DeletePlanningCardSuccess:
        case PlanningCardOverlayActionTypes.DeletePlanningCardSuccess:
        
          let updatedplanningCardList = updateDemandSideOnPlanningCardDelete(planningCards, action.payload.planningCardId);
          let updatedProjects = updateProjectCaseRoll(projectsToDisplay, action.payload.planningCardId);
          let updatedHistoricalProjects = updateProjectCaseRoll(historicalProjectsToDisplay, action.payload.planningCardId);
            return {
              ...state,
              planningCards: updatedplanningCardList,
              projectsToDisplay:updatedProjects,
              historicalProjectsToDisplay:updatedHistoricalProjects
            };

        
        case StaffingDemandActionTypes.UpsertCaseViewNotesSuccess:
          projectToUpdate = findProjectToUpdate(action.payload, projectsToDisplay, planningCards);
          historicalProjectToUpdate = findProjectToUpdate(action.payload, historicalProjectsToDisplay, null);
          if(projectToUpdate) {
            projectToUpdate.casePlanningViewNotes = projectToUpdate.casePlanningViewNotes.filter(x => x.id && x.id !== action.payload.id);
            projectToUpdate.casePlanningViewNotes.unshift(action.payload);
          }
          if(historicalProjectToUpdate) {
            historicalProjectToUpdate.casePlanningViewNotes = historicalProjectToUpdate.casePlanningViewNotes.filter(x => x.id && x.id !== action.payload.id);
            historicalProjectToUpdate.casePlanningViewNotes.unshift(action.payload);
          }
          return {
            ...state,
            projectsToDisplay: projectsToDisplay,
            historicalProjectsToDisplay: historicalProjectsToDisplay,
            planningCards: planningCards
          };

        case StaffingDemandActionTypes.DeleteCaseViewNotesSuccess:
          projectToUpdate = findProjectToUpdate(action.payload, projectsToDisplay, planningCards);
          historicalProjectToUpdate = findProjectToUpdate(action.payload, historicalProjectsToDisplay, null);
          if(projectToUpdate) {
            projectToUpdate.casePlanningViewNotes = projectToUpdate.casePlanningViewNotes.filter(x => x.id !== action.payload.id);
          }   
          if(historicalProjectToUpdate) {
            historicalProjectToUpdate.casePlanningViewNotes = historicalProjectToUpdate.casePlanningViewNotes.filter(x => x.id !== action.payload.id);
          }   
          return {
            ...state,
            projectsToDisplay: projectsToDisplay,
            historicalProjectsToDisplay: historicalProjectsToDisplay,
            planningCards: planningCards
          };
      

        case StaffingDemandActionTypes.UpdatePegPlanningCardSuccess:
          //find the planningCardOpportunityID in planningCrads list with what is coming from payload
          let planningCardToUpdate = planningCards.find(x => x.pegOpportunityId === action.payload.updatedPegPlanningCardData.opportunityId);

          if (!!planningCardToUpdate) {
            planningCardToUpdate.name = action.payload.updatedPegPlanningCardData.name;
            planningCardToUpdate.startDate = action.payload.updatedPegPlanningCardData.startDate;
            planningCardToUpdate.endDate = action.payload.updatedPegPlanningCardData.endDate;
            planningCardToUpdate.sharedOfficeCodes = action.payload.updatedPegPlanningCardData.officeCodes;
            planningCardToUpdate.probabilityPercent = action.payload.updatedPegPlanningCardData.probabilityPercent;
            planningCardToUpdate.regularAllocations.forEach(regularAllocation => {
              regularAllocation.startDate = DateService.convertDateInBainFormat(action.payload.updatedPegPlanningCardData.startDate);
              regularAllocation.endDate = DateService.convertDateInBainFormat(action.payload.updatedPegPlanningCardData.endDate);
            });
            planningCardToUpdate.allocations.forEach(allocation => {
              allocation.startDate = DateService.convertDateInBainFormat(action.payload.updatedPegPlanningCardData.startDate);
              allocation.endDate = DateService.convertDateInBainFormat(action.payload.updatedPegPlanningCardData.endDate);
            });
            planningCardToUpdate.placeholderAllocations.forEach(placeholderAllocation => {
              placeholderAllocation.startDate = DateService.convertDateInBainFormat(action.payload.updatedPegPlanningCardData.startDate);
              placeholderAllocation.endDate = DateService.convertDateInBainFormat(action.payload.updatedPegPlanningCardData.endDate);
            });

            // call the method to filter based on percentage
            if (action.payload.demandFilterCriteria.minOpportunityProbability !== undefined) {
              const minProbabilityPercent = action.payload.demandFilterCriteria.minOpportunityProbability;
              planningCards = filterByMinProbabilityPercent(planningCards, minProbabilityPercent);
            }
          }
          return {
            ...state,
            planningCards: planningCards
          }
  
        default: 
        return state;
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

function getAllocationsCaseCodeOrPipelineId(projects: Project[], allocation: any) {
  if(allocation.oldCaseCode) {
    return projects.find(x => x.oldCaseCode === allocation.oldCaseCode);
  }
  if(allocation.pipelineId) {
    return projects.find(x => x.pipelineId === allocation.pipelineId);
  }
}

function updateDemandSideOnPlanningCardDelete(planningCards, planningCardIdToDelete) {
  planningCards.splice(planningCards.indexOf(planningCards.find(item => item.id === planningCardIdToDelete)), 1);
  return planningCards;
}

function updateProjectCaseRoll(projects, planningCardIdToDelete) {

  projects.forEach(project => {
    if(project.caseRoll?.rolledToPlanningCardId === planningCardIdToDelete) {
      project.caseRoll = null;
    }
  });
  return projects;


}




function updateHomeScreenForSharedPlanningCard(updatedData: PlanningCard, planningCards: PlanningCard[], demandFilterCriteriaObj: any) {
  
  const sharedOfficeCodes = updatedData?.sharedOfficeCodes?.split(',');
  const sharedStaffingTags = updatedData?.sharedStaffingTags?.split(',');
    let isSharedOfficeSelected = false;
    let isSharedStaffingTagSelected = false;
  if(sharedOfficeCodes || sharedStaffingTags) {
    const planningCardToUpdateOrDelete = planningCards.find(x => x.id === updatedData.id);
    if (!!planningCardToUpdateOrDelete) {
      planningCardToUpdateOrDelete.isShared = updatedData?.isShared;
      planningCardToUpdateOrDelete.sharedOfficeCodes = updatedData?.sharedOfficeCodes;
      planningCardToUpdateOrDelete.sharedStaffingTags = updatedData?.sharedStaffingTags;
      planningCardToUpdateOrDelete.includeInCapacityReporting = updatedData?.includeInCapacityReporting;

      sharedOfficeCodes.forEach(officeCode => {
        if (demandFilterCriteriaObj.officeCodes.includes(officeCode)) {
          isSharedOfficeSelected = true;
        }
      });

      sharedStaffingTags.forEach(staffingTag => {
        if ((!demandFilterCriteriaObj.caseAttributeNames
          && staffingTag === ConstantsMaster.ServiceLine.GeneralConsulting)
          || demandFilterCriteriaObj.caseAttributeNames.includes(staffingTag)) {
          isSharedStaffingTagSelected = true;
        }
      });

      if (!isSharedOfficeSelected || !isSharedStaffingTagSelected) {
        planningCards.splice(
          planningCards.indexOf(planningCards.find(item => item.id === updatedData.id)), 1);
      }

      return planningCards;
    }
  }
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

function filterNonUpdatedAllocations(allocations, updatedAllocation) {
  const existingAllocation = allocations.filter(allocation => 
    !isNullOrUndefined(allocation.id) && allocation.id != updatedAllocation.id)
  return existingAllocation;
}

//adding comment to test the git merge issue
function upsertPlaceholderAllocations(updatedData, projects) {
  // find the project that the placeholder allocation belongs to
  let projectToUpdate = findProjectToUpdate(updatedData[0], projects, null);

  if(projectToUpdate) {
    if (projectToUpdate.placeholderAllocations.length > 0) {
      // apply loop on updatedData
      updatedData.forEach(upsertedAllocation => {
        // find the placeholder allocation in the project
        projectToUpdate.placeholderAllocations = filterNonUpdatedAllocations(projectToUpdate.placeholderAllocations, upsertedAllocation);
        projectToUpdate.placeholderAllocations.push(upsertedAllocation);
      });
    } else {
      // add the placeholder allocation to the project
      projectToUpdate.placeholderAllocations = updatedData;
    }
  }
}

function UpsertPlanningCardAllocations(updatedData, planningCards) {
  let planningCardToUpdate = findProjectToUpdate(updatedData[0], null, planningCards);

  if(planningCardToUpdate) {
    if (planningCardToUpdate.allocations.length > 0) {
      // apply loop on updatedData
      updatedData.forEach(upsertedAllocation => {
        // find the resource allocation in the project
        planningCardToUpdate.allocations = filterNonUpdatedAllocations(planningCardToUpdate.allocations, upsertedAllocation);
        planningCardToUpdate.allocations.push(upsertedAllocation);
      });
    } else {
      // add the placeholder allocation to the project
      planningCardToUpdate.allocations = updatedData;
    }
  }
}

function upsertResourceAllocations(updatedData, projects, planningCards) {
    // find the project that the placeholder allocation belongs to
    let projectToUpdate = findProjectToUpdate(updatedData[0], projects, planningCards);
  
    if(projectToUpdate) {
      if (projectToUpdate.allocatedResources.length > 0) {
        // apply loop on updatedData
        updatedData.forEach(upsertedAllocation => {
            // find the resource allocation in the project
            projectToUpdate.allocatedResources = filterNonUpdatedAllocations(projectToUpdate.allocatedResources, upsertedAllocation);
            projectToUpdate.allocatedResources.push(upsertedAllocation);
        });
      } else {
        // add the placeholder allocation to the project
        projectToUpdate.allocatedResources = updatedData;
      }
    }
}

function UpsertCaseRollAndAllocations(payload, projectsToDisplay) {
  let projectRolled = findProjectToUpdate(payload.project, projectsToDisplay, null);
  let caseRoll = payload.caseRollArray[0];

  if(projectRolled) {
    projectRolled.caseRoll = caseRoll;
      let projectToUpdate = projectsToDisplay.find(x => x.oldCaseCode === caseRoll.rolledToOldCaseCode);
      if(projectToUpdate) {
        payload.updatedData.filter(resourceAllocation => resourceAllocation.oldCaseCode == projectToUpdate.oldCaseCode)
        .forEach(resourceAllocation => {
          projectToUpdate.allocatedResources = projectToUpdate.allocatedResources.filter(x => x.id != resourceAllocation.id);
          projectToUpdate.allocatedResources.push(resourceAllocation);
        });
      }
      payload.updatedData.filter(resourceAllocation => resourceAllocation.oldCaseCode == projectRolled.oldCaseCode)
      .forEach(resourceAllocation => {
        projectRolled.allocatedResources = projectRolled.allocatedResources.filter(x => x.id != resourceAllocation.id);
        projectRolled.allocatedResources.push(resourceAllocation);
      });
    }
}

function UpsertCaseRollAndPlaceholderAllocations(payload,projectsToDisplay, planningCards) {
  let projectRolled = findProjectToUpdate(payload.project, projectsToDisplay, null);
    let caseRoll = payload.caseRollArray[0];
    if(projectRolled) {
      projectRolled.caseRoll = caseRoll;
      if(caseRoll.rolledToPlanningCardId) {
        let projectToUpdate = planningCards.find(x => x.id === caseRoll.rolledToPlanningCardId);
        if(projectToUpdate) {
          payload.updatedData.filter(resourceAllocation => resourceAllocation.planningCardId != null)
          .forEach(resourceAllocation => {
            projectToUpdate.allocations = projectToUpdate.allocations.filter(x => x.id != resourceAllocation.id);
            projectToUpdate.allocations.push(resourceAllocation);
          });
          payload.updatedData.filter(resourceAllocation => resourceAllocation.oldCaseCode != null)
          .forEach(resourceAllocation => {
            projectRolled.allocatedResources = projectRolled.allocatedResources.filter(x => x.id != resourceAllocation.id);
            projectRolled.allocatedResources.push(resourceAllocation);
          });
        }
      } 
    }
}

//added to check git branching

function MergePlanningCards(payload, planningCards, projectsToDisplay) {
  let projectToUpdate = null;
  // remove the planning card that was merged
  planningCards.splice(
    planningCards.indexOf(planningCards.find(item => item.id === payload.planningCard.id)), 1);
  // get the project that the placeholder/resource allocation belongs to
  if(payload.placeholderAllocations.length > 0) {
    projectToUpdate = getAllocationsCaseCodeOrPipelineId(projectsToDisplay, payload.placeholderAllocations[0]);
    if(projectToUpdate) {
      projectToUpdate.placeholderAllocations = projectToUpdate?.placeholderAllocations.concat(payload.placeholderAllocations);
    }
  }
  if(payload.resourceAllocations.length > 0) {
    projectToUpdate = getAllocationsCaseCodeOrPipelineId(projectsToDisplay, payload.resourceAllocations[0]);
    if(projectToUpdate) {
      projectToUpdate.allocatedResources = projectToUpdate?.allocatedResources.concat(payload.resourceAllocations);
    }
  }
  if(payload.planningCard.pegOpportunityId && projectToUpdate ){
    projectToUpdate.pegOpportunityId = payload.planningCard.pegOpportunityId;
  }
  // replace project in projects with updated project
  projectsToDisplay.map(project => {
    if(projectToUpdate?.oldCaseCode && project.oldCaseCode === projectToUpdate?.oldCaseCode) {
      project = projectToUpdate;
    }
    if(projectToUpdate?.pipelineId && project.pipelineId === projectToUpdate?.pipelineId) {
      project = projectToUpdate;
    }
    return project;
  });
}

function UpdateUserPreferencesForPin(payload, pinnedProjects, projects, hiddenProjects) {
  let projectToUpdateInPinnedProjects = findProjectToUpdate(payload, pinnedProjects, null);
  let userPreferences = payload.userPreferences;
  let isPinnedProject = 
    userPreferences.caseExceptionShowList
      ?.split(',')
      .includes(payload.oldCaseCode) ||
    userPreferences.opportunityExceptionShowList
      ?.split(',')
      .includes(payload.pipelineId);

  if(isPinnedProject && !projectToUpdateInPinnedProjects) {
      pinnedProjects?.push(payload.updatedProject);
  } else if(!isPinnedProject) {
    pinnedProjects = pinnedProjects?.filter(x => x.oldCaseCode !== payload.oldCaseCode ||
      x.pipelineId !== payload.pipelineId);
  }
  return RestructureProjectsInDisplayFormat(projects, pinnedProjects, hiddenProjects);
}

function UpdateUserPreferencesForHide(payload, hiddenProjects, projects, pinnedProjects) {
  let projectToUpdateInHiddenList = findProjectToUpdate(payload, hiddenProjects, null);
  let userPreferences = payload.userPreferences;

  let isHiddenProject = 
      userPreferences.caseExceptionHideList
        ?.split(',')
        .includes(payload.oldCaseCode) ||
      userPreferences.opportunityExceptionHideList
        ?.split(',')
        .includes(payload.pipelineId);

  if(isHiddenProject && !projectToUpdateInHiddenList) {
    hiddenProjects?.push(payload.updatedProject);
  } else if(!isHiddenProject) {
    hiddenProjects = hiddenProjects?.filter(x => x.oldCaseCode !== payload.oldCaseCode ||
      x.pipelineId !== payload.pipelineId);
  }
  return RestructureProjectsInDisplayFormat(projects, pinnedProjects, hiddenProjects);
}

function isNullOrUndefined(value) {
  return (value === null || value === undefined);
}

function filterByMinProbabilityPercent(planningCards, minProbabilityPercent) {
  return planningCards.filter(planningCard => planningCard.probabilityPercent >= minProbabilityPercent);
}


