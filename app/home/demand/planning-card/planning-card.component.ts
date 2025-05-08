import { CdkDragDrop } from '@angular/cdk/drag-drop';
// -------------------External References---------------------------------------//
import { Component, OnInit, Output, EventEmitter, Input, ViewChild, ViewChildren, QueryList, OnDestroy } from '@angular/core';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { CoreService } from 'src/app/core/core.service';
import { SearchCaseOppDialogService } from 'src/app/overlay/dialogHelperService/searchCaseOppDialog.service';
import { SharePlanningCardDialogService } from 'src/app/overlay/dialogHelperService/share-planning-card-dialog.service';
import { DateService } from 'src/app/shared/dateService';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';
import { Resource } from 'src/app/shared/interfaces/resource.interface';
import { NotificationService } from 'src/app/shared/notification.service';
import { SystemconfirmationFormComponent } from 'src/app/shared/systemconfirmation-form/systemconfirmation-form.component';
import { ValidationService } from 'src/app/shared/validationService';
import { ProjectResourceComponent } from '../../project-resource/project-resource.component';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { PositionGroup } from 'src/app/shared/interfaces/position-group.interface';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { ResourceAllocationService } from 'src/app/shared/services/resourceAllocation.service';

@Component({
  selector: 'app-planning-card',
  templateUrl: './planning-card.component.html',
  styleUrls: ['./planning-card.component.scss'],
  providers: [SearchCaseOppDialogService, SharePlanningCardDialogService]
})
export class PlanningCardComponent implements OnInit, OnDestroy {
  // -----------------------Local Variables--------------------------------------------//
  selectedCase;
  validationObj = {
    isAllocationInvalid: false,
    allocationInvalidMessage: '',
    isEndDateInvalid: false,
    endDateInvalidMessage: ''
  };
  editableCol = '';
  bsConfig: Partial<BsDatepickerConfig>;
  public bsModalRef: BsModalRef;
  planningCardDateRange: any;
  activeResourcesEmailAddresses = '';
  unsubscribe$: Subject<void> = new Subject<void>();
  isPegPlanningCard = false;
  showRingFenceIcon = false;
  positionGroups: PositionGroup[];
  // -----------------------View Child--------------------------------------------//
  @ViewChild('allocation', { static: false }) allocationElement;
  @ViewChildren('projectResourceComponent') projectResourceComponentElements: QueryList<ProjectResourceComponent>;

  // -----------------------Input Events--------------------------------------------//
  @Input() planningCard: PlanningCard;
  @Input() highlightedResourcesInPlanningCards: [];
  @Input() planningCards: PlanningCard[];

  // -----------------------Output Events--------------------------------------------//
  @Output() removePlanningCardEmitter = new EventEmitter();
  @Output() updatePlanningCardEmitter = new EventEmitter<any>();
  @Output() upsertPlaceholderEmitter = new EventEmitter<any>();
  @Output() removePlaceHolderEmitter = new EventEmitter<any>();
  @Output() openResourceDetailsDialog = new EventEmitter();
  @Output() mergePlanningcardToCaseOppEmitter = new EventEmitter<any>();
  @Output() sharePlanningCardEmitter = new EventEmitter<any>();
  @Output() showQuickPeekDialog = new EventEmitter();
  @Output() dropPlanningCardEventEmitter = new EventEmitter();
  @Output() openPegRFPopUpEmitter = new EventEmitter();

  // -----------------------Constructor--------------------------------------------//
  constructor(private modalService: BsModalService,
    private notifyService: NotificationService,
    private searchCaseOppDialogService: SearchCaseOppDialogService,
    private sharePlanningCardDialogService: SharePlanningCardDialogService,
    private coreService: CoreService,
    private localStorageService: LocalStorageService,
    private _resourceAllocationService: ResourceAllocationService) { }

