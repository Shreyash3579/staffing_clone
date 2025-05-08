// ------------------- Angular modules ---------------------------------------//
import { Component, OnInit, Input, Output, EventEmitter, ViewChildren, QueryList, SimpleChanges } from '@angular/core';
import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';

// ------------------- Interfaces ---------------------------------------//
import { Project } from '../../../../shared/interfaces/project.interface';
import { Resource } from '../../../../shared/interfaces/resource.interface';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';

// ------------------- Project Reference ---------------------------------------//
//import { ProjectResourceComponent } from '../../../../app/home-copy/demand/projectview/project-resource/project-resource.component';
import { ProjectType, ServiceLine } from 'src/app/shared/constants/enumMaster';
import { DateService } from 'src/app/shared/dateService';
import { ValidationService } from 'src/app/shared/validationService';
import { ResourceAllocationService } from 'src/app/shared/services/resourceAllocation.service';
import { NotificationService } from 'src/app/shared/notification.service';
import { ResourceOrCasePlanningViewNote } from 'src/app/shared/interfaces/resource-or-case-planning-view-note.interface';
import { OverlayDialogService } from 'src/app/overlay/dialogHelperService/overlayDialog.service';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';
import { Store } from "@ngrx/store";
import * as StaffingDemandActions from "src/app/home-copy/state/actions/staffing-demand.action";
import * as fromStaffingDemand from "src/app/home-copy/state/reducers/staffing-demand.reducer";
import { NotesModalComponent } from 'src/app/shared/notes-modal/notes-modal.component';
import { ProjectResourceComponent } from '../project-resource/project-resource.component';
import { CoreService } from 'src/app/core/core.service';
import { staCommitmentDialogService } from 'src/app/overlay/dialogHelperService/staCommitmentCaseOppDialog.service';


@Component({
  selector: 'app-projectview',
  templateUrl: './projectview.component.html',
  styleUrls: ['./projectview.component.scss']
})
export class ProjectviewComponent implements OnInit {
  // ----------------------- Directives --------------------------------------------//
  @ViewChildren('projectResourceComponent') projectResources: QueryList<ProjectResourceComponent>;

  // -----------------------Input Variables--------------------------------------------//
  @Input() project: Project;
  @Input() projectIndex: number;
  @Input() collapseAll: boolean = false;
  @Input() expandedCompleteScreen: boolean = false;

  // --------------------- Placeholder card ----------------------------------

  // -----------------------Output Events--------------------------------------------//
  @Output() upsertResourceAllocationsToProjectEmitter = new EventEmitter<any>();
  @Output() upsertPlaceholderEmitter = new EventEmitter<any>();
  @Output() removePlaceHolderEmitter = new EventEmitter();
  @Output() showQuickPeekDialog = new EventEmitter();
  @Output() openCaseRollForm = new EventEmitter<any>(); 
  @Output() openShortTermAvailableCommitmentForm = new EventEmitter<any>();
  @Output() openPlaceholderForm = new EventEmitter();
  @Output() addProjectToUserExceptionHideListEmitter = new EventEmitter<any>();
  @Output() addProjectToUserExceptionShowListEmitter = new EventEmitter<any>();
  @Output() removeProjectFromUserExceptionShowListEmitter = new EventEmitter<any>();
  @Output() removePlanningCardEmitter = new EventEmitter();
  @Output()  openOverlappedTeamsPopup = new EventEmitter();
  @Output() tabbingFromAllocation = new EventEmitter();
  @Output() tabbingFromEndDate = new EventEmitter();
  @Output() openPegRFPopUpEmitter = new EventEmitter();
  

  // -----------------------Local Variables--------------------------------------------//
  activeResourcesEmailAddresses = '';
  showSKUTerms = false;
  skuTerm = '';
  projectTitle = '';
  projectType = '';
  showMoreThanYearWarning = false;
  placeholderLists: any = [];
  showPegRFIcon = false;

