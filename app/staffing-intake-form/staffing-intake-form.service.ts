import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CoreService } from '../core/core.service';
import { Observable } from 'rxjs';
import { ProjectDetails } from '../shared/interfaces/projectDetails.interface';
import { CaseIntakeLeadership } from '../shared/interfaces/caseIntakeLeadership.interface';
import { CaseIntakeDetail } from '../shared/interfaces/caseIntakeDetail.interface';
import { CaseIntakeRoleDetails } from '../shared/interfaces/caseIntakeRoleDetails.interface';
import { CaseIntakeWorkstreamDetails } from '../shared/interfaces/caseIntakeWorkstreamDetails.interface';
import { PlanningCard } from '../shared/interfaces/planningCard.interface';
import { demandId } from '../shared/interfaces/demandId';
import { LastUpdatedChanges } from '../shared/interfaces/lastUpdatedChanges.interface';
import { CaseIntakeExpertise } from '../shared/interfaces/caseIntakeExpertise.interface';
import { CaseIntakeBasicDetails} from '../shared/interfaces/caseIntakeBasicDetails.interface';


@Injectable()
export class StaffingIntakeFormService {

  constructor( private http: HttpClient, 
    private coreService: CoreService
  ) { }

  getLeaderShipDetails(data: demandId): Observable<CaseIntakeLeadership[]> {
    return this.http.post<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/CaseIntake/leadershipDetails`, data);      
  }

  getCaseIntakeDetail(data: demandId): Observable<CaseIntakeDetail> {
    return this.http.post<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/CaseIntake/caseIntakeDetails`, data);
  }

  getOpportunityDetails(pipelineId): Observable<ProjectDetails> {
    const params = new HttpParams({
      fromObject: {
        'pipelineId': pipelineId
      }});
    return this.http.get<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/opportunity/opportunityDetails`,
      { params: params });
  }

  getCaseDetails(oldCaseCode): Observable<ProjectDetails> {
    const params = new HttpParams({
      fromObject: {
        'oldCaseCode': oldCaseCode
      }});
    return this.http.get<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/Case/caseDetailsByCaseCode`,
      { params: params });
  }

  getPlanningCardDetails(planningCardId: string) {
    const requestBody = `"${planningCardId}"`;
    const headers = {
      'Content-Type': 'application/json'
    };
    return this.http.post<PlanningCard>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/planningCard/planningCardsByPlanningCardIds`, 
      requestBody, 
      {headers}
    );
  }

  getRoleAndWorkstreamDetails(data: demandId): Observable<any> {
    return this.http.post<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/CaseIntake/getRoleAndWorkstreamDetails`, data);
  }

  upsertRoleDetails(roleData : CaseIntakeRoleDetails[]): Observable<CaseIntakeRoleDetails[]> {
    roleData.forEach(role => {
      role.lastUpdatedBy = this.coreService.loggedInUser.employeeCode
      role.lastUpdatedByName = this.coreService.loggedInUser.fullName;
      //role.workstreamId = 'a2ef5f06-d578-45f9-90bf-b652f514cdd4';
    });
    return this.http.post<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/CaseIntake/upsertRoles`, roleData);
  }


  upsertRoleAndWorkstreamDetails(payload): Observable<any> {
    payload.roleDetails.forEach(role => {
      role.lastUpdatedBy = this.coreService.loggedInUser.employeeCode
      role.lastUpdatedByName = this.coreService.loggedInUser.fullName;
    });
    payload.workstreamDetails.forEach(workstream => {
      workstream.lastUpdatedBy = this.coreService.loggedInUser.employeeCode
      workstream.lastUpdatedByName = this.coreService.loggedInUser.fullName;
    });
    payload.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    return this.http.post<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/CaseIntake/upsertWorkstreamAndRole`, {
      roleDetails: payload.roleDetails, 
      workstreamDetails: payload.workstreamDetails, 
      lastUpdatedBy: payload.lastUpdatedBy,
      opportunityId: payload.opportunityId,
      oldCaseCode: payload.oldCaseCode,
      planningCardId: payload.planningCardId
    });
  }


  deleteWorstreamsById(deleteWorkstreamDetails: CaseIntakeBasicDetails) {
    deleteWorkstreamDetails.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
      return this.http.delete<any>(`
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/CaseIntake/deleteWorkstreamsByIds`, {
        body: deleteWorkstreamDetails
      });
  }

  deleteRolesById(deleteRolesDetails: CaseIntakeBasicDetails) {
    deleteRolesDetails.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
      return this.http.delete<any>(`
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/CaseIntake/deleteRolesByIds`, {
        body: deleteRolesDetails
      });
  }

 upsertCaseIntakeDetail(caseIntakeData : CaseIntakeDetail): Observable<CaseIntakeDetail> {
    caseIntakeData.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    return this.http.post<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/CaseIntake/upsertCaseIntakeDetails`, caseIntakeData);
  }

  upsertLeadershipDetails(leadershipData : CaseIntakeLeadership[]): Observable<CaseIntakeLeadership[]> {
    leadershipData.forEach(leader => {
      leader.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    });

    return this.http.post<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/CaseIntake/upsertLeadershipDetails`, leadershipData);
  }

  deleteLeadershipDetail(deleteLeadershipDetail: CaseIntakeBasicDetails): Observable<any> {
    deleteLeadershipDetail.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    return this.http.request('delete',
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/CaseIntake/deleteLeadershipByCaseRoleCode`,
      {
        body: deleteLeadershipDetail
      });
  }

  getMostRecentUpdateInCaseIntake(demandId: demandId){
    return this.http.post<LastUpdatedChanges>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/CaseIntake/getMostRecentUpdateInCaseIntake`, demandId);
  }

  getExpertiseRequirementList(){
    return this.http.get<CaseIntakeExpertise[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/CaseIntake/expertiseRequirementList`);
  }

  upsertExpersiteRequirement(expertise : CaseIntakeExpertise){
    
    return this.http.post<CaseIntakeExpertise>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/CaseIntake/upsertexpertiseRequirementList`,expertise);

  }

  getPlacesBySearchString(searchString: string){
    const params = new HttpParams({
      fromObject: {
        'searchString': searchString
      }
    });
    return this.http.get<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/CaseIntake/getPlacesTypeAhead`,{params:params});
  }

}
