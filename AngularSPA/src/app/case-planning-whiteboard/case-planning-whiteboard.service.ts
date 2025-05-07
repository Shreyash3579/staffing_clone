import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CoreService } from '../core/core.service';
import { DateService } from '../shared/dateService';
import { CasePlanningBoardBucketPreferences } from '../shared/interfaces/case-planning-board-bucket-preferences.interface';
import { CasePlanningBoardPlaygroundAllocation } from '../shared/interfaces/case-planning-board-playground-allocation.interface';
import { CasePlanningBoardStaffableTeams } from '../shared/interfaces/case-planning-board-staffable-teams.interface';
import { DemandFilterCriteria } from '../shared/interfaces/demandFilterCriteria.interface';
import { PlaceholderAllocation } from '../shared/interfaces/placeholderAllocation.interface';
import { ResourceAllocation } from '../shared/interfaces/resourceAllocation.interface';
import { SupplyFilterCriteria } from '../shared/interfaces/supplyFilterCriteria.interface';
import { CasePlanningBoardStaffableTeamsColumn } from 'src/app/shared/interfaces/case-planning-board-staffable-teams-column';
import { ResourceOrCasePlanningViewNote } from '../shared/interfaces/resource-or-case-planning-view-note.interface';
import { Observable } from 'rxjs/internal/Observable';


@Injectable()
export class CasePlanningWhiteboardService {

  constructor(private http: HttpClient, private coreService: CoreService) {
  }

  // -----------------------Local Functions--------------------------------------------//

