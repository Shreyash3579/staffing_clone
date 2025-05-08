import * as fromRoot from '../../state/app.state';
import { createSelector, createFeatureSelector } from '@ngrx/store';
import { CaseIntakeLeadership } from 'src/app/shared/interfaces/caseIntakeLeadership.interface';
import { CaseIntakeDetail } from 'src/app/shared/interfaces/caseIntakeDetail.interface';
import { StaffingIntakeActionTypes, UpsertLeadershipDetails } from './staffing-intake.actions';
import { ProjectDetails } from 'src/app/shared/interfaces/projectDetails.interface';
import { CaseIntakeRoleDetails } from 'src/app/shared/interfaces/caseIntakeRoleDetails.interface';
import { CaseIntakeWorkstreamDetails } from 'src/app/shared/interfaces/caseIntakeWorkstreamDetails.interface';
import { DateService } from 'src/app/shared/dateService';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';
import { CaseIntakeExpertise } from 'src/app/shared/interfaces/caseIntakeExpertise.interface';

// State for this feature
export interface StaffingIntakeState {
  leadershipDetails: CaseIntakeLeadership[];
  caseIntakeDetails: CaseIntakeDetail;
  caseBasicDetails: ProjectDetails;
  opportunityBasicDetails: ProjectDetails;
  planningCardDetails: PlanningCard;
  planningCardNotFound: boolean;
  roleDetails: CaseIntakeRoleDetails[];
  workstreamDetails: CaseIntakeWorkstreamDetails[];
  lastUpdatedInfo: LastUpdatedInfo;
  expertiseList: CaseIntakeExpertise[];
}

export interface LastUpdatedInfo {
  lastUpdated: Date;
  lastUpdatedByName: string;
}

export interface State extends fromRoot.State {
  leadershipDetails: CaseIntakeLeadership[];
  caseIntakeDetails: CaseIntakeDetail;
  caseBasicDetails: ProjectDetails;
  opportunityBasicDetails: ProjectDetails;
  roleDetails: CaseIntakeRoleDetails[];
  workstreamDetails: CaseIntakeWorkstreamDetails[];
  lastUpdated: Date;
  lastUpdatedByName: string;
  lastUpdatedInfo: LastUpdatedInfo;
  expertiseList: CaseIntakeExpertise[];
}

const initialState = {
  leadershipDetails: null as CaseIntakeLeadership[], //set as null as  [] is valid dataset for initial state when no resources as returned by API
  caseIntakeDetails: null as CaseIntakeDetail,
  caseBasicDetails: null as ProjectDetails,
  opportunityBasicDetails: null as ProjectDetails,
  planningCardDetails: null as PlanningCard,
  planningCardNotFound: false as boolean,
  roleDetails: null as CaseIntakeRoleDetails[],
  workstreamDetails: null as CaseIntakeWorkstreamDetails[],
  lastUpdatedInfo: null as LastUpdatedInfo,
  expertiseList: null as CaseIntakeExpertise[],
};

// Selector Functions
const getStaffingIntakeFeatureState = createFeatureSelector<StaffingIntakeState>(
  'staffingIntake'
);

export const getLeadershipDetails = createSelector(
  getStaffingIntakeFeatureState,
  (state) => state.leadershipDetails
);

export const getcaseIntakeDetails = createSelector(
  getStaffingIntakeFeatureState,
  (state) => state.caseIntakeDetails
);

export const getCaseBasicDetails = createSelector(
  getStaffingIntakeFeatureState,
  (state) => state.caseBasicDetails
);

export const getOpportunityBasicDetails = createSelector(
  getStaffingIntakeFeatureState,
  (state) => state.opportunityBasicDetails
);

export const getPlanningCardDetails = createSelector(
  getStaffingIntakeFeatureState,
  (state) => state.planningCardDetails
);

export const getPlanningCardNotFound = createSelector(
  getStaffingIntakeFeatureState,
  (state) => state.planningCardNotFound
);

export const getRoleDetails = createSelector(
  getStaffingIntakeFeatureState,
  (state) => state.roleDetails
);

export const getWorkstreamDetails = createSelector(
  getStaffingIntakeFeatureState,
  (state) => state.workstreamDetails
);

export const getLastUpdatedInfo = createSelector(
  getStaffingIntakeFeatureState,
  (state) => state.lastUpdatedInfo
);

export const getExpertiseRequirementList = createSelector(
  getStaffingIntakeFeatureState,
  (state) => state.expertiseList
);

