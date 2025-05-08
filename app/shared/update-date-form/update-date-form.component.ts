import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import * as moment from 'moment';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { DateService } from '../dateService';
import { PlaceholderAllocation } from '../interfaces/placeholderAllocation.interface';
import { ResourceAllocation } from '../interfaces/resourceAllocation.interface';
import { PopupDragService } from '../services/popupDrag.service';
import { ResourceAllocationService } from '../services/resourceAllocation.service';
import { ValidationService } from '../validationService';
import { ResourceCommitment } from '../interfaces/resourceCommitment';

@Component({
  selector: 'app-update-date-form',
  templateUrl: './update-date-form.component.html',
  styleUrls: ['./update-date-form.component.scss']
})
export class UpdateDateFormComponent implements OnInit {
  public title = '';
  public resourceAllocations: ResourceAllocation[];
  public resourceCommitments: ResourceCommitment[];
  public placeholderAllocations: PlaceholderAllocation[];
  public formModel = {
    date: { value: null, isInvalid: false, errorMessage: '' }
  };
  bsConfig: Partial<BsDatepickerConfig>;

  // --------------------------Ouput Events--------------------------------//
  @Output() updateDataForSelectedAllocations = new EventEmitter<any>();
  @Output() updateDataForSelectedPlaceholders = new EventEmitter<any>();
  @Output() updateDataForSelectedCommitments = new EventEmitter<any>();

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
    this._popupDragService.dragEvents();

    if (!this.resourceAllocations && !this.placeholderAllocations) {
      this.formModel.date.isInvalid = true;
      this.formModel.date.errorMessage = ValidationService.errorWhileLoading;
    }
    else {
      this.formModel.date.isInvalid = false;
      this.formModel.date.errorMessage = '';
    }
  }

  // --------------------------Event handlers---------------------------------//

  updateAllocations() {
    if (this.validateFormModel()) {
      this.formModel.date.value = DateService.convertDateInBainFormat(this.formModel.date.value);

      if(this.resourceCommitments && this.resourceCommitments.length>0){
        if(this.title.toLowerCase() === 'downday'){
        this.updateDataForSelectedCommitments.emit({
          selectedCommitments: this.resourceCommitments,
          updatedDate: this.formModel.date.value,
          title: this.title
        });
        } 
      }

      if (this.resourceAllocations && this.resourceAllocations.length>0) { 
        if(this.title.toLowerCase() === 'start'|| this.title.toLowerCase() === 'end'){
          this.updateDataForSelectedAllocations.emit({
            selectedAllocations: this.resourceAllocations,
            updatedDate: this.formModel.date.value,
            title: this.title
          });
          }
      }
      
      if (this.placeholderAllocations && this.placeholderAllocations?.length>0) {
        if(this.title.toLowerCase() === 'start'){
          this.placeholderAllocations.forEach(x => x.startDate = this.formModel.date.value)
        } else if (this.title.toLowerCase() === 'end'){
          this.placeholderAllocations.forEach(x => x.endDate = this.formModel.date.value)
        }
        this.updateDataForSelectedPlaceholders.emit({
          placeholderAllocations: this.placeholderAllocations
        });
      
      }

      this.closeForm();
    }
  }

  validateFormModel() {
    this.validateDateRange();
    if (this.formModel.date.isInvalid) {
      return false;
    }
    return true;
  }

  validateDateRange() {
    let allocations = this.resourceAllocations ? [].concat(this.resourceAllocations) : [].concat(this.placeholderAllocations);

    if (!ValidationService.isValidDate(this.formModel.date.value)) {
      this.formModel.date.isInvalid = true;
      this.formModel.date.errorMessage = ValidationService.dateInvalidMessage;
    }
    else {
      switch (this.title.toLowerCase()) {
        case 'start':
          const startDateGreaterThanEndDate = !!(allocations.find(allocation =>
            moment(this.formModel.date.value).isAfter(moment(allocation.endDate))));
          const JoiningDateGreaterThanStartDate = !!(allocations.find(allocation => {
            if (allocation.joiningDate) {
              return moment(this.formModel.date.value).isBefore(moment(allocation.joiningDate));
            }
            return false;
          }));

          if (startDateGreaterThanEndDate) {
            this.formModel.date.isInvalid = true;
            this.formModel.date.errorMessage =
              ValidationService.startDateGreaterThanEndDate + ' of any of the selected allocations';
          } else if (JoiningDateGreaterThanStartDate) {

            this.formModel.date.isInvalid = true;
            this.formModel.date.errorMessage =
              `${ValidationService.joiningDateGreaterThanStartDate} of any of the selected allocations`;
          }
          else {
            this.formModel.date.isInvalid = false;
            this.formModel.date.errorMessage = '';
          }
          break;
        case 'end':
          const endDateLesserThanStartDate = !!(allocations.find(allocation =>
            moment(this.formModel.date.value).isBefore(moment(allocation.startDate))));
          if (endDateLesserThanStartDate) {
            this.formModel.date.isInvalid = true;
            this.formModel.date.errorMessage =
              ValidationService.endDateLesserThanStartDate + ' of any of the selected allocations';
          }
          else {
            this.formModel.date.isInvalid = false;
            this.formModel.date.errorMessage = '';
          }
          break;
        default:
          this.formModel.date.isInvalid = false;
          this.formModel.date.errorMessage = '';
          break;
      }
    }
  }

  closeForm() {
    this.bsModalRef.hide();
  }

}