  getCasePlanningBoardDataBySelectedValues(demandFilterCriteriaObj: DemandFilterCriteria) {
    const employeeCode = this.coreService.loggedInUser.employeeCode;

    let filterObj = {
      'demandFilterCriteria': demandFilterCriteriaObj,
      'employeeCode': employeeCode,
      'date': demandFilterCriteriaObj.startDate
    };

    return this.http.post<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanningMetrics/getCasePlanningBoardColumnsData`, filterObj);
  }

  getNewDemandsDataBySelectedValues(demandFilterCriteriaObj: DemandFilterCriteria) {
    const employeeCode = this.coreService.loggedInUser.employeeCode;

    let filterObj = {
      'demandFilterCriteria': demandFilterCriteriaObj,
      'employeeCode': employeeCode,
      'date': demandFilterCriteriaObj.startDate
    };

    return this.http.post<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanningMetrics/getCasePlanningBoardNewDemandsData`, filterObj);
  }

  getAvailabilityMetricsByFilterValues(supplyFilterCriteriaObj: SupplyFilterCriteria) {
    return this.http.post<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanningMetrics/getAvailabilityMetrics`, supplyFilterCriteriaObj);
  }

  upsertCasePlanningBoardCard(dataToUpsert) {
    dataToUpsert.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;

    return this.http.put<any>(`
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanning`, dataToUpsert);
  }

  upsertCasePlanningBoardData(dataToUpsert) {
   dataToUpsert.forEach(data => {
      data.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
   })

   return this.http.post<any>(`
     ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanning`, dataToUpsert);
 }

  upsertCasePlanningBoardBucketPreferences(dataToUpsert: CasePlanningBoardBucketPreferences) {
    dataToUpsert.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;

    return this.http.put<CasePlanningBoardBucketPreferences>(`
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanning/upsertCasePlanningBoardBucketPreferences`, dataToUpsert);
  }

  upsertCasePlanningBoardIncludeInDemandUserPreferences(dataToUpsert) {
    dataToUpsert.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;

    return this.http.put<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanning/upsertCasePlanningBoardIncludeInDemandPreferences`, dataToUpsert);
  }

  deleteCasePlanningBoardByIds(deletedIds) {

    const params = new HttpParams({
      fromObject: {
        'planningBoardIds': deletedIds,
        'lastUpdatedBy': this.coreService.loggedInUser.employeeCode
      }
    });

    return this.http.delete<any>(`
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanning`, { params: params });

  }

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

  getAllocationsByOldCaseCodes(oldCaseCodes: string) {
    if(oldCaseCodes)
    {
      const payload = {
        'oldCaseCodes': oldCaseCodes
      };

      return this.http.post<PlaceholderAllocation[]>(`
        ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/scheduleMasterPlaceholder/allocationsByCaseCodes`, payload);
    }
  }

  getAllocationsByPipelineIds(pipelineIds: string) {
    if(pipelineIds) {
      const payload = {
        'pipelineIds': pipelineIds
      };

      return this.http.post<PlaceholderAllocation[]>(`
        ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/scheduleMasterPlaceholder/allocationsByPipelineIds`, payload);
    }
  }

  getAllocationsByPlanningCardIds(planningCardIds: string) {
    if(planningCardIds) {
      const payload = {
        'planningCardIds': planningCardIds
      };

      return this.http.post<PlaceholderAllocation[]>(`
        ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/scheduleMasterPlaceholder/allocationsByPlanningCardIds`, payload);
    }
  }

  createCasePlanningBoardMetricsPlayground(demandFilterCriteriaObj: DemandFilterCriteria, supplyFilterCriteriaObj: SupplyFilterCriteria, isCountOfIndividualResourcesToggle: Boolean,
    enableMemberGrouping: Boolean, enableNewlyAvailableHighlighting: Boolean) {
    const employeeCode = this.coreService.loggedInUser.employeeCode;

    let filterObj = {
      'demandFilterCriteriaObj': demandFilterCriteriaObj,
      'supplyFilterCriteriaObj': supplyFilterCriteriaObj,
      'isCountOfIndividualResourcesToggle': isCountOfIndividualResourcesToggle,
      'enableMemberGrouping': enableMemberGrouping,
      'enableNewlyAvailableHighlighting': enableNewlyAvailableHighlighting,
      'lastUpdatedBy': employeeCode
    };

    return this.http.post<any>(`
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanningMetricsPlayground/createCasePlanningBoardMetricsPlayground`, filterObj);
  }

  getAvailabilityMetricsForPlaygroundById(playgroundId: string) {
    return this.http.get<any>(`
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanningMetrics/getAvailabilityMetricsForPlaygroundById?playgroundId=${playgroundId}`);
  }

  getCasePlanningBoardPlaygroundFiltersByPlaygroundId(playgroundId: string) {
    return this.http.get<any>(`
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanningMetrics/getCasePlanningBoardPlaygroundFiltersByPlaygroundId?playgroundId=${playgroundId}`);
  }

  deleteCasePlanningBoardMetricsPlaygroundById(playgroundId: string) {
    const lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    return this.http.delete<any>(`
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanningMetrics/deleteCasePlanningBoardMetricsPlaygroundById?playgroundId=${playgroundId}&lastUpdatedBy=${lastUpdatedBy}`);
  }

  abc(resourceAllocations : ResourceAllocation[]) {
      const playgroundAllocations: CasePlanningBoardPlaygroundAllocation[] = [];

      resourceAllocations.forEach((selectedResource) => {
        const casePlanningBoardPlaygroundAllocation: CasePlanningBoardPlaygroundAllocation = {
          employeeCode: selectedResource.employeeCode,
          newStartDate: DateService.convertDateInBainFormat(selectedResource.startDate),
          newEndDate: DateService.convertDateInBainFormat(selectedResource.endDate),
          newAllocation: selectedResource.allocation,
          previousStartDate: DateService.convertDateInBainFormat(selectedResource.previousStartDate),
          previousEndDate: DateService.convertDateInBainFormat(selectedResource.previousEndDate),
          previousAllocation: selectedResource.previousAllocation,
          isOpportunity: !!selectedResource.pipelineId && !selectedResource.oldCaseCode,
          caseEndDate: DateService.convertDateInBainFormat(selectedResource.caseEndDate)
        };

        playgroundAllocations.push(casePlanningBoardPlaygroundAllocation);
      });

      return playgroundAllocations;
  }

  upsertCasePlanningBoardMetricsPlaygroundCacheForUpsertedAllocations(playgroundId: string, playgroundAllocations: CasePlanningBoardPlaygroundAllocation[]) {
    const employeeCode = this.coreService.loggedInUser.employeeCode;

    let filterObj = {
      'playgroundId': playgroundId,
      'playgroundAllocations': playgroundAllocations,
      'lastUpdatedBy': employeeCode
    };

    return this.http.post<any>(`
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanningMetrics/upsertAndGetCasePlanningBoardMetricsPlaygroundCacheForUpsertedAllocations`, filterObj);
  }

  getCasePlanningBoardStaffableTeams(officeCodes, startWeek, endWeek) {
    let filterObj = {
      'officeCodes': officeCodes,
      'startWeek': startWeek,
      'endWeek': endWeek
    };

    return this.http.post<CasePlanningBoardStaffableTeamsColumn[]>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanningMetrics/getCasePlanningBoardStaffableTeams`, filterObj);
  }

  upsertCasePlanningBoardStaffableTeams(staffableTeamsToUpsert: CasePlanningBoardStaffableTeams[]) {
    const employeeCode = this.coreService.loggedInUser.employeeCode;
    staffableTeamsToUpsert.forEach(x => x.lastUpdatedBy = employeeCode)

    return this.http.post<CasePlanningBoardStaffableTeams[]>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/casePlanningStaffableTeams/upsertCasePlanningBoardStaffableTeams`, staffableTeamsToUpsert);
  }

  getCasePlanningViewNotes(oldCaseCode,pipelineId,planningCardId): Observable<ResourceOrCasePlanningViewNote[]> {
    const loggedInUser = this.coreService.loggedInUser.employeeCode;
    let filterObj = {
        'loggedInUser': loggedInUser,
        'oldCaseCode':oldCaseCode,
        'pipelineId':pipelineId,
        'planningCardId':planningCardId
    };
    return this.http.post<ResourceOrCasePlanningViewNote[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/projectAggregator/getCaseViewNote`, filterObj);
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