  // --------------------------Component LifeCycle Events----------------------------//
  ngOnInit(): void {

    this.bsConfig = {
      containerClass: 'theme-red calendar-demand calendar-align-right',
      customTodayClass: 'custom-today-class',
      rangeInputFormat: 'DD-MMM-YYYY',
      isAnimated: true,
      showWeekNumbers: false,
      selectFromOtherMonth: true
    };

    this.planningCardDateRange = [
      new Date(this.planningCard.startDate?.toString()),
      new Date(this.planningCard.endDate?.toString())];
    this.getActiveResourcesEmailAddress();
    this.setMasterDataFromLocalStorage();

    // subscribe only once to avoid multiple subscriptions being returned
    this.subscribePlaceholderMergingData();
    this.subscribePlanningCardSharingData();
    this.loggedInUserHasAccessToSeePegRFPopUp();
  }

  ngOnDestroy() {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }

  // -----------------------Component Event Handlers-----------------------------------//

  dropPlanningCardEvent(event) {
    this.dropPlanningCardEventEmitter.emit(event);
  }

  toggleMergeDialogHandler() {
    let isValidAllocation = true;
    let planningCardAllocations = this.planningCard.placeholderAllocations;
    planningCardAllocations = planningCardAllocations.concat(this.planningCard.regularAllocations);
    planningCardAllocations?.every((item, index) => {
      if (item.employeeCode !== null) {
        isValidAllocation = this.projectResourceComponentElements.toArray()[index].validateInputForPlaceholder(item);
        if (!isValidAllocation) {
          return false;
        }
      }
      return true;
    });

    if (!isValidAllocation) {
      return false;
    }

    if (this.planningCard.pegOpportunityId) {
      var initialConfig = {
        showMergeAndCopy: false, searchCases: true, searchOpps: false
      }

      this.searchCaseOppDialogService.openSearchCaseOppDialogHandler(this.planningCard, initialConfig);

    } else {
      this.searchCaseOppDialogService.openSearchCaseOppDialogHandler(this.planningCard);
    }


  }

  onPlaceholderDrop(event: CdkDragDrop<any>) {
    // if dropping anything other than resource then return
    if (!Array.isArray(event.previousContainer.data)) {
      return;
    }

    // check if resource is dropped on placeholder element or not
    if (event.container.data === null || event.container.id === event.previousContainer.id) {
      return;
    }

    // if resource is being dragged from case then do nothing
    if (event.previousContainer.data[event.previousIndex].oldCaseCode || event.previousContainer.data[event.previousIndex].pipelineId) {
      this.notifyService.showWarning(`Allocated Resource can not be assinged on a planning card`);
      return;
    }

    // if element is terminated then drag and drop is not allowed
    if (!!event.previousContainer.data[event.previousIndex].terminationDate) {
      this.notifyService.showValidationMsg(ValidationService.terminatedEmployeeAllocation);
      return;
    }

    this.addPlaceholderAllocationOnPlanningCard(event);

  }

