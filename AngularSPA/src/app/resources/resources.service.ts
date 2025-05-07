import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { CoreService } from '../core/core.service';
import { forkJoin, Observable, of } from 'rxjs';
import { ResourceStaffing } from '../shared/interfaces/resourceStaffing.interface';
import { ResourceAllocation } from '../shared/interfaces/resourceAllocation.interface';
import { SupplyFilterCriteria } from '../shared/interfaces/supplyFilterCriteria.interface';
import { Commitment } from '../shared/interfaces/commitment.interface';
import { StaffableAsRole } from '../shared/interfaces/staffableAsRole.interface';
import { SupplyGroupFilterCriteria } from '../shared/interfaces/supplyGroupFilterCriteria.interface';
import { ResourceOrCasePlanningViewNote } from '../shared/interfaces/resource-or-case-planning-view-note.interface';
import { ResourceLastBillableDate } from '../shared/interfaces/resource-last-billable-date.interface';
import { PlaceholderAllocation } from '../shared/interfaces/placeholderAllocation.interface';
import { ResourcesCount } from '../shared/interfaces/resourcesCount.interface';
import { ResourceViewCD } from '../shared/interfaces/resource-view-cd.interface';
import { ResourceViewCommercialModel } from '../shared/interfaces/resource-view-commercial-model.interface';
import { CommonService } from '../shared/commonService';

@Injectable()
export class ResourcesService {

  constructor(private http: HttpClient, private coreService: CoreService) { }

