// -------------------Angular Operators---------------------------------------//
import { Component, OnInit, Input, SimpleChanges, Output, EventEmitter, OnChanges, ViewChild, ElementRef, OnDestroy, ɵbypassSanitizationTrustStyle } from '@angular/core';
import { Subject, fromEvent } from 'rxjs';

// -------------------Constants---------------------------------------//
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';

// -------------------Interfaces---------------------------------------//
import { OfficeHierarchy } from 'src/app/shared/interfaces/officeHierarchy.interface';
import { ServiceLine } from 'src/app/shared/constants/enumMaster';
import { UserPreferences } from 'src/app/shared/interfaces/userPreferences.interface';

// -------------------Services-------------------------------------------//
import { CoreService } from 'src/app/core/core.service';
import { map, debounceTime } from 'rxjs/operators';
import { LevelGrade } from 'src/app/shared/interfaces/levelGrade.interface';
import { ServiceLineHierarchy } from 'src/app/shared/interfaces/serviceLineHierarchy';

// ---------------------External Libraries ---------------------------------//
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { PracticeArea } from 'src/app/shared/interfaces/practiceArea.interface';
import { PositionHierarchy } from 'src/app/shared/interfaces/positionHierarchy.interface';
import { AffiliationRole } from 'src/app/shared/interfaces/affiliationRole.interface';

@Component({
  selector: 'app-sidebar-filter',
  templateUrl: './sidebar-filter.component.html',
  styleUrls: ['./sidebar-filter.component.scss']
})
export class SidebarFilterComponent implements OnInit, OnChanges, OnDestroy {

  // -----------------------Input Events--------------------------------------------//
  @Input() clearSearch: Subject<boolean>;
  @Input() resourceLength: number;
  @Input() officeHierarchy: OfficeHierarchy;
  @Input() staffingTagsHierarchy: ServiceLineHierarchy[];
  @Input() levelGrades: LevelGrade[];
  @Input() practiceAreas: PracticeArea[];
  @Input() affiliationRoles:AffiliationRole[];
  @Input() positionsHierarchy: PositionHierarchy[];

  // -----------------------Output Events--------------------------------------------//
  @Output() getResources = new EventEmitter<any>();
  @Output() getResourcesIncludingTerminatedBySearchString = new EventEmitter<any>();
  @Output() openQuickAddForm = new EventEmitter<any>();
  @Output() getResourcesSortAndGroupBySelectedValuesEmitter = new EventEmitter<any>();
  @Output() getResourcesAvailabilityBySelectedValuesEmitter = new EventEmitter<any>();
  // -----------------------Templare Reference Variables--------------------------------------------//
  @ViewChild('employeeSearchInput', { static: true }) employeeSearchInput: ElementRef;

  // -----------------------Local Variables--------------------------------------------//

  staffingTagDropdownList;
  positionDropdownList;
  levelGradeDropdownList;
  availabilityIncludesDropdownList;
  sortByDropdownList;
  groupByDropdownList;
  practiceAreaDropDownList;
  affiliationRoleDropdownList;
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
  employeeSearchInputFocus: Boolean;
  selectedDateRange: any;
  datePickerRanges: any = [];
  availabilityIncludes = [];
  groupsBy = [];
  sortsBy = [];
  userPreferences: UserPreferences;
  serviceLineEnum: typeof ServiceLine = ServiceLine;
  bsConfig: Partial<BsDatepickerConfig>;
  isAffiliationRoleShown:boolean = false;

  // --------------------------Component Constructor----------------------------//
  constructor(private coreService: CoreService) {
  }

  // --------------------------Component LifeCycle Events----------------------------//
  ngOnInit() {
    this.initialiseDatePicker();
    this.loadMasterData();
    this.attachEventToSearchBox();
    this.subscribeUserPreferences();
    this.showOrHideAffiliationRoleFilter();
  }

  ngOnChanges(changes: SimpleChanges) {
    // use changes object here so as to fire the code only when the changes to that particular object is made

    if (changes.levelGrades) {
      this.setlevelGradesDropDown();
    }

  }
  
  // -----------------------Component Event Handlers----------------------------//

  attachEventToSearchBox() {

    this.clearSearch.subscribe(value => {
      this.clearSearchBox(value);
    });

    fromEvent(this.employeeSearchInput.nativeElement, 'keyup').pipe(
      map((event: any) => {
        return event.target.value;
      }),
      debounceTime(500)
      // ,distinctUntilChanged() //removing this as it was craeting testin to and fro resources
    ).subscribe((text: string) => {
      this.searchEmployee(text);
    });
  }