  // ----------------------- Constructor --------------------------------------------//
  constructor(private resourceAllocationHelperService: ResourceAllocationService,
    private notifyService: NotificationService,
    private overlayDialogService: OverlayDialogService,
     private modalService: BsModalService,
     private demandStore: Store<fromStaffingDemand.State>,
    private coreService: CoreService,
    private staCommitmentDialogService: staCommitmentDialogService) { }

  // --------------------------Component LifeCycle Events----------------------------//

  ngOnInit() {
    this.projectType = this.project.type;
  }

  ngOnChanges(changes : SimpleChanges) {
    if(changes.project && this.project) {
      this.getActiveResourcesEmailAddress();
      this.loggedInUserHasAccessToSeePegRFPopUp();
    }
    if (this.project.skuCaseTerms) {
      this.skuTerm = this.project.skuCaseTerms.skuTerms.map(s => s.name).toString();
    }
  }

  // -------------------Component Event Handlers-------------------------------------//

  checkIfAllocationIdExist(event) {
    return event.previousContainer.data[event.previousIndex].id;
  }

  loggedInUserHasAccessToSeePegRFPopUp() {
    this.showPegRFIcon = this.coreService.loggedInUserClaims.PegC2CAccess && !!this.project.pegOpportunityId;
  } 

  onResourceDrop(event: CdkDragDrop<any>) {

    const data = event.previousContainer.data;

    // These changes are done to get index of dropped element as it correct index was not available 
    // in event.previousIndex when columns are in expanded form
    let droppedElement = event.item.element.nativeElement.id;
    const parts = droppedElement.split("_");
    const droppedElementIndex = parts[parts.length - 1];

    // if dropping anything other than resource then return
    if (!Array.isArray(data)) {
      return;
    }
    // if element is dragged and dropped from and to the same card, then do nothing
    if (event.container.id === event.previousContainer.id) {
      return;
    }
    // if element is terminated then drag and drop is not allowed
    if (event.previousContainer.data[event.previousIndex].terminationDate) {
      this.notifyService.showValidationMsg(ValidationService.terminatedEmployeeAllocation);
      return;
    }

    let resourceAllocation: ResourceAllocation;

    const isAllocationOnPlanningCard = !!event.previousContainer.data[event.previousIndex].planningCardId;
    const allocation = event.previousContainer.data[event.previousIndex];
    /*
      * NOTE: We are calculating opportunityEndDate if a resource is allocated to an opportunity that does not any end date or a duration.
      * For an opportunity that is going to start in future,
      * we have set the end date for the allocation as opportunity start date + 30 days.
      *
      * For an opportunuty that has already started, we have set the end date for the allocation as today + 30 days.
      *
      * TODO: Change the logic once Brittany comes up with the solution
    */

    let [startDate, endDate, showMoreThanYearWarning] = this.resourceAllocationHelperService.getAllocationDates(this.project.startDate, this.project.endDate);
    this.showMoreThanYearWarning = showMoreThanYearWarning;

    // if resource being dropped does not have an id that means resource is being dropped from resources list,
    // else its being dropped from one of the cards
    if (this.checkIfAllocationIdExist(event)) {
      const staffableEmployee: ResourceAllocation = event.previousContainer.data[event.previousIndex];

      resourceAllocation = {
        // if dragging an allocation from planning card then generate a new id
        id: isAllocationOnPlanningCard ? null : staffableEmployee.id,
        oldCaseCode: this.project.oldCaseCode,
        caseName: this.project.caseName,
        caseTypeCode: this.project.caseTypeCode,
        clientName: this.project.clientName,
        pipelineId: this.project.pipelineId,
        opportunityName: this.project.opportunityName,
        employeeCode: staffableEmployee.employeeCode,
        employeeName: staffableEmployee.employeeName,
        internetAddress: staffableEmployee.internetAddress,
        operatingOfficeCode: staffableEmployee.operatingOfficeCode,
        operatingOfficeAbbreviation: staffableEmployee.operatingOfficeAbbreviation,
        currentLevelGrade: staffableEmployee.currentLevelGrade,
        serviceLineCode: staffableEmployee.serviceLineCode,
        serviceLineName: staffableEmployee.serviceLineName,
        allocation: staffableEmployee.allocation,
        startDate: DateService.convertDateInBainFormat(startDate),
        endDate: endDate,
        previousStartDate: staffableEmployee.startDate,
        previousEndDate: staffableEmployee.endDate,
        previousAllocation: staffableEmployee.allocation,
        investmentCode: null,
        investmentName: null,
        caseRoleCode: staffableEmployee.caseRoleCode,
        caseRoleName: staffableEmployee.caseRoleName,
        caseStartDate: this.project.oldCaseCode ? this.project.startDate : null,
        caseEndDate: this.project.oldCaseCode ? this.project.endDate : null,
        opportunityStartDate: !this.project.oldCaseCode ? this.project.startDate : null,
        opportunityEndDate: !this.project.oldCaseCode ? this.project.endDate : null,
        lastUpdatedBy: null
      };

      let [isValidAllocation, monthCloseErrorMessage] = [false, ""];
      if (isAllocationOnPlanningCard) {
        [isValidAllocation, monthCloseErrorMessage] = this.resourceAllocationHelperService.validateMonthCloseForInsertAndDelete(resourceAllocation);

      } else {
        [isValidAllocation, monthCloseErrorMessage] = this.resourceAllocationHelperService.validateMonthCloseForUpdates(resourceAllocation, staffableEmployee);

      }

      if (!isValidAllocation) {
        this.notifyService.showValidationMsg(monthCloseErrorMessage);
        return;
      }

    } else {
      const staffableEmployee: Resource = event.previousContainer.data[droppedElementIndex];

      [startDate, endDate] = this.resourceAllocationHelperService.getAllocationDatesForNotYetStartedEmployee(staffableEmployee.startDate, startDate, endDate);
      if (startDate === null) {
        return;
      }
      resourceAllocation = {
        oldCaseCode: this.project.oldCaseCode,
        caseName: this.project.caseName,
        caseTypeCode: this.project.caseTypeCode,
        clientName: this.project.clientName,
        pipelineId: this.project.pipelineId,
        opportunityName: this.project.opportunityName,
        employeeCode: staffableEmployee.employeeCode,
        employeeName: staffableEmployee.fullName,
        operatingOfficeCode: staffableEmployee.schedulingOffice.officeCode,
        operatingOfficeAbbreviation: staffableEmployee.schedulingOffice.officeAbbreviation,
        internetAddress: staffableEmployee.internetAddress,
        currentLevelGrade: staffableEmployee.levelGrade,
        serviceLineCode: staffableEmployee.serviceLine.serviceLineCode,
        serviceLineName: staffableEmployee.serviceLine.serviceLineName,
        allocation: parseInt(staffableEmployee.percentAvailable.toString(), 10),
        startDate: DateService.convertDateInBainFormat(startDate),
        endDate: endDate,
        previousStartDate: null,
        previousEndDate: null,
        previousAllocation: null,
        investmentCode: null,
        investmentName: null,
        caseRoleCode: null,
        caseStartDate: this.project.oldCaseCode ? this.project.startDate : null,
        caseEndDate: this.project.oldCaseCode ? this.project.endDate : null,
        opportunityStartDate: !this.project.oldCaseCode ? this.project.startDate : null,
        opportunityEndDate: !this.project.oldCaseCode ? this.project.endDate : null,
        lastUpdatedBy: null
      };

      const [isValidAllocation, monthCloseErrorMessage] = this.resourceAllocationHelperService.validateMonthCloseForInsertAndDelete(resourceAllocation);

      if (!isValidAllocation) {
        this.notifyService.showValidationMsg(monthCloseErrorMessage);
        return;
      }

    }

    if (!this.validateResourceData(resourceAllocation)) {
      return;
    }

    
    // if (this.resourceAllocationHelperService.isBackFillRequiredOnProject(this.project.allocatedResources,
    //   this.project)) {

    //   if (this.checkIfAllocationIdExist(event)) {
    //     this.openBackFillPopUp.emit({
    //       project: this.project,
    //       resourceAllocation: resourceAllocation,
    //       showMoreThanYearWarning: this.showMoreThanYearWarning
    //     });
    //   }
    //   else {
    //     this.openBackFillPopUp.emit({
    //       project: this.project,
    //       resourceAllocation: resourceAllocation,
    //       showMoreThanYearWarning: this.showMoreThanYearWarning,
    //       allocationDataBeforeSplitting: [].concat(resourceAllocation)
    //     });
    //   }

    // } else 
    {

      const projectStartDate = DateService.convertDateInBainFormat(this.project.startDate);
      const projectEndDate = DateService.convertDateInBainFormat(this.project.endDate);

      let allocationsData: ResourceAllocation[] = [];

      if (projectStartDate && projectEndDate) {

        allocationsData = this.resourceAllocationHelperService.checkAndSplitAllocationsForPrePostRevenue(resourceAllocation);

      } else {

        allocationsData.push(resourceAllocation);

      }


      if (this.checkIfAllocationIdExist(event)) {
        this.upsertResourceAllocationsToProjectEmitter.emit({
          resourceAllocation: allocationsData,
          event: 'dragdrop',
          showMoreThanYearWarning: this.showMoreThanYearWarning
        });
      } else {
        this.upsertResourceAllocationsToProjectEmitter.emit({
          resourceAllocation: allocationsData,
          event: 'dragdrop',
          showMoreThanYearWarning: this.showMoreThanYearWarning,
          allocationDataBeforeSplitting: [].concat(resourceAllocation)
        });
      }
    
      this.project.allocatedResources.splice(event.currentIndex, 0, ...allocationsData);
      event.previousContainer.data.splice(event.previousIndex, 1);

      if (isAllocationOnPlanningCard) {
        this.removePlaceHolderEmitter.emit({
          placeholderAllocation: [].concat(allocation),
          placeholderIds: allocation.id,
          notifyMessage: null
        });
      }

    }
  }

