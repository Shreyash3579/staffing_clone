// -------------------Angular Operators---------------------------------------//
import {
  Component,
  OnInit,
  Output,
  EventEmitter,
  Input,
  ElementRef,
  ViewChild,
  OnDestroy,
  SimpleChanges,
  OnChanges
} from "@angular/core";
import { BsDatepickerConfig } from "ngx-bootstrap/datepicker";
import { BsModalService } from "ngx-bootstrap/modal";
import { Subject, fromEvent } from "rxjs";
import { map, debounceTime, filter } from "rxjs/operators";
import { BS_DEFAULT_CONFIG } from "src/app/shared/constants/bsDatePickerConfig";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { EmployeeCaseGroupingEnum, ResourcesSupplyFilterGroupEnum, WeeklyMonthlyGroupingEnum } from "src/app/shared/constants/enumMaster";
import { DropdownFilterOption } from "src/app/shared/interfaces/dropdown-filter-option";
import { UserPreferenceSupplyGroupViewModel } from "src/app/shared/interfaces/userPreferenceSupplyGroupViewModel";
import { NotificationService } from "src/app/shared/notification.service";
import { SavedGroupModalComponent } from "../resources-filter/saved-group-modal/saved-group-modal.component";
import { ResourceFilter } from "src/app/shared/interfaces/resource-filter.interface";
import { SystemconfirmationFormComponent } from "src/app/shared/systemconfirmation-form/systemconfirmation-form.component";
import { UserPreferenceSupplyGroup } from "src/app/shared/interfaces/userPreferenceSupplyGroup";
import { UserPreferencesService } from "src/app/core/user-preferences.service";
import { LocalStorageService } from "src/app/shared/local-storage.service";
import { UserPreferencesMessageService } from "src/app/core/user-preferences-message.service";
import { UserPreferences } from "src/app/shared/interfaces/userPreferences.interface";
import { SortRow } from "src/app/shared/interfaces/sort-row.interface";
import { DateService } from "src/app/shared/dateService";
import { FilterRow } from "src/app/shared/interfaces/filter-row.interface";
import { UserPreferenceGroupFilters } from "src/app/shared/interfaces/UserPreferenceGroupFilters";
import { ResourceViewCD } from "src/app/shared/interfaces/resource-view-cd.interface";
import { ResourceViewCommercialModel } from "src/app/shared/interfaces/resource-view-commercial-model.interface";

@Component({
  selector: "resources-filter",
  templateUrl: "./filter.component.html",
  styleUrls: ["./filter.component.scss"]
})
export class FilterComponent implements OnInit, OnDestroy, OnChanges {
  // -----------------------Input Variables--------------------------------------------//
  @Input() clearSearch: Subject<boolean>;
  @Input() dateRange: [Date, Date];
  @Input() thresholdRangeValue: any;
  @Input() resourcesRecentCDList: ResourceViewCD[] = [];
  @Input() resourcesCommercialModelList:ResourceViewCommercialModel[] = [];
  @Input()
  set allSupplyDropdownOptions(value: DropdownFilterOption[]) {
    if(value && value.length){
      this._allSupplyDropdownOptions = value;
      this.setChangesDone();
      this.loadDisplayingDropdown();
      this.setSelectedGroup();
      this.refreshSortAndFilters();
    }
  }
  get allSupplyDropdownOptions() {
    return this._allSupplyDropdownOptions;
  }
  @Input()
  set savedResourceFilters(value: ResourceFilter[]) {
    if(value){
      this._savedResourceFilters = this.filteredItems = value;
      this.setChangesDone();
      this.refreshSortAndFilters();
    }
  }

  get savedResourceFilters() {
    return this._savedResourceFilters;
  }
  @Input()
  set supplyGroupPreferences(value: UserPreferenceSupplyGroupViewModel[]) {
    this._supplyGroupPreferences = this.filteredGroups = value;
    this.setChangesDone();
    this.refreshSortAndFilters();
  }
  get supplyGroupPreferences() {
    return this._supplyGroupPreferences;
  }
  @Input() userPreferences: UserPreferences;

