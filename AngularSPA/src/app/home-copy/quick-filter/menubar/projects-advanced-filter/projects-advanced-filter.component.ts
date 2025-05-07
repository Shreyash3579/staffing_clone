import { Component, OnInit, Output, EventEmitter, Input, SimpleChanges } from "@angular/core";
import { OpportunityStatusType } from "src/app/shared/interfaces/opportunityStatusType";
import { UserPreferences } from "src/app/shared/interfaces/userPreferences.interface";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { ServiceLineHierarchy } from "src/app/shared/interfaces/serviceLineHierarchy";
import { ServiceLine } from "src/app/shared/constants/enumMaster";
import { PracticeArea } from "src/app/shared/interfaces/practiceArea.interface";
import { OfficeHierarchy } from "src/app/shared/interfaces/officeHierarchy.interface";
import { CaseType } from "src/app/shared/interfaces/caseType.interface";

@Component({
  selector: "projects-advanced-filter",
  templateUrl: "./projects-advanced-filter.component.html",
  styleUrls: ["./projects-advanced-filter.component.scss"]
})
export class ProjectsAdvancedFilterComponent implements OnInit {
  // inputs
  @Input() userPreferences: UserPreferences;
  @Input() staffingTagsHierarchy: ServiceLineHierarchy[];
  @Input() isMinOppProbabilityFilterShown: boolean = true;
  @Input() isStaffedFromSupply: boolean = false;
  @Input() officeHierarchy: OfficeHierarchy;
  @Input() opportunityStatusTypes: OpportunityStatusType[];
  @Input() industryPracticeAreas: PracticeArea[];
  @Input() capabilityPracticeAreas: PracticeArea[];
  @Input() caseTypes: CaseType[];
  @Input() demandTypes: any[];
  // outputs
  @Output() getProjectsOnAdvancedFilterChange = new EventEmitter();
  @Output() getAllocationsSortedBySelectedValueEmitter = new EventEmitter();
  @Output() expandCardsEmitter = new EventEmitter();

  // local variables
  isNewDemandChecked = false;
  opportunityStatusTypeDropdownList;
  industryPracticeAreaDropdownList;
  capabilityPracticeAreaDropdownList;
  selectedOpportunityStatusTypeList = [];
  selectedIndustryPracticeAreaList = [];
  selectedCapabilityPracticeAreaList = [];
  selectedOfficeList = [];
  minDemandProbabilityPercent: number;
  staffingTagDropdownList;
  selectedStaffingTagList = [];
  serviceLineEnum: typeof ServiceLine = ServiceLine;
  caseAllocationsSortByList = ConstantsMaster.CaseAllocationsSortByOptions;
  selectedSortByItem: any;
  expandAllCards: boolean = false;
  caseTypeDropdownList;
  demandTypeList;
  selectedCaseTypeList = [];
  selectedDemandTypeList = [];
  showPlanningCardsButton = true;

  constructor() {}

  ngOnInit() {
    
  }

  ngOnChanges(changes: SimpleChanges) {
    // use changes object here so as to fire the code only when the changes to that particular object is made

    if (changes.userPreferences && this.userPreferences) {
      this.resetFiltersToDefault();
    }

  }

  //---------------------------Event Handlers -----------------------------------------//

  toggleStaffFromSupply() {
    this.isStaffedFromSupply = !this.isStaffedFromSupply;
    this.getFilteredProjects();
  }

  toggleExpandAllCards() {
    this.expandAllCards = !this.expandAllCards;
    this.expandCardsEmitter.emit(this.expandAllCards);
  }

  getProjectsBySelectedOffice(officeCodes) {
    if (officeCodes && this.isArrayEqual(this.selectedOfficeList.map(String), officeCodes.split(","))) {
      return false;
    }

    this.selectedOfficeList = officeCodes.split(",");

    this.getFilteredProjects();
  }

  getProjectsBySelectedOppStatusTypes(statusTypeCodes: string) {
    if (
      statusTypeCodes &&
      this.isArrayEqual(this.selectedOpportunityStatusTypeList.map(String), statusTypeCodes.split(","))
    ) {
      return false;
    }

    this.selectedOpportunityStatusTypeList = statusTypeCodes.split(",");

    this.getFilteredProjects();
  }

  getProjectsBySelectedStaffingTags(staffingTagCodes: any) {
    if (this.isArrayEqual(this.selectedStaffingTagList.map(String), staffingTagCodes.split(","))) {
      return false;
    }

    this.selectedStaffingTagList = staffingTagCodes.split(",");

    this.getFilteredProjects();
  }

