import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, forkJoin } from 'rxjs';

import { CoreService } from '../core/core.service';
import { Office } from '../shared/interfaces/office.interface';
import { ResourceGroup } from '../shared/interfaces/resourceGroup.interface';
import { ResourceAllocation } from '../shared/interfaces/resourceAllocation.interface';
import { CommitmentType } from '../shared/interfaces/commitmentType.interface';
import { InvestmentCategory } from '../shared/interfaces/investmentCateogry.interface';
import { Project } from '../shared/interfaces/project.interface';
import { CaseType } from '../shared/interfaces/caseType.interface';
import { SKUCaseTerms } from '../shared/interfaces/skuCaseTerms.interface';
import { CaseRoleType } from '../shared/interfaces/caseRoleType.interface';
import { ServiceLine } from '../shared/interfaces/serviceLine.interface';
import { AuditHistory } from '../shared/interfaces/auditHistory.interface';
import { ProjectDetails } from '../shared/interfaces/projectDetails.interface';
import { OfficeHierarchy } from '../shared/interfaces/officeHierarchy.interface';
import { LevelGrade } from '../shared/interfaces/levelGrade.interface';
import { DemandFilterCriteria } from '../shared/interfaces/demandFilterCriteria.interface';
import { CaseOppChanges } from '../shared/interfaces/caseOppChanges.interface';
import { SupplyFilterCriteria } from '../shared/interfaces/supplyFilterCriteria.interface';
import { ResourceCommitment } from '../shared/interfaces/resourceCommitment';
import { PlaceholderAllocation } from '../shared/interfaces/placeholderAllocation.interface';
import { PlanningCard } from '../shared/interfaces/planningCard.interface';
import { ConstantsMaster } from '../shared/constants/constantsMaster';
import { SupplyGroupFilterCriteria } from '../shared/interfaces/supplyGroupFilterCriteria.interface';
import { map } from 'rxjs/operators';

@Injectable()
export class HomeService {

  // -----------------------Local Variables--------------------------------------------//

  constructor(private http: HttpClient, private coreService: CoreService) {
  }

  // -----------------------Local Functions--------------------------------------------//
  getProjectsFilteredBySelectedValues(demandFilterCriteria: DemandFilterCriteria): Observable<Project[]> {
    return this.http.post<Project[]>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/projectAggregator/projectBySelectedValues`, demandFilterCriteria);
  }

  getCaseDetails(oldCaseCode): Observable<ProjectDetails> {
    const params = new HttpParams({
      fromObject: {
        'oldCaseCode': oldCaseCode
      }
    });
    return this.http.get<any>(`${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/caseDetailsByCaseCode`,
      { params: params });
  }

  getOpportunitiesAndNewDemandsByOffices(demandFilterCriteria: DemandFilterCriteria): Observable<any> {
    const params = new HttpParams({
      fromObject: {
        'filterCriteria': JSON.stringify(demandFilterCriteria)
      }
    });

    return forkJoin([
      this.http.get<any>(
        `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/opportunitiesAndAllocationsByOffices`,
        { params: params }),
      this.http.get<any>(
        `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/newDemandCasesAndAllocationsByOffices`,
        { params: params })
    ]);
  }

  getCasesFilteredBySelectedValues(demandFilterCriteria: DemandFilterCriteria): Observable<any> {
    const params = new HttpParams({
      fromObject: {
        'filterCriteria': JSON.stringify(demandFilterCriteria)
      }
    });

    return this.http.get<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/aggregations/activecasesexceptnewdemandsbyoffices`, { params: params });
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

  getprojectAuditTrails(oldCaseCode, pipelineId): Observable<AuditHistory[]> {
    const params = new HttpParams({
      fromObject: {
        'oldCaseCode': !!oldCaseCode ? oldCaseCode : '',
        'pipelineId': !!pipelineId ? pipelineId : ''
      }
    });
    return this.http.get<any>(`${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/auditCase`,
      { params: params });
  }

  getOpportunityDetails(pipelineId): Observable<ProjectDetails> {
    const params = new HttpParams({
      fromObject: {
        'pipelineId': pipelineId
      }
    });
    return this.http.get<any>(`${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/opportunityByPipelineId`,
      { params: params });
  }

  getOpportunityAllocations(pipelineId): Observable<ResourceAllocation[]> {
    const params = new HttpParams({
      fromObject: {
        'pipelineId': pipelineId
      }
    });
    return this.http.get<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/allocationsByPipelineId`, { params: params });
  }

  getResourcesFilteredBySelectedValues(supplyFilterCriteria: SupplyFilterCriteria): Observable<ResourceCommitment> {

    // Since the filter obj contains more data than needed, so sending only the required fields to API
    const filterObj = {
      'startDate': supplyFilterCriteria.startDate,
      'endDate': supplyFilterCriteria.endDate,
      'officeCodes': supplyFilterCriteria.officeCodes,
      'levelGrades': !!supplyFilterCriteria.levelGrades ? supplyFilterCriteria.levelGrades : '',
      'staffingTags': !!supplyFilterCriteria.staffingTags ? supplyFilterCriteria.staffingTags : '',
      'positionCodes': !!supplyFilterCriteria.positionCodes ? supplyFilterCriteria.positionCodes : '',
      'practiceAreaCodes': !!supplyFilterCriteria.practiceAreaCodes ? supplyFilterCriteria.practiceAreaCodes : '',
      'affiliationRoleCodes': !!supplyFilterCriteria.affiliationRoleCodes ? supplyFilterCriteria.affiliationRoleCodes : ''
    };

    return this.http.post<ResourceCommitment>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAggregator/resourcesFilteredBySelectedValues`, filterObj);
  }

