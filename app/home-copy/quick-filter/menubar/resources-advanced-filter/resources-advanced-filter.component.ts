// -------------------Angular Operators---------------------------------------//
import {
    Component,
    OnInit,
    Input,
    SimpleChanges,
    Output,
    EventEmitter,
    OnChanges,
    ViewChild,
    ElementRef,
    OnDestroy
  } from "@angular/core";
  
  // -------------------Constants---------------------------------------//
  import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
  
  // -------------------Interfaces---------------------------------------//
  import { OfficeHierarchy } from "src/app/shared/interfaces/officeHierarchy.interface";
  import { ServiceLine } from "src/app/shared/constants/enumMaster";
  import { UserPreferences } from "src/app/shared/interfaces/userPreferences.interface";
  
  // -------------------Services-------------------------------------------//
  import { CoreService } from "src/app/core/core.service";
  import { LevelGrade } from "src/app/shared/interfaces/levelGrade.interface";
  import { ServiceLineHierarchy } from "src/app/shared/interfaces/serviceLineHierarchy";
  
  // ---------------------External Libraries ---------------------------------//
  import { PracticeArea } from "src/app/shared/interfaces/practiceArea.interface";
  import { PositionHierarchy } from "src/app/shared/interfaces/positionHierarchy.interface";
  
  @Component({
    selector: "resources-advanced-filter",
    templateUrl: "./resources-advanced-filter.component.html",
    styleUrls: ["./resources-advanced-filter.component.scss"]
  })
  export class ResourcesAdvancedFilterComponent implements OnInit, OnChanges, OnDestroy {
    // -----------------------Input Events--------------------------------------------//
    @Input() userPreferences: UserPreferences;
    @Input() officeHierarchy: OfficeHierarchy;
    @Input() staffingTagsHierarchy: ServiceLineHierarchy[];
    @Input() levelGrades: LevelGrade[];
    @Input() practiceAreas: PracticeArea[];
    @Input() positionsHierarchy: PositionHierarchy[];
    @Input() availabilityIncludes: any;
    @Input() groupsBy: any;
    @Input() sortsBy: any;
    @Input() affiliationRoles: any;
  
    // -----------------------Output Events--------------------------------------------//
    @Output() getResourcesOnAdvancedFilterChange = new EventEmitter<any>();
    // -----------------------Templare Reference Variables--------------------------------------------//
    @ViewChild("employeeSearchInput", { static: true }) employeeSearchInput: ElementRef;
  
    // -----------------------Local Variables--------------------------------------------//
  
    staffingTagDropdownList;
    positionDropdownList;
    levelGradeDropdownList;
    availabilityIncludesDropdownList;
    sortByDropdownList;
    groupByDropdownList;
    practiceAreaDropDownList;
    showFilters = false;
    selectedOfficeList = [];
    selectedStaffingTagList = [];
    selectedPositionList = [];
    selectedLevelGradeList = [];
    selectedAvailabilityIncludesList = [];
    selectedGroupByList = [];
    selectedSortByList = [];
    selectedPracticeAreaCodes = [];
    selectedAffiliationRoles = [];
    affiliationRoleDropdownList;
    isAffiliationRoleShown:boolean = false;
    //userPreferences: UserPreferences;
    serviceLineEnum: typeof ServiceLine = ServiceLine;
  
    // --------------------------Component Constructor----------------------------//
    constructor(private coreService: CoreService) {}
  
    // --------------------------Component LifeCycle Events----------------------------//
    ngOnInit() {
   
    }
  
    ngOnChanges(changes: SimpleChanges) {
      // use changes object here so as to fire the code only when the changes to that particular object is made
  
      if (changes.levelGrades) {
        this.setlevelGradesDropDown();
      }

      if (changes.userPreferences && this.userPreferences) {
        this.resetFiltersToDefault();
      }
  
    }
  
    // -----------------------Component Event Handlers----------------------------//
  
    resetFiltersToDefault() {
        this.setOffices(this.userPreferences.supplyViewOfficeCodes);
        this.setlevelGradesDropDown();
        this.setStaffingTagsDropDown();
        this.setPositionsDropDown();
        this.setAvailabilityIncludesDropDown();
        this.setSortByDropDown();
        this.setGroupByDropDown();
        this.setPracticeAreaDropDown();
        this.setAffiliationRoleDropDown();
        this.showOrHideAffiliationRoleFilter();
  
        if (this.userPreferences && this.userPreferences.supplyViewStaffingTags) {
          this.selectedStaffingTagList = this.userPreferences.supplyViewStaffingTags.split(",");
        } 
    }
  
    toggleFiltersSection() {
      this.showFilters = !this.showFilters;
    }
  
    getResourcesBySelectedPracticeAreaCodes(practiceAreaCodes) {
        if (this.isArrayEqual(this.selectedPracticeAreaCodes.map(String), practiceAreaCodes.split(','))) {
          return false;
        }
        this.selectedPracticeAreaCodes = !practiceAreaCodes?[]:practiceAreaCodes?.split(',');
        if(!practiceAreaCodes){
          this.selectedAffiliationRoles = [];
        }
        this.showOrHideAffiliationRoleFilter();
        this.getFilteredResources();
      }

    getResourcesBySelectedAffilitionRoles(affiliationRole){
        if (this.isArrayEqual(this.selectedAffiliationRoles.map(String), affiliationRole.split(','))) {
          return false;
        }
        this.selectedAffiliationRoles = affiliationRole.split(',');
        this.getFilteredResources();
      }
    
  
    getResourcesBySelectedOffices(officeCodes) {
      /**
       * Hack: Dropdown tree view library emit its event on bootstrap which invoke this function with null office codes
       * However, when this component created, selected office list gets populated in OnChange event first and
       * then this function gets invoked with null office codes via dropdown library.
       * Using badCounter variable to initialize selectedoffice list to empty array if user un-select all office
       */
      /**/
      // if (!this.isArrayEqual(this.selectedOfficeList, officeCodes.split(','))) {
      //   this.selectedOfficeList = officeCodes
      //     ? officeCodes.split(',')
      //     : (this.badCounter > 0 ? [] : this.selectedOfficeList);
      //   this.badCounter++;
      //   this.getFilteredResources();
      // }
  
      if (officeCodes && this.isArrayEqual(this.selectedOfficeList.map(String), officeCodes.split(","))) {
        return false;
      }
  
      this.selectedOfficeList = officeCodes.split(",");
  
      this.getFilteredResources();
    }
  
    getResourcesBySelectedStaffingTags(staffingTagCodes) {
      if (this.isArrayEqual(this.selectedStaffingTagList.map(String), staffingTagCodes.split(","))) {
        return false;
      }
  
      this.selectedStaffingTagList = staffingTagCodes.split(",");

      this.getFilteredResources();
    }
  
    getResourcesBySelectedPositions(positionCodes) {
      if (this.isArrayEqual(this.selectedPositionList.map(String), positionCodes.split(","))) {
        return false;
      }
  
      this.selectedPositionList = positionCodes.split(",");
  
      this.getFilteredResources();
    }
  
    getResourcesBySelectedLevelGrades(levelGrades) {
      if (this.isArrayEqual(this.selectedLevelGradeList.map(String), levelGrades.split(","))) {
        return false;
      }
  
      this.selectedLevelGradeList = levelGrades.split(",");
  
      this.getFilteredResources();
    }
  
    getResourcesBySelectedAvailabilityIncludes(availabilityIncludes) {
      if (this.isArrayEqual(this.selectedAvailabilityIncludesList.map(String), availabilityIncludes.split(","))) {
        return false;
      }
  
      this.selectedAvailabilityIncludesList = availabilityIncludes.split(",");
  
      this.getFilteredResources();
    }
  
    getResourcesGroupBySelectedValue(groupByList) {
      if (this.isArrayEqual(this.selectedGroupByList, groupByList.split(","))) {
        return false;
      }
      this.selectedGroupByList = groupByList.split(",");
      this.getFilteredResources();
    }
  
    getResourcesSortBySelectedValue(sortByList) {
      if (this.isArrayEqual(this.selectedSortByList, sortByList.split(","))) {
        return false;
      }
      this.selectedSortByList = sortByList.split(",");
      this.getFilteredResources();
    }
  
    getFilteredResources() {
      const officeCodes = this.selectedOfficeList.toString();
      const levelGrades = this.selectedLevelGradeList.toString();
      const staffingTags = this.selectedStaffingTagList.toString();
      const positionCodes = this.selectedPositionList.toString();
      const groupBy = this.selectedGroupByList.toString();
      const sortBy = this.selectedSortByList.toString();
      const availabilityIncludes = this.selectedAvailabilityIncludesList.toString();
      const selectedPracticeAreaCodes = this.selectedPracticeAreaCodes.toString();
      const selectedAffiliationRoles = this.selectedAffiliationRoles.toString();
    
      this.getResourcesOnAdvancedFilterChange.emit({
        officeCodes,
        levelGrades,
        staffingTags,
        groupBy,
        sortBy,
        availabilityIncludes,
        selectedPracticeAreaCodes,
        positionCodes,
        selectedAffiliationRoles
      });
    }
  

    // --------------------------helper Functions--------------------------//
  
  
    setOffices(officeCodes: string) {
      this.selectedOfficeList = officeCodes.split(",");
  
      // Re-assign object In order to reflect changes in Demand side office filter when changes are done in User Settings
      if (this.officeHierarchy) {
        this.officeHierarchy = JSON.parse(JSON.stringify(this.officeHierarchy));
      }
    }
  
    setPracticeAreaDropDown() {
      if (this.practiceAreas) {
        this.practiceAreaDropDownList = {
          text: "All",
          value: 0,
          checked: false,
          children: this.practiceAreas.map((item) => {
            return {
              text: item.practiceAreaName,
              value: item.practiceAreaCode,
              checked: false
            };
          })
        };
  
        if (this.userPreferences && this.userPreferences.practiceAreaCodes) {
          this.selectedPracticeAreaCodes = this.userPreferences.practiceAreaCodes.split(",");
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
  
        if (this.userPreferences && this.userPreferences.supplyViewStaffingTags) {
          this.selectedStaffingTagList = this.userPreferences.supplyViewStaffingTags.split(",");
        } 
        
      }
    }
  
    setPositionsDropDown() {
      if (this.positionsHierarchy && this.userPreferences) {
        this.positionDropdownList = {
          text: "All",
          value: 0,
          checked: false,
          children: this.positionsHierarchy.map((item) => {
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
  
        this.selectedPositionList = this.userPreferences.positionCodes
          ? this.userPreferences.positionCodes.split(",")
          : [];
      }
    }
  
    setlevelGradesDropDown() {
      if (this.levelGrades && this.userPreferences) {
        const levelGradeChildrenList = this.levelGrades.map((item) => {
          return {
            text: item.text,
            value: item.value,
            collapsed: true,
            children: item.children.map((child) => {
              return {
                text: child.text,
                value: child.value,
                checked: false
              };
            }),
            checked: false
          };
        });
  
        this.levelGradeDropdownList = {
          text: "All",
          value: 0,
          checked: false,
          children: levelGradeChildrenList
        };
        this.selectedLevelGradeList = this.userPreferences.levelGrades ? this.userPreferences.levelGrades.split(",") : [];
      }
    }
  
    setGroupByDropDown() {
      if (this.groupsBy && this.userPreferences) {
        this.groupByDropdownList = {
          text: "All",
          value: 0,
          checked: false,
          children: this.groupsBy.map((item) => {
            return {
              text: item.name,
              value: item.code,
              checked: false
            };
          })
        };
        this.selectedGroupByList = this.userPreferences.groupBy.split(",");
      }
    }
  
    setAvailabilityIncludesDropDown() {
      if (this.availabilityIncludes && this.userPreferences) {
        this.availabilityIncludesDropdownList = {
          text: "All",
          value: 0,
          checked: false,
          children: this.availabilityIncludes.map((item) => {
            return {
              text: item.name,
              value: item.code,
              checked: false
            };
          })
        };
        this.selectedAvailabilityIncludesList = this.userPreferences.availabilityIncludes
          ? this.userPreferences.availabilityIncludes.split(",")
          : [];
      }
    }
  
    setSortByDropDown() {
      if (this.groupsBy && this.userPreferences) {
        this.sortByDropdownList = {
          text: "All",
          value: 0,
          checked: false,
          children: this.sortsBy.map((item) => {
            return {
              text: item.name,
              value: item.code,
              checked: false
            };
          })
        };
        this.selectedSortByList = this.userPreferences.sortBy.split(",");
      }
    }

    setAffiliationRoleDropDown() {
        if (this.affiliationRoles) {
          this.affiliationRoleDropdownList = {
            text: 'All',
            value: 0,
            checked: false,
            children: this. affiliationRoles.map(item => {
              return {
                text: item.roleName,
                value: item.roleCode,
                checked: false
              }
            })
          };
    
          if (this.userPreferences && this.userPreferences.affiliationRoleCodes) {
            this.selectedAffiliationRoles = this.userPreferences.affiliationRoleCodes.split(',');
          }
        }
    }

    showOrHideAffiliationRoleFilter(){
        this.isAffiliationRoleShown= this.selectedPracticeAreaCodes!==undefined && this.selectedPracticeAreaCodes.length > 0  ? true: false;
    
    }

  
    private isArrayEqual(array1, array2) {
      
      return JSON.stringify(array1) === JSON.stringify(array2);
    }
  
    // ---------------------------Component Unload--------------------------------------------//
  
    ngOnDestroy() {
    }
  }