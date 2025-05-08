import { DateService } from 'src/app/shared/dateService';
import { CommitmentType } from 'src/app/shared/interfaces/commitmentType.interface';
import { SecurityUserDetail } from 'src/app/shared/interfaces/securityUserDetail';
import { AdminActions, AdminActionTypes } from './admin.actions';
import { AdminState, initialState } from './admin.state';
import { SecurityGroup } from 'src/app/shared/interfaces/securityGroup';


export function reducer(state = initialState, action: AdminActions): AdminState {
    switch (action.type) {
        case AdminActionTypes.LoadStaffingUsersSuccess:
        return {
                ...state,
                staffingUsers: action.payload
            };

        case AdminActionTypes.ShowHideStaffingUsersLoader:
            return {
                ...state,
                staffingUsersLoader: action.payload
            };

        case AdminActionTypes.DeleteSaffingUserSuccess:
            const existingStaffingUsers: SecurityUserDetail[] =
                JSON.parse(JSON.stringify(state.staffingUsers));
            const staffingUserToDelete = existingStaffingUsers.find(x =>
                x.employeeCode === action.payload
            );
            existingStaffingUsers.splice(existingStaffingUsers.indexOf(staffingUserToDelete), 1);
            return {
                ...state,
                staffingUsers: existingStaffingUsers
            };

        case AdminActionTypes.LoadPracticeBasedRingfencesSuccess:
            return {
                ...state,
                practiceBasedRingfences: action.payload.filter( x=> x.isStaffingTag)
        };

        case AdminActionTypes.LoadStaffingGroupsSuccess:
            return {
                ...state,
                staffingGroups: action.payload
        };

        case AdminActionTypes.UpsertSecurityGroupSuccess:
            const securityGroups: SecurityGroup[] =
                JSON.parse(JSON.stringify(state.staffingGroups));

            let staffingGroupToBeUpdated = securityGroups.find(r => r.id === action.payload.id);

            if (staffingGroupToBeUpdated) {
                
                staffingGroupToBeUpdated.groupName = action.payload.groupName;
                staffingGroupToBeUpdated.roleCodes = action.payload.roleCodes;
                staffingGroupToBeUpdated.notes = action.payload.notes;
                staffingGroupToBeUpdated.lastUpdated = DateService.convertDateInBainFormat(action.payload.lastUpdated);
                staffingGroupToBeUpdated.lastUpdatedBy = action.payload.lastUpdatedBy;

              }else{
                const newlyAddedStaffingGroup: SecurityGroup = {
                    id: action.payload.id,
                    groupName: action.payload.groupName,
                    roleCodes: action.payload.roleCodes,
                    featureCodes: action.payload.featureCodes,
                    notes: action.payload.notes,
                    lastUpdated: DateService.convertDateInBainFormat(action.payload.lastUpdated),
                    lastUpdatedBy: action.payload.lastUpdatedBy
                };

                securityGroups.unshift(newlyAddedStaffingGroup);
            }

            return {
                ...state,
                staffingGroups: securityGroups
            };

        case AdminActionTypes.DeleteSaffingGroupSuccess:
            const existingGroups: SecurityGroup[] =
                JSON.parse(JSON.stringify(state.staffingGroups));
            const groupToDelete = existingGroups.find(x =>
                x.id === action.payload
            );
            existingGroups.splice(existingGroups.indexOf(groupToDelete), 1);
            return {
                ...state,
                staffingGroups: existingGroups
        };
            
        case AdminActionTypes.UpsertPracticeBasedRingfenceSuccess:
            const practiceBasedRingfences: CommitmentType[] =
                JSON.parse(JSON.stringify(state.practiceBasedRingfences));
            let practiceBasedRingfencesToBeUpdated = practiceBasedRingfences.find(r => r.commitmentTypeCode === action.payload.commitmentTypeCode);

            if (practiceBasedRingfencesToBeUpdated) {

                practiceBasedRingfencesToBeUpdated.isStaffingTag = action.payload.isStaffingTag;
                practiceBasedRingfencesToBeUpdated.allowsStaffingInAmericas = action.payload.allowsStaffingInAmericas;
                practiceBasedRingfencesToBeUpdated.allowsStaffingInEMEA = action.payload.allowsStaffingInEMEA;
                practiceBasedRingfencesToBeUpdated.allowsStaffingInAPAC = action.payload.allowsStaffingInAPAC;
            }else{
                const newlyAddedPracticeBasedRingfence : CommitmentType = {
                    commitmentTypeCode: action.payload.commitmentTypeCode,
                    commitmentTypeName: action.payload.commitmentTypeName,
                    isStaffingTag: action.payload.isStaffingTag,
                    allowsStaffingInAmericas: action.payload.allowsStaffingInAmericas,
                    allowsStaffingInEMEA: action.payload.allowsStaffingInEMEA,
                    allowsStaffingInAPAC: action.payload.allowsStaffingInAPAC,
                    precedence:  action.payload.precedence,
                    reportingPrecedence:  action.payload.reportingPrecedence,
                    lastUpdatedBy :  action.payload.lastUpdatedBy
                };

                practiceBasedRingfences.unshift(newlyAddedPracticeBasedRingfence);
            }
            return {
                ...state,
                practiceBasedRingfences : practiceBasedRingfences
            }
        case AdminActionTypes.UpsertSecurityUserSuccess:
            const staffingUsers: SecurityUserDetail[] =
                JSON.parse(JSON.stringify(state.staffingUsers));

            let staffingUserToBeUpdated = staffingUsers.find(r => r.employeeCode === action.payload.employeeCode);

            if (staffingUserToBeUpdated) {
                staffingUserToBeUpdated.isAdmin = action.payload.isAdmin;
                staffingUserToBeUpdated.roleCodes = action.payload.roleCodes;
                staffingUserToBeUpdated.lastUpdated = DateService.convertDateInBainFormat(action.payload.lastUpdated);
                staffingUserToBeUpdated.lastUpdatedBy = action.payload.lastUpdatedByName;
                staffingUserToBeUpdated.override = action.payload.override;
                staffingUserToBeUpdated.notes = action.payload.notes;
                staffingUserToBeUpdated.endDate = DateService.convertDateInBainFormat(action.payload.endDate);
                staffingUserToBeUpdated.userTypeCode=action.payload.userTypeCode;
                staffingUserToBeUpdated.geoType=action.payload.geoType;
                staffingUserToBeUpdated.officeCodes=action.payload.officeCodes;
                staffingUserToBeUpdated.serviceLineCodes=action.payload.serviceLineCodes;
                staffingUserToBeUpdated.positionGroupCodes=action.payload.positionGroupCodes;
                staffingUserToBeUpdated.levelGrades=action.payload.levelGrades;
                staffingUserToBeUpdated.practiceAreaCodes=action.payload.practiceAreaCodes;
                staffingUserToBeUpdated.ringfenceCodes=action.payload.ringfenceCodes;
                staffingUserToBeUpdated.hasAccessToAISearch=action.payload.hasAccessToAISearch;
                staffingUserToBeUpdated.hasAccessToStaffingInsightsTool=action.payload.hasAccessToStaffingInsightsTool;
                staffingUserToBeUpdated.hasAccessToRetiredStaffingTab=action.payload.hasAccessToRetiredStaffingTab;

              }else{
                const newlyAddedStaffingUser = {
                    employeeCode: action.payload.employeeCode,
                    roleCodes: action.payload.roleCodes,
                    fullName: action.payload.fullName,
                    serviceLine: action.payload.serviceLine,
                    isAdmin: action.payload.isAdmin,
                    lastUpdated: DateService.convertDateInBainFormat(action.payload.lastUpdated),
                    jobTitle: action.payload.jobTitle,
                    lastUpdatedBy: action.payload.lastUpdatedByName,
                    override: action.payload.override,
                    notes: action.payload.notes,
                    endDate: action.payload.endDate,
                    userTypeCode: action.payload.userTypeCode,
                    geoType: action.payload.geoType,
                    officeCodes: action.payload.officeCodes,
                    serviceLineCodes: action.payload.serviceLineCodes,
                    positionGroupCodes: action.payload.positionGroupCodes,
                    levelGrades: action.payload.levelGrades,
                    practiceAreaCodes: action.payload.practiceAreaCodes,
                    ringfenceCodes: action.payload.ringfenceCodes,
                    hasAccessToAISearch: action.payload.hasAccessToAISearch,
                    hasAccessToStaffingInsightsTool: action.payload.hasAccessToStaffingInsightsTool,
                    hasAccessToRetiredStaffingTab: action.payload.hasAccessToRetiredStaffingTab
                };

                staffingUsers.unshift(newlyAddedStaffingUser);
            }

            return {
                ...state,
                staffingUsers: staffingUsers
            };

        default:
            return state;
    }
}
