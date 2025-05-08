import { Component, EventEmitter, OnInit, Output, QueryList, ViewChild, ViewChildren } from "@angular/core";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { DropdownLoaderHelperService } from "src/app/shared/helperServices/dropdown-loader-helper.service";
import { AffiliationRole } from "src/app/shared/interfaces/affiliationRole.interface";
import { LevelGrade } from "src/app/shared/interfaces/levelGrade.interface";
import { OfficeHierarchy } from "src/app/shared/interfaces/officeHierarchy.interface";
import { PracticeArea } from "src/app/shared/interfaces/practiceArea.interface";
import { ServiceLineHierarchy } from "src/app/shared/interfaces/serviceLineHierarchy";
import { LocalStorageService } from "src/app/shared/local-storage.service";
import { MultiSelectDropdownComponent } from "src/app/shared/multi-select-dropdown/multi-select-dropdown.component";
import { OfficeDropdownComponent } from "src/app/shared/office-dropdown/office-dropdown.component";

@Component({
  selector: "app-quick-filters",
  templateUrl: "./quick-filters.component.html",
  styleUrls: ["./quick-filters.component.scss"]
})
export class QuickFiltersComponent implements OnInit {
  selectedFiltersLength = 0;
  officeHierarchy: OfficeHierarchy;
  selectedOfficeCodeList = [];

  staffingTagsHierarchy: ServiceLineHierarchy[];
  staffingTagDropdownList;
  selectedStaffingTagCodeList = [];

  levelGrades: LevelGrade[];
  levelGradeDropdownList;
  selectedLevelGradeList = [];

  practiceAreas: PracticeArea[];
  practiceAreaDropDownList;
  selectedPracticeAreaCodeList = [];

  affiliationRoles: AffiliationRole[];
  affiliationRolesDropdownList;
  selectedAffiliationRoleCodeList=[];

  filtersEnum = {
    ALL: "all",
    OFFICE : "office",
    STAFFING_TAG : "staffingTag",
    LEVEL_GRADE : "levelGrade",
    AFFILIATION: "affiliation",
    AFFILIATION_ROLE: "affiliationRole"
  }

  collapsedFilterArray: string[] = Object.values(this.filtersEnum);
  //-------------------------------------------------------//
  @ViewChild(OfficeDropdownComponent) officeDropdownComponents: OfficeDropdownComponent;
  @ViewChildren(MultiSelectDropdownComponent) multiSelectDropdownComponents!: QueryList<MultiSelectDropdownComponent>;

  //------------------OUTPUT EVENTS--------------------------//
  @Output() applyQuickFilters = new EventEmitter();

  constructor(private localStorageService: LocalStorageService,
     private dropdownLoaderHelperService: DropdownLoaderHelperService) {}

  ngOnInit(): void {
    this.getLookupListFromLocalStorage();
    this.initializeDropdowns();
  }

