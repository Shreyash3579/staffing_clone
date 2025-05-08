// -------------------Angular References---------------------------------------//
import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, SimpleChanges } from '@angular/core';

// -------------------Interfaces---------------------------------------//
import { CaseType } from '../interfaces/caseType.interface';
import { KeyValue } from '../interfaces/keyValue.interface';
import { OfficeHierarchy } from '../interfaces/officeHierarchy.interface';
import { ServiceLineHierarchy } from '../interfaces/serviceLineHierarchy';
import { OpportunityStatusType } from '../interfaces/opportunityStatusType';
import { UserPreferences } from '../interfaces/userPreferences.interface';
import { PracticeArea } from '../interfaces/practiceArea.interface';
import { Client } from '../interfaces/client.interface';

// ---------------------External Libraries ---------------------------------//
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { BS_DEFAULT_CONFIG } from '../constants/bsDatePickerConfig';
import { catchError, debounceTime, distinctUntilChanged, filter, switchMap, tap } from 'rxjs/operators';
import { Observable, of, concat, Subject } from 'rxjs';
import { SharedService } from '../shared.service';

@Component({
  selector: 'shared-demand-filters',
  templateUrl: './demand-filters.component.html',
  styleUrls: ['./demand-filters.component.scss']
})
export class DemandFiltersComponent implements OnInit, OnDestroy {

  // ------------- Input events --------------------------- //
  @Input() userPreferences: UserPreferences;
  @Input() dateRange: any = [];
  @Input() officeHierarchy: OfficeHierarchy[] = [];
  @Input() caseTypes: CaseType[] = [];
  @Input() demandTypes: KeyValue[] = [];
  @Input() opportunityStatusTypes: OpportunityStatusType[] = [];
  @Input() staffingTagsHierarchy: ServiceLineHierarchy[] = []
  @Input() industryPracticeAreas: PracticeArea[];
  @Input() capabilityPracticeAreas: PracticeArea[];

  //-------------------- Output events ------------------------ //
  @Output() getProjectsOnFilterChange = new EventEmitter<any>();
  @Output() closeFilterEmitter = new EventEmitter();

  //-------------------- dropdown Variables ---------------------- //
  caseTypeDropdownList;
  demandTypeList;
  opportunityStatusTypeDropdownList;
  staffingTagDropdownList;
  minDemandProbabilityPercent = 0;
  industryPracticeAreaDropdownList;
  capabilityPracticeAreaDropdownList;

  selectedDateRange: any;
  selectedOfficeList = [];
  selectedCaseTypeList = [];
  selectedDemandTypeList = [];
  selectedOpportunityStatusTypeList = [];
  selectedStaffingTagList = [];
  selectedIndustryPracticeAreaList = [];
  selectedCapabilityPracticeAreaList = [];
  selectedClientCodes:string = null;

  bsConfig: Partial<BsDatepickerConfig>;

  //----------------------Local Variables ---------------------------- //
  isMinOppProbabilityFilterShown = false;
  // Multiselect
  clientsData$: Observable<Client[]>;
  public asyncClientString = '';
  clientInput$ = new Subject<string>();
  isClientSearchOpen = false;

  constructor(private sharedService: SharedService) { }

  ngOnInit(): void {
    this.initializeDateConfig();
    this.attachEventForClientsSearch();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.dateRange && this.dateRange) {
      this.selectedDateRange = this.dateRange;
    }

