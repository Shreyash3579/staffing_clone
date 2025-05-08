import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from "@angular/core";
import { BsDatepickerConfig } from "ngx-bootstrap/datepicker";
import { Subject } from "rxjs";
import { debounceTime } from "rxjs/operators";
import { CoreService } from "src/app/core/core.service";
import { BS_DEFAULT_CONFIG } from "src/app/shared/constants/bsDatePickerConfig";
import { EmployeeSearchResult } from "src/app/shared/interfaces/employeeSearchResult";
import { AzureSearchService } from "../azureSearch.service";
import { OverlayDialogService } from "src/app/overlay/dialogHelperService/overlayDialog.service";
import { DateService } from "src/app/shared/dateService";
import { HomeHelperService } from "../homeHelper.service";
import { SupplyFilterCriteria } from "src/app/shared/interfaces/supplyFilterCriteria.interface";
import { CommitmentType } from "src/app/shared/interfaces/commitmentType.interface";
import { LocalStorageService } from "src/app/shared/local-storage.service";
import { ResourceService } from "src/app/shared/helperServices/resource.service";
import { AzureSearchTriggeredFromEnum, SeachModeEnum } from "src/app/shared/constants/enumMaster";
import { BossSearchResult } from "src/app/shared/interfaces/azureSearchCriteria.interface";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";


@Component({
  selector: "app-quick-filter",
  templateUrl: "./quick-filter.component.html",
  styleUrls: ["./quick-filter.component.scss"],
  providers: [AzureSearchService]
})
export class QuickFilterComponent implements OnInit, OnChanges {
  //Input Variable
  @Input() dateRange: { startDate: any; endDate: any; };
  @Input() isStaffedFromSupply: boolean;
  @Input() clearEmployeeSearch: boolean;
 
  @Output() openQuickAddForm = new EventEmitter();
  @Output() getProjectsAndResourcesOnDateChange = new EventEmitter();
  @Output() getProjectsOnAdvancedFilterChange = new EventEmitter();
  @Output() getResourcesOnAdvancedFilterChange = new EventEmitter();
  @Output() getAllocationsSortedBySelectedValueEmitter = new EventEmitter<any>();
  @Output() onToggleWeeklyDailyView = new EventEmitter<any>();
  @Output() filterResourcesBySearchInSupply = new EventEmitter<string>();
  @Output() clearSearchInSupplyMode = new EventEmitter();

  isMenuOpen: boolean = false;
  //-----for search bar
  isSearchEnabled: boolean = false;
  searchString: string = "";
  searchResults: EmployeeSearchResult[]= [];
  changeInSearchQuery$: Subject<string> = new Subject();
  showResourceLoader: boolean = false;
  showNoResourcesMessage:string = "";
  collapseNewDemandAll: boolean = false;
  selectedGroupingOption = "Weekly";
  selectedSearchModeOption = SeachModeEnum.ALL;
  showSearchResultsWrapper: boolean = false;

  // for date picker
  bsConfig: Partial<BsDatepickerConfig>;
  selectedDateRange;
  
  // for availability calculation
  supplyFilterCriteriaObj: SupplyFilterCriteria = {} as SupplyFilterCriteria;
  commitmentTypes: CommitmentType[];
            
  constructor(private searchService: AzureSearchService, 
    private coreService: CoreService,
    private overlayDialogService: OverlayDialogService,
    private localStorageService: LocalStorageService,
    private homeHelperService: HomeHelperService) { }

