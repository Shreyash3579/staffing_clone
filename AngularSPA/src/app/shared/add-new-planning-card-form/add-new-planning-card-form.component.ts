import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { BsModalRef } from 'ngx-bootstrap/modal';


// Constants and Interfaces
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { OfficeHierarchy } from 'src/app/shared/interfaces/officeHierarchy.interface';
//import { ServiceLineHierarchy } from 'src/app/shared/interfaces/serviceLineHierarchy.interface';

// Services
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { StaffingTag } from '../constants/enumMaster';
import { CoreService } from 'src/app/core/core.service';
import { ServiceLineHierarchy } from '../interfaces/serviceLineHierarchy';
import { ValidationService } from 'src/app/shared/validationService';
import { UserPreferences } from '../interfaces/userPreferences.interface';
import { PlanningCard } from '../interfaces/planningCard.interface';
import { DateService } from '../dateService';
import { v4 as uuidv4 } from 'uuid';
import { BS_DEFAULT_CONFIG } from '../constants/bsDatePickerConfig';
@Component({
  selector: 'app-add-new-planning-card-form',
  templateUrl: './add-new-planning-card-form.component.html',
  styleUrls: ['./add-new-planning-card-form.component.scss']
})
export class AddNewPlanningCardFormComponent implements OnInit {
  formModel = {
    name: { value: '', isInvalid: false },
    startDate: { value: new Date(), isInvalid: false },
    endDate: { value: this.addMonths(new Date(), 3), isInvalid: false },
    office: { value: [], isInvalid: false },
    staffingTag: { value: null, isInvalid: false },
    includeInCapacity: { value: true, isInvalid: false },
  };
  errorList: string[] = [];
  offices = []; // Fetch or define your office options
  staffingStages = []; // Fetch or define your staffing stage options
  bsConfig: Partial<BsDatepickerConfig>;
  officeHierarchy: OfficeHierarchy;
  staffingTagDropdownList = {};
  staffingTags: ServiceLineHierarchy[] = [];
  selectedOfficeList = [];
  selectedStaffingTagList = [];
  constructor(public bsModalRef: BsModalRef,
    private localStorageService: LocalStorageService,
     private coreService: CoreService
  ){}
  userPreferences: any;
 @Output() upsertPlanningCard = new EventEmitter();

  ngOnInit(): void {
    this.initialiseDatePicker();
    this.getMasterDataForDropDowns();
    this.initializeDropDowns();
    this.initializeFormData();
  }

  initialiseDatePicker() {

    this.bsConfig = BS_DEFAULT_CONFIG;

  }

  initializeFormData() {
    const userSettings: UserPreferences = JSON.parse(
      this.localStorageService.get(ConstantsMaster.localStorageKeys.userPreferences)
  );
    this.setSelectedOfficeList(userSettings?.demandViewOfficeCodes);
    this.setSelectedStaffingTagList(userSettings?.caseAttributeNames);
    
  }


