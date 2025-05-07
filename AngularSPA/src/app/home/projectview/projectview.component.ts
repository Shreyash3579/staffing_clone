// ------------------- Angular modules ---------------------------------------//
import { Component, OnInit, Input, Output, EventEmitter, ViewChildren, QueryList } from '@angular/core';
import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';

// ------------------- Interfaces ---------------------------------------//
import { Project } from '../../shared/interfaces/project.interface';
import { Resource } from '../../shared/interfaces/resource.interface';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';

// ------------------- Project Reference ---------------------------------------//
import { ProjectResourceComponent } from '../project-resource/project-resource.component';
import { ProjectType } from 'src/app/shared/constants/enumMaster';
import { DateService } from 'src/app/shared/dateService';
import { ValidationService } from 'src/app/shared/validationService';
import { ResourceAllocationService } from 'src/app/shared/services/resourceAllocation.service';
import { NotificationService } from 'src/app/shared/notification.service';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';
import { InlineEditableNotesComponent } from 'src/app/shared/inline-editable-notes/inline-editable-notes.component';

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

  // --------------------- Placeholder card ----------------------------------

  // -----------------------Output Events--------------------------------------------//
  @Output() mapResourceToProject = new EventEmitter();
  @Output() upsertResourceAllocationsToProject = new EventEmitter<any>();
  @Output() openResourceDetailsDialog = new EventEmitter();
  @Output() tabbingFromAllocation = new EventEmitter();
  @Output() tabbingFromEndDate = new EventEmitter();
  @Output() openProjectDetailsDialog = new EventEmitter();
  @Output() openBackFillPopUp = new EventEmitter<any>();
  @Output() openCaseRollForm = new EventEmitter<any>();
  @Output() openQuickAddForm = new EventEmitter();
  @Output() openPlaceholderForm = new EventEmitter();
  @Output() showQuickPeekDialog = new EventEmitter();
  @Output() addProjectToUserExceptionHideListEmitter = new EventEmitter<any>();
  @Output() addProjectToUserExceptionShowListEmitter = new EventEmitter<any>();
  @Output() removeProjectFromUserExceptionShowListEmitter = new EventEmitter<any>();
  @Output() upsertPlaceholderEmitter = new EventEmitter<any>();
  @Output() removePlaceHolderEmitter = new EventEmitter<any>();
  @Output() removePlanningCardEmitter = new EventEmitter();
  @Output() dropProjectEventEmitter = new EventEmitter();
  @Output() updateProjectChanges = new EventEmitter();
  @Output() deleteNotesOnProject = new EventEmitter();
  @Output() openOverlappedTeamsPopup = new EventEmitter<any>();

  // -----------------------Local Variables--------------------------------------------//
  showSKUTerms = false;
  skuTerm = '';
  activeResourcesEmailAddresses = '';
  projectTitle = '';
  projectType = '';
  showMoreThanYearWarning = false;
  placeholderLists: any = [];
  public bsModalRef: BsModalRef;

  // ----------------------- Constructor --------------------------------------------//
  constructor(private _resourceAllocationService: ResourceAllocationService,
    private modalService: BsModalService,
    private notifyService: NotificationService) { }

  // --------------------------Component LifeCycle Events----------------------------//

  ngOnInit() {

    this.getActiveResourcesEmailAddress();

    this.projectType = this.project.type;
    this.setProjectTitle();

    if (this.project.skuCaseTerms) {
      this.skuTerm = this.project.skuCaseTerms.skuTerms.map(s => s.name).toString();
    }

  }

  // -------------------Component Event Handlers-------------------------------------//

  dropProjectEventHandler(event) {
    this.dropProjectEventEmitter.emit(event);
  }

  checkIfAllocationIdExist(event) {
    return event.previousContainer.data[event.previousIndex].id;
  }

  onResourceDrop(event: CdkDragDrop<any>) {

    const data = event.previousContainer.data;

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

    const isAllocationOnPlanningCard = event.previousContainer.data[event.previousIndex].planningCardId ? true : false;
    const allocationId = event.previousContainer.data[event.previousIndex].id;
    /*
      * NOTE: We are calculating opportunityEndDate if a resource is allocated to an opportunity that does not any end date or a duration.
      * For an opportunity that is going to start in future,
      * we have set the end date for the allocation as opportunity start date + 30 days.
      *
      * For an opportunuty that has already started, we have set the end date for the allocation as today + 30 days.
      *
      * TODO: Change the logic once Brittany comes up with the solution
    */

    let [startDate, endDate, showMoreThanYearWarning] = this._resourceAllocationService.getAllocationDates(this.project.startDate, this.project.endDate);
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
        [isValidAllocation, monthCloseErrorMessage] = this._resourceAllocationService.validateMonthCloseForInsertAndDelete(resourceAllocation);

      } else {
        [isValidAllocation, monthCloseErrorMessage] = this._resourceAllocationService.validateMonthCloseForUpdates(resourceAllocation, staffableEmployee);

      }

      if (!isValidAllocation) {
        this.notifyService.showValidationMsg(monthCloseErrorMessage);
        return;
      }

    } else {
      const staffableEmployee: Resource = event.previousContainer.data[event.previousIndex];

      [startDate, endDate] = this._resourceAllocationService.getAllocationDatesForNotYetStartedEmployee(staffableEmployee.startDate, startDate, endDate);
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

      const [isValidAllocation, monthCloseErrorMessage] = this._resourceAllocationService.validateMonthCloseForInsertAndDelete(resourceAllocation);

      if (!isValidAllocation) {
        this.notifyService.showValidationMsg(monthCloseErrorMessage);
        return;
      }

    }

    if (!this.validateResourceData(resourceAllocation)) {
      return;
    }

    
    if (this._resourceAllocationService.isBackFillRequiredOnProject(this.project.allocatedResources,
      this.project)) {

      if (this.checkIfAllocationIdExist(event)) {
        this.openBackFillPopUp.emit({
          project: this.project,
          resourceAllocation: resourceAllocation,
          showMoreThanYearWarning: this.showMoreThanYearWarning
        });
      }
      else {
        this.openBackFillPopUp.emit({
          project: this.project,
          resourceAllocation: resourceAllocation,
          showMoreThanYearWarning: this.showMoreThanYearWarning,
          allocationDataBeforeSplitting: [].concat(resourceAllocation)
        });
    }

    } else {

      const projectStartDate = DateService.convertDateInBainFormat(this.project.startDate);
      const projectEndDate = DateService.convertDateInBainFormat(this.project.endDate);

      let allocationsData: ResourceAllocation[] = [];

      if (projectStartDate && projectEndDate) {

        allocationsData = this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(resourceAllocation);

      } else {

        allocationsData.push(resourceAllocation);

      }


      if (this.checkIfAllocationIdExist(event)) {
        this.upsertResourceAllocationsToProject.emit({
          resourceAllocation: allocationsData,
          event: 'dragdrop',
          showMoreThanYearWarning: this.showMoreThanYearWarning
        });
      } else {
        this.upsertResourceAllocationsToProject.emit({
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
          placeholderIds: allocationId,
          notifyMessage: null
        });
      }

    }
  }

  caseDragMouseDown(id) {
    if (!id) {
      this.notifyService.showWarning('Please wait for save to complete');
    }
  }

  openResourceDetailsDialogHandler(employeeCode) {
    this.openResourceDetailsDialog.emit(employeeCode);
  }

  updateResourceToProjectHandler(updatedResource) {
    this.mapResourceToProject.emit(updatedResource);
  }

  updateAllocationsPositionInArray(initialAllocationIndex, upsertedAllocations) {
    // When allocations are split after update, then show them in case-card
    if (Array.isArray(upsertedAllocations) && upsertedAllocations.length > 1) {
      this.project.allocatedResources.splice(initialAllocationIndex, 1);
      this.project.allocatedResources.splice(initialAllocationIndex, 0, ...upsertedAllocations);
    }
  }

  upsertResourceAllocationsToProjectHandler(upsertedAllocations) {
    this.updateAllocationsPositionInArray(upsertedAllocations.initialAllocationIndex, upsertedAllocations.resourceAllocation);

    this.upsertResourceAllocationsToProject.emit(upsertedAllocations);
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
    this.openProjectDetailsDialog.emit({ oldCaseCode: projectData.oldCaseCode, pipelineId: projectData.pipelineId });
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

  onCaseRollHandler() {

    if (!ValidationService.isCaseEligibleForRoll(this.project.endDate)) {
      this.notifyService.showValidationMsg(ValidationService.caseRollNotAllowedForInActiveCasesMessage);
    } else {
      this.openCaseRollForm.emit({ project: this.project });
    }

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
    this.project.placeholderAllocations =
      this.project.placeholderAllocations.filter(x => x.id !== placeholderAllocation.id);

    this.removePlaceHolderEmitter.emit({
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
      operatingOfficeCode: null,
      operatingOfficeAbbreviation: null,
      currentLevelGrade: null,
      serviceLineCode: null,
      serviceLineName: null,
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

  openEditNotesDialog() {
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        inputNotes: this.project.notes,
        uniqueId: this.project.oldCaseCode ?? this.project.pipelineId,
        maxLength: 1000,
        event: 'caseCard'
      }
    };

    this.bsModalRef = this.modalService.show(InlineEditableNotesComponent, config);
    this.bsModalRef.content.updateNotesEventEmitter.subscribe(data => {
      this.updateNotes(data);
    });
    this.bsModalRef.content.deleteNotesEventEmitter.subscribe(data => {
      this.deleteNotes();
    });
  }

  updateNotes(data) {
    if (data.updatedNotes !== this.project.notes?.trim()) {
      this.project.notes = data.updatedNotes;
      this.updateProjectChanges.emit(this.project);
    }
  }

  deleteNotes() {
    this.project.notes = null;
    this.deleteNotesOnProject.emit(this.project);
  }

  // -------------------Local functions -------------------------------------//

  private setProjectTitle() {

    switch (this.project.type) {
      case ProjectType.Opportunity: {

        if (this.project.probabilityPercent) {
          this.projectTitle = `${this.project.probabilityPercent}% - ${this.project.clientName} - ${this.project.opportunityName}`;
        } else {
          this.projectTitle = `${this.project.clientName} - ${this.project.opportunityName}`;
        }

        break;
      }
      default: {
        this.projectTitle = `${this.project.oldCaseCode} - ${this.project.clientName} - ${this.project.caseName}`;
        break;
      }
    }

  }

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