  // -----------------------Local Functions--------------------------------------------//
  getActiveResources(supplyFilterCriteria: SupplyFilterCriteria, pageNumber, resourcesPerPage):
    Observable<ResourceStaffing[]> {
    let filterObj = {
      'supplyFilterCriteria': supplyFilterCriteria,
      'pageNumber': pageNumber,
      'resourcesPerPage': resourcesPerPage,
      'loggedInUser': this.coreService.loggedInUser.employeeCode
    };
    // return emplty object if none of the office is selected
    if (supplyFilterCriteria.officeCodes === '' || supplyFilterCriteria.staffingTags === '') {
      return of([]);
    }
    return this.http.post<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/resourcesStaffing`, filterObj);
  }

  getGroupedResources(supplyGroupFilterCriteria: SupplyGroupFilterCriteria, pageNumber, resourcesPerPage):
    Observable<ResourceStaffing[]> {
    let filterObj = {
      'suppplyGroupFilterCriteria': supplyGroupFilterCriteria,
      'pageNumber': pageNumber,
      'resourcesPerPage': resourcesPerPage,
      'loggedInUser': this.coreService.loggedInUser.employeeCode
    };
    // return emplty object if none of the office is selected
    if (!supplyGroupFilterCriteria.employeeCodes) {
      return of([]);
    }
    return this.http.post<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/resourcesStaffingBySupplyGroup`, filterObj);
  }

  getResourcesBySearchString(searchString: string): Observable<ResourceStaffing[]> {
    const params = new HttpParams({
      fromObject: {
        'searchString': searchString
      }
    });

    return this.http.get<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/resourcesStaffingBySearchString`,
      { params: params });
  }

  getResourcesIncludingTerminatedBySearchString(searchString: string, startDate, endDate): Observable<ResourceStaffing[]> {
    const params = new HttpParams({
      fromObject: {
        'searchString': searchString,
        'startDate': startDate,
        'endDate': endDate,
        'loggedInUser': this.coreService.loggedInUser.employeeCode
      }
    });

    return this.http.get<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/resourcesIncludingTerminatedStaffingBySearchString`,
      { params: params });
  }

  updateResourceAssignmentToCase(resourceAllocation: ResourceAllocation): Observable<ResourceAllocation> {
    resourceAllocation.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;

    return this.http.put<ResourceAllocation>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocationAggregator`, resourceAllocation);
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


   
  deletePlaceholdersByIds(placeholderIds) {
    const lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    const params = new HttpParams({
      fromObject: {
        'placeholderIds': placeholderIds,
        'lastUpdatedBy': lastUpdatedBy
      }
    });

    return this.http.delete(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/scheduleMasterPlaceholder`, { params: params });
  }

  mapResourceToProject(resourceAllocation: ResourceAllocation[]): Observable<ResourceAllocation[]> {
    resourceAllocation.forEach(resource => resource.lastUpdatedBy = this.coreService.loggedInUser.employeeCode);

    return this.http.post<ResourceAllocation[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocationAggregator`, resourceAllocation);
  }

  upsertResourceAllocations(resourceAllocation: ResourceAllocation[]): Observable<ResourceAllocation[]> {
     
    let lastUpdatedBy =  this.coreService.loggedInUser.employeeCode;
     
    resourceAllocation = CommonService.updateResourceAllocations(resourceAllocation, lastUpdatedBy);

    return this.http.post<ResourceAllocation[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocationAggregator/upsertResourceAllocations`,
      resourceAllocation);

  }

  upsertPlaceholderAllocations(placeholders): Observable<PlaceholderAllocation[]> {
    let placeholderAllocations = placeholders.placeholderAllocations;
    placeholderAllocations.forEach(placeholders => placeholders.lastUpdatedBy = this.coreService.loggedInUser.employeeCode);

    return this.http.post<PlaceholderAllocation[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourcePlaceholderAllocationAggregator`,
      placeholderAllocations);

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

  upsertResourceViewNote(resourceViewNote: ResourceOrCasePlanningViewNote): Observable<ResourceOrCasePlanningViewNote> {
    
    return this.http.post<ResourceOrCasePlanningViewNote>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/upsertResourceViewNote`, resourceViewNote);
  }

  getResourcesRecentCDList(): Observable<ResourceViewCD[]> {
    return this.http.get<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/note/getRecentCDList`);
  }

  getResourcesCommercialModelList(): Observable<ResourceViewCommercialModel[]> {
    return this.http.get<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/note/getCommercialModelList`);
  }
  upsertResourceViewCD(resourceViewCD: ResourceViewCD): Observable<ResourceViewCD> {
    
    return this.http.post<ResourceViewCD>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/upsertResourceViewCD`, resourceViewCD);
  }

  upsertResourceViewCommercialModel(resourceViewCommercialModel: ResourceViewCommercialModel): Observable<ResourceViewCommercialModel> {
    
    return this.http.post<ResourceViewCommercialModel>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/upsertResourceViewCommercialModel`, resourceViewCommercialModel);
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

  deleteResourceViewCD(idsToDelete: string): Observable<string[]> {
    const params = new HttpParams({
      fromObject: {
        'idsToDelete': idsToDelete,
        'lastUpdatedBy': this.coreService.loggedInUser.employeeCode
      }
    });

    return this.http.delete<string[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/note/deleteResourceViewCD`, { params: params });
  }

  deleteResourceViewCommercialModel(idsToDelete: string): Observable<string[]> {
    const params = new HttpParams({
      fromObject: {
        'idsToDelete': idsToDelete,
        'lastUpdatedBy': this.coreService.loggedInUser.employeeCode
      }
    });

    return this.http.delete<string[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/note/deleteResourceCommercialModel`, { params: params });
  }

  getLastBillableDateByEmployeeCodes(employeeCodes: string): Observable<ResourceLastBillableDate[]> {
    let filterObj = {
      'employeeCodes': employeeCodes
    };
    // return emplty object if employee codes is null or empty
    if (!employeeCodes) {
      return of([]);
    }
    return this.http.post<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocation/lastBillableDateByEmployeeCodes`, filterObj);
  }

  getResourcesCountByCaseCodesAndPipelineIds(oldCaseCodes): Observable<ResourcesCount[]> {
    const payload = {
      'oldCaseCodes': oldCaseCodes
    };
    return this.http.post<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocation/allocationsCountByCaseCodes`, payload);
  }

}
