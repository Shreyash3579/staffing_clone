import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { forkJoin, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { CoreService } from '../core/core.service';
import { ConstantsMaster } from '../shared/constants/constantsMaster';
import { Commitment } from '../shared/interfaces/commitment.interface';
import { DemandFilterCriteria } from '../shared/interfaces/demandFilterCriteria.interface';
import { PlanningCard } from '../shared/interfaces/planningCard.interface';
import { Project } from '../shared/interfaces/project.interface';
import { ResourceAllocation } from '../shared/interfaces/resourceAllocation.interface';
import { StaffableAsRole } from '../shared/interfaces/staffableAsRole.interface';
import { ResourceOrCasePlanningViewNote } from '../shared/interfaces/resource-or-case-planning-view-note.interface';
import { SupplyFilterCriteria } from '../shared/interfaces/supplyFilterCriteria.interface';
import { PlaceholderAllocation } from '../shared/interfaces/placeholderAllocation.interface';
import { IncludeInDemandProjectPreference } from '../shared/interfaces/includeInDemandProjectPreference';
import { CommonService } from '../shared/commonService';

@Injectable({
  providedIn: 'root'
})
export class CasePlanningService {

  // -----------------------Local Variables--------------------------------------------//

  constructor(private http: HttpClient, private coreService: CoreService) {
  }

  // -----------------------Local Functions--------------------------------------------//
  getProjectsFilteredBySelectedValues(demandFilterCriteria: DemandFilterCriteria): Observable<Project[]> {
    const loggedInUser = this.coreService.loggedInUser.employeeCode;

    const filterObj = {
      'demandFilterCriteria': demandFilterCriteria,
      'loggedInUser': loggedInUser
    };

    return this.http.post<Project[]>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/projectAggregator/projectBySelectedValues`, filterObj);
  }

  getCasePlanningBoardDataBySelectedValues(demandFilterCriteriaObj: DemandFilterCriteria) {
    const employeeCode = this.coreService.loggedInUser.employeeCode;

    let filterObj = {
      demandFilterCriteria: demandFilterCriteriaObj,
      employeeCode: employeeCode,
      date: demandFilterCriteriaObj.startDate
    };

    return this.http.post<any>(
      `
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanningMetrics/getCasePlanningBoardColumnsData`,
      filterObj
    );
  }
  
  getPlanningCardsBySelectedValues(demandFilterCriteriaObj: DemandFilterCriteria) {
    const employeeCode = this.coreService.loggedInUser.employeeCode;

    const params = new HttpParams({
      fromObject: {
        'employeeCode': employeeCode,
        'officeCodes': demandFilterCriteriaObj.officeCodes,
        'staffingTags': !demandFilterCriteriaObj.caseAttributeNames
          ? ConstantsMaster.ServiceLine.GeneralConsulting
          : demandFilterCriteriaObj.caseAttributeNames,
        'loggedInUser': employeeCode
      }
    });

    return this.http.get<PlanningCard[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/projectAggregator/planningCardsByEmployeeAndFilters`,
      { params: params });
  }

  getAvailabilityMetricsByFilterValues(supplyFilterCriteriaObj: SupplyFilterCriteria) {
    return this.http.post<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanningMetrics/getAvailabilityMetrics`, supplyFilterCriteriaObj);
  }

  getProjectsBySearchString(searchString): Observable<Project[]> {
    // 'encodeURIComponent' to embed special characters in search term
    const params = new HttpParams({
      fromObject: {
        'searchString': encodeURIComponent(searchString)
      }
    });
    return this.http.get<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/projectAggregator/projectTypeahead`, { params: params }
    ).pipe(map((data: Project[]) => {
      data.forEach(project => {
        project.projectName = project.oldCaseCode ? project.caseName : project.opportunityName;
      });
      return data;
    })
    );
  }

  upsertResourceAllocations(resourceAllocation: ResourceAllocation[]): Observable<ResourceAllocation[]> {
    const  lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
        
    resourceAllocation = CommonService.updateResourceAllocations(resourceAllocation, lastUpdatedBy);

    return this.http.post<ResourceAllocation[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocationAggregator/upsertResourceAllocations`,
      resourceAllocation);

  }

  upsertResourcesCommitments(commitments: Commitment[]): Observable<Commitment[]> {
    commitments.forEach((commitment) => {
      commitment.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    });

    return this.http.post<Commitment[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/commitment/resourcesCommitments`, commitments);
  }

  deleteResourcecommitment(commitmentId) {
    const lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    const params = new HttpParams({
      fromObject: {
        'commitmentId': commitmentId,
        'lastUpdatedBy': lastUpdatedBy
      }
    });

    return this.http.delete(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/commitment`, { params: params });
  }

  deleteResourceAssignmentFromProject(allocationId: string) {
    const lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    const params = new HttpParams({
      fromObject: {
        'allocationId': allocationId,
        'lastUpdatedBy': lastUpdatedBy
      }
    });

    return this.http.delete(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocation`, { params: params });
  }

  deleteResourcesAssignmentsFromProject(allocationIds: string) {
    const lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    const params = {
      'allocationIds': allocationIds,
      'lastUpdatedBy': lastUpdatedBy
    };

    return this.http.post(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocation/deleteAllocationsByIds`, params);
  }

  deleteResourceAllocationsCommitments(dataToDelete) {
    const lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    const allocationParams = {
      'allocationIds': dataToDelete.allocationIds,
      'lastUpdatedBy': lastUpdatedBy
    };
    const commitmentParams = new HttpParams({
      fromObject: {
        'commitmentIds': dataToDelete.commitmentIds,
        'lastUpdatedBy': lastUpdatedBy
      }
    });
    const deleteAllocationsCall = this.http.post(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocation/deleteAllocationsByIds`,
      allocationParams);
    const deleteCommitmentsCall =
      this.http.delete(`${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/commitment/deleteCommitmentsByIds`,
        { params: commitmentParams });

    if (dataToDelete.allocationIds.length > 0 && dataToDelete.commitmentIds.length > 0) {
      return forkJoin([deleteAllocationsCall, deleteCommitmentsCall]);
    } else if (dataToDelete.allocationIds.length > 0 && dataToDelete.commitmentIds.length < 1) {
      return deleteAllocationsCall;
    } else {
      return deleteCommitmentsCall;
    }
  }

  deleteResourceStaffableAsRoleById(staffableAsRoleId: string) {
    const params = new HttpParams({
      fromObject: {
        'idToDelete': staffableAsRoleId,
        'lastUpdatedBy': this.coreService.loggedInUser.employeeCode
      }
    });
    return this.http.delete(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffableAs`, { params });
  }

  upsertResourceStaffableAsRole(event) {
    let staffableRoles = event.staffableRoles;
    let resource = event.resource;
    staffableRoles.map(x => {
      x.employeeCode = resource.employeeCode,
        x.levelGrade = resource.levelGrade,
        x.lastUpdatedBy = this.coreService.loggedInUser.employeeCode
    });
    return this.http.post<StaffableAsRole[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffableAs/upsertResourceStaffableAs`, staffableRoles);
  }

  upsertCasePlanningViewNote(caseViewNote: ResourceOrCasePlanningViewNote): Observable<ResourceOrCasePlanningViewNote> {
    return this.http.post<ResourceOrCasePlanningViewNote>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/projectAggregator/upsertCaseViewNote`, caseViewNote);
  }

  deleteCasePlanningNotes(idsToDelete: string): Observable<string[]> {
    const params = new HttpParams({
      fromObject: {
        'idsToDelete': idsToDelete,
        'lastUpdatedBy': this.coreService.loggedInUser.employeeCode
      }
    });

    return this.http.delete<string[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/note/deleteCaseViewNotes`, { params: params });
  }

  upsertCasePlanningProjectDetails(projectInfo: any): Observable<any> {
    projectInfo.forEach(x => {
      x.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    });
    
    return this.http.put<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanning/upsertCasePlanningProjectDetails`, projectInfo);
  }

// Can be removed once code is moved to a common place
  
  getPlaceholderAllocationsByOldCaseCodes(oldCaseCodes: string) {
    const payload = {
      'oldCaseCodes': oldCaseCodes
    };

    return this.http.post<PlaceholderAllocation[]>(`
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourcePlaceholderAllocationAggregator/allocationsByCaseCodes`, payload);
  }

  getPlaceholderAllocationsByPipelineIds(pipelineIds: string) {
    const payload = {
      'pipelineIds': pipelineIds
    };

    return this.http.post<PlaceholderAllocation[]>(`
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourcePlaceholderAllocationAggregator/allocationsByPipelineIds`, payload);
  }

  getPlaceholderAllocationsByPlanningCardIds(planningCardIds: string) {
    const payload = {
      'planningCardIds': planningCardIds
    };

    return this.http.post<PlaceholderAllocation[]>(`
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourcePlaceholderAllocationAggregator/placeholderAllocationsByPlanningCardIds`, payload);
  }

}