  subscribeUserPreferences() {
    this.coreService.getUserPreferences().subscribe(userPreferences => {
      if (userPreferences) {
        this.userPreferences = userPreferences;

        const startDate = new Date();
        const endDate = new Date();
        endDate.setDate(endDate.getDate() + userPreferences.supplyWeeksThreshold * 7);

        this.selectedDateRange = [startDate, endDate];

        this.setOffices(userPreferences.supplyViewOfficeCodes);
        this.setlevelGradesDropDown();
        this.setStaffingTagsDropDown();
        this.setPositionsDropDown();
        this.setAvailabilityIncludesDropDown();
        this.setSortByDropDown();
        this.setGroupByDropDown();
        this.setPracticeAreaDropDown();
        this.setAffiliationRoleDropDown();
        this.showOrHideAffiliationRoleFilter();
      }

      if (userPreferences && userPreferences.supplyViewStaffingTags) {
        this.selectedStaffingTagList = userPreferences.supplyViewStaffingTags.split(',');
      } else {
        this.selectedStaffingTagList = [this.serviceLineEnum.GeneralConsulting];
      }

    });

  }

  showOrHideAffiliationRoleFilter(){
    this.isAffiliationRoleShown= this.selectedPracticeAreaCodes!==undefined && this.selectedPracticeAreaCodes.length > 0  ? true: false;

}
  
  getResourcesforSelectedDateRange(selectedDateRange) {
    // To avoid API call during initialization we check for non nullable start and end dates
    if (!selectedDateRange || this.selectedDateRange.toString() === selectedDateRange.toString()) {
      return;
    }

    this.selectedDateRange = selectedDateRange;

    this.getFilteredResources();
  }

  shiftDateRange(shiftDate) {
    if (shiftDate === 'left') {
      const startDate = this.selectedDateRange[0];
      const endDate = this.selectedDateRange[1];

      startDate.setDate(startDate.getDate() - 7);
      endDate.setDate(endDate.getDate() - 7);

      this.selectedDateRange = [startDate, endDate];

    } else {

      const startDate = this.selectedDateRange[0];
      const endDate = this.selectedDateRange[1];

      startDate.setDate(startDate.getDate() + 7);
      endDate.setDate(endDate.getDate() + 7);

      this.selectedDateRange = [startDate, endDate];
    }

    this.getFilteredResources();
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

    if (officeCodes && this.isArrayEqual(this.selectedOfficeList.map(String), officeCodes.split(','))) {
      return false;
    }

    this.selectedOfficeList = officeCodes.split(',');

    this.getFilteredResources();

  }

  getResourcesBySelectedStaffingTags(staffingTagCodes) {
    if (this.isArrayEqual(this.selectedStaffingTagList.map(String), staffingTagCodes.split(','))) {
      return false;
    }

    this.selectedStaffingTagList = staffingTagCodes.split(',');

    this.getFilteredResources();
  }

  getResourcesBySelectedPositions(positionCodes) {
    if (this.isArrayEqual(this.selectedPositionList.map(String), positionCodes.split(','))) {
      return false;
    }

    this.selectedPositionList = positionCodes.split(',');

    this.getFilteredResources();
  }

  getResourcesBySelectedLevelGrades(levelGrades) {
    if (this.isArrayEqual(this.selectedLevelGradeList.map(String), levelGrades.split(','))) {
      return false;
    }

    this.selectedLevelGradeList = levelGrades.split(',');

    this.getFilteredResources();
  }

  getResourcesBySelectedAvailabilityIncludes(availabilityIncludes) {
    if (this.isArrayEqual(this.selectedAvailabilityIncludesList.map(String), availabilityIncludes.split(','))) {
      return false;
    }

    this.selectedAvailabilityIncludesList = availabilityIncludes.split(',');

    this.getFilteredResources();
    // const availabilityIncludesList = this.selectedAvailabilityIncludesList;
    // const groupByList = this.selectedGroupByList.toString();
    // const sortByList = this.selectedSortByList.toString();

    // this.getResourcesAvailabilityBySelectedValuesEmitter.emit({ availabilityIncludesList, groupByList, sortByList });
  }

  getResourcesGroupBySelectedValue(groupByList) {
    if (this.isArrayEqual(this.selectedGroupByList, groupByList.split(','))) {
      return false;
    }
    this.selectedGroupByList = groupByList.split(',');
    this.getResourcesSortAndGroupBySelectedValue();
  }

  getResourcesSortBySelectedValue(sortByList) {
    if (this.isArrayEqual(this.selectedSortByList, sortByList.split(','))) {
      return false;
    }
    this.selectedSortByList = sortByList.split(',');

    this.getResourcesSortAndGroupBySelectedValue();
  }

  getResourcesSortAndGroupBySelectedValue() {
    const groupByList = this.selectedGroupByList.toString();
    const sortByList = this.selectedSortByList.toString();

    this.getResourcesSortAndGroupBySelectedValuesEmitter.emit({ groupByList, sortByList });
  }

  getFilteredResources() {
    this.employeeSearchInput.nativeElement.value = '';
    const dateRange = this.selectedDateRange;
    const officeCodes = this.selectedOfficeList.toString();
    const levelGrades = this.selectedLevelGradeList.toString();
    const staffingTags = this.selectedStaffingTagList.toString();
    const positionCodes = this.selectedPositionList.toString();
    const groupBy = this.selectedGroupByList.toString();
    const sortBy = this.selectedSortByList.toString();
    const availabilityIncludes = this.selectedAvailabilityIncludesList.toString();
    const selectedPracticeAreaCodes = this.selectedPracticeAreaCodes.toString();
    const selectedAffiliationRoles = this.selectedAffiliationRoles.toString();
    this.getResources.emit({ dateRange, officeCodes, levelGrades, staffingTags, groupBy, sortBy, availabilityIncludes, selectedPracticeAreaCodes,selectedAffiliationRoles,positionCodes });
  }