  // -----------------------Output Events--------------------------------------------//
  @Output() getResourcesIncludingTerminatedBySearchString = new EventEmitter<any>();
  @Output() openQuickAddFormEmitter = new EventEmitter<any>();
  //TODO: to be removed with app-side-bar-filters
  // @Output() toggleFiltersSection = new EventEmitter<any>();
  @Output() openInfoModal = new EventEmitter<any>();
  @Output() printPdfEmitter = new EventEmitter<any>();
  @Output() getResourcesByDateRange = new EventEmitter();
  @Output() updateThresholdRangeEmitter = new EventEmitter();
  @Output() onSupplyGroupFilterChanged = new EventEmitter<any>();
  // @Output() getResourcesSortBySelectedValues = new EventEmitter();
  @Output() onToggleEmployeeCaseGroup = new EventEmitter<string>();
  @Output() onToggleWeeklyMonthlyGroup = new EventEmitter<string>();
  @Output() onTogglePracticeView = new EventEmitter<boolean>();

  @Output() upsertResourceFilter = new EventEmitter<any>();
  @Output() deleteSavedFilter = new EventEmitter<any>();
  @Output() upsertUserPreferencesSupplyGroupEmitter = new EventEmitter<any>();
  @Output() deleteSupplyGroupsRefresh = new EventEmitter<any>();
  @Output() sortResourcesBySelectedValues = new EventEmitter<any>();
  @Output() filterResourcesBySelectedValues = new EventEmitter<any>();
  @Output() openCustomGroupModalEmitter = new EventEmitter<any>();

  // -----------------------Templare Reference Variables--------------------------------------------//
  @ViewChild("employeeSearchInput", { static: true })
  employeeSearchInput: ElementRef;

  // -----------------------Local Variables--------------------------------------------//
  public accessibleFeatures = ConstantsMaster.appScreens.feature;
  public selectedDateRange: any;
  public bsConfig: Partial<BsDatepickerConfig>;
  displayGroups = [];
  public readonly employeeCaseGroupingEnum = EmployeeCaseGroupingEnum;
  public readonly weeklyMonthlyGroupingEnum = WeeklyMonthlyGroupingEnum;

  public selectedEmployeeCaseGroupingOption: string = this.employeeCaseGroupingEnum.RESOURCES;
  public selectedWeeklyMonthlyGroupingOption: string = this.weeklyMonthlyGroupingEnum.WEEKLY;

  public activeGroup = {
    type: "",
    selected: false,
    group: {} as UserPreferenceSupplyGroupViewModel
  };

  public tempActiveGroup = {
    type: "",
    group: {}
  };

  public selectedSortItem: SortRow[] = [];
  public appliedFilters: FilterRow[] = [];
  public temporarySort: SortRow[] = [];
  public temporaryFilters: FilterRow[] = [];

  public filteredItems: ResourceFilter[];
  public _savedResourceFilters: ResourceFilter[];

  public filteredGroups: UserPreferenceSupplyGroupViewModel[];
  public _supplyGroupPreferences: UserPreferenceSupplyGroupViewModel[];

  public _allSupplyDropdownOptions: DropdownFilterOption[];

  public areChangesMade: boolean = false;
  STAFFING_SETTINGS_FILTER_ENUM = ResourcesSupplyFilterGroupEnum.STAFFING_SETTINGS;
  SAVED_GROUP_FILTER_ENUM = ResourcesSupplyFilterGroupEnum.SAVED_GROUP;
  CUSTOM_GROUP_FILTER_ENUM = ResourcesSupplyFilterGroupEnum.CUSTOM_GROUP;

  // --------------------------Constructor----------------------------//
  constructor(
    private modalService: BsModalService,
    private notifyService: NotificationService,
    private userPreferencesService: UserPreferencesService,
    private userPreferencesMessageService: UserPreferencesMessageService,
    private localStorageService: LocalStorageService
  ) { }