  // WILL REQUIRE THESE COMMENTED METHODS AS WE WILL IMPLEMENT THIS FUNCTIONALITY ON STAFFING 2.0
  
  // caseDragMouseDown(id) {
  //   if (!id) {
  //     this.notifyService.showWarning('Please wait for save to complete');
  //   }
  // }

  // openResourceDetailsDialogHandler(employeeCode) {
  //   this.openResourceDetailsDialog.emit(employeeCode);
  // }

  // updateResourceToProjectHandler(updatedResource) {
  //   this.mapResourceToProject.emit(updatedResource);
  // }

  updateAllocationsPositionInArray(initialAllocationIndex, upsertedAllocations) {
    // When allocations are split after update, then show them in case-card
    if (Array.isArray(upsertedAllocations) && upsertedAllocations.length > 1) {
      this.project.allocatedResources.splice(initialAllocationIndex, 1);
      this.project.allocatedResources.splice(initialAllocationIndex, 0, ...upsertedAllocations);
    }
  }

  openPegRFPopUpHandler() {
  this.openPegRFPopUpEmitter.emit(this.project.pegOpportunityId);
  }

  upsertResourceAllocationsToProjectHandler(upsertedAllocations) {
    this.updateAllocationsPositionInArray(upsertedAllocations.initialAllocationIndex, upsertedAllocations.resourceAllocation);

    this.upsertResourceAllocationsToProjectEmitter.emit(upsertedAllocations);
  }

