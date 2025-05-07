// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';
import { Subject, BehaviorSubject } from 'rxjs';

import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
import { UserPreferences } from 'src/app/shared/interfaces/userPreferences.interface';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { CaseRoll } from 'src/app/shared/interfaces/caseRoll.interface';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';

@Injectable()
export class OverlayMessageService {

  private refreshResource = new Subject<boolean>();
  private refreshCase = new Subject<boolean>();
  private refreshPlanningCard = new Subject<boolean>();
  private refreshCasesAndOpportunities = new Subject<PlaceholderAllocation[]>();
  private refreshHighlightedResource = new Subject<boolean>();
  private resourceAllocation = new Subject<ResourceAllocation[]>();
  private userPreference = new Subject<UserPreferences>();
  private userExceptionCaseAndOpportunity = new Subject<any>();
  private caseOpporutnity = new Subject<any>();
  private caseRoll = new Subject<any>();

  // -----------------------Send Message--------------------------------------------//
  reosurceAssignmentToCase(resourceAssignment: ResourceAllocation[]) {
    this.resourceAllocation.next(resourceAssignment);
  }

  triggerResourceRefresh() {
    this.refreshResource.next(true);
  }

  triggerCaseAndOpportunityRefresh() {
    this.refreshCase.next(true);
  }
  
  triggerPlanningCardRefresh(){
    this.refreshPlanningCard.next(true);
  }
  triggerCaseAndOpportunityRefreshOnCasePlanning(updatedAllocationData: PlaceholderAllocation[]) {
    this.refreshCasesAndOpportunities.next(updatedAllocationData);
  }

  triggerHighlightedResourcesRefresh(allocationId) {
    this.refreshHighlightedResource.next(allocationId);
  }

  triggerProjectRefreshForCaseRoll(updatedResourceAllocation: ResourceAllocation[], project: Project, caseRoll: CaseRoll) {
    this.caseRoll.next({updatedResourceAllocation, project, caseRoll});
  }

  updateUserPreference(data: UserPreferences) {
    this.userPreference.next(data);
  }

  insertUserExceptionCaseAndopportunityList(oldCaseCode, pipelineid, notifyMessage) {
    this.userExceptionCaseAndOpportunity.next({ oldCaseCode, pipelineid, notifyMessage });
  }

  updateCaseOpporunityOject(oldCaseCode, pipelineId) {
    this.caseOpporutnity.next({ oldCaseCode, pipelineId });
  }

  // -----------------------Get message--------------------------------------------//
  getReosurceAssignmentToCase() {
    return this.resourceAllocation.asObservable();
  }

  refreshResources() {
    return this.refreshResource.asObservable();
  }

  refreshCasesAndopportunties() {
    return this.refreshCase.asObservable();
  }
  
  refreshPlanningCards(){
    return this.refreshPlanningCard.asObservable();
  }
  refreshCasesAndOpportunityOnCasePlanning() {
    return this.refreshCasesAndOpportunities.asObservable();
  }

  refreshHighlightedResources() {
    return this.refreshHighlightedResource.asObservable();
  }

  refreshProjectForCaseRoll() {
    return this.caseRoll.asObservable();
  }

  getUpdatedUserPreferences() {
    return this.userPreference.asObservable();
  }

  getUserExcpetionCaseAndOpportunity() {
    return this.userExceptionCaseAndOpportunity.asObservable();
  }

  getCaseopportunityToUpdate() {
    return this.caseOpporutnity.asObservable();
  }

}
