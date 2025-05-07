import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ConstantsMaster } from '../constants/constantsMaster';
import { ServiceLine } from '../constants/enumMaster';
import { OfficeHierarchy } from '../interfaces/officeHierarchy.interface';
import { PlanningCard } from '../interfaces/planningCard.interface';
import { ServiceLineHierarchy } from '../interfaces/serviceLineHierarchy';
import { LocalStorageService } from '../local-storage.service';
import { PopupDragService } from '../services/popupDrag.service';

@Component({
  selector: 'app-share-planning-card',
  templateUrl: './share-planning-card.component.html',
  styleUrls: ['./share-planning-card.component.scss']
})
export class SharePlanningCardComponent implements OnInit {
  // -----------------------Local Variables--------------------------------------------//

  modalHeaderText: string;
  planningCard: PlanningCard;
  isPegPlanningCard: boolean;
  errorMessage = '';
  officeHierarchy: OfficeHierarchy;
  selectedOfficeList = [];
  staffingTagsHierarchy: ServiceLineHierarchy[];
  staffingTagDropdownList: any;
  selectedStaffingTagList = [];
  serviceLineEnum: typeof ServiceLine = ServiceLine;
  officeDropdownMaxHeight = 300;
  staffingTagsDropdownMaxHeight = 270;
  includeInCapacityReporting: boolean = false;

  // -----------------------Output Events--------------------------------------------//

  @Output() sharePlanningCardEmitter = new EventEmitter<any>();

  constructor(public bsModalRef: BsModalRef,
    private _popupDragService: PopupDragService,
    private localStorageService: LocalStorageService,
  ) { }

  ngOnInit() {
    this._popupDragService.dragEvents();
    this.setDropdownValuesFromLocalStorage();
    this.setStaffingTagsDropDown();
    this.setSharedPlanningCardInfo();
    this.setIncludeInCapacityReporting();
  }
 
  setIncludeInCapacityReporting(){
    if (!!this.planningCard.includeInCapacityReporting) {
      this.includeInCapacityReporting = true;
    }
  }
  setSharedPlanningCardInfo() {
    if (!!this.planningCard.isShared) {
      this.selectedOfficeList = this.planningCard.sharedOfficeCodes.split(',');
      this.selectedStaffingTagList = this.planningCard.sharedStaffingTags.split(',');
    }
  }

  updateSelectedStaffingTagList(staffingTagCodes) {
    if (this.isArrayEqual(this.selectedStaffingTagList.map(String), staffingTagCodes.split(','))) {
      return false;
    }

    this.selectedStaffingTagList = staffingTagCodes.split(',');
  }

  updateSelectedOfficeList(officeCodes) {
    if (officeCodes && this.isArrayEqual(this.selectedOfficeList.map(String), officeCodes.split(','))) {
      return false;
    }

    this.selectedOfficeList = officeCodes.split(',');
  }

  setStaffingTagsDropDown() {
    if (this.staffingTagsHierarchy) {
      this.staffingTagDropdownList = {
        text: 'All',
        value: 0,
        checked: false,
        children: this.staffingTagsHierarchy.map((item) => {
          return {
            text: item.text,
            value: item.value,
            collapsed: true,
            children: item.children != null ? item.children.map(child => {
              return {
                text: child.text,
                value: child.value,
                checked: false
              };
            }) : null,
            checked: false
          };
        })
      };

      this.selectedStaffingTagList = [this.serviceLineEnum.GeneralConsulting];
    }
  }


  sharePlaceholder() {
    if (this.isSelectedValuesValid()) {
      const officeCodes = this.selectedOfficeList.toString();
      const staffingTags = this.selectedStaffingTagList.toString();
      this.planningCard.sharedOfficeCodes = officeCodes;
      this.planningCard.sharedStaffingTags = staffingTags;
      this.planningCard.includeInCapacityReporting = this.includeInCapacityReporting;
      this.planningCard.isShared = true;

      this.sharePlanningCardEmitter.emit({ planningCard: this.planningCard});
      this.closeDialogHandler();
    }
  }

  closeDialogHandler() {
    this.errorMessage = '';
    this.bsModalRef.hide();
  }

  setDropdownValuesFromLocalStorage() {
    this.staffingTagsHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffingTagsHierarchy);
    this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.officeHierarchy);
  }

  isSelectedValuesValid() {
    if ((Array.isArray(this.selectedOfficeList) && this.selectedOfficeList.toString().length > 0)
      && (Array.isArray(this.selectedStaffingTagList) && this.selectedStaffingTagList.toString().length > 0)) {
      this.errorMessage = '';
      return true;
    }
    this.errorMessage = 'Please select atleast one option in both the dropdowns';
    return false;
  }

  private isArrayEqual(array1, array2) {
    return JSON.stringify(array1) === JSON.stringify(array2);
  }

}