  confirmPlaceholderAllocationHandler(placeholderAllocation) {
    if (!this.validateResourceData(placeholderAllocation)) {
      return;
    }

    this.openPlaceholderForm.emit({
      placeholderAllocationData: placeholderAllocation,
      isUpdateModal: true,
      projectData: this.project
    });
  }

  tabbingFromAllocationHandler(resourceIndex) {
    this.tabbingFromAllocation.emit({ resourceIndex: resourceIndex, projectIndex: this.projectIndex });
  }

  tabbingFromEndDateHandler(resourceIndex) {
    this.tabbingFromEndDate.emit({ resourceIndex: resourceIndex, projectIndex: this.projectIndex });
  }

  openProjectDetailsDialogHandler(projectData) {
    this.overlayDialogService.openProjectDetailsDialogHandler({ oldCaseCode: projectData.oldCaseCode, pipelineId: projectData.pipelineId });
    //this.openProjectDetailsDialog.emit({ oldCaseCode: projectData.oldCaseCode, pipelineId: projectData.pipelineId });
  }

  toggleSkuSizeDiv(projectData) {
    this.showSKUTerms = !this.showSKUTerms;
  }

  onTogglePinHandler(isPinned: boolean) {

    const pipelineId = this.project.pipelineId || null;
    const oldCaseCode = this.project.oldCaseCode || null;

    if (isPinned) {

      this.project.isProjectPinned = true;
      this.addProjectToUserExceptionShowListEmitter.emit({ pipelineId, oldCaseCode });

    } else {

      this.project.isProjectPinned = false;
      this.removeProjectFromUserExceptionShowListEmitter.emit({ pipelineId, oldCaseCode });

    }

  }

