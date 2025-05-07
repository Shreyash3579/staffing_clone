import { Component, EventEmitter, Input, OnInit, Output, QueryList, SimpleChanges, ViewChildren } from '@angular/core';
import { DateService } from 'src/app/shared/dateService';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { ProjectsGroup } from 'src/app/shared/interfaces/projectGroup.interface';
import { PlanningCardGroup } from 'src/app/shared/interfaces/planningCardGroup.interface';
import { ProjectviewComponent } from './projectview/projectview.component';
import { PlanningCardComponent } from './planning-card/planning-card.component';
import { CommonService } from 'src/app/shared/commonService';
import { NotificationService } from 'src/app/shared/notification.service';
import { ValidationService } from 'src/app/shared/validationService';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
import { ResourceAllocationService } from 'src/app/shared/services/resourceAllocation.service';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';
import { Subscription } from 'rxjs';
import { HomeHelperService } from '../../homeHelper.service';

@Component({
  selector: 'app-demand',
  templateUrl: './demand.component.html',
  styleUrls: ['./demand.component.scss']
})
export class DemandComponent implements OnInit {


  @ViewChildren('projectviewComponent') projectviews: QueryList<ProjectviewComponent>;
  // -----------------------Local Variables--------------------------------------------//

  projectGroups: ProjectsGroup[] = [];
  @Input() highlightedResourcesInPlanningCards: string[] = [];

  //planningCardGroups: PlanningCardGroup[] = [];
  showMoreThanYearWarning = false;
  editableCol = '';
  subscription: Subscription = new Subscription();
  collapseNewDemandAll: Boolean = false;

  // -----------------------Input Variables--------------------------------------------//
  @Input() planningCards: PlanningCard[];
  @Input() projects: Project[];
  @Input() groupingArray: string[] = [];
  @Input() selectedGroupingOption: string;

  /// -----------------------Output Events--------------------------------------------//
  @Output() upsertResourceAllocationsToProjectEmitter = new EventEmitter<any>();
  @Output() upsertPlaceholderAllocationEmitter = new EventEmitter<any>();
  @Output() removePlaceHolderEmitter = new EventEmitter();
  @Output() openPegRFPopUpEmitter = new EventEmitter();
  @Output() showQuickPeekDialog = new EventEmitter<any>();
  @Output() mergePlanningCardAndAllocations = new EventEmitter<any>();
  @Output() openCaseRollForm = new EventEmitter<any>();
  @Output() openPlaceholderForm = new EventEmitter();
  @Output() upsertPlaceholderEmitter = new EventEmitter<any>();
  @Output() updatePlanningCardEmitter = new EventEmitter<any>();
  @Output() addProjectToUserExceptionHideListEmitter = new EventEmitter<any>();
  @Output() addProjectToUserExceptionShowListEmitter = new EventEmitter<any>();
  @Output() removeProjectFromUserExceptionShowListEmitter = new EventEmitter<any>();
  @Output() addCardPlaceholderEmitter = new EventEmitter();
  @Output() removePlanningCardEmitter = new EventEmitter();
  @Output() sharePlanningCardEmitter = new EventEmitter();
  @Output() openOverlappedTeamsForm = new EventEmitter<any>();

  constructor(private _resourceAllocationService: ResourceAllocationService,
    private _notifyService: NotificationService,
    private homeHelperService: HomeHelperService) { }

  ngOnInit(): void {
    this.subscribeEvents();
  }

  ngOnChanges(changes : SimpleChanges) {

    if(changes.groupingArray && changes.groupingArray.currentValue && this.projects && this.planningCards) {    
      this.convertProjectsDataToBuckets();
    }

    if(this.projects && this.planningCards) {
      this.convertProjectsDataToBuckets();
    }  

  }

  trackByFn(index, item) {
    return item.groupTitle;
  }

  trackByFnForPlanningCard(index, item) {
    return item.id;
  }

  trackByFnForProject(index, item) {
    return item.pipelineId ?? item.oldCaseCode;
  }

  subscribeEvents() {
    this.subscription.add(this.homeHelperService.getCollapseNewDemandAll().subscribe(result => {    
      this.collapseNewDemandAll = result;
    }))
  }

  //divide projects in buckets where each bucket represents a week or day based on the selected view
  convertProjectsDataToBuckets() {
    this.projectGroups = [];
    this.groupingArray.forEach(currDate => {
      const dailyData: ProjectsGroup = {
        groupTitle: currDate,
        projects: this.getProjectsForWeeklyOrDailyView(currDate, this.projects),
        planningCards: this.getProjectsForWeeklyOrDailyView(currDate, this.planningCards)
      }
      this.projectGroups.push(dailyData);
    })
  }


  //filter projects according to the bucket in which they lie
  getProjectsForWeeklyOrDailyView(currDate, projects) {
    const startDate = new Date(currDate).getTime();
    const endDate = new Date(DateService.getEndOfWeek(true, currDate)).getTime();
    let filteredProjects;
    // filter projects for the week
    if(this.selectedGroupingOption == 'Weekly'){
      filteredProjects = projects.filter(project => {
        const projectStartDate = new Date(project.startDate).getTime();
        return (projectStartDate <= endDate && projectStartDate >= startDate);
      });
    }
     // filter projects for the day
    else{
      filteredProjects = projects.filter(project => {
        const projectStartDate = new Date(project.startDate).getTime();
        return (projectStartDate === startDate);
      });
    }
    
    return filteredProjects;
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
        this.upsertResourceAllocationsToProjectEmitter.emit({
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

  upsertResourceAllocationsToProjectHandler(upsertedAllocationsData){
    this.upsertResourceAllocationsToProjectEmitter.emit(upsertedAllocationsData);
  }

  removePlaceHolderHandler(event){
    this.removePlaceHolderEmitter.emit(event);
  }

  openPegRFPopUpHandler(pegOpportunityId){
    this.openPegRFPopUpEmitter.emit(pegOpportunityId);
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

  openOverlappedTeamsPopupHandler(event) {
    this.openOverlappedTeamsForm.emit(event);
  } 
  
  openCaseRollPopUpHandler(event) {
    this.openCaseRollForm.emit(event);
  }

  openPlaceholderFormHandler(event) {
    this.openPlaceholderForm.emit(event);
  }

  upsertPlaceholderHandler(payload, isMergeFromPlanningCard = false, isCopyAndMerge = false) {
    payload = payload.placeholderAllocation ? payload.placeholderAllocation : payload;

    this.upsertPlaceholderEmitter.emit({ 
      allocations: payload, 
      isMergeFromPlanningCard: isMergeFromPlanningCard, 
      isCopyAndMerge: isCopyAndMerge });
  }
  
  updatePlanningCardEmitterHandler(event) {
    this.updatePlanningCardEmitter.emit(event);
  }

  removePlanningCardEmitterHandler(event) {
    this.removePlanningCardEmitter.emit({ id: event.id });
  }

  addPlanningCard(weekIndex) {
    let startDate = DateService.getFormattedDate(new Date(this.groupingArray[weekIndex]));

    // Create a new Date object for endDate and add 3 months
    let endDate = new Date(this.groupingArray[weekIndex]);
    endDate.setMonth(endDate.getMonth() + 3);

    // Format the endDate as a string
    let formattedEndDate = DateService.getFormattedDate(endDate);

    this.addCardPlaceholderEmitter.emit({
        startDate: startDate,
        endDate: formattedEndDate,
        probabilityPercent: 100 //we want it as 100 for newly created planning cards
    });
  }

  sharePlanningCardEmitterHandler(event) {
    this.sharePlanningCardEmitter.emit(event);
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


}

