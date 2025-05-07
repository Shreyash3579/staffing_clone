import * as fromRoot from '../../../app/state/app.state';
import { CasePlanningActions, CasePlanningActionTypes } from './case-planning.actions';
import { createSelector, createFeatureSelector } from '@ngrx/store';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';
import { ProjectOverlayActionTypes } from 'src/app/state/actions/project-overlay.action';
import { ProjectAllocationsActionTypes } from 'src/app/state/actions/project-allocations.action';
import { DateService } from 'src/app/shared/dateService';
import { PlanningCardOverlayActionTypes } from 'src/app/state/actions/planning-card-overlay.action';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';

// State for this feature
export interface CasePlanningTabState {
  projects: Project[];
  planningCards: PlanningCard[];
  availabilityMetrics: any;
  metricsDemandData: any;
  searchedProjects: Project[];
  // refreshCaseAndResourceOverlay: boolean;
  casePlanningLoader: boolean;
}

export interface State extends fromRoot.State {
  projects: CasePlanningTabState;
  planningCards: CasePlanningTabState;
  availabilityMetrics: CasePlanningTabState;
  metricsDemandData: CasePlanningTabState;
  searchedProjects: CasePlanningTabState;
  // refreshCaseAndResourceOverlay: boolean;
  casePlanningLoader: boolean;
}

const initialState = {
  projects: [] as Project[],
  planningCards: [] as PlanningCard[],
  availabilityMetrics: null,
  metricsDemandData: null,
  searchedProjects: [] as Project[],
  // refreshCaseAndResourceOverlay: false,
  casePlanningLoader: false,
};

// Selector Functions
const getCasePlanningProjectsFeatureState = createFeatureSelector<CasePlanningTabState>(
  'casePlanningCopy'
);

export const getProjects = createSelector(
  getCasePlanningProjectsFeatureState,
  (state) => state.projects
);

export const getPlanningCards = createSelector(
  getCasePlanningProjectsFeatureState,
  (state) => state.planningCards
);

export const getAvailabilityMetrics = createSelector(
  getCasePlanningProjectsFeatureState,
  (state) => state.availabilityMetrics
)

export const getDemandMetricsProjects = createSelector(
  getCasePlanningProjectsFeatureState,
  (state) => state.metricsDemandData
);

export const getSearchedProjects = createSelector(
  getCasePlanningProjectsFeatureState,
  (state) => state.searchedProjects
);

// export const refreshCaseAndResourceOverlay = createSelector(
//   getCasePlanningProjectsFeatureState,
//   (state) => state.refreshCaseAndResourceOverlay
// );

export const casePlanningLoader = createSelector(
  getCasePlanningProjectsFeatureState,
  (state) => state.casePlanningLoader
);