    if (changes.userPreferences && this.userPreferences) {
      this.updateFiltersOnUserPrefrencesChange();
    }
  }

  updateFiltersOnUserPrefrencesChange() {
    //date range is not re-set on user preferences change here as it will be done when dtaeRanges @Input updates
    this.setOffices();
    this.setCaseTypes();
    this.setDemandTypes();
    this.setOpportunityStatusTypes();
    this.setStaffingTags();
    this.setIndustryPracticeAreas();
    this.setCapabilityPracticeAreas();

    this.minDemandProbabilityPercent = this.userPreferences?.minOpportunityProbability;
  }

  // -------------------Event Handlers -----------------------------//

  attachEventForClientsSearch() {
    this.clientsData$ = concat(
      of([]), // default items
      this.clientInput$.pipe(
        debounceTime(500),
        distinctUntilChanged(),
        filter(x =>  x.length > 2 ),
        switchMap(term => this.sharedService.getClientsBySearchString(term).pipe(
          catchError(() => of([])), // empty list on error
        )),
        tap(() => {
          this.isClientSearchOpen = true;
        })
      )
    );

  }

  /// Multiselect TODO: use type-ahead instead of custom search function once we replace bootstrap typeahead with ng-select
  // we can't do it right now as [typeahead] directive creates conflict in bootstrap and ng-select
  onClientSearchChange($event) {
    if ($event.term.length > 2) {
      this.clientInput$.next($event.term);
    }

    // to reset search term if keyword's length is less than 3
    // if ($event.term.length < 3) {
    //   this.clientInput$.next(null);
    // }

    // TODO: below condition should be removed once permanent solution is applied
    if ($event.term.length < 1) {
      this.isClientSearchOpen = false;
    }
  }

  selectedClientsChange(selectedClients) {
    this.isClientSearchOpen = false;

    this.selectedClientCodes = selectedClients.map( x => x.clientCode).join(",");
    this.getFilteredProjects();
   
  }
  
  getFilteredProjects() {
    const dateRange = this.selectedDateRange;
    const officeCodes = this.selectedOfficeList.toString();
    const caseTypeCodes = this.selectedCaseTypeList.toString();
    const demandTypes = this.selectedDemandTypeList.toString();
    const opportunityStatusTypeCodes = this.selectedOpportunityStatusTypeList.toString();
    const staffingTags = this.selectedStaffingTagList.toString();
    const minDemandProbabilityPercent = this.minDemandProbabilityPercent;
    const industryPracticeAreaCodes = this.selectedIndustryPracticeAreaList.toString();
    const capabilityPracticeAreaCodes = this.selectedCapabilityPracticeAreaList.toString();
    const clientCodes = this.selectedClientCodes;

    this.getProjectsOnFilterChange.emit({ dateRange, officeCodes, caseTypeCodes, demandTypes, opportunityStatusTypeCodes, staffingTags, minDemandProbabilityPercent, industryPracticeAreaCodes, capabilityPracticeAreaCodes, clientCodes });
  }

  getProjectsBySelectedDateRange(selectedDateRange) {
    // To avoid API call during initialization we check for non nullable start and end dates
    if (!selectedDateRange || this.selectedDateRange.toString() === selectedDateRange.toString()) {
      return;
    }

    this.selectedDateRange = selectedDateRange;

    this.getFilteredProjects();

  }

  getProjectsBySelectedOffice(officeCodes) {
    if (officeCodes && this.isArrayEqual(this.selectedOfficeList.map(String), officeCodes.split(','))) {
      return false;
    }

    this.selectedOfficeList = officeCodes.split(',');

    this.getFilteredProjects();

  }

  getProjectsBySelectedCaseTypes(caseTypeCodes: string) {
    if (caseTypeCodes && this.isArrayEqual(this.selectedCaseTypeList.map(String), caseTypeCodes.split(','))) {
      return false;
    }

    this.selectedCaseTypeList = caseTypeCodes.split(',');

    this.getFilteredProjects();
  }

  getProjectsByDemandTypes(typeNames: string) {
    if (typeNames && this.isArrayEqual(this.selectedDemandTypeList.map(String), typeNames.split(','))) {
      return false;
    }

    this.selectedDemandTypeList = typeNames.split(',');

    this.showHideMinOppProbabilityFilter();
    this.getFilteredProjects();

  }

  getProjectsBySelectedOppStatusTypes(statusTypeCodes: string) {
    if (statusTypeCodes && this.isArrayEqual(this.selectedOpportunityStatusTypeList.map(String), statusTypeCodes.split(','))) {
      return false;
    }

    this.selectedOpportunityStatusTypeList = statusTypeCodes.split(',');

    this.getFilteredProjects();
  }

  getProjectsBySelectedStaffingTags(staffingTags: any) {
    if (this.isArrayEqual(this.selectedStaffingTagList.map(String), staffingTags.split(','))) {
      return false;
    }

    this.selectedStaffingTagList = staffingTags.split(',');

    this.getFilteredProjects();
  }

  getOpportunitiesByMinProbability(minDemandProbabilityPercent) {
    this.minDemandProbabilityPercent = minDemandProbabilityPercent;

    this.getFilteredProjects();
  }

  getProjectsByIndustryPracticeAreas(industryPracticeAreaCodes: string) {
    if (industryPracticeAreaCodes && this.isArrayEqual(this.selectedIndustryPracticeAreaList.map(String), industryPracticeAreaCodes.split(','))) {
      return false;
    }

    this.selectedIndustryPracticeAreaList = industryPracticeAreaCodes.split(',');

    this.getFilteredProjects();
  }

  getProjectsByCapabilityPracticeAreas(capabilityPracticeAreaCodes: string) {
    if (capabilityPracticeAreaCodes && this.isArrayEqual(this.selectedCapabilityPracticeAreaList.map(String), capabilityPracticeAreaCodes.split(','))) {
      return false;
    }

    this.selectedCapabilityPracticeAreaList = capabilityPracticeAreaCodes.split(',');

    this.getFilteredProjects();
  }

  private initializeDateConfig() {
    this.bsConfig = BS_DEFAULT_CONFIG;
    this.bsConfig.containerClass = 'theme-red calendar-dropdown calendar-align-right';
  }

  shiftDateRange(shiftDate: string) {
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

  closeFilters() {
    this.closeFilterEmitter.emit();
  }

  //--------------------------- Helper Functions ------------------------------------------------
  setOffices() {
    this.selectedOfficeList = this.userPreferences?.demandViewOfficeCodes?.split(',');

    // Re-assign object In order to reflect changes in Demand side office filter when changes are done in User Settings
    if (this.officeHierarchy) {
      this.officeHierarchy = JSON.parse(JSON.stringify(this.officeHierarchy));
    }
  }

  setCaseTypes() {

    if (this.caseTypes) {
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

      this.selectedCaseTypeList = this.caseTypes.filter(caseType => this.userPreferences?.caseTypeCodes.indexOf(caseType.caseTypeCode.toString()) > -1)
        .map(caseType => caseType.caseTypeCode);

    }

  }

  setDemandTypes() {
    if (this.demandTypes) {
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

      this.selectedDemandTypeList = this.demandTypes.filter(statusType => this.userPreferences?.demandTypes.indexOf(statusType.type.toString()) > -1)
        .map(x => x.type);
    }

    this.showHideMinOppProbabilityFilter(); //demand types drive this filter

  }

  setOpportunityStatusTypes() {
    if (this.opportunityStatusTypes) {
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
        statusType => this.userPreferences?.opportunityStatusTypeCodes.indexOf(statusType.statusCode.toString()) > -1)
        .map(type => type.statusCode);
    }

  }

  setStaffingTags() {
    if (this.staffingTagsHierarchy) {
      const staffingTagHierarchyChildrenList = this.staffingTagsHierarchy.map((item) => {
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
      });

      this.staffingTagDropdownList = {
        text: 'All',
        value: 0,
        checked: false,
        children: staffingTagHierarchyChildrenList
      };

      this.selectedStaffingTagList = this.userPreferences?.caseAttributeNames.split(',');
    }
  }

  setIndustryPracticeAreas() {
    if (this.industryPracticeAreas) {
      const statusTypeChildrenList = this.industryPracticeAreas.map(item => {
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
        children: statusTypeChildrenList
      };

      this.selectedOpportunityStatusTypeList = this.opportunityStatusTypes.filter(
        statusType => this.userPreferences?.opportunityStatusTypeCodes.indexOf(statusType.statusCode.toString()) > -1)
        .map(type => type.statusCode);
    }

    if (this.userPreferences && this.userPreferences.industryPracticeAreaCodes) {
      this.selectedIndustryPracticeAreaList = this.userPreferences.industryPracticeAreaCodes.split(',');
    } else {
      this.selectedIndustryPracticeAreaList = [];
    }

  }

  setCapabilityPracticeAreas() {
    if (this.capabilityPracticeAreas) {
      const statusTypeChildrenList = this.capabilityPracticeAreas.map(item => {
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
        children: statusTypeChildrenList
      };

      this.selectedOpportunityStatusTypeList = this.opportunityStatusTypes.filter(
        statusType => this.userPreferences?.opportunityStatusTypeCodes.indexOf(statusType.statusCode.toString()) > -1)
        .map(type => type.statusCode);
    }

    if (this.userPreferences && this.userPreferences.capabilityPracticeAreaCodes) {
      this.selectedCapabilityPracticeAreaList = this.userPreferences.capabilityPracticeAreaCodes.split(',');
    } else {
      this.selectedCapabilityPracticeAreaList = [];
    }

  }

  showHideMinOppProbabilityFilter() {
    if (this.selectedDemandTypeList.toString().indexOf('Opportunity') < 0) {
      this.isMinOppProbabilityFilterShown = false;
    } else {
      this.isMinOppProbabilityFilterShown = true;
    }
  }

  private isArrayEqual(array1, array2) {
    return JSON.stringify(array1) === JSON.stringify(array2);
  }

  ngOnDestroy(){
    this.clientInput$?.unsubscribe();
  }

}