  getResourcesFilteredBySelectedGroup(supplyGroupFilterCriteria: SupplyGroupFilterCriteria): Observable<ResourceCommitment> {

    // Since the filter obj contains more data than needed, so sending only the required fields to API
    const filterObj = {
      'startDate': supplyGroupFilterCriteria.startDate,
      'endDate': supplyGroupFilterCriteria.endDate,
      'employeeCodes': supplyGroupFilterCriteria.employeeCodes
    };

    return this.http.post<ResourceCommitment>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAggregator/resourcesFilteredBySelectedGroupValues`, filterObj);
  }

  getResourcescommitments(employeeCodes, startDate, endDate): Observable<ResourceCommitment> {

    // Since the filter obj contains more data than needed, so sending only the required fields to API
    const payload = {
      'employeeCodes': employeeCodes,
      'startDate': startDate,
      'endDate': endDate,
      'commitmentTypes': null
    };

    return this.http.post<ResourceCommitment>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAggregator/resourcescommitmentsWithinDateRange`, payload);
  }

  //TODO: 07-mar-2023: remove this and corresponding ocelot keys if not used anywhere
  // getCommitmentsByEmployeeCodesAndDateRange(employeeCodes, startDate, endDate): Observable<any> {
  //   const searchCriteria = {
  //     'employeeCodes': employeeCodes,
  //     'startDate': startDate,
  //     'endDate': endDate,
  //     'commitmentTypeCode': null // start sending value(s) when there is a use. Right now this param used by API internally only
  //   };

  //   return forkJoin([
  //     this.http.post<ResourceAllocation[]>(`
  //     ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocationAggregator/allocationsbyemployees`,
  //       searchCriteria),
  //     this.http.post<any>(`${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourcesloa/loaByEmployees`,
  //       searchCriteria),
  //     this.http.post<any>(`${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/vacationrequest/vacationsWithinDateRangeByEmployees`,
  //       searchCriteria),
  //     this.http.post<any>(`${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/training/trainingsWithinDateRangeByEmployees`,
  //       searchCriteria),
  //     this.http.post<any>(`${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/commitment/commitmentsWithinDateRangeByEmployees`,
  //       searchCriteria),
  //     this.http.post<any>(`${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resources/transitionByEmployees`,
  //       searchCriteria),
  //     this.http.post<any>(`${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resources/transferByEmployees`,
  //       searchCriteria)
  //   ]);

  //   // Aggregations are not working for post requests. If figured out later then use the below endpoint
  //   // return this.http.post<any>(`
  //   // ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/aggregations/commitmentsbyEmployeeCodes`, searchCriteria);
  // }

  // TODO: Remove if not referenced anywhere in the app
  // getEmployeeAllocationsByEmployeeCodes(employeeCodes): Observable<ResourceGroup[]> {
  //   const params = new HttpParams({
  //     fromObject: {
  //       'employeeCodes': employeeCodes
  //     }
  //   });
  //   return this.http.get<any>(`
  //   ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocation/allocationsbyemployees`, { params: params });
  // }

  getResourcesBySearchString(searchString, addTransfers?): Observable<ResourceCommitment> {
    const params = new HttpParams({
      fromObject: {
        'searchString': searchString,
        'addTransfers': addTransfers || false// Optional property. Needed only for supply panel search
      }
    });
    return this.http.get<ResourceCommitment>(`
     ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAggregator/resourcesBySearchString`, { params: params });
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

  //TODO: commented on 12-Jun-23: These are not being used naymore and core service is used
  // getCaseTypeList(): Observable<CaseType[]> {
  //   return this.http.get<CaseType[]>(
  //     `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/caseType/casetypes`);
  // }

  // getOfficeList(): Observable<Office[]> {
  //   return this.http.get<Office[]>(
  //     `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/office/offices`);
  // }

  // getOfficeHierarchy(): Observable<OfficeHierarchy> {
  //   return this.http.get<OfficeHierarchy>(
  //     `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/office/officeHierarchy`);
  // }

  
  // getServiceLineList(): Observable<ServiceLine[]> {
  //   return this.http.get<ServiceLine[]>(
  //     `${this.coreService.appSettings.resourcesApiBaseUrl}/api/lookup/serviceLineList`);
  // }

  // getLevelGradeList(): Observable<LevelGrade[]> {
  //   return this.http.get<LevelGrade[]>(
  //     `${this.coreService.appSettings.resourcesApiBaseUrl}/api/lookup/levelGrades`);
  // }

  // getPositionsList(): Observable<string[]> {
  //   return this.http.get<string[]>(
  //     `${this.coreService.appSettings.resourcesApiBaseUrl}/api/lookup/positionList`);
  // }

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

  mapResourceToProject(resourceAllocation: ResourceAllocation[]): Observable<ResourceAllocation[]> {
    resourceAllocation.forEach(resource => resource.lastUpdatedBy = this.coreService.loggedInUser.employeeCode);

    return this.http.post<ResourceAllocation[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocationAggregator`, resourceAllocation);
  }

  updateResourceAssignmentToCase(resourceAllocation: ResourceAllocation): Observable<ResourceAllocation> {
    resourceAllocation.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;

    return this.http.put<ResourceAllocation>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocationAggregator`, resourceAllocation);
  }

  updateOppChanges(oppChanges: CaseOppChanges): Observable<CaseOppChanges> {
    oppChanges.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;

    return this.http.put<CaseOppChanges>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/caseOppChanges/upsertPipelineChanges`, oppChanges);
  }

  updateCaseChanges(caseChanges: CaseOppChanges): Observable<CaseOppChanges> {
    caseChanges.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;

    return this.http.put<CaseOppChanges>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/caseOppChanges/upsertCaseChanges`, caseChanges);
  }

  deleteResourceAssignmentFromProject(allocationId) {
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

  upsertPlaceholderAllocations(placeholders: PlaceholderAllocation[]): Observable<PlaceholderAllocation[]> {
    placeholders.forEach(placeholder => {
      placeholder.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    });

    return this.http.post<PlaceholderAllocation[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourcePlaceholderAllocationAggregator`, placeholders);
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



  getEmployeeInfoWithGxcAffiliations(employeeCode): Observable<any> {
    const params = new HttpParams({
      fromObject: {
        'employeeCode': employeeCode
      }
    });

    return this.http.get<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/aggregations/employeeInfoWithGxcAffiliations`, { params: params });
  }

  getHistoricalStaffingAllocationsByEmployee(employeeCode): Observable<any> {
    const params = new HttpParams({
      fromObject: {
        'employeeCode': employeeCode
      }
    });

    return this.http.get<any>(`${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/historicalStaffingAllocationsByEmployee`, { params: params });
  }

  getAuditTrailForEmployee(employeeCode): Observable<any> {
    const params = new HttpParams({
      fromObject: {
        'employeeCode': employeeCode
      }
    });

    return this.http.get<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/auditEmployee`, { params: params });
  }

  getAllCommitmentsForEmployee(employeeCode): Observable<any> {
    const params = new HttpParams({
      fromObject: {
        'employeeCode': employeeCode
      }
    });

    return this.http.get<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/aggregations/resourcecommitments`, { params: params });
  }

  getCommitmentTypes(): Observable<CommitmentType[]> {
    return this.http.get<CommitmentType[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/lookup/getcommitmenttypelist`);
  }

  getInvestmentCategories(): Observable<InvestmentCategory[]> {
    return this.http.get<InvestmentCategory[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/lookup/investmentTypes`);
  }

  getCaseRoleTypes(): Observable<CaseRoleType[]> {
    return this.http.get<CaseRoleType[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/lookup/caseRoleTypes`);
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

  getSKUTermsForCase(oldCaseCode): Observable<SKUCaseTerms[]> {
    const params = new HttpParams({
      fromObject: {
        'oldCaseCode': oldCaseCode
      }
    });

    return this.http.get<SKUCaseTerms[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/skuCaseTerms/getskutermsforcase`, { params: params });

  }

  getSKUTermsForOpportunity(pipelineId): Observable<SKUCaseTerms[]> {
    const params = new HttpParams({
      fromObject: {
        'pipelineId': pipelineId
      }
    });

    return this.http.get<SKUCaseTerms[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/skuCaseTerms/getskutermsforopportunity`, { params: params });
  }

  insertSKUCaseTerms(skuCaseTerms): Observable<SKUCaseTerms> {
    skuCaseTerms.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;

    return this.http.post<SKUCaseTerms>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/skuCaseTerms/insertskutermsforcase`, skuCaseTerms);


  }

  updateSKUCaseTerms(skuCaseTerms): Observable<SKUCaseTerms> {
    skuCaseTerms.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;

    return this.http.put<SKUCaseTerms>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/skuCaseTerms/updateskutermsforcase`, skuCaseTerms);

  }

  deleteSKUCaseTerms(skuCaseTermsId) {
    const params = new HttpParams({
      fromObject: {
        'id': skuCaseTermsId
      }
    });

    return this.http.delete(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/skuCaseTerms/deleteskutermsforcase`, { params: params });
  }

  // Planning Card
  deletePlanningCardAndItsAllocations(id: string) {
    const lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    const params = new HttpParams({
      fromObject: {
        'id': id,
        'lastUpdatedBy': lastUpdatedBy
      }
    });
    return this.http.delete(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/planningCard`, { params: params });
  }

  insertPlanningCard(planningCard: PlanningCard): Observable<PlanningCard> {
    planningCard.createdBy = this.coreService.loggedInUser.employeeCode;
    planningCard.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    return this.http.post<PlanningCard>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/planningCard`, planningCard);
  }


  sharePlanningCard(planningCard: PlanningCard): Observable<PlanningCard> {
    planningCard.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;

    return this.http.post<PlanningCard>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/planningCard/sharePlanningCard`, planningCard);
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
        'isStaffedFromSupply': demandFilterCriteriaObj.isStaffedFromSupply
      }
    });

    return this.http.get<PlanningCard[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/projectAggregator/planningCardsByEmployeeAndFilters`,
      { params: params });
  }

}