  getLookupListFromLocalStorage(){
    this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.officeHierarchy);
    this.staffingTagsHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffingTagsHierarchy);
    this.levelGrades = this.localStorageService.get(ConstantsMaster.localStorageKeys.levelGradesHierarchy);
    this.practiceAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.practiceAreas);
    this.affiliationRoles = this.localStorageService.get(ConstantsMaster.localStorageKeys.affiliationRoles);
  }

  initializeDropdowns(){
    this.initializeOfficesFilter();
    this.initializeStaffingTagsFilter();
    this.initializeLevelGradesFilter();
    this.initializeAffiliationFilter();
    this.initializeAffiliationRoleFilter();
  }

  private initializeOfficesFilter() {
    if (this.officeHierarchy) {
        this.officeHierarchy = JSON.parse(JSON.stringify(this.officeHierarchy));
    }
    this.selectedOfficeCodeList = [];
  }

  private initializeStaffingTagsFilter() {
    if (this.staffingTagsHierarchy) {
        this.staffingTagDropdownList = this.dropdownLoaderHelperService
              .getFormattedHierarchyDropDownItems(this.staffingTagsHierarchy, "text", "value");   

        this.selectedStaffingTagCodeList = [];
    }
  }

  private initializeLevelGradesFilter() {
      if (this.levelGrades) {
        this.levelGradeDropdownList = this.dropdownLoaderHelperService
            .getFormattedHierarchyDropDownItems(this.levelGrades, "text", "value"); 

          this.selectedLevelGradeList = [];
      }
  }

  private initializeAffiliationFilter() {
    if (this.practiceAreas) {
      this.practiceAreaDropDownList = this.dropdownLoaderHelperService
          .getFormattedHierarchyDropDownItems(this.practiceAreas, "practiceAreaName", "practiceAreaCode"); 

        this.selectedPracticeAreaCodeList = [];
    }
  }

  private initializeAffiliationRoleFilter() {
    if (this.affiliationRoles) {
      this.affiliationRolesDropdownList = this.dropdownLoaderHelperService
          .getFormattedHierarchyDropDownItems(this.affiliationRoles, "roleName", "roleCode"); 

        this.selectedAffiliationRoleCodeList = [];
    }
  }

  // Toggle Filter menu and rows
  toggleFilter(filterEnumToToggle) {
    
    if(this.collapsedFilterArray.includes(filterEnumToToggle)){
      this.collapsedFilterArray.splice(this.collapsedFilterArray.indexOf(filterEnumToToggle), 1);
    }else{
      this.collapsedFilterArray.push(filterEnumToToggle);
    }

  }

  onSelectChange(selectedFilterValue, filterName) {
    switch (filterName) {
      case this.filtersEnum.OFFICE:{
        if (selectedFilterValue && this.isArrayEqual(this.selectedOfficeCodeList.map(String), selectedFilterValue.split(','))) {
          return false;
        }

        this.selectedOfficeCodeList = selectedFilterValue ? selectedFilterValue.split(',') : [];
        break;
      }
      case this.filtersEnum.STAFFING_TAG: {
        if (selectedFilterValue && this.isArrayEqual(this.selectedStaffingTagCodeList.map(String), selectedFilterValue.split(','))) {
          return false;
        }

        this.selectedStaffingTagCodeList = selectedFilterValue ? selectedFilterValue.split(',') : [];
        break;
      }
      case this.filtersEnum.LEVEL_GRADE:{
        if (selectedFilterValue && this.isArrayEqual(this.selectedLevelGradeList.map(String), selectedFilterValue.split(','))) {
          return false;
        }

        this.selectedLevelGradeList = selectedFilterValue ? selectedFilterValue.split(',') : [];
        break;
      }
      case this.filtersEnum.AFFILIATION: {
        if (selectedFilterValue && this.isArrayEqual(this.selectedPracticeAreaCodeList.map(String), selectedFilterValue.split(','))) {
          return false;
        }

        this.selectedPracticeAreaCodeList = selectedFilterValue ? selectedFilterValue.split(',') : [];
        this.showHideAffiliationRoleFilter();

        break;
      }
      case this.filtersEnum.AFFILIATION_ROLE: {
        if (selectedFilterValue && this.isArrayEqual(this.selectedAffiliationRoleCodeList.map(String), selectedFilterValue.split(','))) {
          return false;
        }

        this.selectedAffiliationRoleCodeList = selectedFilterValue ? selectedFilterValue.split(',') : [];
        break;
      }
    }

    this.updateSelectedFiltersLength();
  }


  showHideAffiliationRoleFilter(){
    if(!this.selectedPracticeAreaCodeList.length){
      this.selectedAffiliationRoleCodeList = [];
    }
  }

  // Updating selections and getting total # of selections
  updateSelectedFiltersLength() {
    this.selectedFiltersLength = this.selectedOfficeCodeList.length + this.selectedLevelGradeList.length
      + this.selectedStaffingTagCodeList.length + this.selectedPracticeAreaCodeList.length + this.selectedAffiliationRoleCodeList.length;

  }

  // Apply Filters
  applyFilters() {
    const officeCodes = this.selectedOfficeCodeList.toString();
    const staffingTags = this.selectedStaffingTagCodeList.toString();
    const levelGrades = this.selectedLevelGradeList.toString();
    const practiceAreaCodes = this.selectedPracticeAreaCodeList.toString();
    const affiliationRoleCodes = this.selectedAffiliationRoleCodeList.toString();

    this.applyQuickFilters.emit({ officeCodes, staffingTags, levelGrades, practiceAreaCodes, affiliationRoleCodes });

    this.closeAllFilters();
  }

  // Uncheck all selected filters
  clearAll() {

    this.officeDropdownComponents.deSelectAllItems();
    this.multiSelectDropdownComponents.forEach( 
      x => x.deSelectAllItems()
    );

    this.selectedOfficeCodeList = [];
    this.selectedStaffingTagCodeList = [];
    this.selectedLevelGradeList = [];
    this.selectedPracticeAreaCodeList = [];
    this.selectedAffiliationRoleCodeList = [];
    this.selectedFiltersLength = 0;

  }

  closeAllFilters(){
    this.collapsedFilterArray = Object.values(this.filtersEnum);
  }

  private isArrayEqual(array1, array2) {
    return JSON.stringify(array1) === JSON.stringify(array2);
  }
}
