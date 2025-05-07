import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { AppSettings } from './interfaces/appSettings.interface';
import { CoreService } from '../core/core.service';
import { Resource } from './interfaces/resource.interface';
import { Project } from './interfaces/project.interface';
import { ResourceAllocation } from './interfaces/resourceAllocation.interface';
import { ResourceFilter } from './interfaces/resource-filter.interface';
import { Commitment } from './interfaces/commitment.interface';
import { PlanningCardModel } from './interfaces/planningCardModel.interface';
import { Client } from './interfaces/client.interface';
import { ResourceOrCasePlanningViewNote } from './interfaces/resource-or-case-planning-view-note.interface';
import { ResourceCommitment } from './interfaces/resourceCommitment';
import { CaseOppCommitment } from './interfaces/caseOppCommitment.interface';
import { DemandFilterCriteria } from './interfaces/demandFilterCriteria.interface';
import { ProjectViewModel } from './interfaces/projectViewModel.interface';
import { PlanningCard } from './interfaces/planningCard.interface';
import { ConstantsMaster } from './constants/constantsMaster';

@Injectable()
export class SharedService {

  public appSettings: AppSettings;

  constructor(
    private http: HttpClient,
    private coreService: CoreService) {
    this.appSettings = environment.settings;
  }

  getCasesBySearchString(searchString): Observable<Project[]> {
    const params = new HttpParams({
      fromObject: {
        'searchString': searchString
      }
    });
    return this.http.get<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/projectAggregator/caseTypeahead`,
      { params: params });
  }

  getResourcesBySearchString(searchString): Observable<Resource[]> {
    const params = new HttpParams({
      fromObject: {
        'searchString': searchString
      }
    });
    return this.http.get<any>(`
    ${this.coreService.appSettings.resourcesApiBaseUrl}/api/resources/employeesBySearchString`, { params: params }).pipe(
      map((data: Resource[]) => {

        data.forEach(resource => {
          resource.employeeSearchData = `${resource.employeeCode} ${resource.fullName} ${resource.firstName} ${resource.lastName}`
        })
        return data;
      })
    );;
  }

  getResourceDetailsByEmployeeCode(employeeCode: string, oDataSelectParams:string = null): Observable<Resource[]> {
    let oDataQuery: HttpParams;
    const filterQuery = `employeecode eq '${employeeCode}'`;

    if(!oDataSelectParams){
      oDataQuery = new HttpParams({
        fromObject: {
          $filter: filterQuery,
        }
      });

    }else{
      oDataQuery = new HttpParams({
        fromObject: {
          $filter: filterQuery,
          $select: oDataSelectParams
        }
      });
    }

    return this.http.get<Resource[]>(
      `${this.coreService.appSettings.resourcesApiBaseUrl}/api/resources/employeesIncludingTerminated`,
      { params: oDataQuery }
    );
  }

  getResourceDetailsByEmployeeCodes(employeeCodes: string): Observable<Resource[]> {
    // Encode and format the employee codes for the OData query
    const encodedEmployeeCodes = employeeCodes.split(',').map(code => `'${code}'`).join(', ');

    const odataQuery = `employeecode in (${encodedEmployeeCodes})`;

    const params = new HttpParams({
      fromObject: {
        $filter: odataQuery,
      }
    });

    return this.http.get<Resource[]>(
      `${this.coreService.appSettings.resourcesApiBaseUrl}/api/resources/employeesIncludingTerminated`,
      { params: params }
    );
  }


  getClientsBySearchString(searchString): Observable<Client[]> {
    const params = new HttpParams({
      fromObject: {
        'searchString': searchString
      }
    });
    return this.http.get<Client[]>(`${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/clientCaseAPI/typeaheadClients`, { params: params });
  }

  getCaseDetailsAndAllocations(oldCaseCode): Observable<any> {
    const params = new HttpParams({
      fromObject: {
        'oldCaseCode': oldCaseCode
      }
    });
    return this.http.get<any>(`
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/caseandAllocationsbycasecode`, { params: params });
  }

  getOpportunityDetailsAndAllocations(pipelineId): Observable<any> {
    const params = new HttpParams({
      fromObject: {
        'pipelineId': pipelineId
      }
    });
    return this.http.get<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/opportunityAndAllocationsByPipelineId`,
      { params: params });
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
    ).pipe(
      map((data: Project[]) => {
        data.forEach(project => {
          project.projectName = project.oldCaseCode ? project.caseName : project.opportunityName;
        });
        return data;
      })
    );
  }

  getResourcesIncludingTerminatedBySearchString(searchString, addTransfers?): Observable<ResourceCommitment> {
    const params = new HttpParams({
      fromObject: {
        'searchString': searchString,
        'addTransfers': addTransfers || false// Optional property. Needed only for supply panel search
      }
    });
    return this.http.get<ResourceCommitment>(`
     ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAggregator/resourcesIncludingTerminatedBySearchString`, { params: params });
  }

  getPlanningCardsBySearchString(searchString): Observable<PlanningCardModel[]> {
        // 'encodeURIComponent' to embed special characters in search term
        return this.http.get<any>(`
        ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/PlanningCard/planningCardTypeAhead?searchString=${encodeURIComponent(searchString)}`);
  }

  getCaseAllocations(oldCaseCode, effectiveFromDate): Observable<ResourceAllocation[]> {
    const params = new HttpParams({
      fromObject: {
        'oldCaseCode': oldCaseCode,
        'effectiveFromDate': effectiveFromDate
      }
    });
    return this.http.get<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/allocationsByCaseCode`, { params: params });
  }



  getProjectSTACommitmentDetails(oldCaseCode: string, opportunityId: string, planningCardId: string): Observable<CaseOppCommitment> {
    const payload = {
      oldCaseCode: oldCaseCode,
      opportunityIds: opportunityId,
      planningCardIds: planningCardId
    };
  
    return this.http.post<any>(`
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/Commitment/getProjectSTACommitmentDetails`, 
      payload
    );
  }

  

  getSavedResourceFiltersForLoggedInUser() {
    const params = new HttpParams({
      fromObject: {
        'employeeCode': this.coreService.loggedInUser.employeeCode
      }
    });
    return this.http.get<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/userPreferenceGroupAggregator/getUserPreferenceSavedGroups`, { params: params });
  }

  upsertResourceFiltersForLoggedInUser(resourceFiltersData: ResourceFilter[]) {
    resourceFiltersData.forEach(resourceFilter => resourceFilter.lastUpdatedBy = this.coreService.loggedInUser.employeeCode);
    return this.http.post<ResourceFilter[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/userPreferenceGroupAggregator/upsertUserPreferencesSavedGroupWithSharedInfo`, resourceFiltersData);
  }

  deleteSavedResourceFilter(filterIdToDelete: string) {
    const params = new HttpParams({
      fromObject: {
        'filterIdToDelete': filterIdToDelete,
        'lastUpdatedBy': this.coreService.loggedInUser.employeeCode
      }
    });
    return this.http.delete<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/userCustomFilter`, { params: params });
  }

  getOverlappingTeamsInPreviousProjects(employeeCode: string, caseStartDate: string): Observable<ResourceAllocation[]> {
    const params = new HttpParams({
      fromObject: {
        'employeeCode': employeeCode,
        'date': caseStartDate
      }
    });
    return this.http.get<ResourceAllocation[]>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/ResourceAllocationAggregator/getLastTeamByEmployeeCode`, { params: params });
  }

  public checkPegRingfenceAllocationAndInsertDownDayCommitments(resourceAllocation : any): Observable<Commitment[]> {
    const requesParam = {
        'resourceAllocations': resourceAllocation
    };
    return this.http.post<Commitment[]>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/commitment/checkPegRingfenceAllocationAndInsertDownDayCommitments`, requesParam);
}


