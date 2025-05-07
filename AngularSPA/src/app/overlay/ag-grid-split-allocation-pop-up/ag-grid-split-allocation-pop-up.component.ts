import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { PopupDragService } from 'src/app/shared/services/popupDrag.service';
import { DateService } from 'src/app/shared/dateService';
import { ValidationService } from 'src/app/shared/validationService';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
import { ResourceAllocationService } from 'src/app/shared/services/resourceAllocation.service';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';

@Component({
  selector: 'app-ag-grid-split-allocation-pop-up',
  templateUrl: './ag-grid-split-allocation-pop-up.component.html',
  styleUrls: ['./ag-grid-split-allocation-pop-up.component.scss'],
  providers: [PopupDragService]
})
export class AgGridSplitAllocationPopUpComponent implements OnInit {
  // -----------------------Local Variables--------------------------------------------//
  public allocationData: any;
  public notes = '';
  bsConfig: Partial<BsDatepickerConfig>;
  firstMinDate: Date;
  firstMaxDate: Date;
  secondMinDate: Date;
  secondMaxDate: Date;

  public formModel = {
    firstAllocation: {
      startDate: { value: null, isInvalid: false, errorMessage: '' },
      endDate: { value: null, isInvalid: false, errorMessage: '' },
      allocation: { value: null, isInvalid: false, errorMessage: '' }
    },
    secondAllocation: {
      startDate: { value: null, isInvalid: false, errorMessage: '' },
      endDate: { value: null, isInvalid: false, errorMessage: '' },
      allocation: { value: null, isInvalid: false, errorMessage: '' }
    },
    allocations: { isInvalid: false, errorMessage: '' }
  };

  // --------------------------Ouput Events--------------------------------//
  @Output() upsertResourceAllocationsToProject = new EventEmitter<any>();
  @Output() upsertPlaceholderAllocationsToProject = new EventEmitter<any>();

  constructor(public bsModalRef: BsModalRef,
    private _popupDragService: PopupDragService,
    private _resourceAllocationService: ResourceAllocationService) { }

  ngOnInit() {
    this.bsConfig = {
      containerClass: 'theme-red',
      customTodayClass: 'custom-today-class',
      dateInputFormat: 'DD-MMM-YYYY',
      isAnimated: true,
      showWeekNumbers: false,
      selectFromOtherMonth: true
    };
    if (this.allocationData) {

      if (this.allocationData.startDate && this.allocationData.endDate) {
        this.setMinMaxDates();
        this.formModel.firstAllocation.startDate.value = this.allocationData.startDate;
        this.formModel.secondAllocation.endDate.value = this.allocationData.endDate;
      }
      if (this.allocationData.allocation) {
        const allocation = isNaN(this.allocationData.allocation)
          ? (this.allocationData.allocation.indexOf('%') !== -1)
            ? Number(this.allocationData.allocation.slice(0, this.allocationData.allocation.indexOf('%')))
            : Number(this.allocationData.allocation)
          : this.allocationData.allocation;
        this.formModel.firstAllocation.allocation.value = allocation;
        this.formModel.secondAllocation.allocation.value = allocation;
      }

    }

    this._popupDragService.dragEvents();
  }

  // --------------------------Event handlers---------------------------------//

  startDateChange() {
    const startDate = this.formModel.secondAllocation.startDate;
    const endDate = this.formModel.firstAllocation.endDate;

    if (!!startDate.value) {
      if (!this.isDateWithinDateRange(startDate.value)) {
        startDate.value = new Date(this.formModel.secondAllocation.endDate.value);
      }
      endDate.value = new Date(startDate.value);
      if (new Date(startDate.value) > new Date(this.formModel.firstAllocation.startDate.value)) {
        endDate.value.setDate(startDate.value.getDate() - 1);
      }
      endDate.isInvalid = false;
      endDate.errorMessage = '';
    }
  }

  endDateChange() {
    const startDate = this.formModel.secondAllocation.startDate;
    const endDate = this.formModel.firstAllocation.endDate;

    if (!!endDate.value) {
      if (!this.isDateWithinDateRange(endDate.value)) {
        endDate.value = new Date(this.formModel.firstAllocation.startDate.value);
      }

      startDate.value = new Date(endDate.value);
      if (new Date(endDate.value) < new Date(this.formModel.secondAllocation.endDate.value)) {
        startDate.value.setDate(startDate.value.getDate() + 1);
      }
      startDate.isInvalid = false;
      startDate.errorMessage = '';
    }
  }

