// -------------------Angular References---------------------------------------//
import { Component, OnInit, Input, Output, EventEmitter, OnChanges, SimpleChanges, ViewEncapsulation } from '@angular/core';
import { Observable } from 'rxjs';
import { mergeMap, debounceTime } from 'rxjs/operators';

// -------------------Interfaces---------------------------------------//
import { CaseType } from 'src/app/shared/interfaces/caseType.interface';
import { OfficeHierarchy } from 'src/app/shared/interfaces/officeHierarchy.interface';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { UserPreferences } from 'src/app/shared/interfaces/userPreferences.interface';

// -------------------Services-------------------------------------------//
import { CoreService } from 'src/app/core/core.service';
import { HomeService } from '../../home.service';
// import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';

// ---------------------External Libraries ---------------------------------//
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { CdkDragDrop } from '@angular/cdk/drag-drop';

@Component({
  selector: 'app-filter',
  templateUrl: './filter.component.html',
  styleUrls: ['./filter.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class FilterComponent implements OnInit, OnChanges {
  // -----------------------Local Variables--------------------------------------------//

  caseTypeDropdownList;
  demandTypeList;
  projectSearchInputFocus: Boolean;
  asyncProjectString = '';
  projects: Observable<Project>;
  caseTypeDropdownSettings = {};
  selectedCaseTypeList = [];
  selectedDemandTypeList = [];
  selectedOfficeList = [];
  selectedDateRange: any;
  datePickerRanges: any = [];
  userPreferences: UserPreferences;
  showAdvancedFilters = false;
  showPlanningCardsButton = true;
  bsConfig: Partial<BsDatepickerConfig>;

  // -----------------------Input Events--------------------------------------------//
  @Input() caseTypes: CaseType[];
  @Input() demandTypes: any[];
  @Input() officeHierarchy: OfficeHierarchy;

  // -----------------------Output Events--------------------------------------------//

  @Output() getProjectsOnBasicFilterChange = new EventEmitter<any>();
  @Output() searchProjectByNameOrCode = new EventEmitter<any>();
  @Output() openProjectDetailsDialogFromTypeahead = new EventEmitter();
  @Output() toggleAdvancedFilters = new EventEmitter();
  @Output() showMinOppProbabilityFilter = new EventEmitter();
  @Output() addProjectToUserExceptionShowListEmitter = new EventEmitter<any>();
  @Output() addCardPlaceholderEmitter = new EventEmitter();
  // --------------------------Constructor----------------------------//

  constructor(private homeService: HomeService, private coreService: CoreService) {
  }

  // --------------------------Component LifeCycle Events----------------------------//

  ngOnInit() {
    this.bsConfig = {
      containerClass: 'theme-red calendar-demand calendar-align-right',
      customTodayClass: 'custom-today-class',
      rangeInputFormat: 'DD-MMM-YYYY',
      isAnimated: true,
      showWeekNumbers: false,
      selectFromOtherMonth: true
    };

    // this.datePickerRanges = ConstantsMaster.datePickerRanges;

    this.projects = Observable.create((observer: any) => {
      // Runs on every search

      this.typeAheadOpenClose('block');

      observer.next(this.asyncProjectString);
    }).pipe(
      debounceTime(500),
      mergeMap((token: string) => this.homeService.getProjectsBySearchString(token))
    );

    this.subscribeUserPreferences();

  }

  ngOnChanges(changes: SimpleChanges) {

    if (changes.caseTypes) {
      this.setCaseTypes();
    }

    if (changes.demandTypes) {
      this.setDemandTypes();
    }

  }

  // -----------------------Component Event Handlers----------------------------//

  getProjectsBySelectedOffice(officeCodes) {
    if (officeCodes && this.isArrayEqual(this.selectedOfficeList.map(String), officeCodes.split(','))) {
      return false;
    }

    this.selectedOfficeList = officeCodes.split(',');

    this.getFilteredProjects();

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

    this.showHideMinOppProbabilityFilterHandler();
    this.showHideAddPlanningCardButton();
    this.getFilteredProjects();

  }

  showHideMinOppProbabilityFilterHandler() {
    if (this.selectedDemandTypeList.toString().indexOf('Opportunity') < 0) {
      this.showMinOppProbabilityFilter.emit(false);
    } else {
      this.showMinOppProbabilityFilter.emit(true);
    }
  }

  showHideAddPlanningCardButton() {
    if (this.selectedDemandTypeList.toString().indexOf('PlanningCards') < 0) {
      this.showPlanningCardsButton = false;
    } else {
      this.showPlanningCardsButton = true;
    }
  }

  getProjectsforSelectedDateRange(selectedDateRange) {
    // To avoid API call during initialization we check for non nullable start and end dates
    if (!selectedDateRange || this.selectedDateRange.toString() === selectedDateRange.toString()) {
      return;
    }

    this.selectedDateRange = selectedDateRange;

    this.getFilteredProjects();

  }

  shiftDateRange(shiftDate) {
    const caseTypeCodes = this.selectedCaseTypeList.toString();

    const officeCodes = this.selectedOfficeList.toString();

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

    this.getFilteredProjects();
  }

  typeaheadOnSelect(data) {
    this.clearSearchBox();
    this.openProjectDetailsDialogFromTypeahead.emit(data.item);
  }

  clearSearchBox() {
    this.asyncProjectString = '';
  }

  toggleAdvancedFiltersHandler() {
    this.showAdvancedFilters = !this.showAdvancedFilters;
    this.toggleAdvancedFilters.emit(this.showAdvancedFilters);
  }

  // -------------------Helper Functions -----------------------------//

  getFilteredProjects() {

    const officeCodes = this.selectedOfficeList.toString();
    const demandTypes = this.selectedDemandTypeList.toString();
    const dateRange = this.selectedDateRange;
    const caseTypeCodes = this.selectedCaseTypeList.toString();

    this.getProjectsOnBasicFilterChange.emit({ dateRange, officeCodes, caseTypeCodes, demandTypes });

  }

  setOffices(officeCodes: string) {
    this.selectedOfficeList = officeCodes.split(',');

    // Re-assign object In order to reflect changes in Demand side office filter when changes are done in User Settings
    if (this.officeHierarchy) {
      this.officeHierarchy = JSON.parse(JSON.stringify(this.officeHierarchy));
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
    if (this.demandTypes != null && this.userPreferences) {
      const demandTypeChildrenList = this.demandTypes.map(item => {
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

      this.selectedDemandTypeList = this.demandTypes.filter(statusType => this.userPreferences.demandTypes.indexOf(statusType.type.toString()) > -1)
        .map(x => x.type);
    }

  }

  subscribeUserPreferences() {
    this.coreService.getUserPreferences().subscribe(userPreferences => {

      if (userPreferences) {
        this.userPreferences = userPreferences;

        const startDate = new Date();
        const endDate = new Date();
        endDate.setDate(endDate.getDate() + userPreferences.demandWeeksThreshold * 7);

        this.selectedDateRange = [startDate, endDate];

        this.setOffices(userPreferences.demandViewOfficeCodes);
        this.setCaseTypes();
        this.setDemandTypes();
        this.showHideMinOppProbabilityFilterHandler();
        this.showHideAddPlanningCardButton();
      }

    });
  }

  private isArrayEqual(array1, array2) {
    return JSON.stringify(array1) === JSON.stringify(array2);
  }

  onCaseOppDragStarted(event) {
    document.getElementById('casedropModal').style.visibility = 'visible';
  }
  onCaseOppDrop(event: CdkDragDrop<any>) {
    const data = event.previousContainer.data;
    if (!Array.isArray(data)) {
      this.addProjectToUserExceptionShowListEmitter.emit({ pipelineId: data.pipelineId, oldCaseCode: data.oldCaseCode });
    }
    document.getElementById('casedropModal').style.visibility = 'hidden';
    this.typeAheadOpenClose('none');
    this.clearSearchBox();
  }
  onCaseOppDragReleased(event) {
    document.getElementById('casedropModal').style.visibility = 'hidden';
    const dropdownItems = document.getElementsByClassName('dropdown-item');
    for (let i = 0; i < dropdownItems.length; i++) {
      const dropdownItem = dropdownItems[i] as HTMLElement;
      dropdownItem.className = dropdownItem.className.replace(' caseOppDragCustomStyle', '');
    }
  }
  onCaseOppDragMoved($event, index) {
    const dropdownItems = document.getElementsByClassName('dropdown-item');
    for (let i = 0; i < dropdownItems.length; i++) {
      const dropdownItem = dropdownItems[i] as HTMLElement;
      if (!dropdownItem.classList.contains('caseOppDragCustomStyle')) {
        dropdownItem.className += ' caseOppDragCustomStyle';
      }
    }
  }
  onCaseOppMouseEnter() {
    const dropdownItems = document.getElementsByClassName('dropdown-item');
    for (let i = 0; i < dropdownItems.length; i++) {
      const dropdownItem = dropdownItems[i] as HTMLElement;
      if (dropdownItem.classList.contains('active')) {
        dropdownItem.style.display = 'none';
      }
    }
  }
  typeAheadOpenClose(action) {
    const typeheads = document.getElementsByTagName('typeahead-container');
    if (typeheads != undefined) {
      for (let i = 0; i < typeheads.length; i++) {
        const typehead = typeheads[i] as HTMLElement;
        typehead.style.display = action;
      }
    }
  }

  addPlanningCard() {
    this.addCardPlaceholderEmitter.emit();
  }
}