  // --------------------------Component LifeCycle Events----------------------------//
  ngOnInit() {
    this.attachEventToSearchBox();
    this.initializeDateConfig();
  }

  ngOnChanges(change: SimpleChanges) {
    let currentDateRange = null;
    let previousDateRange = null;

    if (change.dateRange && this.dateRange) {
      currentDateRange = change.dateRange.currentValue;
      previousDateRange = change.dateRange.previousValue;
      this.selectedDateRange = this.dateRange;
    }

    let changesInDateRange = this.isChangesInDateRange(currentDateRange, previousDateRange);

    if ((!change.dateRange || (change.dateRange && changesInDateRange))) {
      // this.loadDisplayingDropdown();
    }
  }

  setSelectedGroup() {
    if(!this.tempActiveGroup || !this.tempActiveGroup.group || !this.tempActiveGroup.type){

      this.displayGroups.forEach((option) => {
        option.items.forEach((group) => {
          if (group.selected) {
            this.tempActiveGroup = {
              type: group.filterGroupId,
              group: group
            };
          }
        });
      });

    }

    const activeGroup: any = this.tempActiveGroup.group;
    const defaultOption = this.allSupplyDropdownOptions.find(x => x.isDefault);
    const isActiveGroup = this.allSupplyDropdownOptions.some(x => x.id == activeGroup.id)

    if (this.isDefaultChanged() || (activeGroup.id && !isActiveGroup)) {
      this.setSelectedViewingFilter(defaultOption);
    }
    else if (this.isArrayExistAndHasValues(this.allSupplyDropdownOptions) && !!activeGroup && !!activeGroup.id) {
      this.allSupplyDropdownOptions.find(x => x.isDefault).selected = false;
      this.allSupplyDropdownOptions.find(x => x.id === activeGroup.id).selected = true;
    }
  }

  isArrayExistAndHasValues(array) {
    return !!array && array.length > 0;
  }

  isDefaultChanged() {
    let isDefaultChanged = false;
    if (this.isArrayExistAndHasValues(this.displayGroups) && this.isArrayExistAndHasValues(this.allSupplyDropdownOptions)) {
      const newDefault = this.allSupplyDropdownOptions.find(x => x.isDefault);
      this.displayGroups.forEach(group => {
        const defaultGroup = group.items.find(item => item.isDefault);
        if (!!defaultGroup && defaultGroup.id != newDefault.id) {
          isDefaultChanged = true;
        }
        return group;
      })
    }
    return isDefaultChanged;
  }
  isChangesInDateRange(currentDateRange, previousDateRange) {
    let changesInDateRange = false;
    if (currentDateRange || previousDateRange) {
      const currentStartDate = !!currentDateRange ? DateService.convertDateInBainFormat(currentDateRange[0]) : null;
      const currentEndDate = !!currentDateRange ? DateService.convertDateInBainFormat(currentDateRange[1]) : null;
      const previousStartDate = !!previousDateRange ? DateService.convertDateInBainFormat(previousDateRange[0]) : null;
      const previousEndDate = !!previousDateRange ? DateService.convertDateInBainFormat(previousDateRange[1]) : null;

      if (!DateService.isSame(currentStartDate, previousStartDate) || !DateService.isSame(currentEndDate, previousEndDate)) {
        changesInDateRange = true;
      }
      return changesInDateRange;
    }
  }