  addPlaceholderAllocationOnPlanningCard(event) {

    let placeholderAllocation: PlaceholderAllocation;
    let startDate = null;
    let endDate = null;
    if (this.planningCard.startDate && this.planningCard.endDate) {
      startDate = DateService.convertDateInBainFormat(this.planningCard.startDate)
      endDate = DateService.convertDateInBainFormat(this.planningCard.endDate) ;
    } else {
      //set start date of today while allocating on planning card
      startDate = new Date().toLocaleDateString('en-US');
    }

    // if resource being dropped does not have an id that means resource is being dropped from resources list,
    // else its being dropped from one of the cards
    if (event.previousContainer.data[event.previousIndex].id) {
      const staffableEmployee: PlaceholderAllocation = event.previousContainer.data[event.previousIndex];

      placeholderAllocation = {
        id: staffableEmployee.id,
        planningCardId: this.planningCard.id,
        oldCaseCode: null,
        caseName: null,
        clientName: null,
        pipelineId: null,
        opportunityName: null,
        employeeCode: staffableEmployee.employeeCode,
        employeeName: staffableEmployee.employeeName,
        internetAddress: staffableEmployee.internetAddress,
        operatingOfficeCode: staffableEmployee.operatingOfficeCode,
        operatingOfficeAbbreviation: staffableEmployee.operatingOfficeAbbreviation,
        currentLevelGrade: staffableEmployee.currentLevelGrade,
        serviceLineCode: staffableEmployee.serviceLineCode,
        serviceLineName: staffableEmployee.serviceLineName,
        allocation: staffableEmployee.allocation,
        startDate :startDate,
        endDate : endDate,
        investmentCode: staffableEmployee.investmentCode,
        investmentName: staffableEmployee.investmentName,
        caseRoleCode: staffableEmployee.caseRoleCode,
        positionGroupCode: staffableEmployee.positionGroupCode,
        isPlaceholderAllocation: false,
        caseTypeCode: null,
        lastUpdatedBy: null
      };

      if (!this.validateResourceData(placeholderAllocation)) {
        return;
      }

      const allocationId = event.previousContainer.data[event.previousIndex].id;
      const previousPlanningCard = this.planningCards.find(x => x.allocations.some(y => y.id == allocationId));

      this.planningCard.regularAllocations.splice(event.currentIndex, 0, placeholderAllocation);
      this.planningCard.allocations.splice(event.currentIndex, 0, placeholderAllocation);
      event.previousContainer.data.splice(event.previousIndex, 1);

      let allocationIndexInPreviousContainer = previousPlanningCard.allocations.findIndex(x => x.id == staffableEmployee.id);
      previousPlanningCard.allocations.splice(allocationIndexInPreviousContainer, 1);

      //const allocationIndexInPreviousContainer = this.planningCards.find(x => x.allocations.find(r => r.id==))
      //this.planningCards.find(x => x.id== event.previousContainer.data[event.previousIndex].id).allocations.splice();

    } else {
      const staffableEmployee: Resource = event.previousContainer.data[event.previousIndex];

      placeholderAllocation = {
        id: null,
        planningCardId: this.planningCard.id,
        oldCaseCode: null,
        caseName: null,
        clientName: null,
        pipelineId: null,
        caseTypeCode: null,
        opportunityName: null,
        employeeCode: staffableEmployee.employeeCode,
        employeeName: staffableEmployee.fullName,
        internetAddress: staffableEmployee.internetAddress,
        operatingOfficeCode: staffableEmployee.schedulingOffice.officeCode,
        operatingOfficeAbbreviation: staffableEmployee.schedulingOffice.officeAbbreviation,
        currentLevelGrade: staffableEmployee.levelGrade,
        serviceLineCode: staffableEmployee.serviceLine.serviceLineCode,
        serviceLineName: staffableEmployee.serviceLine.serviceLineName,
        allocation: parseInt(staffableEmployee.percentAvailable.toString(), 10),
        startDate :startDate,
        endDate : endDate,
        investmentCode: null,
        investmentName: null,
        caseRoleCode: null,
        positionGroupCode: this.positionGroups.find(x => x.positionGroupName === staffableEmployee.position.positionGroupName)?.positionGroupCode,
        isPlaceholderAllocation: false,
        lastUpdatedBy: null
      };

      this.planningCard.regularAllocations.splice(event.currentIndex, 0, placeholderAllocation);
      this.planningCard.allocations.splice(event.currentIndex, 0, placeholderAllocation);
    }

    this.upsertPlaceholderEmitter.emit(placeholderAllocation);
    this.getActiveResourcesEmailAddress();
  }

  private validateResourceData(resourceAllocation: PlaceholderAllocation) {
    if (ValidationService.isAllocationValid(resourceAllocation.allocation)) {
      return true;
    }
    return false;
  }

  updatePlanningCardName(event) {
    if (event.target.value.length < 1) {
      return false;
    }
    this.updatePlanningCard();
  }

  updatePlanningCardDateRange(selectedDateRange) {
    this.planningCard.startDate = selectedDateRange[0];
    this.planningCard.endDate = selectedDateRange[1];
    this.planningCardDateRange = selectedDateRange;

    // Update the start date and end date of allocations to match the planning card's dates
    this.updateAllocationsDatesForPlanningCardDateChange(selectedDateRange)
    this.updatePlanningCard();
  }

  updateAllocationsDatesForPlanningCardDateChange(selectedDateRange) {
    this.planningCard.allocations.forEach((allocation) => {
      allocation.startDate = DateService.convertDateInBainFormat(new Date(selectedDateRange[0]));
      allocation.endDate = DateService.convertDateInBainFormat(new Date(selectedDateRange[1]));
    });
  }

