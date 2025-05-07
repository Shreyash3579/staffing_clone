// -------------------Angular References---------------------------------------//
import { Component, Input, EventEmitter, Output, ViewChildren, QueryList, OnInit } from '@angular/core';

// -------------------Interfaces---------------------------------------//

import { CaseType } from 'src/app/shared/interfaces/caseType.interface';
import { OfficeHierarchy } from 'src/app/shared/interfaces/officeHierarchy.interface';
import { Project } from '../../shared/interfaces/project.interface';
import { ProjectviewComponent } from '../projectview/projectview.component';
import { ServiceLineHierarchy } from 'src/app/shared/interfaces/serviceLineHierarchy';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
import { DateService } from 'src/app/shared/dateService';
import { ValidationService } from 'src/app/shared/validationService';
import { ResourceAllocationService } from 'src/app/shared/services/resourceAllocation.service';
import { NotificationService } from 'src/app/shared/notification.service';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';

@Component({
  selector: 'app-stage',
  templateUrl: './stage.component.html',
  styleUrls: ['./stage.component.scss']
})
export class StageComponent implements OnInit {

  // -----------------------Directives --------------------------------------------//
  @ViewChildren('projectviewComponent') projectviews: QueryList<ProjectviewComponent>;

  // -----------------------Local Variables--------------------------------------------//
  selectedDemandTypes = [];
  projectPlaceHolders = [];
  showAdvancedFilters = false;
  isMinOppProbabilityFilterShown = true;
  showMoreThanYearWarning = false;
  // -----------------------Input Events--------------------------------------------//

  @Input() projects: Project[];
  @Input() caseTypes: CaseType[];
  @Input() demandTypes: any[];
  @Input() officeHierarchy: OfficeHierarchy;
  @Input() staffingTagsHierarchy: ServiceLineHierarchy[];
  @Input() planningCards: PlanningCard[];
  @Input() highlightedResourcesInPlanningCards: [];
  @Input() isStaffedFromSupply : Boolean;

  // -----------------------Output Events--------------------------------------------//

  @Output() getProjectsOnBasicFilterChange = new EventEmitter();
  @Output() getProjectsOnAdvancedFilterChange = new EventEmitter();
  @Output() mapResourceToProject = new EventEmitter();
  @Output() upsertResourceAllocationsToProject = new EventEmitter<any>();
  @Output() openResourceDetailsDialog = new EventEmitter();
  @Output() openProjectDetailsDialog = new EventEmitter();
  @Output() openBackFillForm = new EventEmitter<any>();
  @Output() openOverlappedTeamsForm = new EventEmitter<any>();
  @Output() openProjectDetailsDialogFromTypeahead = new EventEmitter();
  @Output() openCaseRollForm = new EventEmitter<any>();
  @Output() openQuickAddForm = new EventEmitter();
  @Output() openPlaceholderForm = new EventEmitter();
  @Output() showQuickPeekDialog = new EventEmitter<any>();
  @Output() addProjectToUserExceptionHideListEmitter = new EventEmitter<any>();
  @Output() addProjectToUserExceptionShowListEmitter = new EventEmitter<any>();
  @Output() removeProjectFromUserExceptionShowListEmitter = new EventEmitter<any>();
  @Output() upsertPlaceholderEmitter = new EventEmitter<any>();
  @Output() removePlaceHolderEmitter = new EventEmitter<any>();
  @Output() getAllocationsSortedBySelectedValueEmitter = new EventEmitter();
  @Output() addPlanningCardEmitter = new EventEmitter<any>();
  @Output() removePlanningCardEmitter = new EventEmitter<any>();
  @Output() updatePlanningCardEmitter = new EventEmitter<any>();
  @Output() sharePlanningCardEmitter = new EventEmitter<any>();
  @Output() updateDemandCardsSortOrderEmitter = new EventEmitter<any>();
  @Output() updateProjectChanges = new EventEmitter();
  @Output() deleteNotesOnProject = new EventEmitter();
  @Output() mergePlanningCardAndAllocations = new EventEmitter<any>();
  @Output() checkPegRingfenceAllocationAndInsertDownDayCommitments = new EventEmitter<any>();
  @Output() openPegRFPopUpEmitter = new EventEmitter();