  loadDisplayingDropdown() {
    this.displayGroups = [];
    if (this.allSupplyDropdownOptions && this.allSupplyDropdownOptions.length > 0) {
      const staffingSettings: DropdownFilterOption[] = this.allSupplyDropdownOptions.filter(
        (x) => x.filterGroupId === this.STAFFING_SETTINGS_FILTER_ENUM
      );
      const supplyGroups: DropdownFilterOption[] = this.allSupplyDropdownOptions.filter(
        (x) => x.filterGroupId === this.CUSTOM_GROUP_FILTER_ENUM
      );
      const savedFilters: DropdownFilterOption[] = this.allSupplyDropdownOptions.filter(
        (x) => x.filterGroupId === this.SAVED_GROUP_FILTER_ENUM
      );

      this.displayGroups.push(
        {
          filterGroupName: "STAFFING SETTINGS",
          filterGroupId: this.STAFFING_SETTINGS_FILTER_ENUM,
          items: staffingSettings
        },
        {
          filterGroupName: "CUSTOM GROUPS",
          filterGroupId: this.CUSTOM_GROUP_FILTER_ENUM,
          items: supplyGroups
        },
        {
          filterGroupName: "SAVED GROUPS",
          filterGroupId: this.SAVED_GROUP_FILTER_ENUM,
          items: savedFilters
        }
      );

      this.displayGroups = this.displayGroups.filter((x) => x.items.length > 0);
    }
  }


  applyFilterSortEdits($event) {
    this.areChangesMade = true;
    if ($event.type === 'sort') {
      this.temporarySort = $event.data;
      this.selectedSortItem = $event.data;
      this.sortResourcesBySelectedValuesHandler(this.selectedSortItem);
    }
    else if($event.type === 'filter'){
      this.temporaryFilters = $event.data;
      this.filterResourcesBySelectedValuesHandler($event.data, this.selectedSortItem);
    }
  }

  sortResourcesBySelectedValuesHandler(sortRows: SortRow[]) {
    this.sortResourcesBySelectedValues.emit(sortRows);
  }

  filterResourcesBySelectedValuesHandler(filterBy: FilterRow[], sortRows: SortRow[]) {
    this.filterResourcesBySelectedValues.emit({filterBy, sortRows});
  }

  refreshSortAndFilters() {
    if (!!this.allSupplyDropdownOptions && this.allSupplyDropdownOptions.length > 0) {
      this.selectedSortItem = [];
      this.appliedFilters = [];
      const selectedOption = this.allSupplyDropdownOptions.find(x => x.selected);
      if (selectedOption.filterGroupId === this.SAVED_GROUP_FILTER_ENUM) {
        const selectedSavedFilter = this.savedResourceFilters.find(x => x.id === selectedOption.id);
        if (!!selectedSavedFilter) {
          this.setSortByOptions(selectedSavedFilter.resourcesTabSortBy);
          this.setFilterByOptions(selectedSavedFilter.filterBy);
        }
      }
      else if (selectedOption.filterGroupId === this.CUSTOM_GROUP_FILTER_ENUM) {
        const selectedCustomGroup = this.supplyGroupPreferences.find(x => x.id === selectedOption.id);
        if (!!selectedCustomGroup) {
          this.setSortByOptions(selectedCustomGroup.sortBy);
          this.setFilterByOptions(selectedCustomGroup.filterBy);
        }
      }
    }
  }

  setSortByOptions(sortBy = '') {
    if (!!sortBy) {
      const options: string[] = sortBy.split(',');

      options.forEach(x => {
        if (!!x) {
          const field = x.split('|')[0];
          const direction = x.split('|')[1];
          const option: SortRow = {
            field: field,
            direction: direction
          }
          this.selectedSortItem.push(option);
        }
      });
    }
  }

  setFilterByOptions(filterBy: UserPreferenceGroupFilters[]) {
    if(!! filterBy) {
      filterBy.forEach(filter => {
        const appliedFilter: FilterRow = {
          andOr: filter.andOr,
          filterField: filter.filterField,
          filterOperator: filter.filterOperator,
          filterValue: filter.filterValue
        }
        this.appliedFilters.push(appliedFilter);
      })
    }
  }

  handleCreateMoreSelection(groupType) {
    switch (groupType) {
      case "customGroup":
        this.openCustomGroupModalEmitter.emit({isEditMode:false, groupToEdit: null});
        break;
      case "savedGroup":
        this.handleCreateNewSavedGroup();
        break;
    }
  }