  getMasterDataForDropDowns() {
    //TODO: Hard coding for now to proivde EMEA practice staffing users access to only EMEA data.
    //DELETE once integrated multiple-role based office security is implemented
    if(this.coreService.loggedInUserClaims.Roles?.includes('PracticeStaffing')){
      this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.accessibleOfficeHierarchyForUser);
    }else{
      this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.officeHierarchy);
    }

    this.staffingTags = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffingTagsHierarchy);

  }

  initializeDropDowns() {

    this.staffingTagDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.staffingTags.map((item) => {
        return {
          text: item.text,
          value: item.value,
          collapsed: true,
          children: item.children != null ? item.children.map(child => {
            return {
              text : child.text,
              value : child.value,
              checked: false
            };
          }) : null,
          checked: false
        };
      })
    };
  }

  addMonths(date: Date, months: number): Date {
    let result = new Date(date);
    result.setMonth(result.getMonth() + months);
    return result;
  }

  setSelectedOfficeList(officeCodes) {
    this.selectedOfficeList = officeCodes.split(',');
    this.formModel.office.value = this.selectedOfficeList;
  }


  setSelectedStaffingTagList(staffingTags) {
    this.selectedStaffingTagList = staffingTags.split(',');
    this.formModel.staffingTag.value =this.selectedStaffingTagList;
  }

  private isPlanningCardDataValid() {
    this.errorList = [];
    this.validateField('startDate');
    this.validateField('endDate');
    this.validateField('name');
    this.validateField('offices');
    this.validateField('staffingTags');
    if (this.formModel.name.isInvalid ||this.formModel.office.isInvalid || this.formModel.staffingTag.isInvalid
    || this.formModel.startDate.isInvalid || this.formModel.endDate.isInvalid ) {
      return false;
    } else {
      return true;
    }
  }

  private validateField(fieldName) {
    switch (fieldName) {

      case 'name': {
        if (!this.formModel.name.value) {
          this.formModel.name.isInvalid = true;
          this.addToErrorList('required');
        } else {
          this.formModel.name.isInvalid = false;
        }
        break;
      }

      case 'startDate': {
        if (!this.formModel.startDate.value) {
          this.formModel.startDate.isInvalid = true;
          this.addToErrorList('required');
        } else if (this.formModel.startDate.value.toDateString() === undefined
          || this.formModel.startDate.value.toDateString() === 'Invalid Date') {
          this.formModel.startDate.isInvalid = true;
          this.addToErrorList('dateInvalid');
        } else {
          this.formModel.startDate.isInvalid = false;
        }
        break;
      }

      case 'endDate': {
        if (!this.formModel.endDate.value) {
          this.formModel.endDate.isInvalid = true;
          this.addToErrorList('required');
        } else if (this.formModel.endDate.value.toDateString() === undefined
          || this.formModel.endDate.value.toDateString() === 'Invalid Date') {
          this.formModel.endDate.isInvalid = true;
          this.addToErrorList('dateInvalid');
        } else if (Date.parse(this.formModel.endDate.value.toDateString()) < Date.parse(this.formModel.startDate.value.toDateString())) {
          this.formModel.endDate.isInvalid = true;
          this.addToErrorList('startDateGreaterThanEndDate');
        } else {
          this.formModel.endDate.isInvalid = false;
        }
        break;
      }
      case 'offices': {
        if (!this.formModel.office.value || this.formModel.office.value[0 ] == "") {
          this.formModel.office.isInvalid = true;
          this.addToErrorList('required');
        } else {
          this.formModel.office.isInvalid = false;
        }
        break;
      }
      case 'staffingTags': {
        if (!this.formModel.staffingTag.value || this.formModel.staffingTag.value[0 ] == "") {
          this.formModel.staffingTag.isInvalid = true;
          this.addToErrorList('required');
        } else {
          this.formModel.staffingTag.isInvalid = false;
        }
        break;
      }
    }
  }

  addToErrorList(type) {
    switch (type) {
      case 'required': {
        if (this.errorList.indexOf(ValidationService.requiredMessage) === -1) {
          this.errorList.push(ValidationService.requiredMessage);
        }
        break;
      }
      case 'dateInvalid': {
        if (this.errorList.indexOf(ValidationService.dateInvalidMessage) === -1) {
          this.errorList.push(ValidationService.dateInvalidMessage);
        }
        break;
      }
      case 'numberInvalid': {
        if (this.errorList.indexOf(ValidationService.numberInvalidMessage) === -1) {
          this.errorList.push(ValidationService.numberInvalidMessage);
        }
        break;
      }
      case 'typeaheadInvalid': {
        if (this.errorList.indexOf(ValidationService.typeaheadInvalidMessage) === -1) {
          this.errorList.push(ValidationService.typeaheadInvalidMessage);
        }
        break;
      }
      case 'dateDiffInvalid': {
        if (this.errorList.indexOf(ValidationService.dateDiffInvalid) === -1) {
          this.errorList.push(ValidationService.dateDiffInvalid);
        }
        break;
      }
      case 'startDateGreaterThanEndDate': {
        if (this.errorList.indexOf(ValidationService.startDateGreaterThanEndDate) === -1) {
          this.errorList.push(ValidationService.startDateGreaterThanEndDate);
        }
        break;
      }
    }
  }


  private upsertPlanningCardDetails() {
    this.upsertPlanningCard.emit({ 
    id: uuidv4(),
    startDate: DateService.getFormattedDate(new Date(this.formModel.startDate?.value)),
    endDate: DateService.getFormattedDate(new Date(this.formModel.endDate?.value)),
    name: this.formModel.name?.value,
    sharedOfficeCodes: this.formModel.office?.value.join(','),
    sharedStaffingTags: this.formModel.staffingTag?.value.join(','),
    isShared: true,
    includeInCapacityReporting: this.formModel.includeInCapacity?.value,
    probabilityPercent:100 });
  }

  closeForm(): void {

    this.bsModalRef.hide();
  }

  savePlanningCard() {
    if (!this.isPlanningCardDataValid()) {
      return false;
    }
    this.upsertPlanningCardDetails();
    this.bsModalRef.hide();
}

}