  constructor(private _resourceAllocationService: ResourceAllocationService,
    private _notifyService: NotificationService) {
  }
  // -----------------------Component Event Handlers-----------------------------------//
  ngOnInit() {
  }

  // planning card's drag drop events
  dragPlanningCardStartHandler(event) {
    event.dataTransfer.setData("planningCardId", event.target.id);
    event.target.style.opacity = '0.4';
  }

  dragOverPlanningCardHandler(event) {
    event.preventDefault();
  }

  dragEndPlanningCardHandler(event) {
    this.resetCardOpacity(event);
  }

  dropPlanningCardEventEmitterHandler(event) {
    let sourcePlanningCardId = event.dataTransfer.getData("planningCardId");
    let targetPlanningCardId = this.getRootNode(event.target, 'section').id;
    if (this.isDragDropValid(sourcePlanningCardId, targetPlanningCardId)) {
      if (sourcePlanningCardId) {
        document.getElementById(sourcePlanningCardId).style.opacity = '1';
      } else {
        this.showDropErrorMessageOnDifferentCardTypes();
      }
      this.resetCardOpacity(event);
      return;
    }

    if (this.isDroppedTargetPlanningCard(targetPlanningCardId)) {
      this.sortPlanningCards(sourcePlanningCardId, targetPlanningCardId);
    } else {
      this.resetCardOpacity(event);
      this.showDropErrorMessage();
    }
  }

  sortPlanningCards(sourcePlanningCardId, targetPlanningCardId) {
    let indexOfSource = this.planningCards.indexOf(this.planningCards.find(x => x.id === sourcePlanningCardId));
    let indexOfTarget = this.planningCards.indexOf(this.planningCards.find(x => x.id === targetPlanningCardId));
    let sourceClone = this.planningCards[indexOfSource];
    this.planningCards.splice(indexOfSource, 1);
    this.planningCards.splice(indexOfTarget, 0, sourceClone);
    const planningCardSortOrder = this.planningCards.map(x => x.id).join(',');
    this.updateDemandCardsSortOrderEmitter.emit({ planningCardsSortOrder: planningCardSortOrder });
  }

  isDroppedTargetPlanningCard(targetPlanningCardId) {
    return this.planningCards.some(x => x.id === targetPlanningCardId);
  }

  // project's drag drop events
  dragOverProjectHandler(event) {
    event.preventDefault();
  }

  dragProjectStartHandler(event) {
    event.dataTransfer.setData("caseOppUniqueId", event.target.id);
    event.target.style.opacity = '0.4';
  }

  dragEndProjectHandler(event) {
    this.resetCardOpacity(event);
  }

  selectStartHandler(event) {
    event.preventDefault();
  }

  dropProjectEventEmitterHandler(event) {
    let sourceCaseOppUniqueId = event.dataTransfer.getData("caseOppUniqueId");
    let targetCaseOppUniqueId = this.getRootNode(event.target, 'section').id;

    if (this.isDragDropValid(sourceCaseOppUniqueId, targetCaseOppUniqueId)) {
      if (sourceCaseOppUniqueId) {
        document.getElementById(sourceCaseOppUniqueId).style.opacity = '1';
      } else {
        this.showDropErrorMessageOnDifferentCardTypes();
      }
      this.resetCardOpacity(event);
      return;
    }

    if (this.isDroppedTargetPinnedCaseOpp(targetCaseOppUniqueId)) {
      this.sortCaseOppCards(sourceCaseOppUniqueId, targetCaseOppUniqueId);
    } else {
      this.resetCardOpacity(event);
      this.showDropErrorMessage();
    }
  }

  updateProjectChangesHandler(event){
    this.updateProjectChanges.emit(event);
  }

  deleteNotesOnProjectHandler(event){
    this.deleteNotesOnProject.emit(event);
  }

  private isDragDropValid(sourceUniqueId, targetUniqueId) {
    return ValidationService.isStringNullOrEmpty(sourceUniqueId) || ValidationService.isStringNullOrEmpty(targetUniqueId) ||
      (sourceUniqueId == targetUniqueId);
  }