  onToggleHideHandler(isHidden: boolean) {
    const pipelineId = this.project.pipelineId || null;
    const oldCaseCode = this.project.oldCaseCode || null;

    if (isHidden) {

      this.addProjectToUserExceptionHideListEmitter.emit({ pipelineId, oldCaseCode });

    }

  }

  onCaseRollHandler(event : any) {

    if (!ValidationService.isCaseEligibleForRoll(this.project.endDate)) {
      this.notifyService.showValidationMsg(ValidationService.caseRollNotAllowedForInActiveCasesMessage);
    } else {
      this.openCaseRollForm.emit({ project: this.project });
    }

  }


  shortTermAvailableCaseOppCommitmentEmitterHandler() {
    this.staCommitmentDialogService.openSTACommitmentFormHandler({ project: this.project });
  }

  openPersistentTeamPopupHandler(){
    const modalData = {
      projectData: this.project,
      overlappedTeams : null,
      allocation: this.project.allocatedResources[0]
    };

    this.openOverlappedTeamsPopup.emit(modalData);
  }

  removeResourceFromPlaceholderHandler(event) {
    const selectedResource = this.placeholderLists.findIndex(function (value) {
      return value.employeeCode === event;
    });
    this.placeholderLists.splice(selectedResource, 1, 'placeholder');
  }

  addResourceToProjectHandler(event) {
    const selectedResource = this.placeholderLists.findIndex(function (value) {
      return value.employeeCode === event;
    });

    this.project.allocatedResources.push(this.placeholderLists[selectedResource]);
    this.placeholderLists.splice(selectedResource, 1);

  }

  removePlaceHolderEmitterHandler(placeholderAllocation) {
    this.removePlaceHolderEmitter.emit({
      placeholderAllocation: [].concat(placeholderAllocation),
      placeholderIds: placeholderAllocation.id,
      notifyMessage: 'Placeholder Deleted'
    });
  }