  updatePlanningCard() {
    const updatedPlanningCard: PlanningCard = {
      id: this.planningCard.id,
      name: this.planningCard.name,
      startDate: DateService.convertDateInBainFormat(this.planningCard.startDate),
      endDate: DateService.convertDateInBainFormat(this.planningCard.endDate),
      sharedOfficeCodes: this.planningCard.sharedOfficeCodes,
      sharedStaffingTags: this.planningCard.sharedStaffingTags,
      isShared: this.planningCard.isShared,
      probabilityPercent: this.planningCard.probabilityPercent
    }
    this.updatePlanningCardEmitter.emit(updatedPlanningCard);
  }

  onInputChange(event) {
    this.planningCard.name = event.target.value;
  }

  onAddPlaceHolderHandler() {
    const placeholder: PlaceholderAllocation = {
      id: null,
      planningCardId: this.planningCard.id,
      oldCaseCode: null,
      caseName: null,
      clientName: null,
      pipelineId: null,
      opportunityName: null,
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
      isPlaceholderAllocation: true,
      investmentCode: null,
      investmentName: null,
      caseRoleCode: null,
      caseTypeCode: null,
      caseStartDate: this.planningCard.startDate ? DateService.convertDateInBainFormat(this.planningCard.startDate) : null,
      caseEndDate: this.planningCard.endDate ? DateService.convertDateInBainFormat(this.planningCard.endDate) : null,
      opportunityStartDate: null,
      opportunityEndDate: null,
      lastUpdatedBy: null
    };
    this.upsertPlaceholderEmitter.emit(placeholder);
  }

  upsertPlaceholderAllocationHandler(placeholderAllocation) {
    // this.planningCard.allocations.map(obj => obj.id === placeholderAllocation.id || obj);
    this.upsertPlaceholderEmitter.emit(placeholderAllocation);
    this.getActiveResourcesEmailAddress();
  }

  confirmPlaceholderAllocationHandler(placeholderAllocation: PlaceholderAllocation) {
    // remove from placeholders object and add to regular allocations on confirming
    placeholderAllocation.isPlaceholderAllocation = false;
    placeholderAllocation.isConfirmed = true;

    this.planningCard.placeholderAllocations.splice(this.planningCard.placeholderAllocations.findIndex(x =>
      x.id === placeholderAllocation.id), 1);
    this.planningCard.regularAllocations.push(placeholderAllocation);

    this.upsertPlaceholderEmitter.emit(placeholderAllocation);
    this.getActiveResourcesEmailAddress();
  }

  removePlaceHolderEmitterHandler(placeholderAllocation) {
    this.planningCard.placeholderAllocations.splice(this.planningCard.placeholderAllocations.findIndex(x =>
      x.id === placeholderAllocation.id), 1);

    this.removePlaceHolderEmitter.emit({ 
          placeholderIds : placeholderAllocation.id,
          placeholderAllocation: [].concat(placeholderAllocation),
          notifyMessage: 'Placeholder Deleted'
        });
  }

  removeResourceFromPlaceholderHandler(event) {
    // for (let i = 0; i < this.planningCard.resource.length; i++) {
    //     if (this.planningCard.resource[i] !== 'placeholder') {
    //         if (this.planningCard.resource[i].employeeCode === event) {
    //             this.planningCard.resource.splice(i, 1, 'placeholder');
    //         }
    //     }
    // }
  }

  removePlanningCardAndItsAllocation(id) {
    this.removePlanningCardEmitter.emit({ id: id });
  }

  deletePlanningCardHandler() {
    const confirmationPopUpBodyMessage = !this.planningCard.isShared
      ? `You are about to delete planning card.
        This will delete all allocations associated to it. Are you sure you want to delete ?`
      : `This planning card had been shared and will be deleted for all the shared resources.
        Continue?`;
    this.openSystemConfirmationPlanningCardHandler({
      planningCardId: this.planningCard.id,
      confirmationPopUpBodyMessage: confirmationPopUpBodyMessage
    });
  }