  getDemandsByMinProbability(minDemandProbabilityPercent) {
    this.minDemandProbabilityPercent = minDemandProbabilityPercent;

    this.getFilteredProjects();
  }

  getProjectsBySelectedIndustryPracticeAreas(industryPracticeAreaCodes: any) {
    if (this.isArrayEqual(this.selectedIndustryPracticeAreaList.map(String), industryPracticeAreaCodes.split(","))) {
      return false;
    }

    this.selectedIndustryPracticeAreaList = industryPracticeAreaCodes.split(",");

    this.getFilteredProjects();
  }

  getProjectsBySelectedCapabilityPracticeAreas(capabilityPracticeAreaCodes: any) {
    if (
      this.isArrayEqual(this.selectedCapabilityPracticeAreaList.map(String), capabilityPracticeAreaCodes.split(","))
    ) {
      return false;
    }

    this.selectedCapabilityPracticeAreaList = capabilityPracticeAreaCodes.split(",");

    this.getFilteredProjects();
  }

  //-------------------------Helper Functions ------------------------------------------//

  getFilteredProjects() {
    const opportunityStatusTypeCodes = this.selectedOpportunityStatusTypeList.toString();
    const caseAttributeNames = this.selectedStaffingTagList.toString();
    const minDemandProbabilityPercent = this.minDemandProbabilityPercent;
    const selectedSortByItem = this.selectedSortByItem;
    const selectedIndustryPracticeAreaCodes = this.selectedIndustryPracticeAreaList.toString();
    const selectedCapabilityPracticeAreaCodes = this.selectedCapabilityPracticeAreaList.toString();
    const isStaffedFromSupply = this.isStaffedFromSupply;
    const officeCodes = this.selectedOfficeList.toString();
    const demandTypes = this.selectedDemandTypeList.toString();
    const caseTypeCodes = this.selectedCaseTypeList.toString();


    this.getProjectsOnAdvancedFilterChange.emit({
      opportunityStatusTypeCodes,
      isStaffedFromSupply,
      caseAttributeNames,
      minDemandProbabilityPercent,
      selectedSortByItem,
      selectedIndustryPracticeAreaCodes,
      selectedCapabilityPracticeAreaCodes,
      officeCodes,
      demandTypes,
      caseTypeCodes
    });
  }

  setOpportunityStatusTypes() {
    if (this.opportunityStatusTypes) {
      const statusTypeChildrenList = this.opportunityStatusTypes.map((item) => {
        return {
          text: item.statusText,
          value: item.statusCode,
          checked: false,
          children: []
        };
      });

      this.opportunityStatusTypeDropdownList = {
        text: "All",
        value: 0,
        children: statusTypeChildrenList
      };

      this.selectedOpportunityStatusTypeList = this.opportunityStatusTypes
        .filter(
          (statusType) => this.userPreferences.opportunityStatusTypeCodes.indexOf(statusType.statusCode.toString()) > -1
        )
        .map((type) => type.statusCode);
    }
  }

  resetFiltersToDefault() {

    this.setOpportunityStatusTypes();
    this.minDemandProbabilityPercent = this.userPreferences.minOpportunityProbability;
    this.setStaffingTagsDropDown();
    this.setIndustryPracticeAreaDropdown();
    this.setCapabilityPracticeAreaDropDown();
    this.setOffices(this.userPreferences.demandViewOfficeCodes);
    this.selectedSortByItem = this.userPreferences.caseAllocationsSortBy;
    this.setCaseTypes();
    this.setDemandTypes();
  }

  setOffices(officeCodes: string) {
    this.selectedOfficeList = officeCodes.split(",");

    // Re-assign object In order to reflect changes in Demand side office filter when changes are done in User Settings
    if (this.officeHierarchy) {
      this.officeHierarchy = JSON.parse(JSON.stringify(this.officeHierarchy));
    }
  }

  setIndustryPracticeAreaDropdown() {
    if (this.industryPracticeAreas != null && this.userPreferences) {
      const industryPracticeAreaChildrenList = this.industryPracticeAreas.map((item) => {
        return {
          text: item.practiceAreaName,
          value: item.practiceAreaCode,
          checked: false,
          children: []
        };
      });

      this.industryPracticeAreaDropdownList = {
        text: "All",
        value: 0,
        children: industryPracticeAreaChildrenList
      };

      if (this.userPreferences && this.userPreferences.industryPracticeAreaCodes) {
        this.selectedIndustryPracticeAreaList = this.userPreferences.industryPracticeAreaCodes.split(",");
      } else {
        this.selectedIndustryPracticeAreaList = [];
      }
    }
  }

