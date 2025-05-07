import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { OpportunityStatusType } from 'src/app/shared/interfaces/opportunityStatusType';
import { CoreService } from 'src/app/core/core.service';
import { UserPreferences } from 'src/app/shared/interfaces/userPreferences.interface';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { ServiceLineHierarchy } from 'src/app/shared/interfaces/serviceLineHierarchy';
import { ServiceLine } from 'src/app/shared/constants/enumMaster';
import { PracticeArea } from 'src/app/shared/interfaces/practiceArea.interface';

@Component({
  selector: 'app-advanced-filters',
  templateUrl: './advanced-filters.component.html',
  styleUrls: ['./advanced-filters.component.scss']
})
export class AdvancedFiltersComponent implements OnInit {
  // -----------------------Local Variables--------------------------------------------//
  isNewDemandChecked = false;
  userPreferences: UserPreferences;
  opportunityStatusTypeDropdownList;
  industryPracticeAreaDropdownList;
  capabilityPracticeAreaDropdownList;
  selectedOpportunityStatusTypeList = [];
  selectedIndustryPracticeAreaList = [];
  selectedCapabilityPracticeAreaList = [];
  private opportunityStatusTypes: OpportunityStatusType[];
  private industryPracticeAreas: PracticeArea[];
  private capabilityPracticeAreas: PracticeArea[];
  minOpportunityProbability: number;
  staffingTagDropdownList;
  selectedStaffingTagList = [];
  serviceLineEnum: typeof ServiceLine = ServiceLine;
  caseAllocationsSortByList = ConstantsMaster.CaseAllocationsSortByOptions;
  selectedSortByItem = '';

  // -----------------------Input Events--------------------------------------------//
  @Input() staffingTagsHierarchy: ServiceLineHierarchy[];
  @Input() isMinOppProbabilityFilterShown: boolean = true;
  @Input() isStaffedFromSupply : boolean = false;

  // -----------------------Output Events--------------------------------------------//
  @Output() getProjectsOnAdvancedFilterChange = new EventEmitter();
  @Output() getAllocationsSortedBySelectedValueEmitter = new EventEmitter();

  constructor(private coreService: CoreService, private localStorageService: LocalStorageService) { }