  searchEmployee(searchText = '') {
    this.getResourcesIncludingTerminatedBySearchString.emit({ typeahead: searchText });
  }

  clearSearchBox(clearSearchOnly) {
    this.employeeSearchInput.nativeElement.value = '';
    this.searchEmployee();
  }



  // Emits event to home.component.ts for opening modal pop-up
  onQuickAddClick() {
    this.openQuickAddForm.emit();
  }

  // --------------------------helper Functions--------------------------//

  initialiseDatePicker() {

    this.bsConfig = {
      containerClass: 'theme-red calendar-supply calendar-align-left',
      customTodayClass: 'custom-today-class',
      rangeInputFormat: 'DD-MMM-YYYY',
      isAnimated: true,
      showWeekNumbers: false,
      selectFromOtherMonth: true
    };

  }

  loadMasterData() {

    // this.datePickerRanges = ConstantsMaster.datePickerRanges;
    this.availabilityIncludes = ConstantsMaster.availabilityIncludes;
    this.groupsBy = ConstantsMaster.groupBy;
    this.sortsBy = ConstantsMaster.sortBy;

  }

  setOffices(officeCodes: string) {
    this.selectedOfficeList = officeCodes.split(',');

    // Re-assign object In order to reflect changes in Demand side office filter when changes are done in User Settings
    if (this.officeHierarchy) {
      this.officeHierarchy = JSON.parse(JSON.stringify(this.officeHierarchy));
    }
  }

  setPracticeAreaDropDown() {
    if (this.practiceAreas) {
      this.practiceAreaDropDownList = {
        text: 'All',
        value: 0,
        checked: false,
        children: this.practiceAreas.map(item => {
          return {
            text: item.practiceAreaName,
            value: item.practiceAreaCode,
            checked: false
          }
        })
      };

      if (this.userPreferences && this.userPreferences.practiceAreaCodes) {
        this.selectedPracticeAreaCodes = this.userPreferences.practiceAreaCodes.split(',');
      }
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

      if (this.userPreferences && this.userPreferences.supplyViewStaffingTags) {
        this.selectedStaffingTagList = this.userPreferences.supplyViewStaffingTags.split(',');
      } else {
        this.selectedStaffingTagList = [this.serviceLineEnum.GeneralConsulting];
      }
    }
  }

  setPositionsDropDown() {
    if (this.positionsHierarchy && this.userPreferences) {
      this.positionDropdownList = {
        text: 'All',
        value: 0,
        checked: false,
        children: this.positionsHierarchy.map((item) => {
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

      this.selectedPositionList = this.userPreferences.positionCodes ? this.userPreferences.positionCodes.split(',') : [];
    }
  }

  setlevelGradesDropDown() {
    if (this.levelGrades && this.userPreferences) {
      const levelGradeChildrenList = this.levelGrades.map(item => {
        return {
          text: item.text,
          value: item.value,
          collapsed: true,
          children: item.children.map(child => {
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
        text: 'All',
        value: 0,
        checked: false,
        children: levelGradeChildrenList
      };
      this.selectedLevelGradeList = this.userPreferences.levelGrades ? this.userPreferences.levelGrades.split(',') : [];
    }
  }

  setGroupByDropDown() {

    if (this.groupsBy && this.userPreferences) {
      this.groupByDropdownList = {
        text: 'All',
        value: 0,
        checked: false,
        children: this.groupsBy.map(item => {
          return {
            text: item.name,
            value: item.code,
            checked: false
          };
        })
      };
      this.selectedGroupByList = this.userPreferences.groupBy.split(',');
    }
  }

  setAvailabilityIncludesDropDown() {
    if (this.availabilityIncludes && this.userPreferences) {
      this.availabilityIncludesDropdownList = {
        text: 'All',
        value: 0,
        checked: false,
        children: this.availabilityIncludes.map(item => {
          return {
            text: item.name,
            value: item.code,
            checked: false
          };
        })
      };
      this.selectedAvailabilityIncludesList = this.userPreferences.availabilityIncludes ? this.userPreferences.availabilityIncludes.split(',') : [];
    }
  }

  setSortByDropDown() {
    if (this.groupsBy && this.userPreferences) {
      this.sortByDropdownList = {
        text: 'All',
        value: 0,
        checked: false,
        children: this.sortsBy.map(item => {
          return {
            text: item.name,
            value: item.code,
            checked: false
          };
        })
      };
      this.selectedSortByList = this.userPreferences.sortBy.split(',');
    }
  }

  private isArrayEqual(array1, array2) {
    // if (array2[0] === '') {
    //   array2 = [];
    // }
    return JSON.stringify(array1) === JSON.stringify(array2);
  }

  // ---------------------------Component Unload--------------------------------------------//

  ngOnDestroy() {
    this.clearSearch.unsubscribe();
  }

}