  private sortCaseOppCards(sourceCaseOppUniqueId, targetCaseOppUniqueId) {
    let indexOfSource = this.projects.indexOf(this.projects.find(x => x.oldCaseCode == sourceCaseOppUniqueId || x.pipelineId == sourceCaseOppUniqueId));
    let indexOfTarget = this.projects.indexOf(this.projects.find(x => x.oldCaseCode == targetCaseOppUniqueId || x.pipelineId == targetCaseOppUniqueId));
    let sourceClone = this.projects[indexOfSource];
    this.projects.splice(indexOfSource, 1);
    this.projects.splice(indexOfTarget, 0, sourceClone);
    const pinnedCaseOppSortOrder = this.projects.filter(x => x.isProjectPinned === true).map(x => x.oldCaseCode || x.pipelineId).join(',');
    this.updateDemandCardsSortOrderEmitter.emit({ caseOppSortOrder: pinnedCaseOppSortOrder });
  }

  private isDroppedTargetPinnedCaseOpp(targetCaseOppUniqueId) {
    return this.projects.some(x => (x.pipelineId === targetCaseOppUniqueId || x.oldCaseCode === targetCaseOppUniqueId) && x.isProjectPinned === true);
  }

  private resetCardOpacity(event) {
    event.target.style.opacity = '1';
  }

  private showDropErrorMessageOnDifferentCardTypes() {
    this._notifyService.showWarning('Custom sort only for same card types');
  }

  private showDropErrorMessage() {
    this._notifyService.showWarning('Custom sort only for pinned cases');
  }

  private getRootNode(tag, tagName) {
    if (tag.tagName.toLowerCase() === tagName) {
      return tag;
    } else {
      return this.getRootNode(tag.parentElement, tagName);
    }
  }

  getProjectsOnBasicFilterChangeHandler(event) {
    this.getProjectsOnBasicFilterChange.emit(event);
  }

  getProjectsOnAdvancedFilterChangeHandler(event) {
    this.getProjectsOnAdvancedFilterChange.emit(event);
  }

  upsertResourceAllocationsToProjectHandler(upsertedAllocations) {
    this.upsertResourceAllocationsToProject.emit(upsertedAllocations);
  }

  toggleAdvancedFiltersHandler(showFilter) {
    this.showAdvancedFilters = showFilter;
  }

  showMinOppProbabilityFilterHandler(isMinOppProbabilityFilterShown) {
    this.isMinOppProbabilityFilterShown = isMinOppProbabilityFilterShown;
  }

  openResourceDetailsDialogHandler(employeeCode) {
    this.openResourceDetailsDialog.emit(employeeCode);
  }

  tabbingFromAllocationHandler(event) {
    const projectList = this.projectviews.toArray();

    const project = projectList[event.projectIndex];
    const projectResourceList = project.projectResources.toArray();
    const projectResource = projectResourceList[event.resourceIndex];

    projectResource.editableCol = 'enddate';
    setTimeout(() => {
      projectResource.editEndate();
    }, 0);

  }

  tabbingFromEndDateHandler(event) {
    const projectList = this.projectviews.toArray();
    let project = projectList[event.projectIndex];
    let projectResourceList = project.projectResources.toArray().filter(x =>
      x.placeholderAllocation?.employeeCode !== null);
    const isLastProjectResource = event.resourceIndex === projectResourceList.length - 1;
    let projectResource = null;
    if (!isLastProjectResource) {
      projectResource = projectResourceList[event.resourceIndex + 1];
    } else {
      let indexOfNextProject: number = event.projectIndex + 1;

      for (let i = event.projectIndex + 1; i <= projectList.length; i++) {
        if (projectList[i].projectResources.length > 0) {
          indexOfNextProject = i;
          break;
        }
      }

      project = projectList[indexOfNextProject];
      projectResourceList = project.projectResources.toArray();
      projectResource = projectResourceList[0];
    }
    projectResource.allocationElement.nativeElement.style.display = 'block';
    setTimeout(() => {
      projectResource.editableCol = 'allocation';
      projectResource.allocationElement.nativeElement.select();
      projectResource.allocationElement.nativeElement.focus();
    });
  }

  openProjectDetailsDialogHandler(projectData) {
    this.openProjectDetailsDialog.emit(projectData);
  }

  openBackFillPopUpHandler(event) {
    this.openBackFillForm.emit(event);
  }