  updateCommitment() {
    if (this.validateFormModel()) {
   
   
      if(this.allocationData.oldCaseCode || this.allocationData.pipelineId){
        const regularAllocation: ResourceAllocation = this.convertToResourceAllocationModel(this.formModel.firstAllocation, this.allocationData.id, true);
        const splittedAllocation: ResourceAllocation = this.convertToResourceAllocationModel(this.formModel.secondAllocation, null);
        const allocationsToUpsert = this._resourceAllocationService
        .checkAndSplitAllocationsForPrePostRevenue([].concat(regularAllocation).concat(splittedAllocation));
        this.upsertResourceAllocationsToProject.emit({
          resourceAllocation: allocationsToUpsert,
          event: 'splitAllocationPopUp',
          showMoreThanYearWarning: false
        });
    }
    else{
      const regularPlaceHolderAllocation: ResourceAllocation = this.convertToPlaceHolderAllocationModel(this.formModel.firstAllocation, this.allocationData.id, true);
      const splittedPlaceholderAllocation: ResourceAllocation = this.convertToPlaceHolderAllocationModel(this.formModel.secondAllocation, null);
      const allocationsToUpsert = [].concat(regularPlaceHolderAllocation).concat(splittedPlaceholderAllocation);
      this.upsertPlaceholderAllocationsToProject.emit({
        placeholderAllocations: allocationsToUpsert,
      });
    }

      this.closeForm();
    }
  }

  convertToResourceAllocationModel(updatedAllocationData, allocationId = null, firstAllocation = false){
    const resourceAllocation: ResourceAllocation = {
      id: allocationId,
      caseName: this.allocationData.caseName,
      clientName: this.allocationData.clientName,
      oldCaseCode: this.allocationData.oldCaseCode,
      employeeCode: this.allocationData.employeeCode,
      employeeName: this.allocationData.employeeName,
      currentLevelGrade: this.allocationData.currentLevelGrade,
      operatingOfficeCode: this.allocationData.operatingOfficeCode,
      operatingOfficeAbbreviation: this.allocationData.operatingOfficeAbbreviation,
      pipelineId: this.allocationData.pipelineId,
      opportunityName: this.allocationData.opportunityName,
      planningCardId: this.allocationData.planningCardId,
      planningCardTitle: this.allocationData.planningCardTitle,
      isPlaceholderAllocation: this.allocationData.isPlaceholderAllocation,
      investmentCode: this.allocationData.investmentCode,
      investmentName: this.allocationData.investmentName,
      caseRoleCode: this.allocationData.caseRoleCode,
      allocation: updatedAllocationData.allocation.value,
      startDate: DateService.convertDateInBainFormat(updatedAllocationData.startDate.value),
      endDate: DateService.convertDateInBainFormat(updatedAllocationData.endDate.value),
      previousStartDate: firstAllocation ? DateService.convertDateInBainFormat(this.allocationData.startDate) : null,
      previousEndDate: firstAllocation ? DateService.convertDateInBainFormat(this.allocationData.endDate) : null,
      previousAllocation: firstAllocation ? this.allocationData.allocation : null,
      caseStartDate: this.allocationData.caseStartDate,
      caseEndDate: this.allocationData.caseEndDate,
      opportunityStartDate: this.allocationData.opportunityStartDate,
      opportunityEndDate: this.allocationData.opportunityEndDate,
      lastUpdatedBy: this.allocationData.lastUpdatedBy,
      notes: this.allocationData.notes,
      serviceLineCode: this.allocationData.serviceLineCode,
      serviceLineName: this.allocationData.serviceLineName
    };

    return resourceAllocation;
  }

  convertToPlaceHolderAllocationModel(updatedAllocationData, allocationId = null, firstAllocation = false){
    const resourceAllocation: PlaceholderAllocation = {
      id: allocationId,
      caseName: this.allocationData.caseName,
      clientName: this.allocationData.clientName,
      oldCaseCode: this.allocationData.oldCaseCode,
      employeeCode: this.allocationData.employeeCode,
      employeeName: this.allocationData.employeeName,
      currentLevelGrade: this.allocationData.currentLevelGrade,
      operatingOfficeCode: this.allocationData.operatingOfficeCode,
      operatingOfficeAbbreviation: this.allocationData.operatingOfficeAbbreviation,
      pipelineId: this.allocationData.pipelineId,
      opportunityName: this.allocationData.opportunityName,
      planningCardId: this.allocationData.planningCardId,
      planningCardTitle: this.allocationData.planningCardTitle,
      isPlaceholderAllocation: this.allocationData.isPlaceholderAllocation,
      investmentCode: this.allocationData.investmentCode,
      investmentName: this.allocationData.investmentName,
      caseRoleCode: this.allocationData.caseRoleCode,
      allocation: updatedAllocationData.allocation.value,
      startDate: DateService.convertDateInBainFormat(updatedAllocationData.startDate.value),
      endDate: DateService.convertDateInBainFormat(updatedAllocationData.endDate.value),
      caseStartDate: this.allocationData.caseStartDate,
      caseEndDate: this.allocationData.caseEndDate,
      opportunityStartDate: this.allocationData.opportunityStartDate,
      opportunityEndDate: this.allocationData.opportunityEndDate,
      lastUpdatedBy: this.allocationData.lastUpdatedBy,
      notes: this.allocationData.notes,
      serviceLineCode: this.allocationData.serviceLineCode,
      serviceLineName: this.allocationData.serviceLineName
    };

    return resourceAllocation;
  }