  handleCreateNewSavedGroup() {
    const modalRef = this.modalService.show(SavedGroupModalComponent, {
      backdrop: false,
      class: "saved-group-modal modal-dialog-centered"
    });

    modalRef.content.upsertResourceFilter.subscribe((group) => {
      this.upsertResourceFilterHandler(group);
    });
  }

  upsertUserPreferenceSupplyGroupsEmitterHandler(supplyGroupDataToUpsert: UserPreferenceSupplyGroup | UserPreferenceSupplyGroupViewModel[]) {
    this.upsertUserPreferencesSupplyGroupEmitter.emit(supplyGroupDataToUpsert);
  }


  setStaffingSettingsLastUpdatedTimeStampInLocalStorage() {
    //This is done so that tableau  reports load with latest staffing settings on load
    this.localStorageService.set(ConstantsMaster.localStorageKeys.userPreferencesLastUpdatedTimestamp, new Date().getTime());
  }

  // Handle group editing
  handleEditGroupSelection(filterItemToEdit, secondPage: false) {
    if (filterItemToEdit.filterGroupId == this.CUSTOM_GROUP_FILTER_ENUM) {
      let groupToEdit = this.filteredGroups.find(x => x.id == filterItemToEdit.id);
      this.openCustomGroupModalEmitter.emit({isEditMode: true, groupToEdit})
    } else {
      let groupToEdit = this.filteredItems.find(x => x.id == filterItemToEdit.id);
      this.openSavedGroupModal(true, groupToEdit, secondPage);
    }
  }

  openSavedGroupModal(isEditMode: boolean = false, groupToEdit: ResourceFilter = null, secondPage: boolean = false) {
    const modalRef = this.modalService.show(SavedGroupModalComponent, {
      backdrop: false,
      class: "saved-group-modal modal-dialog-centered",
      initialState: {
        isEditMode: isEditMode,
        groupToEdit: groupToEdit,
        onSecondPage: secondPage
      }
    });

    modalRef.content.upsertResourceFilter.subscribe((upsertedSavedGroupFilter) => {
      this.setChangesDone();
      this.upsertResourceFilterHandler(upsertedSavedGroupFilter);
    });
  }

  setChangesDone(){
    this.areChangesMade = false;
  }

  cancelChanges(type){
    const selectedOption = this.allSupplyDropdownOptions.find(x => x.selected);
    if (type == 'sort') {
      this.selectedSortItem = [];

      if (this.temporarySort.length > 0) {
        this.selectedSortItem = this.temporarySort;
      } else {
        this.resetToSavedSort(selectedOption);
      }
    } 
    else if (type == 'filter') {
      this.appliedFilters = [];

      if (this.temporaryFilters.length > 0) {
        this.appliedFilters = this.temporaryFilters;
      } else {
        this.resetToSavedFilter(selectedOption);
      }
    } else {
      return;
    }
  }

  resetToSavedFilter(selectedOption) {
    if (selectedOption.filterGroupId === this.SAVED_GROUP_FILTER_ENUM) {
      const selectedSavedFilter = this.savedResourceFilters.find(x => x.id === selectedOption.id);
      this.setFilterByOptions(selectedSavedFilter.filterBy);
    }
    else if (selectedOption.filterGroupId === this.CUSTOM_GROUP_FILTER_ENUM) {
      const selectedCustomGroup = this.supplyGroupPreferences.find(x => x.id === selectedOption.id);
      this.setFilterByOptions(selectedCustomGroup.filterBy);
    }
  }

  resetToSavedSort(selectedOption) {
    if (selectedOption.filterGroupId === this.SAVED_GROUP_FILTER_ENUM) {
      const selectedSavedFilter = this.savedResourceFilters.find(x => x.id === selectedOption.id);
      this.setSortByOptions(selectedSavedFilter.resourcesTabSortBy);
    }
    else if (selectedOption.filterGroupId === this.CUSTOM_GROUP_FILTER_ENUM) {
      const selectedCustomGroup = this.supplyGroupPreferences.find(x => x.id === selectedOption.id);
      this.setSortByOptions(selectedCustomGroup.sortBy);
    }
  }

