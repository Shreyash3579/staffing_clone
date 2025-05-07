import { HttpClient, HttpParams } from '@angular/common/http';
import { CoreService } from '../core/core.service';
import { Injectable } from '@angular/core';
import { forkJoin, Observable } from 'rxjs';
import { CommitmentType } from '../shared/interfaces/commitmentType.interface';
import { ConstantsMaster } from '../shared/constants/constantsMaster';
import { ProjectDetails } from '../shared/interfaces/projectDetails.interface';
import { ResourceAllocation } from '../shared/interfaces/resourceAllocation.interface';
import { AuditHistory } from '../shared/interfaces/auditHistory.interface';
import { SKUCaseTerms } from '../shared/interfaces/skuCaseTerms.interface';
import { Commitment } from '../shared/interfaces/commitment.interface';
import { CaseOppChanges } from '../shared/interfaces/caseOppChanges.interface';
import { CaseRoll } from '../shared/interfaces/caseRoll.interface';
import { LevelGradeTransactionModel } from '../shared/interfaces/level-grade-transaction.interface';
import { EmployeeLanguage } from '../shared/interfaces/employeeLanguage.interface';
import { EmployeeCertification } from '../shared/interfaces/employeeCertification.interface';
import { EmployeeSchoolHistory } from '../shared/interfaces/employeeSchoolHistory';
import { EmployeeWorkHistory } from '../shared/interfaces/employeeWorkHistory';
import { EmployeeStaffingPreferences } from '../shared/interfaces/employeeStaffingPreferences';
import { CaseRoleAllocation } from '../shared/interfaces/caseRoleAllocation.interface';
import { StaffableAsRole } from '../shared/interfaces/staffableAsRole.interface';
import { PlaceholderAllocation } from '../shared/interfaces/placeholderAllocation.interface';
import { ResourceReviewRating } from '../shared/interfaces/resourceReviewRating.interface';
import { TransferTransactionModel } from '../shared/interfaces/tranfer-transaction.interface';
import { EmployeeWorkAndSchoolHistory } from '../shared/interfaces/employeeWorkAndSchoolHistory.interface';
import { ResourceTimeInLevel } from '../shared/interfaces/resourceTimeInLevel.interface';
import { ResourceOrCasePlanningViewNote } from '../shared/interfaces/resource-or-case-planning-view-note.interface';
import { SMAPMissionNote } from '../shared/interfaces/SMAPMissionNote';
import { CaseOppCortexTeamSize } from '../shared/interfaces/case-opp-cortex-team-size.interface';
import { EmployeeStaffingInfo } from '../shared/interfaces/employeeStaffingInfo';
import { Project } from '../shared/interfaces/project.interface';
import { PlanningCard } from '../shared/interfaces/planningCard.interface';
import { CaseType } from '../shared/constants/enumMaster';
import { CommonService } from '../shared/commonService';
import { CaseOppCommitment } from '../shared/interfaces/caseOppCommitment.interface';
import { CommitmentWithCaseOppInfo } from '../shared/interfaces/commitmentView';

@Injectable()
export class OverlayService {

  // -----------------------Local Variables--------------------------------------------//

  constructor(private http: HttpClient, private coreService: CoreService) {
  }