  openOverlappedTeamsPopupHandler(event) {
    this.openOverlappedTeamsForm.emit(event);
  }

  openProjectDetailsDialogFromTypeaheadHandler(event) {
    this.openProjectDetailsDialogFromTypeahead.emit(event);
  }

  openCaseRollPopUpHandler(event) {
    this.openCaseRollForm.emit(event);
  }

  openQuickAddFormHandler(event) {
    this.openQuickAddForm.emit(event);
  }

  openPlaceholderFormHandler(event) {
    this.openPlaceholderForm.emit(event);
  }

  showQuickPeekDialogHandler(event) {
    this.showQuickPeekDialog.emit(event);
  }

  addProjectToUserExceptionHideListHandler(event) {
    this.addProjectToUserExceptionHideListEmitter.emit(event);
  }

  addProjectToUserExceptionShowListHandler(event) {
    this.addProjectToUserExceptionShowListEmitter.emit(event);
  }

  removeProjectFromUserExceptionShowListHandler(event) {
    this.removeProjectFromUserExceptionShowListEmitter.emit(event);
  }

  upsertPlaceholderHandler(event, isMergeFromPlanningCard = false, isCopyAndMerge = false) {
    this.upsertPlaceholderEmitter.emit({ allocations: event, isMergeFromPlanningCard: isMergeFromPlanningCard, isCopyAndMerge: isCopyAndMerge });
  }
  removePlaceHolderEmitterHandler(event) {
    this.removePlaceHolderEmitter.emit(event);
  }
  getAllocationsSortedBySelectedValueHandler(event) {
    this.getAllocationsSortedBySelectedValueEmitter.emit(event);
  }

  addPlanningCardHandler() {
    this.addPlanningCardEmitter.emit();
  }

  removePlanningCardEmitterHandler(event) {
    this.removePlanningCardEmitter.emit({ id: event.id });
  }

  updatePlanningCardEmitterHandler(event) {
    this.updatePlanningCardEmitter.emit(event);
  }

  sharePlanningCardEmitterHandler(event) {
    this.sharePlanningCardEmitter.emit(event);
  }

