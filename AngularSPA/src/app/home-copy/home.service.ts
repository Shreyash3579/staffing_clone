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
import { ProjectViewModel } from '../shared/interfaces/projectViewModel.interface';

@Injectable()
export class HomeService {

  // -----------------------Local Variables--------------------------------------------//

  constructor(private http: HttpClient, private coreService: CoreService) {
  }

  // -----------------------Local Functions--------------------------------------------//

  getResourcesFilteredBySelectedValues(supplyFilterCriteria: SupplyFilterCriteria): Observable<ResourceCommitment> {
    const loggedInUser = this.coreService.loggedInUser.employeeCode;

    // Since the filter obj contains more data than needed, so sending only the required fields to API
    const supplyFilterCriteriaObj = {
      'startDate': supplyFilterCriteria.startDate,
      'endDate': supplyFilterCriteria.endDate,
      'officeCodes': supplyFilterCriteria.officeCodes,
      'levelGrades': !!supplyFilterCriteria.levelGrades ? supplyFilterCriteria.levelGrades : '',
      'staffingTags': !!supplyFilterCriteria.staffingTags ? supplyFilterCriteria.staffingTags : '',
      'positionCodes': !!supplyFilterCriteria.positionCodes ? supplyFilterCriteria.positionCodes : '',
      'practiceAreaCodes': !!supplyFilterCriteria.practiceAreaCodes ? supplyFilterCriteria.practiceAreaCodes : '',
      'affiliationRoleCodes': !!supplyFilterCriteria.affiliationRoleCodes ? supplyFilterCriteria.affiliationRoleCodes : '',
      'staffableAsTypeCodes' : !!supplyFilterCriteria.staffableAsTypeCodes ? supplyFilterCriteria.staffableAsTypeCodes : '',
    };

    const filterObj = {
      'supplyFilterCriteria': supplyFilterCriteriaObj,
      'loggedInUser': loggedInUser
    };

    return this.http.post<ResourceCommitment>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAggregator/resourcesFilteredBySelectedValues`, filterObj);
  }

  getResourcesFilteredBySelectedGroup(supplyGroupFilterCriteria: SupplyGroupFilterCriteria): Observable<ResourceCommitment> {
    const loggedInUser = this.coreService.loggedInUser.employeeCode;

    // Since the filter obj contains more data than needed, so sending only the required fields to API
    const supplyGroupFilterCriteriaObj = {
      'startDate': supplyGroupFilterCriteria.startDate,
      'endDate': supplyGroupFilterCriteria.endDate,
      'employeeCodes': supplyGroupFilterCriteria.employeeCodes
    };

    const filterObj = {
      'supplyGroupFilterCriteria': supplyGroupFilterCriteriaObj,
      'loggedInUser': loggedInUser
    };

    return this.http.post<ResourceCommitment>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAggregator/resourcesFilteredBySelectedGroupValues`, filterObj);
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

  upsertPlaceholderAllocations(placeholders: PlaceholderAllocation[]): Observable<PlaceholderAllocation[]> {
    placeholders.forEach(placeholder => {
      placeholder.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    });

    return this.http.post<PlaceholderAllocation[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourcePlaceholderAllocationAggregator`, placeholders);
  }

  upsertPlanningCard(planningCard: PlanningCard): Observable<PlanningCard> {
    planningCard.createdBy = this.coreService.loggedInUser.employeeCode;
    planningCard.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    const requestBody = {
      'planningCard': planningCard,
      'loggedInUser': this.coreService.loggedInUser.employeeCode
    };
    return this.http.post<PlanningCard>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/StaffingAggregator/upsertPlanningCard`, requestBody);
  }

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
}
