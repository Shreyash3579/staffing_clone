import { Component, OnInit, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { BsDatepickerConfig, BsDatepickerDirective } from 'ngx-bootstrap/datepicker';
import { ResourceAllocation } from '../../../../shared/interfaces/resourceAllocation.interface';
import { DateService } from 'src/app/shared/dateService';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';
import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { Resource } from 'src/app/shared/interfaces/resource.interface';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { ResourceAllocationService } from 'src/app/shared/services/resourceAllocation.service';
import { NotificationService } from 'src/app/shared/notification.service';
import { ValidationService } from 'src/app/shared/validationService';
import { PositionGroup } from 'src/app/shared/interfaces/position-group.interface';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { OverlayDialogService } from 'src/app/overlay/dialogHelperService/overlayDialog.service';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';
import { BS_DEFAULT_CONFIG } from 'src/app/shared/constants/bsDatePickerConfig';

@Component({
  selector: '[app-project-resource]',
  templateUrl: './project-resource.component.html',
  styleUrls: ['./project-resource.component.scss']
})
export class ProjectResourceComponent implements OnInit {
  // -----------------------Local Variables--------------------------------------------//
  editableCol = '';
  validationObj = {
    isAllocationInvalid: false,
    allocationInvalidMessage: '',
    isEndDateInvalid: false,
    endDateInvalidMessage: ''
  };
  bsConfig: Partial<BsDatepickerConfig>;
  placeholderIcon = false;
  showGuessedPlaceholder = false;
  private existingUserAllocation: ResourceAllocation;
  positionGroups: PositionGroup[];

  //TODO: Using getter setter to update id to the exisiting allocation so that it can be verified
  // against month close
// -----------------------Input Parameters--------------------------------------------//

  private _userAllocation: ResourceAllocation;
  get userAllocation() {
    if (this.existingUserAllocation) {
      this.existingUserAllocation.id = this._userAllocation.id;
    }
    return this._userAllocation;
  }
  @Input() set userAllocation(value: ResourceAllocation) {
    this._userAllocation = value;
    this.existingUserAllocation = Object.assign({}, value);
  }

  @Input() project: Project;
  @Input() planningCard: PlanningCard;
  @Input() projectResourceComponentIndex: number;
  @Input() placeholderAllocation: ResourceAllocation;

  // -----------------------Output Events--------------------------------------------//
  
  @Output() confirmPlaceholderAllocationEmitter = new EventEmitter();
  @Output() removePlaceHolder = new EventEmitter();
  @Output() upsertPlaceholderAllocationEmitter = new EventEmitter<any>();
  @Output() openResourceDetailsDialog = new EventEmitter();
  @Output() upsertResourceAllocationsToProject = new EventEmitter<any>();
  @Output() tabbingFromAllocation = new EventEmitter();
  @Output() tabbingFromEndDate = new EventEmitter();

  @ViewChild('allocation', { static: false }) allocationElement;
  @ViewChild('enddate', { static: false }) endDateElement;
  @ViewChild(BsDatepickerDirective, { static: false }) datepicker: BsDatepickerDirective;
  
  endDate;

  constructor(private localStorageService: LocalStorageService,
    private resourceAllocationHelperService: ResourceAllocationService,  
    private overlayDialogService: OverlayDialogService,
    private _resourceAllocationService: ResourceAllocationService,
    private notifyService: NotificationService) { }

  ngOnInit() {
    this.bsConfig = {
      containerClass: 'theme-red calendar-supply calendar-align-left',
      customTodayClass: 'custom-today-class',
      dateInputFormat: 'DD-MMM-YYYY',
      isAnimated: true,
      adaptivePosition: true,
      showWeekNumbers: false,
      selectFromOtherMonth: true
    };

    if(!this.showGuessedPlaceholder && this.placeholderAllocation && !this.placeholderAllocation.employeeCode && !this.placeholderAllocation.operatingOfficeCode)
    this.showGuessedPlaceholder = true;
    // else if(!this.showGuessedPlaceholder && this.placeholderAllocation && this.placeholderAllocation.employeeCode)
    // this.showGuessedPlaceholder = false;

    if (this.placeholderAllocation?.endDate) {
      this.endDate = new Date(this.placeholderAllocation.endDate);
      this.placeholderAllocation.endDate = DateService.convertDateInBainFormat(this.placeholderAllocation.endDate);
    }
    if (this.userAllocation?.endDate) {
      this.endDate = new Date(this.userAllocation.endDate);
      this.userAllocation.endDate = DateService.convertDateInBainFormat(this.userAllocation.endDate);
    }

    this.positionGroups = this.localStorageService.get(ConstantsMaster.localStorageKeys.positionsGroups);
  }

  ngOnChanges(){
    if(this.placeholderAllocation && !this.placeholderAllocation.employeeCode && !this.placeholderAllocation.operatingOfficeCode)
    this.showGuessedPlaceholder = true;

  }

  getResourceNameTooltip() {
    let tooltipText = this.userAllocation.employeeName;
    if (!!this.userAllocation.investmentName) {
      tooltipText += ' - ' + this.userAllocation.investmentName;
    }
    if (!!this.userAllocation.caseRoleName) {
      tooltipText += ' - ' + this.userAllocation.caseRoleName;
    }
    return tooltipText;
  }

  openResourceDetailsDialogHandler(employeeCode) {
    this.overlayDialogService.openResourceDetailsDialogHandler(employeeCode);
  }

  onDrop(event: CdkDragDrop<any>) {
    // check if resource is dropped on placeholder element or not
    if (event.container.data === null) {
      return;
    }

    // if resource is being dragged from case then do nothing
    if (event.previousContainer.data[event.previousIndex].id) {
      this.notifyService.showWarning(`Allocated Resource can not be assinged as placeholder`);
      return;
    }

    // if element is terminated then drag and drop is not allowed
    if (!!event.previousContainer.data[event.previousIndex].terminationDate) {
      this.notifyService.showValidationMsg(ValidationService.terminatedEmployeeAllocation);
      return;
    }

    if (this.project) {
      this.addPlaceholderAllocationOnProject(event);
    } else {
      this.addPlaceholderAllocationOnPlanningCard(event);
    }
    this.showGuessedPlaceholder = false;

  }

  upsertGuessedPlaceholderAllocationHandler(event) {
    if (this.project) {
      let [startDate, endDate] = this._resourceAllocationService.getAllocationDates(this.project.startDate, this.project.endDate);
      this.placeholderAllocation.startDate = startDate;
      this.placeholderAllocation.endDate = endDate;
      this.upsertPlaceholderAllocationEmitter.emit(this.placeholderAllocation);
    } else {
      let startDate = null;
      let endDate = null;
      if (this.planningCard.startDate && this.planningCard.endDate) {
        [startDate, endDate] = this._resourceAllocationService.getAllocationDates(this.planningCard.startDate, this.planningCard.endDate);
      } else {
        //set start date of today while allocating on planning card
        startDate = new Date().toLocaleDateString('en-US');
      }
      this.placeholderAllocation.startDate = startDate ? DateService.convertDateInBainFormat(startDate) : null;
      this.placeholderAllocation.endDate = endDate ? DateService.convertDateInBainFormat(endDate) : null;
      this.upsertPlaceholderAllocationEmitter.emit(this.placeholderAllocation);
    }
  }

  confirmGuessedPlaceholder() {

  }

  addPlaceholderAllocationOnProject(event) {


    /*
      * NOTE: We are calculating opportunityEndDate if a resource is allocated to an opportunity that does not any end date or a duration.
      * For an opportunity that is going to start in future,
      * we have set the end date for the allocation as opportunity start date + 30 days.
      *
      * For an opportunuty that has already started, we have set the end date for the allocation as today + 30 days.
      *
      * TODO: Change the logic once Brittany comes up with the solution
    */

    let [startDate, endDate] = this.resourceAllocationHelperService.getAllocationDates(this.project.startDate, this.project.endDate);

    const staffableEmployee: Resource = event.previousContainer.data[event.previousIndex];

    [startDate, endDate] = this.resourceAllocationHelperService.getAllocationDatesForNotYetStartedEmployee(staffableEmployee.startDate, startDate, endDate);
    if (startDate === null) {
      return;
    }
    this.placeholderAllocation.employeeCode = staffableEmployee.employeeCode;
    this.placeholderAllocation.employeeName = staffableEmployee.fullName;
    this.placeholderAllocation.operatingOfficeCode = staffableEmployee.schedulingOffice.officeCode;
    this.placeholderAllocation.operatingOfficeAbbreviation = staffableEmployee.schedulingOffice.officeAbbreviation;
    this.placeholderAllocation.currentLevelGrade = staffableEmployee.levelGrade;
    this.placeholderAllocation.allocation = parseInt(staffableEmployee.percentAvailable.toString(), 10);
    this.placeholderAllocation.startDate = startDate;
    this.placeholderAllocation.endDate = endDate;
    this.placeholderAllocation.internetAddress = staffableEmployee.internetAddress;
    this.placeholderAllocation.investmentCode = null;
    this.placeholderAllocation.caseRoleCode = null;
    this.placeholderAllocation.positionGroupCode = this.positionGroups.find(x => x.positionGroupName === staffableEmployee.position.positionGroupName)?.positionGroupCode;

    // Populate endDate to show date on opening of calendar popup while editing date
    this.endDate = this.placeholderAllocation.endDate;
    this.upsertPlaceholderAllocationEmitter.emit(this.placeholderAllocation);
  }

  addPlaceholderAllocationOnPlanningCard(event) {
    let startDate = null;
    let endDate = null;
    if (this.planningCard.startDate && this.planningCard.endDate) {
      [startDate, endDate] = this.resourceAllocationHelperService.getAllocationDates(this.planningCard.startDate, this.planningCard.endDate);
    } else {
      //set start date of today while allocating on planning card
      startDate = new Date().toLocaleDateString('en-US');
    }

    const staffableEmployee: Resource = event.previousContainer.data[event.previousIndex];

    [startDate, endDate] = this.resourceAllocationHelperService.getAllocationDatesForNotYetStartedEmployee(staffableEmployee.startDate, startDate, endDate);
    if (startDate === null) {
      return;
    }
    this.placeholderAllocation.employeeCode = staffableEmployee.employeeCode;
    this.placeholderAllocation.employeeName = staffableEmployee.fullName;
    this.placeholderAllocation.operatingOfficeCode = staffableEmployee.schedulingOffice.officeCode;
    this.placeholderAllocation.operatingOfficeAbbreviation = staffableEmployee.schedulingOffice.officeAbbreviation;
    this.placeholderAllocation.currentLevelGrade = staffableEmployee.levelGrade;
    this.placeholderAllocation.allocation = parseInt(staffableEmployee.percentAvailable.toString(), 10);
    this.placeholderAllocation.startDate = startDate ? DateService.convertDateInBainFormat(startDate) : null;
    this.placeholderAllocation.endDate = endDate ? DateService.convertDateInBainFormat(endDate) : null;
    this.placeholderAllocation.internetAddress = staffableEmployee.internetAddress;
    this.placeholderAllocation.investmentCode = null;
    this.placeholderAllocation.caseRoleCode = null;
    this.placeholderAllocation.positionGroupCode = this.positionGroups.find(x => x.positionGroupName === staffableEmployee.position.positionGroupName)?.positionGroupCode;

    // Populate endDate to show date on opening of calendar popup while editing date
    this.endDate = this.placeholderAllocation.endDate;

    this.upsertPlaceholderAllocationEmitter.emit(this.placeholderAllocation);
  }

  disableEdit(event) {
    this.editableCol = '';
    if (event.currentTarget.style.display !== 'none') {
      event.currentTarget.style.display = 'none';
    }
  }

  hideValidationMessage(target, event) {
    if (target === 'allocation') {
      this.validationObj.isAllocationInvalid = false;
      event.stopPropagation();
    } else if (target === 'endDate') {
      this.validationObj.isEndDateInvalid = false;
      event.stopPropagation();
    }
  }

  editAllocation() {
    // fix fo preventing multiple calls when user keeps clicking inside the same text box
    if (this.editableCol === 'allocation') {
      return;
    }

    // Manually set style for element because display set to /none/ while using tab functionality
    this.allocationElement.nativeElement.style.display = 'block';
    this.editableCol = 'allocation';

    setTimeout(() => {
      this.allocationElement.nativeElement.select();
      this.allocationElement.nativeElement.focus();
    });
  }

  onTabbingFromAllocation() {
    this.tabbingFromAllocation.emit(this.projectResourceComponentIndex);
  }

  editEndate() {
    // fix fo preventing multiple calls when user keeps clicking inside the same text box
    if (this.editableCol === 'enddate') {
      return;
    }
    if (this.placeholderAllocation?.endDate) {
      this.endDate = new Date(this.placeholderAllocation.endDate);

    }

    if (this.userAllocation?.endDate) {
      this.endDate = new Date(this.userAllocation.endDate);
    }
    this.editableCol = 'enddate';

    const _self = this;
    setTimeout(() => {
      if (!_self.datepicker.isOpen) {
        _self.datepicker.show();
      }
      _self.endDateElement.nativeElement.select();
      _self.endDateElement.nativeElement.focus();
    }, 0);
  }

  onTabbingFromEndDate(event) {
    this.disableEndDateEdit(event);
    this.tabbingFromEndDate.emit(this.projectResourceComponentIndex);
  }

  disableEndDateEdit(event) {
    const _self = this;

    setTimeout(() => {

      if (event.relatedTarget) {
        event.preventDefault();
        return false;
      }

      _self.editableCol = '';

    }, 200); // DO NOT decrease the time. We need datepicker change to fire first and then disable to occur

  }

  isNullOrUndefined(value) {
    return (value === null || value === undefined);
  }

  onEndDateChange(data) {
    if (this.isNullOrUndefined(data) && this.planningCard) {
      this.userAllocation.endDate = null;
      this.updateResource();
      // This is done to close editing of input as it remained open when you changed date afer correcting an error in input
      this.disableEndDateEdit('');
    } else if (isNaN(data.valueOf()) || (this.isNullOrUndefined(data) && this.userAllocation.endDate)) {
      this.userAllocation.endDate = '';
      this.validationObj.isEndDateInvalid = true;
      this.validationObj.endDateInvalidMessage = ValidationService.dateInvalidMessage;
    } else if (ValidationService.isDateLessThanMinDateForBOSS(data)) {
      this.userAllocation.endDate = '';
      this.validationObj.isEndDateInvalid = true;
      this.validationObj.endDateInvalidMessage = ValidationService.minDateInvalidMessage;
    } else {
      const newDate = DateService.convertDateInBainFormat(data);

      if (newDate !== this.userAllocation.endDate) {
        this.userAllocation.endDate = newDate;
        this.updateResource();
        // This is done to close editing of input as it remained open when you changed date afer correcting an error in input
        this.disableEndDateEdit('');
      }
    }

  }

  onPlaceholderEndDateChange(data) {
    if (this.isNullOrUndefined(data) && this.planningCard) {
      this.placeholderAllocation.endDate = null;
      this.updateResource();
      // This is done to close editing of input as it remained open when you changed date afer correcting an error in input
      this.disableEndDateEdit('');
    } else if (isNaN(data?.valueOf()) || (this.isNullOrUndefined(data) && this.placeholderAllocation.endDate)) {
      this.placeholderAllocation.endDate = '';
      this.validationObj.isEndDateInvalid = true;
      this.validationObj.endDateInvalidMessage = ValidationService.dateInvalidMessage;
    } else if (ValidationService.isDateLessThanMinDateForBOSS(data)) {
      this.placeholderAllocation.endDate = '';
      this.validationObj.isEndDateInvalid = true;
      this.validationObj.endDateInvalidMessage = ValidationService.minDateInvalidMessage;
    } else {
      const newDate = DateService.convertDateInBainFormat(data);

      if (newDate !== this.placeholderAllocation.endDate) {
        this.placeholderAllocation.endDate = newDate;
        this.updateResource();
        // This is done to close editing of input as it remained open when you changed date afer correcting an error in input
        this.disableEndDateEdit('');
      }
    }

  }

  updateResource() {
    // added check for placeholder or regular allocation on planning card
    if (this.planningCard && (this.userAllocation || this.placeholderAllocation)) {
      if (this.userAllocation) {
        if (this.validateInputForPlaceholder(this.userAllocation)) {
          // add planning card ID to correctly save in DB
          const alloc = <PlaceholderAllocation>this.userAllocation;
          alloc.planningCardId = this.planningCard.id;
          this.upsertPlaceholderAllocationEmitter.emit(alloc);
        }
      } else if (this.placeholderAllocation) {
        if (this.validateInputForPlaceholder(this.placeholderAllocation)) {
          this.upsertPlaceholderAllocationEmitter.emit(this.placeholderAllocation);
        }
      }
    } else if (this.userAllocation && !this.planningCard) {
      if (this.validateInput(this.userAllocation)) {
        const [isValidAllocation, errorMessage] = this._resourceAllocationService.validateMonthCloseForUpdates(this.userAllocation, this.existingUserAllocation);

        if (!isValidAllocation) {
          this.notifyService.showValidationMsg(errorMessage);
        } else {
          this.userAllocation.previousAllocation = this.existingUserAllocation.allocation;
          this.userAllocation.previousEndDate = this.existingUserAllocation.endDate;
          this.userAllocation.previousStartDate = this.existingUserAllocation.startDate;
          this.checkForPrePostAndUpsertResourceAllocation(this.userAllocation);
        }

      }
    } else if (this.placeholderAllocation) {
      if (this.validateInput(this.placeholderAllocation)) {
        this.upsertPlaceholderAllocationEmitter.emit(this.placeholderAllocation);
      }
    }
  }

  checkForPrePostAndUpsertResourceAllocation(resourceAllocation: ResourceAllocation) {

    const projectStartDate = DateService.convertDateInBainFormat(
      resourceAllocation.oldCaseCode ? resourceAllocation.caseStartDate : resourceAllocation.opportunityStartDate);
    const projectEndDate = DateService.convertDateInBainFormat(
      resourceAllocation.oldCaseCode ? resourceAllocation.caseEndDate : resourceAllocation.opportunityEndDate);
    let allocationsData: ResourceAllocation[] = [];

    if (projectStartDate && projectEndDate) {

      allocationsData = this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(resourceAllocation);

    } else {

      allocationsData.push(resourceAllocation);

    }


    this.upsertResourceAllocationsToProject.emit({
      resourceAllocation: allocationsData,
      event: 'editResource',
      initialAllocationIndex: this.projectResourceComponentIndex
    });

  }

  validateInput(resourceAllocation) {
    let isValid = true;
    if (!ValidationService.isAllocationValid(resourceAllocation.allocation)) {
      this.validationObj.isAllocationInvalid = true;
      this.validationObj.allocationInvalidMessage = ValidationService.invalidAllocation;
      isValid = false;
    } else {
      this.validationObj.isAllocationInvalid = false;
      this.validationObj.allocationInvalidMessage = '';
      resourceAllocation.allocation = parseInt(resourceAllocation.allocation?.toString(), 10);
    }
    const dateErrorMessage = ValidationService.isDateValid(resourceAllocation.startDate?.toString(),
      resourceAllocation.endDate?.toString());
    if (dateErrorMessage !== '') {
      this.validationObj.isEndDateInvalid = true;
      this.validationObj.endDateInvalidMessage = dateErrorMessage;
      isValid = false;
    } else {
      this.validationObj.isEndDateInvalid = false;
      this.validationObj.endDateInvalidMessage = '';
    }
    return isValid;
  }

  validateInputForPlaceholder(resourceAllocation) {
    let isValid = true;
    if (!ValidationService.isAllocationValid(resourceAllocation.allocation)) {
      this.validationObj.isAllocationInvalid = true;
      this.validationObj.allocationInvalidMessage = ValidationService.invalidAllocation;
      isValid = false;
    } else if (!resourceAllocation.employeeCode) {
      this.notifyService.showWarning(ValidationService.employeeMissingOnGuessedPlaceholder);
      isValid = false;
    } else {
      this.validationObj.isAllocationInvalid = false;
      this.validationObj.allocationInvalidMessage = '';
      resourceAllocation.allocation = parseInt(resourceAllocation.allocation?.toString(), 10);
    }

    if (resourceAllocation.startDate && resourceAllocation.endDate) {

      const dateErrorMessage = ValidationService.isDateValid(resourceAllocation.startDate?.toString(),
        resourceAllocation.endDate?.toString());

      if (dateErrorMessage !== '') {
        this.validationObj.isEndDateInvalid = true;
        this.validationObj.endDateInvalidMessage = dateErrorMessage;
        isValid = false;
      } else {
        this.validationObj.isEndDateInvalid = false;
        this.validationObj.endDateInvalidMessage = '';
      }
    }

    return isValid;
  }

  removeResourceFromPlaceholderEmiiter() {
    this.placeholderAllocation.employeeCode = null;
    this.placeholderAllocation.employeeName = null;
    this.placeholderAllocation.operatingOfficeCode = null;
    this.placeholderAllocation.operatingOfficeAbbreviation = null;
    this.placeholderAllocation.currentLevelGrade = null;
    this.placeholderAllocation.allocation = null;
    this.placeholderAllocation.startDate = null;
    this.placeholderAllocation.endDate = null;
    this.placeholderAllocation.investmentCode = null;
    this.placeholderAllocation.caseRoleCode = null;
    this.placeholderAllocation.serviceLineCode = null;
    this.placeholderAllocation.serviceLineName = null;
    this.placeholderAllocation.commitmentTypeCode = null;
    this.placeholderAllocation.commitmentTypeName = null;
    this.placeholderAllocation.positionGroupCode = null;

    this.showGuessedPlaceholder = false;

    this.upsertPlaceholderAllocationEmitter.emit(this.placeholderAllocation);
    //this.removePlaceHolder.emit(this.placeholderAllocation);
  }

  confirmPlaceholderAllocation() {
    if (this.planningCard) {
      if (this.validateInputForPlaceholder(this.placeholderAllocation)) {
        this.confirmPlaceholderAllocationEmitter.emit(this.placeholderAllocation);
      }
    } else if (this.project) {
      if (this.validateInput(this.placeholderAllocation)) {
        this.confirmPlaceholderAllocationEmitter.emit(this.placeholderAllocation);
      }
    }
    this.showGuessedPlaceholder = false;
  }

  removePlaceHolderEmitter() {
    this.removePlaceHolder.emit(this.placeholderAllocation);
  }

  // toggleGuessedPlaceholder() {
  //   this.showGuessedPlaceholder = !this.showGuessedPlaceholder;
  // }
}