  validateFormModel() {
    this.validateDateRange(this.formModel.firstAllocation.startDate, this.formModel.firstAllocation.endDate);
    this.validateAllocation(this.formModel.firstAllocation.allocation);
    this.validateDateRange(this.formModel.secondAllocation.startDate, this.formModel.secondAllocation.endDate);
    this.validateAllocation(this.formModel.secondAllocation.allocation);
    this.validateEntireData();

    if (this.formModel.firstAllocation.startDate.isInvalid || this.formModel.secondAllocation.startDate.isInvalid ||
      this.formModel.firstAllocation.endDate.isInvalid || this.formModel.secondAllocation.endDate.isInvalid ||
      this.formModel.firstAllocation.allocation.isInvalid || this.formModel.secondAllocation.allocation.isInvalid ||
      this.formModel.allocations.isInvalid) {
      return false;
    }
    return true;
  }

  validateEntireData() {
    const firstStartDate = DateService.convertDateInBainFormat(this.formModel.firstAllocation.startDate.value);
    const secondStartDate = DateService.convertDateInBainFormat(this.formModel.secondAllocation.startDate.value);
    const firstEndDate = DateService.convertDateInBainFormat(this.formModel.firstAllocation.endDate.value);
    const secondEndDate = DateService.convertDateInBainFormat(this.formModel.secondAllocation.endDate.value);
    const firstAllocation = this.formModel.firstAllocation.allocation.value;
    const secondAllocation = this.formModel.secondAllocation.allocation.value;

    const convertedAllocation =   this.convertToResourceAllocationModel(this.formModel.firstAllocation,this.allocationData.id);
    
    const [isValidAllocation, errorMessage] = this._resourceAllocationService.validateMonthCloseForUpdates(convertedAllocation, this.allocationData);

    if (firstStartDate === secondStartDate && firstEndDate === secondEndDate &&
      firstAllocation === secondAllocation) {
      this.formModel.allocations.isInvalid = true;
      this.formModel.allocations.errorMessage = 'Cannot create duplicate allocations';
    } else if(!isValidAllocation){
      this.formModel.allocations.isInvalid = true;
      this.formModel.allocations.errorMessage = errorMessage;
    }else {
      this.formModel.allocations.isInvalid = false;
      this.formModel.allocations.errorMessage = '';
    }
  }

  validateDateRange(startDate: { value: any; isInvalid: boolean; errorMessage: string; },
    endDate: { value: any; isInvalid: boolean; errorMessage: string; }) {
    if (!this.validateDate(startDate).isInvalid && !this.validateDate(endDate).isInvalid) {
      if (Date.parse(new Date(endDate.value).toDateString()) < Date.parse(new Date(startDate.value).toDateString())) {
        if (Object.prototype.toString.call(startDate.value) === '[object Date]') {
          startDate.isInvalid = true;
          startDate.errorMessage = ValidationService.startDateGreaterThanEndDate;
        } else {
          endDate.isInvalid = true;
          endDate.errorMessage = ValidationService.startDateGreaterThanEndDate;
        }
      }
    }
  }

  validateAllocation(allocation: { value: any; isInvalid: boolean; errorMessage: string; }) {
    if (!allocation.value || !ValidationService.isAllocationValid(allocation.value)) {
      allocation.isInvalid = true;
      allocation.errorMessage = ValidationService.numberInvalidMessage;
    } else {
      allocation.isInvalid = false;
      allocation.errorMessage = '';
    }
  }

  validateDate(dateObj: { value: any; isInvalid: boolean; errorMessage: string; }) {
    if (!dateObj || !ValidationService.isValidDate(DateService.convertDateInBainFormat(dateObj.value))) {
      dateObj.isInvalid = true;
      dateObj.errorMessage = ValidationService.dateInvalidMessage;
    } else if (!this.isDateWithinDateRange(dateObj.value)) {
      dateObj.isInvalid = true;
      dateObj.errorMessage = ValidationService.dateOutsideDateRange;
    } else {
      dateObj.isInvalid = false;
      dateObj.errorMessage = '';
    }
    return dateObj;
  }

  isDateWithinDateRange(date: any) {
    if (date) {
      const selectedDate = Date.parse(new Date(date).toDateString());
      const allocationStartDate = Date.parse(new Date(this.formModel.firstAllocation.startDate.value).toDateString());
      const allocationEndDate = Date.parse(new Date(this.formModel.secondAllocation.endDate.value).toDateString());

      if (selectedDate >= allocationStartDate && selectedDate <= allocationEndDate) {
        return true;
      }
    }
    return false;
  }

  setMinMaxDates() {
    this.firstMinDate = new Date(this.allocationData.startDate);
    this.secondMinDate = new Date(this.allocationData.startDate);
    this.secondMinDate.setDate(this.secondMinDate.getDate() + 1);

    this.firstMaxDate = new Date(this.allocationData.endDate);
    this.secondMaxDate = new Date(this.allocationData.endDate);
    this.firstMaxDate.setDate(this.secondMaxDate.getDate() - 1);
  }

  closeForm() {
    this.bsModalRef.hide();
  }

}