  setCapabilityPracticeAreaDropDown() {
    if (this.capabilityPracticeAreas != null && this.userPreferences) {
      const capabilityPracticeAreaChildrenList = this.capabilityPracticeAreas.map((item) => {
        return {
          text: item.practiceAreaName,
          value: item.practiceAreaCode,
          checked: false,
          children: []
        };
      });

      this.capabilityPracticeAreaDropdownList = {
        text: "All",
        value: 0,
        children: capabilityPracticeAreaChildrenList
      };

      if (this.userPreferences && this.userPreferences.capabilityPracticeAreaCodes) {
        this.selectedCapabilityPracticeAreaList = this.userPreferences.capabilityPracticeAreaCodes.split(",");
      } else {
        this.selectedCapabilityPracticeAreaList = [];
      }
    }
  }

  setStaffingTagsDropDown() {
    if (this.staffingTagsHierarchy) {
      this.staffingTagDropdownList = {
        text: "All",
        value: 0,
        checked: false,
        children: this.staffingTagsHierarchy.map((item) => {
          return {
            text: item.text,
            value: item.value,
            collapsed: true,
            children:
              item.children != null
                ? item.children.map((child) => {
                    return {
                      text: child.text,
                      value: child.value,
                      checked: false
                    };
                  })
                : null,
            checked: false
          };
        })
      };

      if (this.userPreferences && this.userPreferences.caseAttributeNames) {
        this.selectedStaffingTagList = this.userPreferences.caseAttributeNames.split(",");
      }
      // else {
      //   this.selectedStaffingTagList = [this.serviceLineEnum.GeneralConsulting];
      // }
    }
  }

  setCaseTypes() {

    if (this.caseTypes != null && this.userPreferences) {
      const caseTypeChildrenList = this.caseTypes.map(item => {
        return {
          text: item.caseTypeName,
          value: item.caseTypeCode,
          checked: false,
          children: []
        };
      });

      this.caseTypeDropdownList = {
        text: 'All',
        value: 0,
        children: caseTypeChildrenList
      };

      this.selectedCaseTypeList = this.caseTypes.filter(caseType => this.userPreferences.caseTypeCodes.indexOf(caseType.caseTypeCode.toString()) > -1)
        .map(caseType => caseType.caseTypeCode);
    }

  }

  setDemandTypes() {

    // dropdown list to show : opp, pc, casesnew 
    if (this.demandTypes != null && this.userPreferences) {
      const allowedTypes = ['Opportunity', 'NewDemand', 'PlanningCards','CasesStaffedBySupply'];
      const demandTypeChildrenList = this.demandTypes
        .filter(item=>allowedTypes.includes(item.type))
        .map(item => {
          return {
            text: item.name,
            value: item.type,
            checked: false,
            children: []
          };
        });

      this.demandTypeList = {
        text: 'All',
        value: 0,
        children: demandTypeChildrenList
      };

      this.selectedDemandTypeList = this.demandTypes.filter(statusType => allowedTypes.includes(statusType.type) && this.userPreferences.demandTypes.indexOf(statusType.type.toString()) > -1)
        .map(x => x.type);
    }

  }

  getProjectsBySelectedCaseTypes(caseTypeCodes: any) {

    if (caseTypeCodes && this.isArrayEqual(this.selectedCaseTypeList.map(String), caseTypeCodes.split(','))) {
      return false;
    }

    this.selectedCaseTypeList = caseTypeCodes.split(',');

    this.getFilteredProjects();
  }

  getProjectsByDemandTypes(typeNames) {
    if (typeNames && this.isArrayEqual(this.selectedDemandTypeList.map(String), typeNames.split(','))) {
      return false;
    }

    this.selectedDemandTypeList = typeNames.split(',');

    //this.showHideMinOppProbabilityFilterHandler();
    this.showHideAddPlanningCardButton();
    this.getFilteredProjects();

  }

  // showHideMinOppProbabilityFilterHandler() {
  //   if (this.selectedDemandTypeList.toString().indexOf('Opportunity') < 0) {
  //     this.showMinOppProbabilityFilter.emit(false);
  //   } else {
  //     this.showMinOppProbabilityFilter.emit(true);
  //   }
  // }

  showHideAddPlanningCardButton() {
    if (this.selectedDemandTypeList.toString().indexOf('PlanningCards') < 0) {
      this.showPlanningCardsButton = false;
    } else {
      this.showPlanningCardsButton = true;
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