  onAddPlaceHolderHandler(event) {
    const placeholder: PlaceholderAllocation = {
      id: null,
      planningCardId: null,
      oldCaseCode: this.project.oldCaseCode,
      caseName: this.project.caseName,
      clientName: this.project.clientName,
      pipelineId: this.project.pipelineId,
      opportunityName: this.project.opportunityName,
      employeeCode: null,
      employeeName: null,
      operatingOfficeCode: Number(this.project.managingOfficeCode),
      operatingOfficeAbbreviation: this.project.managingOfficeAbbreviation,
      currentLevelGrade: null,
      serviceLineCode: ServiceLine.GeneralConsulting,
      serviceLineName: "General Consulting",
      allocation: null,
      startDate: null,
      endDate: null,
      investmentCode: null,
      investmentName: null,
      caseRoleCode: null,
      caseStartDate: this.project.oldCaseCode ? this.project.startDate : null,
      caseEndDate: this.project.oldCaseCode ? this.project.endDate : null,
      opportunityStartDate: !this.project.oldCaseCode ? this.project.startDate : null,
      opportunityEndDate: !this.project.oldCaseCode ? this.project.endDate : null,
      lastUpdatedBy: null
    };

    // this.project.placeholderAllocations.push(placeholder);

    this.upsertPlaceholderEmitter.emit(placeholder);
  }

  upsertPlaceholderAllocationHandler(placeholderAllocation) {
    this.project.placeholderAllocations.map(obj => obj.id === placeholderAllocation.id || obj);
    this.upsertPlaceholderEmitter.emit(placeholderAllocation);
  }

  quickPeekIntoResourcesCommitmentsHandler() {
    let employeesOnPlaceholders = this.project.placeholderAllocations?.map(x => {
      return {
        employeeCode: x.employeeCode,
        employeeName: x.employeeName,
        levelGrade: x.currentLevelGrade,
      };
    });
    employeesOnPlaceholders = employeesOnPlaceholders.filter(employee => !!employee.employeeCode);

    let employeesOnRegularAllocations = this.project.allocatedResources?.map(x => {
      return {
        employeeCode: x.employeeCode,
        employeeName: x.employeeName,
        levelGrade: x.currentLevelGrade,
      };
    });
    
    let employees = Array.from(new Set([...employeesOnPlaceholders, ...employeesOnRegularAllocations]));

    this.showQuickPeekDialog.emit(employees);
  }

  toggleNoteModal() {
    const cardObject = {
      name: this.project.caseName || this.project.opportunityName,
      data: {
        oldCaseCode: this.project.oldCaseCode,
        pipelineId: this.project.pipelineId,
        planningCardId: this.project.planningCardId,
        notes: this.project.casePlanningViewNotes
      },
      //casePlanningNotes: this.project.casePlanningViewNotes
    };

    const modalRef = this.modalService.show(NotesModalComponent, {
      initialState: {
        cardData: cardObject
      },
      class: "demand-notes-modal modal-dialog-centered",
      ignoreBackdropClick: false,
      backdrop: false
    });

    modalRef.content.setNotes.subscribe((casePlanningViewNotes) => {
      this.project.casePlanningViewNotes = casePlanningViewNotes;
    });

    modalRef.content.upsertNotes.subscribe((upsertedNote: ResourceOrCasePlanningViewNote) => {
      this.demandStore.dispatch( new StaffingDemandActions.UpsertCaseViewNotes(upsertedNote));
    });

    modalRef.content.deleteNotes.subscribe((caseNoteIdToDelete) => {
      this.demandStore.dispatch( new StaffingDemandActions.DeleteCaseViewNotes(caseNoteIdToDelete));
    })
  }

  // -------------------Local functions -------------------------------------//

  private validateResourceData(resourceAllocation: ResourceAllocation) {
    if (ValidationService.isAllocationValid(resourceAllocation.allocation)) {
      return true;
    }
    return false;
  }

  deletePlanningCard(id) {
    this.removePlanningCardEmitter.emit(id);
  }

  getActiveResourcesEmailAddress() {
    this.activeResourcesEmailAddresses = '';
    if (this.project.allocatedResources) {
      this.project.allocatedResources.forEach(resource => {
        if (!this.activeResourcesEmailAddresses.includes(resource.internetAddress)) {
          this.activeResourcesEmailAddresses += resource.internetAddress + ';';
        }
      });
    }
  }

}

