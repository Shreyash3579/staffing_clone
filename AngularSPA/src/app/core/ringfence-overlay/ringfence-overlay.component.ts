import { Component, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';

// --------------- Serivce --------------------- //
import { LocalStorageService } from '../../shared/local-storage.service';

// --------------- Interfaces --------------------- //
import { OfficeHierarchy } from 'src/app/shared/interfaces/officeHierarchy.interface';

// --------------------------utilities -----------------------------------------//
import { ConstantsMaster } from '../../shared/constants/constantsMaster';
import { CommitmentType } from 'src/app/shared/interfaces/commitmentType.interface';
import { CoreService } from '../core.service';
import { RingfenceManagement } from 'src/app/shared/interfaces/ringfenceManagement.interface';
import { DateService } from 'src/app/shared/dateService';
import { BS_DEFAULT_CONFIG } from 'src/app/shared/constants/bsDatePickerConfig';
import { UserPreferences } from 'src/app/shared/interfaces/userPreferences.interface';
import { ValidationService } from 'src/app/shared/validationService';
import { NotificationService } from 'src/app/shared/notification.service';

@Component({
  selector: 'app-ringfence-overlay',
  templateUrl: './ringfence-overlay.component.html',
  styleUrls: ['./ringfence-overlay.component.scss']
})
export class RingfenceOverlayComponent implements OnInit {

  constructor(private modalRef: BsModalRef, private localStorageService: LocalStorageService, 
    private coreService: CoreService, private notifyService: NotificationService) { }

  //----------------------------- Local Variables ------------------------//
  ringFencesDropdownList;
  officeHierarchy: OfficeHierarchy;
  ringFences: CommitmentType[] = [];
  selectedOfficeList = [];
  selectedRingFenceList: string[] = [];
  ringfenceData: RingfenceManagement[] = [];
  auditLog: RingfenceManagement[] = [];
  selectedIndex: number;
  bsConfig: Partial<BsDatepickerConfig>;
  userPreferences: UserPreferences;
  errorMessage: string = '';
  //----------------------------- Input Variables ------------------------//
  
  ngOnInit() {
    this.userPreferences = this.coreService.getUserPreferencesValue();
    this.getLookupListFromLocalStorage();
    this.initializeDropdown();
    this.getTotalResourcesByOfficesAndRingfences();
    
    this.bsConfig = BS_DEFAULT_CONFIG;
  }

  initializeDropdown(){
    this.setOfficesDropDown();
    this.setRingfencesDropDown();
  }

  //--------------- Event Handlers -------------------//

  getDataBySelectedRingfences(ringfenceTypeCodes: string){
    if (ringfenceTypeCodes && this.isArrayEqual(this.selectedRingFenceList.map(String), ringfenceTypeCodes.split(','))) {
      return false;
    }

    this.selectedRingFenceList = ringfenceTypeCodes.split(',');

    this.getTotalResourcesByOfficesAndRingfences();
  }

  getDataBySelectedOffices(officeCodes) {
    if (officeCodes && this.isArrayEqual(this.selectedOfficeList.map(String), officeCodes.split(','))) {
      return false;
    }

    this.selectedOfficeList = officeCodes.split(',');

    this.getTotalResourcesByOfficesAndRingfences();
  }

  getRingfenceAuditLogByOfficeAndCommitmentCode(selectedIndex: number, officeCode: string, commitmentTypeCode: string){
    this.coreService.getRingfenceAuditLogByOfficeAndCommitmentCode(officeCode,commitmentTypeCode).subscribe(data => {
      this.selectedIndex = selectedIndex;
      this.auditLog = data;
    });
  }

  getTotalResourcesByOfficesAndRingfences(){
    const officeCodes = this.selectedOfficeList.toString();
    const ringfenceCodes = this.selectedRingFenceList.toString();
    this.errorMessage = '';

    if(!officeCodes || !ringfenceCodes){
      this.errorMessage = "Please select atleast one office and ringfence to view data"
      this.ringfenceData = [];
      return;
    }

    this.coreService.getTotalResourcesByOfficesAndRingfences(officeCodes,ringfenceCodes).subscribe(data => {
      this.ringfenceData = data.map(x => {
        if(!x.effectiveDate) {
          x.effectiveDate = DateService.getBainFormattedToday();
        }else{
          x.effectiveDate = DateService.convertDateInBainFormat(x.effectiveDate);
        }
        return x;
      })
    });
  }

  isValid(dataToValidate: RingfenceManagement){
    const rfValidationObj  = ValidationService.validateDecimalBetween(dataToValidate.rfTeamsOwed);
    const effectiveDateValidationObj = ValidationService.validateDate(dataToValidate.effectiveDate);

    return {
      isValid: rfValidationObj.isValid && effectiveDateValidationObj.isValid,
      errorMessage: rfValidationObj.errorMessage + ". " + effectiveDateValidationObj.errorMessage
    }
  }

  upsertRingfenceDetails(upsertedData: RingfenceManagement){
    this.errorMessage = '';
    const validationObject = this.isValid(upsertedData);

    if(!validationObject.isValid){
      this.errorMessage = validationObject.errorMessage;
      return;
    }

    upsertedData.effectiveDate = DateService.convertDateInBainFormat(upsertedData.effectiveDate);
    this.coreService.upsertRingfenceDetails(upsertedData).subscribe(data => {
      upsertedData.id = data.id;
      upsertedData.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
      upsertedData.lastUpdatedByName = this.coreService.loggedInUser.fullName;
      this.notifyService.showSuccess("Ringfence data updated successfully !!")

    });
  }  

  private isArrayEqual(array1, array2) {
    return JSON.stringify(array1) === JSON.stringify(array2);
  }

  closeModal(){
    this.modalRef.hide();
  }

  //--------------- Helper functions -------------------//
  setOfficesDropDown(){

    this.selectedOfficeList = this.userPreferences?.supplyViewOfficeCodes?.split(',');

    // Re-assign object In order to reflect changes in Demand side office filter when changes are done in User Settings
    if (this.officeHierarchy) {
      this.officeHierarchy = JSON.parse(JSON.stringify(this.officeHierarchy));
    }
  }

  setRingfencesDropDown() {
    if (this.ringFences) {
      const ringfencesChildrenList = this.ringFences.map(item => {
        return {
          text: item.commitmentTypeName,
          value: item.commitmentTypeCode,
          checked: false,
          children: []
        };
      });

      this.ringFencesDropdownList = {
        text: 'All',
        value: 0,
        checked: false,
        children:ringfencesChildrenList
      };

      //const defaultRingfences = this.ringFences.filter(x => this.userPreferences.supplyViewStaffingTags?.split(",").includes(x.commitmentTypeCode)).map(y => y.commitmentTypeCode)
      this.selectedRingFenceList = [];

    }
  }

  private getLookupListFromLocalStorage() {
    this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.officeHierarchy);
    this.ringFences = this.localStorageService.get(ConstantsMaster.localStorageKeys.ringfences).filter(x => x.commitmentTypeCode !== 'PS'); //exclude PEG Syrge from RF;
  }

}