  openSystemConfirmationPlanningCardHandler(event) {
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        confirmationPopUpBodyMessage: event.confirmationPopUpBodyMessage
      }
    };

    this.bsModalRef = this.modalService.show(SystemconfirmationFormComponent, config);

    this.bsModalRef.content.deleteResourceNote.subscribe(() => {
      this.removePlanningCardAndItsAllocation(this.planningCard.id);
    });
  }

  caseDragMouseDown(id) {
    if (!id) {
      this.notifyService.showWarning('Please wait for save to complete');
    }
  }

  disableDropList() {
    return false;
  }

  openResourceDetailsDialogHandler(employeeCode) {
    this.openResourceDetailsDialog.emit(employeeCode);
  }

  getActiveResourcesEmailAddress() {
    this.activeResourcesEmailAddresses = '';
    if (this.planningCard.regularAllocations) {
      this.planningCard.regularAllocations.forEach(resource => {
        if (resource.employeeCode && !this.activeResourcesEmailAddresses.includes(resource.internetAddress)) {
          this.activeResourcesEmailAddresses += resource.internetAddress + ';';
        }
      });
    }
  }

  quickPeekIntoResourcesCommitments() {
    
    let employeesOnPlaceholders = this.planningCard.placeholderAllocations?.map(x => {
      return {
        employeeCode: x.employeeCode,
        employeeName: x.employeeName,
        levelGrade: x.currentLevelGrade,
      };
    });
    employeesOnPlaceholders = employeesOnPlaceholders.filter(employee => !!employee.employeeCode);

    let employeesOnRegularAllocations = this.planningCard.regularAllocations?.map(x => {
      return {
        employeeCode: x.employeeCode,
        employeeName: x.employeeName,
        levelGrade: x.currentLevelGrade,
      };
    });
    
    let employees = Array.from(new Set([...employeesOnPlaceholders, ...employeesOnRegularAllocations]));

    this.showQuickPeekDialog.emit(employees);
  }
  

  sharePlaceHolderHandler() {
    this.sharePlanningCardDialogService.openSharePlanningCardDialogHandler({
      planningCard : this.planningCard,
      isPegPlanningCard: this.isPegPlanningCard
    });
  }

  subscribePlanningCardSharingData() {
    this.sharePlanningCardDialogService.getSharedPlanningCardData().pipe(
      takeUntil(this.unsubscribe$)
    ).subscribe((selectedFilters) => {
      if (selectedFilters !== null) {
        const selectedPlanningCard: PlanningCard = {
          id: this.planningCard.id,
          name: this.planningCard.name,
          startDate: this.planningCard.startDate,
          endDate: this.planningCard.endDate,
          isShared: true,
          sharedOfficeCodes: selectedFilters.officeCodes,
          sharedStaffingTags: selectedFilters.staffingTags,
          includeInCapacityReporting: selectedFilters.includeInCapacityReporting,
          createdBy: this.planningCard.createdBy,
          lastUpdatedBy: this.planningCard.lastUpdatedBy,
          allocations: this.planningCard.allocations,
          placeholderAllocations: this.planningCard.placeholderAllocations,
          regularAllocations: this.planningCard.regularAllocations
        };

        this.sharePlanningCardEmitter.emit({ planningCard: selectedPlanningCard });
      }
    });
  }

  subscribePlaceholderMergingData() {
    this.searchCaseOppDialogService.getSelectedCaseOpp().pipe(
      takeUntil(this.unsubscribe$)
    ).subscribe((selectedCaseOppPlanningCard) => {
      if (selectedCaseOppPlanningCard !== null) {
        this.mergePlanningcardToCaseOppEmitter.emit({ project: selectedCaseOppPlanningCard.selectedCase, planningCard: this.planningCard, action: selectedCaseOppPlanningCard.action });
      }
    });
  }

  isHighlightAllocation(allocation) {
    return this.highlightedResourcesInPlanningCards.some(resource => resource === allocation.employeeCode);
  }

  loggedInUserHasAccessToSeePegRFPopUp() {
    this.isPegPlanningCard = !!this.planningCard.pegOpportunityId;
    this.showRingFenceIcon = this.coreService.loggedInUserClaims.PegC2CAccess && !!this.planningCard.pegOpportunityId;
  }

  openPegRFPopUpHandler() {
    this.openPegRFPopUpEmitter.emit(this.planningCard.pegOpportunityId);
  }

  setMasterDataFromLocalStorage() {
    this.positionGroups = this.localStorageService.get(ConstantsMaster.localStorageKeys.positionsGroups);
  }
}