getProjectsFilteredBySelectedValues(demandFilterCriteria: DemandFilterCriteria): Observable<ProjectViewModel> {
  const loggedInUser = this.coreService.loggedInUser.employeeCode;
  const filterObj = {
    'demandFilterCriteria': demandFilterCriteria,
    'loggedInUser': loggedInUser
  };

  return this.http.post<ProjectViewModel>(`
  ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/projectAggregator/newProjectBySelectedValues`, filterObj);
}

getOngoingCasesBySelectedValues(demandFilterCriteria: DemandFilterCriteria): Observable<ProjectViewModel> {
  const loggedInUser = this.coreService.loggedInUser.employeeCode;

  const filterObj = {
    'demandFilterCriteria': demandFilterCriteria,
    'loggedInUser': loggedInUser
  };

  return this.http.post<ProjectViewModel>(`
  ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/projectAggregator/ongoingCasesBySelectedValues`, filterObj);
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
      'isStaffedFromSupply': demandFilterCriteriaObj.isStaffedFromSupply,
      'loggedInUser':  employeeCode
    }
  });

  return this.http.get<PlanningCard[]>(
    `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/projectAggregator/planningCardsByEmployeeAndFilters`,
    { params: params });
}

getResourceViewNotes(employeeCode, noteTypeCode): Observable<ResourceOrCasePlanningViewNote[]> {
  const loggedInEmployeeCode = this.coreService.loggedInUser.employeeCode;
  const params = new HttpParams({
    fromObject: {
      'employeeCode': employeeCode,
      'loggedInEmployeeCode': loggedInEmployeeCode,
      'noteTypeCode': noteTypeCode
    }
  });

  return this.http.get<ResourceOrCasePlanningViewNote[]>(
    `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/resourceNotes`, { params: params });
}

upsertResourceViewNote(resourceViewNote: ResourceOrCasePlanningViewNote): Observable<ResourceOrCasePlanningViewNote> {

  return this.http.post<ResourceOrCasePlanningViewNote>(`
  ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/upsertResourceViewNote`, resourceViewNote);
}

deleteResourceViewNotes(idsToDelete: string): Observable<string[]> {
  const params = new HttpParams({
    fromObject: {
      'idsToDelete': idsToDelete,
      'lastUpdatedBy': this.coreService.loggedInUser.employeeCode
    }
  });

  return this.http.delete<string[]>(
    `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/note/deleteResourceViewNotes`, { params: params });
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

}