  //---------------------------Life Cycle Events---------------------------------
  ngOnInit(): void {
    this.initializeDateConfig();
    this.setUserAuthorizationForSearch();
    this.commitmentTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.commitmentTypes);
  }

  ngOnChanges(changes : SimpleChanges): void {
    if(changes.dateRange && this.dateRange) {
      this.setDatePicker();
    }

    if(changes.clearEmployeeSearch && this.clearEmployeeSearch){
      this.searchString = "";
    }
  }
  
  collapseNewDemandAllHandler() {
    this.collapseNewDemandAll = !this.collapseNewDemandAll;
    this.homeHelperService.setCollapseNewDemandAll(this.collapseNewDemandAll);
  }

  //-----------------------------Event Handlers--------------------------------------
  setDatePicker() {
    this.selectedDateRange = [DateService.parseDateInLocalTimeZone(this.dateRange.startDate), DateService.parseDateInLocalTimeZone(this.dateRange.endDate)];
  }
  
  onEnterKeyPressed(keyPressEvent) {
    if(this.isSearchEnabled){
      this.getResourcesBySearchStringFromAzure();
    }else{
      this.showNoResourcesMessage = "You are not authorized for search";
    }
  }

  onSearchModeToggle(event){
    if (event.target.checked === true) {
      this.selectedSearchModeOption = SeachModeEnum.SUPPLY;
      this.searchResults = [];
      this.showNoResourcesMessage = "";
      this.showSearchResultsWrapper = false;
      this.getResourcesBySearchStringFromAzure();
    } else {
      this.selectedSearchModeOption = SeachModeEnum.ALL;
      this.searchString = "";
      this.searchResults = [];
      this.showNoResourcesMessage = "";
      this.showSearchResultsWrapper = false;
      this.clearSearchInSupplyMode.emit();
    }
    
  }

  subscribeSearchQueryChanges() {
    this.changeInSearchQuery$.pipe(debounceTime(500)).subscribe(() => {
      if (this.searchString.length < 2) return;

      this.getResourcesBySearchStringFromAzure();

    });
  }

  clearSearch(){
    this.searchString = "";
    this.searchResults = [];
    this.showNoResourcesMessage = "";
    this.showSearchResultsWrapper = false;

    if(this.selectedSearchModeOption === SeachModeEnum.SUPPLY){
      this.clearSearchInSupplyMode.emit();
    }

  }
  getResourcesBySearchStringFromAzure(){
    if (this.searchString.length < 2) return;

    this.showResourceLoader = true;
    this.searchResults  = [];
    this.showNoResourcesMessage = "";

    if(this.selectedSearchModeOption === SeachModeEnum.SUPPLY){
      this.showSearchResultsWrapper = false;
      this.filterResourcesBySearchInSupply.emit(this.searchString);
    }else {
      this.showSearchResultsWrapper = true;

      this.searchService.getResourcesBySearchStringFromAzure(this.searchString, AzureSearchTriggeredFromEnum.HOME_SEARCH_ALL)
      .subscribe((searchResults: BossSearchResult) => {
        
        if(!searchResults.generatedLuceneSearchQuery?.isErrorInGeneratingSearchQuery){
          if(searchResults.searches?.length){
            const userPreferences = this.coreService.getUserPreferencesValue();
            const [startDate, endDate] = DateService.getDateRangeAvalabilityFilterExpression(searchResults.generatedLuceneSearchQuery.filter);
  
            let availableResources = ResourceService.createResourcesDataForResourcesTab(
              searchResults.searches, startDate, endDate,
              this.supplyFilterCriteriaObj, this.commitmentTypes, userPreferences, true
            );
          
            //exclude resources that are not available
            availableResources = availableResources.filter(item => item.resource.dateFirstAvailable);
            
            if(startDate){
              availableResources = availableResources.filter(item => {
                const date = DateService.parseDateInLocalTimeZone(item.resource.dateFirstAvailable).setHours(0,0,0,0);
                return date >= startDate.getTime();
              });
            }
  
            if(endDate){
              availableResources = availableResources.filter(item => {
                const date = DateService.parseDateInLocalTimeZone(item.resource.dateFirstAvailable).setHours(0,0,0,0);
                return date <= endDate.getTime();
              });
            }
           
            availableResources.forEach(item => {
              const employeeSearchRecord = searchResults["searches"].find(x => x.resource.employeeCode == item.resource.employeeCode);
              var searchItem: EmployeeSearchResult = employeeSearchRecord.document;
              searchItem.score = employeeSearchRecord.score;
              
              //filter dates to get only the ones that are on or after today. remove time
              searchItem.staffingTag = searchItem.aggregatedRingfences? searchItem.aggregatedRingfences : searchItem.serviceLineName;
              searchItem.dateFirstAvailable = item.resource.dateFirstAvailable;
              searchItem.percentAvailable = item.resource.percentAvailable;
              searchItem.aggregatedLanguages = searchItem.languages?.map((item) => `${item.name} (${item.proficiencyName})` ).join(', ');
              
              this.searchResults.push(searchItem);
            });
  
            this.showNoResourcesMessage = availableResources.length ? "" : "No resources found for the search query. Please try with different query";
          }else{
            this.showNoResourcesMessage = "No resources found for the search query. Please try with different query"
          }
        }else{
          this.showNoResourcesMessage = "Error while getting resources. Please try with different  query"
        }
  
        this.showResourceLoader = false;
      })
    }
  }

   openResourceDetailsDialogHandler(employeeCode){
    this.overlayDialogService.openResourceDetailsDialogHandler(employeeCode);
  }


  openQuickAddFormHandler(event) {
    this.openQuickAddForm.emit(event);
  }

  getProjectsandResourcesforSelectedDateRange(selectedDateRange) {
    // To avoid API call during initialization we check for non nullable start and end dates
    if (!selectedDateRange || this.selectedDateRange.toString() === selectedDateRange.toString()) {
      return;
    }

    this.selectedDateRange = selectedDateRange;

    this.getFilteredProjectsAndResourcesOnDateChange();

  }

  OnWeeklyOrDailyToggle(event) {
    if (event.target.checked === true) {
      this.selectedGroupingOption = 'Daily';
    } else {
      this.selectedGroupingOption = 'Weekly';
    }
    this.onToggleWeeklyDailyView.emit(this.selectedGroupingOption);

  }

  getFilteredProjectsAndResourcesOnDateChange() {

    const dateRange = this.selectedDateRange;

    this.getProjectsAndResourcesOnDateChange.emit({ dateRange});

  }

  getProjectsOnAdvancedFilterChangeHandler(event){
    this.getProjectsOnAdvancedFilterChange.emit(event);
  }

  getResourcesOnAdvancedFilterChangeHandler(event){
    this.getResourcesOnAdvancedFilterChange.emit(event);
  }

  getAllocationsSortedBySelectedValueHandler(event) {
    this.getAllocationsSortedBySelectedValueEmitter.emit(event);
  }


  //----------------------Priavte Helpers---------------------//
  private setUserAuthorizationForSearch() {
    this.isSearchEnabled = this.coreService.loggedInUser.hasAccessToAISearch;
    return this.isSearchEnabled;
  }

  private initializeDateConfig() {
    const minDate = DateService.getStartOfWeek();

    this.bsConfig = Object.assign({}, BS_DEFAULT_CONFIG);
    this.bsConfig.containerClass = 'theme-red calendar-supply calendar-align-left';
    this.bsConfig.daysDisabled =  [0, 2, 3, 4, 5, 6];
    this.bsConfig.minDate = minDate;
  }
  

}
