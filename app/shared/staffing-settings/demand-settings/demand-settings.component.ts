// -------------------Angular Operators---------------------------------------//
import { Component, OnInit, Input } from '@angular/core';

// -------------------Interfaces---------------------------------------//
import { CaseType } from 'src/app/shared/interfaces/caseType.interface';
import { Office } from 'src/app/shared/interfaces/office.interface';
import { OfficeHierarchy } from 'src/app/shared/interfaces/officeHierarchy.interface';
import { UserPreferences } from 'src/app/shared/interfaces/userPreferences.interface';

// -------------------Services-------------------------------------------//
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { OpportunityStatusType } from 'src/app/shared/interfaces/opportunityStatusType';
import { CaseType as CaseTypeEnum } from 'src/app/shared/constants/enumMaster';
import { ValidationService } from 'src/app/shared/validationService';
import { ServiceLineHierarchy } from 'src/app/shared/interfaces/serviceLineHierarchy';
import { PracticeArea } from 'src/app/shared/interfaces/practiceArea.interface';
@Component({
  selector: 'app-demand-settings',
  templateUrl: './demand-settings.component.html',
  styleUrls: ['./demand-settings.component.scss']
})
export class DemandSettingsComponent implements OnInit {

  // -------------------Local variables-------------------------------------------//

  isIntervalDisabled = false;
  selectedDemandViewOfficeList = [];
  selectedCaseTypeList = [];
  selectedStaffingTagDropdownList = [];
  selectedOpportunityStatusTypeList = [];
  selectedDemandTypeList = [];
  selectedIndustryPracticeAreaList = [];
  selectedCapabilityPracticeAreaList = [];

  // -------------------Variable for Master List from DB ---------------------------------------------------//

  caseTypes: CaseType[] = [];
  opportunityStatusTypes: OpportunityStatusType[] = [];
  demandTypes = [];
  industryPracticeAreas: PracticeArea[] = [];
  capabilityPracticeAreas: PracticeArea[] = [];
  // -------------------Data sources for Dropdowns-------------------------------------------//

  caseTypeDropdownList;
  opportunityStatusTypeDropdownList;
  demandTypeList;
  officeHierarchy: OfficeHierarchy;
  staffingTagDropdownList;
  staffingTagsHierarchy: ServiceLineHierarchy[];
  caseAllocationsSortByList = ConstantsMaster.CaseAllocationsSortByOptions;
  selectedSortByItem = '';
  industryPracticeAreaDropdownList;
  capabilityPracticeAreaDropdownList;

  // -------------------Input Variables -------------------------------------------//

  @Input() userPreferences: UserPreferences;
  @Input() loggedInUserHomeOffice: Office;

  // -------------------Constructor -------------------------------------------//

  constructor(private localStorageService: LocalStorageService) { }

  // -------------------Life Cycle Events -------------------------------------------//

  ngOnInit() {
    this.getMasterDataForDropDowns();
    this.intilializeDropDowns();
    this.setUserSettings();
  }

  // -------------------Helper Functions -------------------------------------------//