  getEmployeeInfoWithGxcAffiliations(employeeCode): Observable<any> {
    const params = new HttpParams({
      fromObject: {
        'employeeCode': employeeCode
      }
    });

    return this.http.get<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.employeeInfoWithGxcAffiliations}`
      , { params: params });
  }

  getEmployeeRatings(employeeCode): Observable<ResourceReviewRating[]> {
    const params = new HttpParams({
      fromObject: {
        'employeeCode': employeeCode
      }
    });

    return this.http.get<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.employeeReviewRatings}`
      , { params: params });
  }

  getAllCommitmentsForEmployee(employeeCode, effectiveFromDate): Observable<any> {
    const params = new HttpParams({
      fromObject: {
        'employeeCode': employeeCode,
        'effectiveFromDate': effectiveFromDate
      }
    });

    return this.http.get<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.resourceCommitments}`
      , { params: params });
  }

  getHistoricalStaffingAllocationsByEmployee(employeeCode): Observable<any> {
    const params = new HttpParams({
      fromObject: {
        'employeeCode': employeeCode
      }
    });

    return this.http.get<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.resourceHistoricalStaffingAllocations}`
      , { params: params });
  }

  getHistoricalAllocationsForProject(oldCaseCode, pipelineId): Observable<ResourceAllocation[]> {
    const params = new HttpParams({
      fromObject: {
        'oldCaseCode': !!oldCaseCode ? oldCaseCode : '',
        'pipelineId': !!pipelineId ? pipelineId : ''
      }
    });
    return this.http.get<any>(`${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.projectHistoricalStaffingAllocations}`,
      { params: params });
  }

  getAuditTrailForEmployee(employeeCode, limit?, offset?): Observable<any> {
    const params = new HttpParams({
      fromObject: {
        'employeeCode': employeeCode,
        'limit': limit || null,
        'offset': offset || 0
      }
    });

    return this.http.get<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.resourceAuditTrail}`
      , { params: params });
  }

  getCommitmentTypes(): Observable<CommitmentType[]> {
    return this.http.get<CommitmentType[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/lookup/getcommitmenttypelist`);
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

  getOpportunityDetails(pipelineId): Observable<ProjectDetails> {
    const params = new HttpParams({
      fromObject: {
        'pipelineId': pipelineId
      }
    });
    return this.http.get<any>(`${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/opportunityByPipelineId`,
      { params: params });
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

  getOpportunityAllocations(pipelineId, effectiveFromDate): Observable<ResourceAllocation[]> {
    const params = new HttpParams({
      fromObject: {
        'pipelineId': pipelineId,
        'effectiveFromDate': effectiveFromDate
      }
    });
    return this.http.get<any>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/allocationsByPipelineId`, { params: params });
  }

  getPlanningCardAllocations(planningCardIds: string[], effectiveDate: string): Observable<ResourceAllocation[]> {
    const payload = {
      'planningCardIds': planningCardIds,
      'effectiveDate': effectiveDate
    };

    return this.http.post<PlaceholderAllocation[]>(`
      ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourcePlaceholderAllocationAggregator/allocationsByPlanningCardIds`, payload);
  }



  getprojectAuditTrails(oldCaseCode, pipelineId, limit?, offset?): Observable<AuditHistory[]> {
    const params = new HttpParams({
      fromObject: {
        'oldCaseCode': !!oldCaseCode ? oldCaseCode : '',
        'pipelineId': !!pipelineId ? pipelineId : '',
        'limit': limit || null,
        'offset': offset || 0
      }
    });
    return this.http.get<any>(`${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/auditCase`,
      { params: params });
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

  getSKUTermsForCasesOrOpportunitiesForDuration(skuTab, demandFilterCriteria) {
    const oldCaseCodes = skuTab.oldCaseCode;
    const pipelineIds = skuTab.pipelineId;
    const startDate = demandFilterCriteria.startDate;
    const endDate = demandFilterCriteria.endDate;
    const payload = {
        oldCaseCodes,
        pipelineIds,
        startDate,
        endDate
      };

    return this.http.post<SKUCaseTerms[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/skuCaseTerms/getskutermsforcasesoropportunitiesforduration`, payload);
  }

  insertResourcesCommitments(commitments: Commitment[]): Observable<Commitment[]> {
    commitments.forEach((commitment) => {
      commitment.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    });

    return this.http.post<Commitment[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/commitment/resourcesCommitments`, commitments);
  }


  insertCaseOppCommitments(commitments: CaseOppCommitment[]): Observable<CommitmentWithCaseOppInfo[]> {
    // Add user info to each commitment
    commitments.forEach((commitment) => {
      commitment.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    });

    
    return this.http.post<CommitmentWithCaseOppInfo[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/commitment/upsertCaseOppCommitments`, commitments);
  }

  deleteCaseOppCommitments(commitmentIds: string) {
    const deleteCommitmentDetails = {
      commitmentIds: commitmentIds, // comma-separated string
      lastUpdatedBy: this.coreService.loggedInUser.employeeCode
    };
  
    return this.http.delete<any>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/commitment/deleteCaseOppCommitments`,
      {
        body: deleteCommitmentDetails
      }
    );
  }



  deleteResourceCommitments(commitmentIds) {
    const lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    const params = new HttpParams({
      fromObject: {
        'commitmentIds': commitmentIds,
        'lastUpdatedBy': lastUpdatedBy
      }
    });

    return this.http.delete(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/commitment/deleteCommitmentsByIds`, { params: params });
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

  deleteResourcesAllocationsCommitments(dataToDelete) {
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

  deleteResourcesAssignmentsFromProject(allocationIds, lastUpdatedByuser = null) {
    const lastUpdatedBy = lastUpdatedByuser ?? this.coreService.loggedInUser.employeeCode;
    const params = {
      'allocationIds': allocationIds,
      'lastUpdatedBy': lastUpdatedBy
    };

    return this.http.post(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocation/deleteAllocationsByIds`, params);
  }

  updateResourceAssignmentToCase(resourceAllocation: ResourceAllocation): Observable<ResourceAllocation> {
    resourceAllocation.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;

    return this.http.put<ResourceAllocation>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocationAggregator`, resourceAllocation);
  }

  upsertResourceAllocations(resourceAllocation: ResourceAllocation[], lastUpdatedBy = null): Observable<ResourceAllocation[]> {

    lastUpdatedBy = lastUpdatedBy ?? this.coreService.loggedInUser.employeeCode;

    resourceAllocation = CommonService.updateResourceAllocations(resourceAllocation, lastUpdatedBy);

    return this.http.post<ResourceAllocation[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocationAggregator/upsertResourceAllocations`, resourceAllocation);

  }


  mapResourceToProject(resourceAllocation: ResourceAllocation[]): Observable<ResourceAllocation[]> {
    resourceAllocation.forEach(resource => resource.lastUpdatedBy = this.coreService.loggedInUser.employeeCode);

    return this.http.post<ResourceAllocation[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocationAggregator`, resourceAllocation);
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

  detailsByCaseCodeOrPipelineId(payload) {
    const loggedInUser = this.coreService.loggedInUser.employeeCode;

    if(payload.oldCaseCode) {
      const params = new HttpParams({
        fromObject: {
          'oldCaseCode': !!payload.oldCaseCode ? payload.oldCaseCode : '',
          'loggedInUser': loggedInUser
        }
      });

      return this.http.get<Project>(
        `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/projectAggregator/caseDetailsByCaseCode`,{ params: params });

    } else if(payload.pipelineId) {

      const params = new HttpParams({
        fromObject: {
          'pipelineId': !!payload.pipelineId ? payload.pipelineId : '',
          'loggedInUser': loggedInUser
        }
      });
  
      return this.http.get<Project>(
        `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/projectAggregator/opporunityDetailsByPipelineId`, { params: params });
    }
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

  upsertCaseRollsAndAllocations(caseRolls: CaseRoll[], resourceAllocations: ResourceAllocation[]) {
    caseRolls.forEach(caseRoll => caseRoll.lastUpdatedBy = this.coreService.loggedInUser.employeeCode);

    let lastUpdatedBy =  this.coreService.loggedInUser.employeeCode;
     
    resourceAllocations = CommonService.updateResourceAllocations(resourceAllocations, lastUpdatedBy);
    
    return this.http.post(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocationAggregator/upsertCaseRollsAndAllocations`, {
      caseRolls: caseRolls,
      resourceAllocations: resourceAllocations
    });
  }

  upsertCaseRollsAndPlaceholderAllocations(caseRolls: CaseRoll[], resourceAllocations: ResourceAllocation[]) {
    caseRolls.forEach(caseRoll => caseRoll.lastUpdatedBy = this.coreService.loggedInUser.employeeCode);
    resourceAllocations.forEach(resource => resource.lastUpdatedBy = this.coreService.loggedInUser.employeeCode);

    return this.http.post(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourcePlaceholderAllocationAggregator/upsertCaseRollsAndPlaceholderAllocations`, {
      caseRolls: caseRolls,
      resourceAllocations: resourceAllocations
    });
  }

  revertCaseRollAndAllocations(caseRoll: CaseRoll, resourceAllocations: ResourceAllocation[]) {
    caseRoll.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    resourceAllocations.forEach(resource => resource.lastUpdatedBy = this.coreService.loggedInUser.employeeCode);

    return this.http.post(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocationAggregator/revertCaseRollsAndAllocations`, {
      caseRoll: caseRoll,
      resourceAllocations: resourceAllocations
    });
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

  upsertPlaceholderAllocations(placeholders): Observable<PlaceholderAllocation[]> {
    let placeholderAllocations = placeholders.placeholderAllocations ? placeholders.placeholderAllocations : placeholders;
    placeholderAllocations.forEach(placeholder => {
      placeholder.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    });

    return this.http.post<PlaceholderAllocation[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourcePlaceholderAllocationAggregator`, placeholderAllocations);
  }

  upsertPlaceholderCreatedForCortexInfo(caseOppCortexTeamSize: CaseOppCortexTeamSize): Observable<CaseOppCortexTeamSize[]> {
    caseOppCortexTeamSize.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;

    return this.http.post<CaseOppCortexTeamSize[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/cortexSku/upsertPlaceholderCreatedForCortexPlaceholders`, caseOppCortexTeamSize);
  }

  getSmapMissionNotes(employeeCode: string) {
    const params = new HttpParams({
      fromObject: {
        'employeeCodes': employeeCode,
      }
    });
    return this.http.get<SMAPMissionNote>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/sharePoint/getSmapMissionNotesByEmployeeCodes`, { params });
  }

  getResourceNotes(employeeCode, noteTypeCode): Observable<ResourceOrCasePlanningViewNote[]> {
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

  upsertResourceNote(resourceViewNote: ResourceOrCasePlanningViewNote): Observable<ResourceOrCasePlanningViewNote> {
    return this.http.post<ResourceOrCasePlanningViewNote>(`
    ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/upsertResourceViewNote`, resourceViewNote);
  }

  deleteResourceNote(idsToDelete: string): Observable<string[]> {
    const params = new HttpParams({
      fromObject: {
        'idsToDelete': idsToDelete,
        'lastUpdatedBy': this.coreService.loggedInUser.employeeCode
      }
    });

    return this.http.delete<string[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/note/deleteResourceViewNotes`, { params: params });
  }

  getLevelGradeHistoryData(employeeCode: string) {
    return this.http.post<LevelGradeTransactionModel[]>(
      `${this.coreService.appSettings.resourcesApiBaseUrl}/api/resourcestransactions/employeesLevelGradeHistory`,
      { listEmployeeCodes: employeeCode });
  }

  getEmployeeTransfersByEmployeeCodes(employeeCode: string) {
    const params = new HttpParams({
      fromObject: {
        'employeeCode': employeeCode,
      }
    });
    return this.http.get<TransferTransactionModel[]>(
      `${this.coreService.appSettings.resourcesApiBaseUrl}/api/resourcestransactions/employeeTransfers`, { params });
  }

  getEmployeeLanguagesByEmployeeCodes(employeeCodes: string) {
    return this.http.post<EmployeeLanguage[]>(
      `${this.coreService.appSettings.resourcesApiBaseUrl}/api/resourcesLanguage/employeeLanguagesByEmployeeCodes`, { listEmployeeCodes: employeeCodes });
  }

  getCertificationsByEmployeeCodes(employeeCodes: string) {
    return this.http.post<EmployeeCertification[]>(
      `${this.coreService.appSettings.resourcesApiBaseUrl}/api/resourcesCertification/certificationsByEmployees`, { listEmployeeCodes: employeeCodes });
  }

  getEmployeeTimeInLevelByEmployeeCodes(employeeCodes: string) {
    return this.http.post<ResourceTimeInLevel[]>(
      `${this.coreService.appSettings.resourcesApiBaseUrl}/api/resourcesTimeInLevel/timeInLevelByEmployees`, { listEmployeeCodes: employeeCodes });
  }

  getEmployeeWorkAndSchoolHistory(employeeCode: string) {
    const params = new HttpParams({
      fromObject: {
        'employeeCode': employeeCode
      }
    });
    return this.http.get<EmployeeWorkAndSchoolHistory>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/employeeWorkAndSchoolHistory/workSchoolHistoryByEmployeeCode`, { params });
  }

  getEmployeeStaffingPreferences(employeeCode: string): Observable<EmployeeStaffingPreferences[]> {
    const params = new HttpParams({
      fromObject: {
        'employeeCode': employeeCode
      }
    });
    return this.http.get<EmployeeStaffingPreferences[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/employeeStaffingPreference`, { params });
  }

  upsertEmployeeStaffingPreferences(employeeStaffingPreferences: EmployeeStaffingPreferences): Observable<EmployeeStaffingPreferences[]> {
    employeeStaffingPreferences.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    return this.http.put<EmployeeStaffingPreferences[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/employeeStaffingPreference`, employeeStaffingPreferences);
  }

  getCaseRoleAllocationsByOldCaseCodes(oldCaseCodes: string) {
    return this.http.post<CaseRoleAllocation[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocationAggregator/getCaseRoleAllocationsByOldCaseCodes`, { oldCaseCodes });
  }

  getCaseRoleAllocationsByPipelineIds(pipelineIds: string) {
    return this.http.post<CaseRoleAllocation[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/resourceAllocationAggregator/getCaseRoleAllocationsByPipelineIds`, { pipelineIds });
  }

  getResourceActiveStaffableAsByEmployeeCode(employeeCodes: string) {
    return this.http.post<StaffableAsRole[]>(`${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffableAs/activeStaffableAsByEmployeeCodes`, { employeeCodes: employeeCodes });
  }

  getResourceStaffingResponsibleDataByEmployeeCode(employeeCode: string){
    const params = new HttpParams({
      fromObject: {
        'employeeCode': employeeCode
      }
    });
    return this.http.get<EmployeeStaffingInfo>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffingAggregator/getResourceStaffingResponsibeByEmployeeCode`, { params });
  }
  
  upsertResourceStaffingResponsibleData(employeeStaffingInfo:EmployeeStaffingInfo[]){
    employeeStaffingInfo[0].lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    return this.http.post<EmployeeStaffingInfo[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/employeeStaffingInfo/upsertResourceStaffingResponsible`, employeeStaffingInfo);
  }
  

  deleteResourceStaffableAsById(staffableAsRoleId: string) {
    const params = new HttpParams({
      fromObject: {
        'idToDelete': staffableAsRoleId,
        'lastUpdatedBy': this.coreService.loggedInUser.employeeCode
      }
    });
    return this.http.delete(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffableAs`, { params });
  }

  upsertResourceStaffableAs(staffableRoles: StaffableAsRole[]) {
    staffableRoles.map(x => {
      x.lastUpdatedBy = this.coreService.loggedInUser.employeeCode
    });
    return this.http.post<StaffableAsRole[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/staffableAs/upsertResourceStaffableAs`, staffableRoles);
  }

  getPlanningCardDataByPlanningCardId(planningCardId: string) {
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

  getGlobalTrainingsData(employeeCode: string) {

    const url = `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/api/training`;
    const params = new HttpParams({
      fromObject: {
        'employeeCode': employeeCode,
        'effectiveFromDate':'01-01-1900' // Hardcoding the Default Start Date 
      }
    });

    return this.http.get<any[]>( url, { params });
  }

}