  resetChanges() {
    const selectedOption = this.allSupplyDropdownOptions.find(x => x.selected);   
    this.temporarySort = [];
    this.temporaryFilters = [];
    this.selectedSortItem = [];
    this.appliedFilters = [];
    this.resetToSavedFilter(selectedOption);
    this.resetToSavedSort(selectedOption);
    this.sortResourcesBySelectedValuesHandler(this.selectedSortItem);
    this.filterResourcesBySelectedValuesHandler(this.appliedFilters, this.selectedSortItem);
    this.setChangesDone();
  }

  // Save filter | sort changes
  saveFilterSortChanges() {
    const selectedOption = this.allSupplyDropdownOptions.find(x => x.selected);

    if (selectedOption.filterGroupId === this.SAVED_GROUP_FILTER_ENUM) {
      const selectedSavedFilter = this.savedResourceFilters.find(x => x.id === selectedOption.id);
      selectedSavedFilter.resourcesTabSortBy = this.setResourcesTabSortByValue(this.selectedSortItem);
      selectedSavedFilter.filterBy = this.setResourcesTabFilterByValue(this.appliedFilters, selectedSavedFilter);

      const resourceFiltersData: ResourceFilter[] = [];
      resourceFiltersData.push(selectedSavedFilter);
      this.upsertResourceFilterHandler(resourceFiltersData);
    }
    else if (selectedOption.filterGroupId === this.CUSTOM_GROUP_FILTER_ENUM) {
      const selectedCustomGroup = this.supplyGroupPreferences.find(x => x.id === selectedOption.id);
      selectedCustomGroup.sortBy = this.setResourcesTabSortByValue(this.selectedSortItem);
      selectedCustomGroup.filterBy = this.setResourcesTabFilterByValue(this.appliedFilters, selectedCustomGroup);

      const upsertedCustomGroups: UserPreferenceSupplyGroupViewModel[] = [];
      upsertedCustomGroups.push(selectedCustomGroup)
      this.upsertUserPreferenceSupplyGroupsEmitterHandler(upsertedCustomGroups);
    }

    this.setChangesDone();
  }

  upsertResourceFilterHandler(resourceFiltersData) {
    this.upsertResourceFilter.emit(resourceFiltersData)
  }

  saveAsNewGroup() {
    const selectedOption = this.allSupplyDropdownOptions.find(x => x.selected);
    if (selectedOption.filterGroupId === this.SAVED_GROUP_FILTER_ENUM) {
      const selectedSavedFilter = this.savedResourceFilters.find(x => x.id === selectedOption.id);
      const newSavedFilter: ResourceFilter = { ...selectedSavedFilter }
      newSavedFilter.id = null;
      newSavedFilter.title = '';
      newSavedFilter.description = '';
      newSavedFilter.isDefault = false;
      newSavedFilter.resourcesTabSortBy = this.setResourcesTabSortByValue(this.selectedSortItem);
      newSavedFilter.filterBy = this.setResourcesTabFilterByValue(this.appliedFilters, newSavedFilter);
      newSavedFilter.sharedWith.forEach(x => x.id = null);

      this.openSavedGroupModal(true, newSavedFilter);
    }
    else if (selectedOption.filterGroupId === this.CUSTOM_GROUP_FILTER_ENUM) {
      const selectedCustomGroup = this.supplyGroupPreferences.find(x => x.id === selectedOption.id);
      const newCustomGroup: UserPreferenceSupplyGroupViewModel = { ...selectedCustomGroup }
      newCustomGroup.id = null;
      newCustomGroup.name = '';
      newCustomGroup.description = '';
      newCustomGroup.isDefault = false;
      newCustomGroup.isDefaultForResourcesTab = false;
      newCustomGroup.sortBy = this.setResourcesTabSortByValue(this.selectedSortItem);
      newCustomGroup.sortRows = this.selectedSortItem;
      newCustomGroup.filterBy = this.setResourcesTabFilterByValue(this.appliedFilters, newCustomGroup);
      newCustomGroup.sharedWith.forEach(x => x.id = null);
      //newCustomGroup.filterRows = this.appliedFilters;  

      this.openCustomGroupModalEmitter.emit({isEditMode: true, groupToEdit: newCustomGroup})
    }
  }