  getMasterDataForDropDowns() {

    this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.officeHierarchy);
    this.caseTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.caseTypes);
    this.opportunityStatusTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.opportunityStatusTypes);
    this.staffingTagsHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffingTagsHierarchy);
    this.demandTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.demandTypes);
    this.industryPracticeAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.industryPracticeAreas);
    this.capabilityPracticeAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.capabilityPracticeAreas);
  }

  intilializeDropDowns() {

    this.caseTypeDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.caseTypes.map(type => {
        return {
          text: type.caseTypeName,
          value: type.caseTypeCode.toString(),
          checked: false
        };
      })
    };

    this.opportunityStatusTypeDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.opportunityStatusTypes.map(type => {
        return {
          text: type.statusText,
          value: type.statusCode,
          checked: false
        };
      })
    };

    this.demandTypeList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.demandTypes.map(x => {
        return {
          text: x.name,
          value: x.type,
          checked: false
        };
      })
    };


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

    this.industryPracticeAreaDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.industryPracticeAreas.map(x => {
        return {
          text: x.practiceAreaName,
          value: x.practiceAreaCode,
          checked: false
        };
      })
    };

    this.capabilityPracticeAreaDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.capabilityPracticeAreas.map(x => {
        return {
          text: x.practiceAreaName,
          value: x.practiceAreaCode,
          checked: false
        };
      })
    };

  }

  setUserSettings() {

    /*
      ** if user has saved some user prefrences, then set them else set defaults
      ** assigning the saved preferences to variables will bind/check the corresponding dropdown items
    */

    if (this.userPreferences && this.userPreferences.demandViewOfficeCodes) {
      this.selectedDemandViewOfficeList = this.userPreferences.demandViewOfficeCodes.split(',');
    } else {
      this.selectedDemandViewOfficeList.push(this.loggedInUserHomeOffice.officeCode.toString());
    }

    if (this.userPreferences && this.userPreferences.caseTypeCodes) {
      this.selectedCaseTypeList = this.userPreferences.caseTypeCodes.split(',');
    } else {
      this.selectedCaseTypeList.push(CaseTypeEnum.Billable); // billable by default
    }

    if (this.userPreferences && this.userPreferences.caseAttributeNames) {
      this.selectedStaffingTagDropdownList = this.userPreferences.caseAttributeNames.split(',');
    } else {
      this.selectedStaffingTagDropdownList = [];
    }

    if (this.userPreferences && this.userPreferences.opportunityStatusTypeCodes) {
      this.selectedOpportunityStatusTypeList = this.userPreferences.opportunityStatusTypeCodes.split(',');
    } else {
      this.selectedOpportunityStatusTypeList = this.opportunityStatusTypes.map(x => x.statusCode); // show all by default
    }

    if (this.userPreferences && this.userPreferences.demandTypes) {
      this.selectedDemandTypeList = this.userPreferences.demandTypes.split(',');
    } else {
      this.selectedDemandTypeList = this.demandTypes.map(x => x.type); // show all by default
    }

    if (this.userPreferences && this.userPreferences.caseAllocationsSortBy) {
      this.selectedSortByItem = this.userPreferences.caseAllocationsSortBy;
    } else {
      this.selectedSortByItem = this.caseAllocationsSortByList[0].value;
    }

    if (this.userPreferences && this.userPreferences.industryPracticeAreaCodes) {
      this.selectedIndustryPracticeAreaList = this.userPreferences.industryPracticeAreaCodes.split(',');
    } else {
      this.selectedIndustryPracticeAreaList = [];
    }

    if (this.userPreferences && this.userPreferences.capabilityPracticeAreaCodes) {
      this.selectedCapabilityPracticeAreaList = this.userPreferences.capabilityPracticeAreaCodes.split(',');
    } else {
      this.selectedCapabilityPracticeAreaList = [];
    }
  }

  // -------------------Event Handlers -------------------------------------------//

  setSelectedStaffingTagDropdownList(staffingTagDropdownList) {
    this.selectedStaffingTagDropdownList = staffingTagDropdownList.split(',');
  }

  setSelectedCaseTypeList(caseTypeCodes) {
    this.selectedCaseTypeList = caseTypeCodes.split(',');
  }

  setSelectedDemandViewOfficeList(officeCodes) {
    this.selectedDemandViewOfficeList = officeCodes.split(',');
  }

  setSelectedOpportunityStatusTypeList(statusTypes) {
    this.selectedOpportunityStatusTypeList = statusTypes.split(',');
  }

  setSelectedDemandTypeList(demandTypes) {
    this.selectedDemandTypeList = demandTypes.split(',');
  }

  setSelectedIndustryPracticeAreaList(industryPracticeAreaCodes) {
    this.selectedIndustryPracticeAreaList = industryPracticeAreaCodes.split(',');
  }

  setSelectedCapabilityPracticeAreaList(capabilityPracticeAreaCodes) {
    this.selectedCapabilityPracticeAreaList = capabilityPracticeAreaCodes.split(',');
  }

  setDemandWeeksThreshold(demandWeeksThreshold) {
    if (ValidationService.isInteger(demandWeeksThreshold)) {
      this.userPreferences.demandWeeksThreshold = demandWeeksThreshold;
    }
  }

  getDemandsFilteredByMinProbabilityHandler(minDemandProbabilityPercent) {
    this.userPreferences.minOpportunityProbability = minDemandProbabilityPercent;
  }

  setCaseAllocationsSortedBySelectedValue(selectedOption) {
    this.userPreferences.caseAllocationsSortBy = this.selectedSortByItem = selectedOption.value;
  }
}