export function staffingIntakeReducer(state = initialState, action: any): StaffingIntakeState {
  
  let roleDetails : CaseIntakeRoleDetails[] = JSON.parse(JSON.stringify(state.roleDetails));
  let workstreamDetails : CaseIntakeWorkstreamDetails[] = JSON.parse(JSON.stringify(state.workstreamDetails));
  let newlastUpdatedByName = '';

  switch (action.type) {

    case StaffingIntakeActionTypes.ClearStaffingIntakeState:
      return {
        ...state,
        leadershipDetails: null,
        caseIntakeDetails: null,
        caseBasicDetails: null,
        opportunityBasicDetails: null,
        planningCardDetails: null,
        planningCardNotFound: false,
        roleDetails: null,
        workstreamDetails: null,
        lastUpdatedInfo: null,
        expertiseList: null,
      };

    case StaffingIntakeActionTypes.LoadLastUpdatedByChangesSuccess:
      let lastUpdatedInfoFromBackend : LastUpdatedInfo = JSON.parse(JSON.stringify(action.payload));
      
      //for converting from UCT to local time zone
      if(lastUpdatedInfoFromBackend)
        lastUpdatedInfoFromBackend.lastUpdated = new Date(lastUpdatedInfoFromBackend.lastUpdated + 'Z');
      
      return {
        ...state,
        lastUpdatedInfo: lastUpdatedInfoFromBackend
      };
    
    case StaffingIntakeActionTypes.LoadLeadershipDetailsSuccess:
      let leadershipDetails : CaseIntakeLeadership[] = JSON.parse(JSON.stringify(action.payload));
      
      return {
          ...state,
          leadershipDetails: leadershipDetails, 
        };

    case StaffingIntakeActionTypes.LoadCaseIntakeDetailsSuccess:
      let caseIntakeDetails : CaseIntakeDetail = JSON.parse(JSON.stringify(action.payload));

      return {
        ...state,
        caseIntakeDetails: caseIntakeDetails,
      };

    case StaffingIntakeActionTypes.LoadCaseBasicDetailsSuccess:
      let caseBasicDetails : any = JSON.parse(JSON.stringify(action.payload));
      return {
        ...state,
        caseBasicDetails: caseBasicDetails
      };

    case StaffingIntakeActionTypes.LoadOpportunityDetailsSuccess:
      let opportunityDetails : any = JSON.parse(JSON.stringify(action.payload));
      return {
        ...state,
        opportunityBasicDetails: opportunityDetails
      };

    case StaffingIntakeActionTypes.LoadPlanningCardDetailsSuccess:
      let planningCardDetails : any = JSON.parse(JSON.stringify(action.payload[0]));
      return {
        ...state,
        planningCardDetails: planningCardDetails
      };

    case StaffingIntakeActionTypes.LoadPlanningCardDetailsNotFound:
      return {
        ...state,
        planningCardDetails: null,
        planningCardNotFound: true
      };


    case StaffingIntakeActionTypes.LoadRoleAndWorkstreamDetailsSuccess:
      let roleAndWorkstreamDetails : any = JSON.parse(JSON.stringify(action.payload));
      
      return {
        ...state,
        roleDetails: roleAndWorkstreamDetails.roleDetails,
        workstreamDetails: roleAndWorkstreamDetails.workStreamDetails,
      };

    case StaffingIntakeActionTypes.UpsertRoleDetailsSuccess:
      const upsertedRoleDetailsArray = action.payload;

      upsertedRoleDetailsArray.forEach(upsertedRoleDetails => {
      const existingRoleDetailsIndex = roleDetails.findIndex(role => role.id === upsertedRoleDetails.id);
  
      if (existingRoleDetailsIndex > -1) {
        // Update existing entry
        roleDetails[existingRoleDetailsIndex] = upsertedRoleDetails;
      } else {
        // Add new entry
        roleDetails.push(upsertedRoleDetails);
      }
    });
        
      return {
        ...state,
        roleDetails: roleDetails,
        lastUpdatedInfo: {
          lastUpdated: new Date(),
          lastUpdatedByName: upsertedRoleDetailsArray[0].lastUpdatedByName
        }
      };



      case StaffingIntakeActionTypes.UpsertRolesAndWorkstreamDetailsSuccess:
        let upsertedroleAndWorkstreamDetails : any = JSON.parse(JSON.stringify(action.payload));
        let upsertedWorkstreamDetails : CaseIntakeWorkstreamDetails[] = upsertedroleAndWorkstreamDetails.workStreamDetails;
        let upsertedRoleDetails : CaseIntakeRoleDetails[] = upsertedroleAndWorkstreamDetails.roleDetails;
        
        
        // Update workstream details
        workstreamDetails = workstreamDetails ?? [];
        roleDetails = roleDetails ?? [];

        // Upsert Workstream Details
        upsertedWorkstreamDetails.forEach(workstream => {
          const index = workstreamDetails.findIndex(ws => ws.id === workstream.id);
          if (index !== -1) {
              workstreamDetails[index] = workstream;
          } else {
              workstreamDetails.push(workstream);
          }
        });

        // Upsert Role Details
        upsertedRoleDetails.forEach(role => {
          const index = roleDetails?.findIndex(r => r.id === role.id);
          if (index !== -1) {
              roleDetails[index] = role;
          } else {
              roleDetails?.push(role);
          }
        });

        return {
          ...state,
          workstreamDetails: workstreamDetails, // Ensure we return a new reference
          roleDetails: roleDetails,
          lastUpdatedInfo: {
            lastUpdated: new Date(),
            lastUpdatedByName: action.payload.lastUpdatedByName
          }
        };

    case StaffingIntakeActionTypes.DeleteWorstreamsByIdSuccess:
      let deletedWorkstreamIds = action.payload.deletedWorkstreamIds.split(',');
      workstreamDetails = workstreamDetails.filter(ws => !deletedWorkstreamIds.includes(ws.id));
      roleDetails = roleDetails.filter(role => !deletedWorkstreamIds.includes(role.workstreamId));
      return {
        ...state,
        workstreamDetails: workstreamDetails,
        roleDetails : roleDetails,
        lastUpdatedInfo: {
          lastUpdated: new Date(),
          lastUpdatedByName: action.payload.lastUpdatedByName
        }
      };

      case StaffingIntakeActionTypes.DeleteRolesByIdSuccess:
        let deletedRoleIds = action.payload.deletedRoleIds.split(',');
        roleDetails = roleDetails.filter(ws => !deletedRoleIds.includes(ws.id));
        return {
          ...state,
          roleDetails: roleDetails,
          lastUpdatedInfo: {
            lastUpdated: new Date(),
            lastUpdatedByName: action.payload.lastUpdatedByName
          }
        };

    case StaffingIntakeActionTypes.UpsertCaseIntakeDetailsSuccess:
      let updatedCaseIntakeDetails : CaseIntakeDetail = JSON.parse(JSON.stringify(action.payload));
      return {
        ...state,
        caseIntakeDetails: updatedCaseIntakeDetails,
        lastUpdatedInfo: {
          lastUpdated: new Date(),
          lastUpdatedByName: updatedCaseIntakeDetails.lastUpdatedByName
        }
      };

    case StaffingIntakeActionTypes.UpsertLeadershipDetailsSuccess:
      let leadershipDetailsInState : CaseIntakeLeadership[] = state.leadershipDetails;
      let upsertedLeadershipDetail : CaseIntakeLeadership = JSON.parse(JSON.stringify(action.payload));

      leadershipDetailsInState = upsertLeadershipDetailsHelper(upsertedLeadershipDetail, leadershipDetailsInState);
     
      return {
        ...state,
        leadershipDetails: leadershipDetailsInState,
        lastUpdatedInfo: {
          lastUpdated: new Date(),
          lastUpdatedByName: upsertedLeadershipDetail[0].lastUpdatedByName
        }
      };

    case StaffingIntakeActionTypes.DeleteLeadershipDetailSuccess:
      let leadershipDataInState : CaseIntakeLeadership[] = state.leadershipDetails;
      let deletedLeadershipDetailId : string = action.payload.id;
      let index = leadershipDataInState.findIndex(x => x.id === deletedLeadershipDetailId);
      if(index > -1) {
        leadershipDataInState.splice(index, 1);
      }
      return {
        ...state,
        leadershipDetails: leadershipDataInState,
        lastUpdatedInfo: {
          lastUpdated: new Date(),
          lastUpdatedByName: action.payload.lastUpdatedByName
        }
      };
    
    case StaffingIntakeActionTypes.GetExpertiseRequirementListSuccess:
      let expertiseRequirementList = JSON.parse(JSON.stringify(action.payload));
      return {
        ...state,
        expertiseList: expertiseRequirementList
      };

    case StaffingIntakeActionTypes.UpsertExpertiseRequirementListSuccess:
      let expertiseListInState : CaseIntakeExpertise[] = state.expertiseList;
      let updatedExpertiseRequirement = JSON.parse(JSON.stringify(action.payload));
      let updatedexpertiseListInState = upsertExpertiseListHelper(updatedExpertiseRequirement, expertiseListInState);
      
      return {
        ...state,
        expertiseList: updatedexpertiseListInState
      };

      default: 
        return state;
  }

}

function upsertExpertiseListHelper(updatedExpertiseRequirement, expertiseListInState) {
  let updatedExpertiseListState = JSON.parse(JSON.stringify(expertiseListInState));
  
  let index = updatedExpertiseListState.findIndex(x => x.expertiseAreaCode === updatedExpertiseRequirement.expertiseAreaCode);
  if(index > -1) {
    updatedExpertiseListState[index] = updatedExpertiseRequirement;
  } else {
    updatedExpertiseListState.push(updatedExpertiseRequirement);
  }

  return updatedExpertiseListState;
}

function upsertLeadershipDetailsHelper(upsertedLeadershipDetail, leadershipDetailsInState) {

  const caseRoleCodeToRemove = upsertedLeadershipDetail[0].caseRoleCode;

  let recordsInState = JSON.parse(JSON.stringify(leadershipDetailsInState));
  recordsInState = recordsInState.filter(record => record.caseRoleCode !== caseRoleCodeToRemove);
  recordsInState.push(...upsertedLeadershipDetail);

  return recordsInState;
}