  mergePlanningcardToCaseOppEmitterHandler(event) {
    const project = event.project;
    const planningCard = event.planningCard;

    if (planningCard.placeholderAllocations?.length === 0 && planningCard.regularAllocations?.length === 0) {

      this._notifyService.showWarning(`Allocate resources in order to merge planning card`);
      return true;
    }
    const today = new Date().toLocaleDateString('en-US');
    const startDate = Date.parse(project.startDate) > Date.parse(today)
      ? project.startDate
      : Date.parse(project.endDate) < Date.parse(today)
        ? project.startDate
        : today;

    /*
      * NOTE: We are calculating opportunityEndDate if a resource is allocated to an opportunity that does not any end date or a duration.
      * For an opportunity that is going to start in future,
      * we have set the end date for the allocation as opportunity start date + 30 days.
      *
      * For an opportunuty that has already started, we have set the end date for the allocation as today + 30 days.
      *
      * TODO: Change the logic once Brittany comes up with the solution
    */

    let proposedEndDate = new Date(startDate);
    const defaultAllocationStartDate = new Date(startDate);
    const defaultAllocationEndDate = new Date(project.endDate);
    proposedEndDate.setDate(proposedEndDate.getDate() + 30);

    proposedEndDate = project.endDate !== null
      ? proposedEndDate
      : (!!planningCard.endDate ? new Date(planningCard.endDate) : proposedEndDate);

    const opportunityEndDate = proposedEndDate.toLocaleDateString('en-US');
    const maxEndDate = DateService.getMaxEndDateForAllocation(defaultAllocationStartDate, defaultAllocationEndDate);
    this.showMoreThanYearWarning = ValidationService.checkIfAllocationIsOfOneYear(defaultAllocationStartDate, defaultAllocationEndDate);

    const allocationEndDate = maxEndDate.toLocaleDateString('en-US');

    // if resource being dropped does not have an id that means resource is being dropped from resources list,
    // else its being dropped from one of the cards

    const resourceAllocations: ResourceAllocation[] = planningCard.regularAllocations
      .map(item => {
        return {
          oldCaseCode: project.oldCaseCode,
          caseName: project.caseName,
          clientName: project.clientName,
          pipelineId: project.pipelineId,
          caseTypeCode: project.caseTypeCode,
          opportunityName: project.opportunityName,
          employeeCode: item.employeeCode,
          employeeName: item.employeeName,
          operatingOfficeCode: item.operatingOfficeCode,
          operatingOfficeAbbreviation: item.operatingOfficeAbbreviation,
          currentLevelGrade: item.currentLevelGrade,
          serviceLineCode: item.serviceLineCode,
          serviceLineName: item.serviceLineName,
          allocation: item.allocation,
          startDate: this.getAllocationStartDate(item.startDate, project.startDate),
          endDate: this.getAllocationEndDate(allocationEndDate, opportunityEndDate),
          previousStartDate: null,
          previousEndDate: null,
          previousAllocation: null,
          investmentCode: item.investmentCode,
          investmentName: item.investmentName,
          caseRoleCode: item.caseRoleCode,
          caseStartDate: project.oldCaseCode ? project.startDate : null,
          caseEndDate: project.oldCaseCode ? project.endDate : null,
          opportunityStartDate: !project.oldCaseCode ? project.startDate : null,
          opportunityEndDate: !project.oldCaseCode ? project.endDate : null,
          lastUpdatedBy: null
        };
      });

    let allocationsToUpsert : ResourceAllocation[] ;

    if (resourceAllocations.length > 0) {
      resourceAllocations.forEach(alloc => {
        if (Date.parse(alloc.endDate) < Date.parse(alloc.startDate)) {
          alloc.endDate = alloc.startDate;
        }
      });

      const [isValidAllocation, monthCloseErrorMessage] = this._resourceAllocationService.validateMonthCloseForInsertAndDelete(resourceAllocations);

      if (!isValidAllocation) {
        this._notifyService.showValidationMsg(monthCloseErrorMessage);
        return;
      }

      allocationsToUpsert = this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(resourceAllocations)

    }

    if (event.action === ConstantsMaster.PlanningCardMergeActions.CopyAndMerge) {
      //TODO: update this logic and make it better
      if(allocationsToUpsert?.length > 0){
        this.upsertResourceAllocationsToProject.emit({
          resourceAllocation: allocationsToUpsert,
          showMoreThanYearWarning: this.showMoreThanYearWarning,
          allocationDataBeforeSplitting: resourceAllocations
        });
      }
      this.copyAndMergePlanningCard(planningCard, project, allocationEndDate, opportunityEndDate);
    } else {
      this.mergeAndUpdatePlanningCard(planningCard, allocationsToUpsert, project, allocationEndDate, opportunityEndDate, resourceAllocations);
    }
  }

  private copyAndMergePlanningCard(planningCard, project, allocationEndDate, opportunityEndDate) {
    const placeholderAllocations: PlaceholderAllocation[] = Object.assign([], planningCard).placeholderAllocations
      .map(item => {
        return {
          id: null,
          planningCardId: null,
          oldCaseCode: project.oldCaseCode,
          caseName: project.caseName,
          clientName: project.clientName,
          caseTypeCode: project.caseTypeCode,
          pipelineId: project.pipelineId,
          opportunityName: project.opportunityName,
          employeeCode: item.employeeCode,
          employeeName: item.employeeName,
          operatingOfficeCode: item.operatingOfficeCode,
          operatingOfficeAbbreviation: item.operatingOfficeAbbreviation,
          currentLevelGrade: item.currentLevelGrade,
          serviceLineCode: item.serviceLineCode,
          serviceLineName: item.serviceLineName,
          allocation: item.allocation,
          startDate: this.getAllocationStartDate(item.startDate, project.startDate),
          endDate: this.getAllocationEndDate(allocationEndDate, opportunityEndDate),
          investmentCode: item.investmentCode,
          investmentName: item.investmentName,
          caseRoleCode: item.caseRoleCode,
          caseStartDate: project.oldCaseCode ? project.startDate : null,
          caseEndDate: project.oldCaseCode ? project.endDate : null,
          opportunityStartDate: !project.oldCaseCode ? project.startDate : null,
          opportunityEndDate: !project.oldCaseCode ? project.endDate : null,
          isPlaceholderAllocation: item.isPlaceholderAllocation,
          positionGroupCode: item.positionGroupCode,
          lastUpdatedBy: null
        };
      });

    placeholderAllocations?.forEach(alloc => {
      if (Date.parse(alloc.endDate) < Date.parse(alloc.startDate)) {
        alloc.endDate = alloc.startDate;
      }
    });
    this.upsertPlaceholderHandler(placeholderAllocations, true, true);
  }