  ngOnInit() {
    this.opportunityStatusTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.opportunityStatusTypes);
    this.industryPracticeAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.industryPracticeAreas);
    this.capabilityPracticeAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.capabilityPracticeAreas);
    this.subscribeUserPreferences();
  }

  //---------------------------Event Handlers -----------------------------------------//

  toggleStaffFromSupply() {
   this.isStaffedFromSupply = !this.isStaffedFromSupply;
   this.getFilteredProjects();
  }

  getProjectsBySelectedOppStatusTypes(statusTypeCodes: string) {
    if (statusTypeCodes && this.isArrayEqual(this.selectedOpportunityStatusTypeList.map(String), statusTypeCodes.split(','))) {
      return false;
    }

    this.selectedOpportunityStatusTypeList = statusTypeCodes.split(',');

    this.getFilteredProjects();
  }

  getProjectsBySelectedStaffingTags(staffingTagCodes: any) {
    if (this.isArrayEqual(this.selectedStaffingTagList.map(String), staffingTagCodes.split(','))) {
      return false;
    }

    this.selectedStaffingTagList = staffingTagCodes.split(',');

    this.getFilteredProjects();
  }

  getOpportunitiesByMinProbability(minOpportunityProbability) {
    this.minOpportunityProbability = minOpportunityProbability;

    this.getFilteredProjects();
  }

  getProjectsBySelectedIndustryPracticeAreas(industryPracticeAreaCodes: any) {
    if (this.isArrayEqual(this.selectedIndustryPracticeAreaList.map(String), industryPracticeAreaCodes.split(','))) {
      return false;
    }

    this.selectedIndustryPracticeAreaList = industryPracticeAreaCodes.split(',');

    this.getFilteredProjects();
  }

  getProjectsBySelectedCapabilityPracticeAreas(capabilityPracticeAreaCodes: any) {
    if (this.isArrayEqual(this.selectedCapabilityPracticeAreaList.map(String), capabilityPracticeAreaCodes.split(','))) {
      return false;
    }

    this.selectedCapabilityPracticeAreaList = capabilityPracticeAreaCodes.split(',');

    this.getFilteredProjects();
  }

  //-------------------------Helper Functions ------------------------------------------//

  getFilteredProjects() {

    const opportunityStatusTypeCodes = this.selectedOpportunityStatusTypeList.toString();
    const caseAttributeNames = this.selectedStaffingTagList.toString();
    const minOpportunityProbability = this.minOpportunityProbability;
    const selectedSortByItem = this.selectedSortByItem;
    const selectedIndustryPracticeAreaCodes = this.selectedIndustryPracticeAreaList.toString();
    const selectedCapabilityPracticeAreaCodes = this.selectedCapabilityPracticeAreaList.toString();
    const isStaffedFromSupply = this.isStaffedFromSupply;

    this.getProjectsOnAdvancedFilterChange.emit({
      opportunityStatusTypeCodes,isStaffedFromSupply, caseAttributeNames, minOpportunityProbability, selectedSortByItem, selectedIndustryPracticeAreaCodes, selectedCapabilityPracticeAreaCodes
    });
  }

  setOpportunityStatusTypes() {
    if (this.opportunityStatusTypes != null && this.userPreferences) {
      const statusTypeChildrenList = this.opportunityStatusTypes.map(item => {
        return {
          text: item.statusText,
          value: item.statusCode,
          checked: false,
          children: []
        };
      });

      this.opportunityStatusTypeDropdownList = {
        text: 'All',
        value: 0,
        children: statusTypeChildrenList
      };

      this.selectedOpportunityStatusTypeList = this.opportunityStatusTypes.filter(
        statusType => this.userPreferences.opportunityStatusTypeCodes.indexOf(statusType.statusCode.toString()) > -1)
        .map(type => type.statusCode);
    }

  }

  subscribeUserPreferences() {
    this.coreService.getUserPreferences().subscribe(userPreferences => {

      if (userPreferences) {
        this.userPreferences = userPreferences;

        this.setOpportunityStatusTypes();
        this.minOpportunityProbability = this.userPreferences.minOpportunityProbability;
        this.setStaffingTagsDropDown();
        this.setIndustryPracticeAreaDropdown();
        this.setCapabilityPracticeAreaDropDown();
        this.selectedSortByItem = userPreferences.caseAllocationsSortBy;
      }

    });
  }

  setIndustryPracticeAreaDropdown() {
    if (this.industryPracticeAreas != null && this.userPreferences) {
      const industryPracticeAreaChildrenList = this.industryPracticeAreas.map(item => {
        return {
          text: item.practiceAreaName,
          value: item.practiceAreaCode,
          checked: false,
          children: []
        };
      });

      this.industryPracticeAreaDropdownList = {
        text: 'All',
        value: 0,
        children: industryPracticeAreaChildrenList
      };

      if (this.userPreferences && this.userPreferences.industryPracticeAreaCodes) {
        this.selectedIndustryPracticeAreaList = this.userPreferences.industryPracticeAreaCodes.split(',');
      } else {
        this.selectedIndustryPracticeAreaList = [];
      }
    }
  }

  setCapabilityPracticeAreaDropDown() {
    if (this.capabilityPracticeAreas != null && this.userPreferences) {
      const capabilityPracticeAreaChildrenList = this.capabilityPracticeAreas.map(item => {
        return {
          text: item.practiceAreaName,
          value: item.practiceAreaCode,
          checked: false,
          children: []
        };
      });

      this.capabilityPracticeAreaDropdownList = {
        text: 'All',
        value: 0,
        children: capabilityPracticeAreaChildrenList
      };

      if (this.userPreferences && this.userPreferences.capabilityPracticeAreaCodes) {
        this.selectedCapabilityPracticeAreaList = this.userPreferences.capabilityPracticeAreaCodes.split(',');
      }else {
        this.selectedCapabilityPracticeAreaList = [];
      }
    }
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

      if (this.userPreferences && this.userPreferences.caseAttributeNames) {
        this.selectedStaffingTagList = this.userPreferences.caseAttributeNames.split(',');
      }
      // else {
      //   this.selectedStaffingTagList = [this.serviceLineEnum.GeneralConsulting];
      // }
    }
  }

  getAllocationsSortedBySelectedValue(selectedOption) {
    this.selectedSortByItem = selectedOption.value;
    this.getAllocationsSortedBySelectedValueEmitter.emit(selectedOption.value);
  }

  private isArrayEqual(array1, array2) {
    // if (array2[0] === '') {
    //   array2 = [];
    // }
    return JSON.stringify(array1) === JSON.stringify(array2);
  }

}