export function casePlanningTabReducer(state = initialState, action: any): CasePlanningTabState {
  
  let projects: Project[] = JSON.parse(JSON.stringify(state.projects));
  let planningCards: PlanningCard[] = JSON.parse(JSON.stringify(state.planningCards));
  let projectToUpdate = null;
  let updatedData = null;

  switch (action.type) {
    case CasePlanningActionTypes.CasePlanningLoader:
      return {
        ...state,
        casePlanningLoader: action.payload
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

    case CasePlanningActionTypes.LoadAvailabilityMetricsSuccess:
      return {
        ...state,
        availabilityMetrics: action.payload
      };

    case CasePlanningActionTypes.LoadMetricsDemandDataSuccess:
      return {
        ...state,
        metricsDemandData: action.payload
      };

    case ProjectOverlayActionTypes.UpdateProjectSuccess:
      updatedData = action.payload.updatedData
      projectToUpdate = findProjectToUpdate(updatedData, projects, null);
      if (projectToUpdate) {
        projectToUpdate.caseServedByRingfence = updatedData.caseServedByRingfence;
        if(projectToUpdate.startDate != updatedData.startDate) {
          projectToUpdate.overrideStartDate = DateService.convertDateInBainFormat(updatedData.startDate);
        }
        if(projectToUpdate.endDate != updatedData.endDate) {
          projectToUpdate.overrideEndDate = DateService.convertDateInBainFormat(updatedData.endDate);
        }
        if(projectToUpdate.probabilityPercent != updatedData.probabilityPercent) {
          projectToUpdate.overrideProbabilityPercent = updatedData.probabilityPercent;
        }
      }
      return {
        ...state,
        projects: projects
      };

    case CasePlanningActionTypes.LoadProjectsBySearchStringSuccess:
      return {
        ...state,
        searchedProjects: action.payload
      };

    case CasePlanningActionTypes.ClearSearchData:
      return {
        ...state,
        searchedProjects: []
      };

    case PlanningCardOverlayActionTypes.UpsertPlanningCardSuccess:
      if (action.payload.demandFilterCriteria.demandTypes.includes("PlanningCards")) {
          projectToUpdate = planningCards.find((x) => x.id === action.payload.upsertedData?.id);
       
          // update
          if (projectToUpdate) {
            projectToUpdate.startDate = action.payload.upsertedData.startDate;
            projectToUpdate.endDate = action.payload.upsertedData.endDate;
            projectToUpdate.isShared = action.payload.upsertedData.isShared;
            projectToUpdate.sharedOfficeCodes = action.payload.upsertedData.sharedOfficeCodes;
            projectToUpdate.probabilityPercent = action.payload.upsertedData.probabilityPercent;

            //ToDo : Discuss and move this to coreservice
            if (action.payload.upsertedData.isShared != null) {
              planningCards = updateHomeScreenForSharedPlanningCard(
                projectToUpdate,
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
        
    case PlanningCardOverlayActionTypes.DeletePlanningCardSuccess:
      planningCards = planningCards.filter(x => x.id !== action.payload.planningCardId);
      let updatedProjects = updateProjectCaseRoll(projects, action.payload.planningCardId);
      return {
        ...state,
        planningCards: planningCards,
        projects: updatedProjects
      };

    case PlanningCardOverlayActionTypes.MergePlanningCardsSuccess:
      planningCards = planningCards.filter(x => x.id !== action.payload.planningCard.id);
      return {
        ...state,
        planningCards: planningCards
      };

      case ProjectAllocationsActionTypes.UpsertResourceAllocationsSuccess:
        projectToUpdate = findProjectToUpdate(action.payload.supplyUpdatedData[0], projects, null);
        if(projectToUpdate) {
          action.payload.resourceAllocation.forEach(resourceAllocation => {
            if(resourceAllocation.id) {
              projectToUpdate.allocatedResources = projectToUpdate.allocatedResources.filter(x => x.id != resourceAllocation.id);
            }
            projectToUpdate.allocatedResources.push(resourceAllocation);
          });
        }
        return {
          ...state,
          projects: projects
        };

      case ProjectAllocationsActionTypes.DeleteResourceAllocationCommitmentSuccess:
        projectToUpdate = findProjectToUpdate(action.payload.allocation[0], projects, null);
        if(projectToUpdate) {
          action.payload.allocation.forEach(allocation => {
            projectToUpdate.allocatedResources = projectToUpdate.allocatedResources.filter(x => x.id != allocation.id);
          });
        }
        return {
          ...state,
          projects: projects
        };

      // case ProjectAllocationsActionTypes.UpsertPlanningCardAllocationsSuccess: 
      //   projectToUpdate = findProjectToUpdate(action.payload.supplyUpdatedData[0], null, planningCards);
      //   if(projectToUpdate) {
      //     action.payload.updatedData.forEach(resourceAllocation => {
      //       if(resourceAllocation.id) {
      //         projectToUpdate.regularAllocations = projectToUpdate.regularAllocations.filter(x => x.id != resourceAllocation.id);
      //       } 
      //       projectToUpdate.regularAllocations.push(resourceAllocation);
      //     });
      //   }
      //   return {
      //     ...state,
      //     planningCards: planningCards
      //   };

      case ProjectOverlayActionTypes.UpsertCaseRollAndAllocationsSuccess:
        UpsertCaseRollAndAllocations(action.payload, projects);
        return {
          ...state,
          projects: projects
        };

      case ProjectOverlayActionTypes.UpsertCaseRollAndPlaceholderAllocationsSuccess:
        UpsertCaseRollAndPlaceholderAllocations(action.payload,projects, planningCards);
        return {
          ...state,
          planningCards: planningCards,
          projects: projects,
        };

      case ProjectOverlayActionTypes.RevertCaseRollAndAllocationsSuccess:
        projectToUpdate = findProjectToUpdate(action.payload.project, projects, null);
        if(projectToUpdate) {
          projectToUpdate.caseRoll = null;
          action.payload.updatedData.forEach(resourceAllocation => {
            projectToUpdate.allocatedResources = projectToUpdate.allocatedResources.filter(x => x.id != resourceAllocation.id);
            projectToUpdate.allocatedResources.push(resourceAllocation);
          });
        }
        return {
          ...state,
          projects: projects
        };

    case CasePlanningActionTypes.UpsertCasePlanningViewNoteSuccess:
      projectToUpdate = findProjectToUpdate(action.payload, projects, planningCards);
      if(projectToUpdate) {
        projectToUpdate.casePlanningViewNotes = projectToUpdate.casePlanningViewNotes.filter(x => x.id && x.id !== action.payload.id);
        projectToUpdate.casePlanningViewNotes.unshift(action.payload);
      }
      return {
        ...state,
        projects: projects,
        planningCards: planningCards
      };

    case CasePlanningActionTypes.DeleteCasePlanningViewNotesSuccess:
      projectToUpdate = findProjectToUpdate(action.payload, projects, planningCards);
      if(projectToUpdate) {
        projectToUpdate.casePlanningViewNotes = projectToUpdate.casePlanningViewNotes.filter(x => x.id !== action.payload.id);
      }
      return {
        ...state,
        projects: projects,
        planningCards: planningCards
      };

    case ProjectAllocationsActionTypes.UpsertPlaceholderAllocationsSuccess:
      updatedData = action.payload.demandUpdatedData;
      projectToUpdate = findProjectToUpdate(updatedData[0], projects, null);
      if(projectToUpdate) {
        updatedData.forEach(upsertedAllocation => {
          projectToUpdate.placeholderAllocations = filterNonUpdatedAllocations(projectToUpdate.placeholderAllocations, upsertedAllocation);
          projectToUpdate.placeholderAllocations.push(upsertedAllocation);
        });
        projectToUpdate.combinedSkuTerm = updatedData[0].combinedSkuTerm;
        projectToUpdate.skuTerm = updatedData[0].skuTerms;
      }
    return {
      ...state,
      projects: projects
    };

    case ProjectAllocationsActionTypes.UpsertPlanningCardAllocationsSuccess:
      updatedData = action.payload.demandUpdatedData;
      projectToUpdate = findProjectToUpdate(updatedData[0], null, planningCards);
      if(projectToUpdate) {
        updatedData.forEach(upsertedAllocation => {
          projectToUpdate.allocations = filterNonUpdatedAllocations(projectToUpdate.allocations, upsertedAllocation);
          projectToUpdate.allocations.push(upsertedAllocation);
        });
        projectToUpdate.combinedSkuTerm = updatedData[0].combinedSkuTerm;
        projectToUpdate.skuTerm = updatedData[0].skuTerms;
      }
      return {
        ...state,
        planningCards: planningCards
      };

    case ProjectAllocationsActionTypes.DeletePlaceholderAllocationsSuccess:
      projectToUpdate = findProjectToUpdate(action.payload.placeholderAllocation[0], projects, planningCards);
      if(projectToUpdate) {
        projectToUpdate.placeholderAllocations = projectToUpdate.placeholderAllocations?.filter(x => x.id !== action.payload.placeholderIds);
        projectToUpdate.allocations = projectToUpdate.allocations?.filter(x => x.id !== action.payload.placeholderIds);
        projectToUpdate.regularAllocations = projectToUpdate.regularAllocations?.filter(x => x.id !== action.payload.placeholderIds);
      }
      projectToUpdate.combinedSkuTerm = "";
      projectToUpdate.skuTerm = "";

      return {
        ...state,
        projects: projects,
        planningCards: planningCards
      };

    case CasePlanningActionTypes.UpsertCasePlanningProjectDetailsSuccess:
        projects = projects.map(project => {
          const projDetails = action.payload.projDetails.find(x => x.oldCaseCode === project.oldCaseCode
            || x.pipelineId === project.pipelineId
          );
          if(projDetails) {
            project.includeInDemand = projDetails.includeInDemand;
            project.isFlagged = projDetails.isFlagged;
          }
          return project;
        });

        planningCards = planningCards.map(planningCard => {
          const planCardDetails = action.payload.projDetails.find(x => x.planningCardId === planningCard.id);
          if(planCardDetails) {
            planningCard.includeInDemand = planCardDetails.includeInDemand;
            planningCard.isFlagged = planCardDetails.isFlagged;
          }
          return planningCard;
        });

      return {
        ...state,
        projects: projects,
        planningCards: planningCards
      };

    default:
      return state;
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

  function filterByMinProbabilityPercent(planningCards, minProbabilityPercent) {
    return planningCards.filter(planningCard => planningCard.probabilityPercent >= minProbabilityPercent);
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

  
function updateProjectCaseRoll(projects, planningCardIdToDelete) {
  projects.forEach(project => {
    if(project.caseRoll?.rolledToPlanningCardId === planningCardIdToDelete) {
      project.caseRoll = null;
    }
  });
  return projects;
}

function UpsertCaseRollAndAllocations(payload, projects) {
  let projectRolled = findProjectToUpdate(payload.project, projects, null);
  let caseRoll = payload.caseRollArray[0];

  if(projectRolled) {
    projectRolled.caseRoll = caseRoll;
      let projectToUpdate = projects.find(x => x.oldCaseCode === caseRoll.rolledToOldCaseCode);
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

function UpsertCaseRollAndPlaceholderAllocations(payload,projects, planningCards) {
  let projectRolled = findProjectToUpdate(payload.project, projects, null);
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

function filterNonUpdatedAllocations(allocations, updatedAllocation) {
  const existingAllocation = allocations.filter(allocation => 
    !isNullOrUndefined(allocation.id) && allocation.id != updatedAllocation.id)
  return existingAllocation;
}

function isNullOrUndefined(value) {
  return (value === null || value === undefined);
}