  setResourcesTabSortByValue(sortRows: SortRow[] = []) {
    let sortBy = '';
    sortRows.forEach(x => {
      sortBy += `${x.field}|${x.direction},`
    });
    sortBy = sortBy.slice(0, -1);

    return sortBy;
  }

  setResourcesTabFilterByValue(filterRows: FilterRow[] = [], selectedGroup) {
    let filterBy: UserPreferenceGroupFilters[] = [];
    filterRows.forEach(x => {
      const filterObj = {
        groupId: selectedGroup.id,
        andOr: x.andOr,
        filterField: x.filterField,
        filterOperator: x.filterOperator,
        filterValue: x.filterValue
      } 
      filterBy.push(filterObj);
    })
    return filterBy
  }

  deleteGroup() {
    const confirmationPopUpBodyMessage = 'Are you sure you want to delete "' + this.tempActiveGroup.group["text"] + '" filter ?';
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        confirmationPopUpBodyMessage: confirmationPopUpBodyMessage
      }
    };

    const bsModalRef = this.modalService.show(SystemconfirmationFormComponent, config);
    bsModalRef.content.deleteResourceNote.subscribe(() => {
      if (this.tempActiveGroup.type == this.SAVED_GROUP_FILTER_ENUM) {
        this.deleteSavedFilter.emit(this.tempActiveGroup.group["id"]);
      } else if (this.tempActiveGroup.type == this.CUSTOM_GROUP_FILTER_ENUM) {
        this.deleteUserPreferenceSupplyGroupByIds(this.tempActiveGroup.group["id"]);
      }
    });
  }

  deleteUserPreferenceSupplyGroupByIds(deletedGroupIds) {
    this.userPreferencesService.deleteUserPreferenceSupplyGroupByIds(deletedGroupIds)
      .subscribe((deletedData => {
        this.notifyService.showSuccess('Supply Group Deleted successfully');
        this.deleteSupplyGroupsRefresh.emit([].concat(deletedGroupIds));
      }));
  }

  setSelectedViewingFilter(selectedFilterItem) {
    const selectedId = selectedFilterItem.id;
    
    this.temporarySort = [];
    this.temporaryFilters = [];
    this.setChangesDone();

    this.displayGroups.forEach((x) => {
      x.items.forEach((item) => {
        if (item.id != selectedId) {
          item.selected = false;
        } else {
          item.selected = true;

          this.tempActiveGroup = {
            type: selectedFilterItem.filterGroupId,
            group: selectedFilterItem
          };
        }
      });
    });

    this.toggleSupplyGroupingSelection(selectedId);
    this.refreshSortAndFilters();
  }

  setSelectedAsDefault() {
    const selectedOption = this.allSupplyDropdownOptions.find(x => x.selected);

    if (selectedOption.isDefault || selectedOption.isDefaultForResourcesTab) {
      this.notifyService.showValidationMsg("Group already selected as default!");
      return;
    }

    if (selectedOption.filterGroupId === this.SAVED_GROUP_FILTER_ENUM) {
      const selectedSavedFilter = this.savedResourceFilters.find(x => x.id === selectedOption.id);
      selectedSavedFilter.isDefault = true;

      const resourceFiltersData: ResourceFilter[] = [];
      resourceFiltersData.push(selectedSavedFilter);
      this.upsertResourceFilterHandler(resourceFiltersData);
    }
    else if (selectedOption.filterGroupId === this.CUSTOM_GROUP_FILTER_ENUM) {
      const selectedCustomGroup = this.supplyGroupPreferences.find(x => x.id === selectedOption.id);
      selectedCustomGroup.isDefaultForResourcesTab = true;

      const upsertedCustomGroups: UserPreferenceSupplyGroupViewModel[] = [];
      upsertedCustomGroups.push(selectedCustomGroup)
      this.upsertUserPreferenceSupplyGroupsEmitterHandler(upsertedCustomGroups);
    }
  }

  updateThresholdRangeHandler(data) {
    this.updateThresholdRangeEmitter.emit(data);
  }

  shiftDateRange(shiftDate) {
    if (shiftDate === "left") {
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

    this.getFilteredResourcesByDateRange();
  }

  onDateRangeChange(selectedDateRange) {
    // To avoid API call during initialization we check for non nullable start and end dates
    if (!selectedDateRange || this.selectedDateRange.toString() === selectedDateRange.toString()) {
      return;
    }

    this.selectedDateRange = selectedDateRange;

    this.getFilteredResourcesByDateRange();
  }

  private getFilteredResourcesByDateRange() {
    const dateRange = this.selectedDateRange;

    this.getResourcesByDateRange.emit({
      dateRange
    });
  }

  //loadSupplyGroupDropDown(){

  //  //this.allSupplyDropdownOptions;

  //}

  //Export functionality: future requirements, should not be removed
  printPdfHandler() {
    this.printPdfEmitter.emit();
  }


  // -----------------------Component Event Handlers----------------------------//
  searchEmployee(searchText = "") {
    this.getResourcesIncludingTerminatedBySearchString.emit({
      typeahead: searchText
    });
  }

  clearSearchBox(clearSearchOnly) {
    this.employeeSearchInput.nativeElement.value = "";
    if (!clearSearchOnly) {
      this.searchEmployee();
    }
  }

  attachEventToSearchBox() {
    this.clearSearch.subscribe((value) => {
      this.clearSearchBox(value);
    });

    fromEvent(this.employeeSearchInput.nativeElement, "keyup")
      .pipe(
        map((event: any) => {
          if(!event.target.value){
            this.clearSearchBox(false);
          }
          return event.target.value;
        }),
        debounceTime(500),
        filter((res) => res.length > 2),
        // ,distinctUntilChanged() //removing this as it was craeting testin to and fro resources
      )
      .subscribe((text: string) => {
        this.searchEmployee(text);
      });
  }

  addResourceCommitment(event) {
    this.openQuickAddFormEmitter.emit(event);
  }

  showInformation(event) {
    this.openInfoModal.emit(event);
  }

  //TODO: to be removed with app-side-bar-filters
  // openFilterSection() {
  //   this.toggleFiltersSection.emit();
  // }

  toggleSupplyGroupingSelection(event) {
    this.onSupplyGroupFilterChanged.emit(event);
  }

  private initializeDateConfig() {
    this.bsConfig = Object.assign({}, BS_DEFAULT_CONFIG);
    this.bsConfig.containerClass = "theme-dark-blue calendar-dropdown calendar-align-left";
  }

  // getResourcesSortBySelectedValuesHandler(event) {
  //   this.getResourcesSortBySelectedValues.emit(event);
  // }

  // Handle grouping selection
  toggleEmployeeCaseGroupHandler(selectedGroupingOption: string) {
    this.selectedEmployeeCaseGroupingOption = selectedGroupingOption;

    this.onToggleEmployeeCaseGroup.emit(selectedGroupingOption);
  }

  toggleWeeklyMonthlyViewHandler(selectedGroupingViewOption: string) {
    this.selectedWeeklyMonthlyGroupingOption = selectedGroupingViewOption;
    this.onToggleWeeklyMonthlyGroup.emit(selectedGroupingViewOption);
  }

  togglePracticeViewHandler(isSelectedPracticeView: boolean) {
    this.onTogglePracticeView.emit(isSelectedPracticeView);
  }

  // ---------------------------Component Unload--------------------------------------------//

  ngOnDestroy() {
    // unsubscribe to ensure no memory leaks
    this.clearSearch.unsubscribe();
  }
}