  private mergeAndUpdatePlanningCard(planningCard : PlanningCard, regularAllocationsToUpsert: ResourceAllocation[], project, allocationEndDate, opportunityEndDate, resourceAllocations) {
    const placeholderAllocations: PlaceholderAllocation[] = planningCard.placeholderAllocations
      .map(item => {
        return {
          id: item.id,
          planningCardId: null,
          oldCaseCode: project.oldCaseCode,
          caseTypeCode: project.caseTypeCode,
          caseName: project.caseName,
          clientName: project.clientName,
          pipelineId: project.pipelineId,
          opportunityName: project.opportunityName,
          employeeCode: item.employeeCode,
          employeeName: item.employeeName,
          operatingOfficeCode: item.operatingOfficeCode,
          operatingOfficeAbbreviation: item.operatingOfficeAbbreviation,
          currentLevelGrade: item.currentLevelGrade,
          serviceLineCode: item.serviceLineCode,
          serviceLineName: item.serviceLineName,
          allocation: item.allocation,
          startDate: this.getAllocationStartDate(item.startDate, project.startDate),
          endDate: this.getAllocationEndDate(allocationEndDate, opportunityEndDate),
          investmentCode: item.investmentCode,
          investmentName: item.investmentName,
          caseRoleCode: item.caseRoleCode,
          caseStartDate: project.oldCaseCode ? project.startDate : null,
          caseEndDate: project.oldCaseCode ? project.endDate : null,
          opportunityStartDate: !project.oldCaseCode ? project.startDate : null,
          opportunityEndDate: !project.oldCaseCode ? project.endDate : null,
          isPlaceholderAllocation: item.isPlaceholderAllocation,
          positionGroupCode: item.positionGroupCode,
          lastUpdatedBy: null
        };
      });

    placeholderAllocations?.forEach(alloc => {
      if (Date.parse(alloc.endDate) < Date.parse(alloc.startDate)) {
        alloc.endDate = alloc.startDate;
      }
    });

    //Update  planning card as merged and save case
    planningCard.mergedCaseCode = project.oldCaseCode;
    planningCard.isMerged = true;

    var payload = {
      resourceAllocations: regularAllocationsToUpsert ?? [],
      placeholderAllocations : placeholderAllocations ?? [],
      planningCard: planningCard,
      allocationDataBeforeSplitting: resourceAllocations
    }
    this.mergePlanningCardAndAllocations.emit(payload);
  }

  private getAllocationStartDate(allocationStartDate, caseOppStartDate) {
    const today = new Date().toLocaleDateString('en-US');
    const date7DaysAgo = new Date(new Date().setDate(new Date().getDate() - 7)).toLocaleDateString('en-US');
    if (caseOppStartDate === null && allocationStartDate === null) {
      return today;
    }
    if (caseOppStartDate === null && allocationStartDate !== null) {
      return DateService.convertDateInBainFormat(allocationStartDate);
    }
    if (Date.parse(caseOppStartDate) < Date.parse(date7DaysAgo)) {
      return today;
    }
    if (Date.parse(caseOppStartDate) >= Date.parse(date7DaysAgo) && Date.parse(caseOppStartDate) <= Date.parse(today)) {
      return DateService.convertDateInBainFormat(caseOppStartDate);
    }
    if (Date.parse(caseOppStartDate) > Date.parse(today) && allocationStartDate) {
      return DateService.convertDateInBainFormat(allocationStartDate);
    }
    return today;
  }
  private getAllocationEndDate(allocationEndDate, opportunityEndDate) {
    return allocationEndDate !== null
      ? DateService.convertDateInBainFormat(allocationEndDate)
      : DateService.convertDateInBainFormat(opportunityEndDate);
  }

  openPegRFPopUpHandler(pegOpportunityId){
    this.openPegRFPopUpEmitter.emit(pegOpportunityId);
  }
}
