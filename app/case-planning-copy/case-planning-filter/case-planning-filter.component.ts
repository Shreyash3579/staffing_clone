import { Component, ElementRef, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { fromEvent, Subject } from 'rxjs';
import { debounceTime, map } from 'rxjs/operators';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { ResourcesSupplyFilterGroupEnum } from 'src/app/shared/constants/enumMaster';

// components

// services
import { LaunchDarklyService } from 'src/app/core/services/launch-darkly.service';
import { BsModalService } from 'ngx-bootstrap/modal';

// interfaces
import { Project } from 'src/app/shared/interfaces/project.interface';
import { UserPreferences } from 'src/app/shared/interfaces/userPreferences.interface';
import { KeyValue } from 'src/app/shared/interfaces/keyValue.interface';
import { UserPreferenceSupplyGroupViewModel } from 'src/app/shared/interfaces/userPreferenceSupplyGroupViewModel';
import { ResourceFilter } from 'src/app/shared/interfaces/resource-filter.interface';
import { DropdownFilterOption } from 'src/app/shared/interfaces/dropdown-filter-option';

@Component({
  selector: 'app-case-planning-filter',
  templateUrl: './case-planning-filter.component.html',
  styleUrls: ['./case-planning-filter.component.scss']
})
export class CasePlanningFilterComponent implements OnInit, OnDestroy {
  // inputs
  @Input() clearSearch: Subject<boolean>;
  @Input() searchedProjects: Subject<Project[]>;
  @Input() userPreferences: UserPreferences;
  @Input() demandTypes: KeyValue[] = [];

  @Input()
  set allSupplyDropdownOptions(value: DropdownFilterOption[]) {
    if (value && value.length) {
      this._allSupplyDropdownOptions = value;
      this.setChangesDone();
      this.loadDisplayingDropdown();
      this.setSelectedGroup();
    }
  }
  get allSupplyDropdownOptions() {
    return this._allSupplyDropdownOptions;
  }

  @Input()
  set supplyGroupPreferences(value: UserPreferenceSupplyGroupViewModel[]) {
    this._supplyGroupPreferences = this.filteredGroups = value;
    this.setChangesDone();
  }
  get supplyGroupPreferences() {
    return this._supplyGroupPreferences;
  }

  // outputs
  @Output() toggleFiltersSectionEmitter = new EventEmitter<any>();
  @Output() getProjectsBySearchString = new EventEmitter<any>();
  @Output() openProjectDetailsDialogFromTypeahead = new EventEmitter();
  @Output() getProjectsOnFilterChange = new EventEmitter();
  @Output() openCustomGroupModalEmitter = new EventEmitter();
  @Output() upsertResourceFilter = new EventEmitter();
  @Output() onSupplyGroupFilterChanged = new EventEmitter<any>();
  @Output() getProjectsOnGroupChange = new EventEmitter<any>();

  // template references
  @ViewChild('projectSearchInput', { static: true }) projectSearchInput: ElementRef;

  // local variables
  projectSearchInputFocus: Boolean;
  asyncProjectString = '';
  feature = ConstantsMaster.appScreens.feature;

  // demand types
  demandTypeList;
  selectedDemandTypeList = [];

  // viewing groups
  areChangesMade: boolean = false;

  filteredItems: ResourceFilter[];

  filteredGroups: UserPreferenceSupplyGroupViewModel[];
  _supplyGroupPreferences: UserPreferenceSupplyGroupViewModel[];

  STAFFING_SETTINGS_FILTER_ENUM = ResourcesSupplyFilterGroupEnum.STAFFING_SETTINGS;
  SAVED_GROUP_FILTER_ENUM = ResourcesSupplyFilterGroupEnum.SAVED_GROUP;
  CUSTOM_GROUP_FILTER_ENUM = ResourcesSupplyFilterGroupEnum.CUSTOM_GROUP;

  _allSupplyDropdownOptions: DropdownFilterOption[];

  displayGroups = [];
  tempActiveGroup = {
    type: "",
    group: {}
  };

  mockDisplayGroups = [
    {
      filterGroupId: "staffingSettings",
      filterGroupName: "STAFFING SETTINGS",
      items: [
        {
          filterGroupId: "staffingSettings",
          id: "staffingSettings",
          isDefault: true,
          isDefaultForResourcesTab: false,
          selected: true,
          text: "Staffing Settings",
          value: "Staffing Settings Value"
        }
      ]
    }
  ];

  constructor(private launchDarklyService: LaunchDarklyService, private modalService: BsModalService) { }

  ngOnInit() {
    this.attachEventToSearchBox();
    this.setDemandTypes();
    this.launchDarklyService.trackUser(); //used to save the logged in user in launchdarkly
  }

  // event handlers
  // setFeatureFlagVariationOnCasePlanningBoard(){
  //   if(this.launchDarklyService.isClientInitialized){
  //     this.showPlanningBoardFeature = this.launchDarklyService.getFeatureFlagVariation(FeatureFlagNames.CASE_PLANNING_WHITEBOARD);
  //   }else{
  //     this.launchDarklyService.waitForLDClientInitialization().then(() => {
  //       this.showPlanningBoardFeature = this.launchDarklyService.getFeatureFlagVariation(FeatureFlagNames.CASE_PLANNING_WHITEBOARD);
  //     });
  //   }

  // }

  toggleFiltersSection() {
    this.toggleFiltersSectionEmitter.emit();
  }

  // helper functions
  attachEventToSearchBox() {
    this.clearSearch.subscribe(value => {
      this.clearSearchBox(value);
    });

    fromEvent(this.projectSearchInput.nativeElement, 'keyup').pipe(
      map((event: any) => {
        return event.target.value;
      }),
      debounceTime(500)
      // ,distinctUntilChanged() //removing this as it was craeting testin to and fro resources
    ).subscribe((text: string) => {
      this.searchProjects(text);
    });
  }

  clearSearchBox(clearSearchOnly) {
    this.asyncProjectString = ''
    this.projectSearchInput.nativeElement.value = '';
    if (!clearSearchOnly) {
      this.searchProjects();
    }
  }

  searchProjects(searchText = '') {
    this.getProjectsBySearchString.emit({ typeahead: searchText });
  }

  typeaheadOnSelect(data) {
    this.clearSearchBox(true);
    this.openProjectDetailsDialogFromTypeahead.emit(data.item);
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
  }

  getProjectsByDemandTypes(typeNames: string) {
    if (typeNames && this.isArrayEqual(this.selectedDemandTypeList.map(String), typeNames.split(','))) {
      return false;
    }

    this.selectedDemandTypeList = typeNames.split(',');

    const demandTypes = this.selectedDemandTypeList.toString();
    this.getProjectsOnFilterChange.emit(demandTypes);
  }

  toggleCasePlanningGroupHandler(event) {
    this.getProjectsOnGroupChange.emit(event);
  }

  private isArrayEqual(array1, array2) {
    return JSON.stringify(array1) === JSON.stringify(array2);
  }

  // viewing groups
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

  setSelectedGroup() {
    if (!this.tempActiveGroup || !this.tempActiveGroup.group || !this.tempActiveGroup.type) {

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

  isArrayExistAndHasValues(array) {
    return !!array && array.length > 0;
  }

  setSelectedViewingFilter(selectedFilterItem) {
    const selectedId = selectedFilterItem.id;

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
  }

  toggleSupplyGroupingSelection(event) {
    this.onSupplyGroupFilterChanged.emit(event);
  }

  // handleCreateMoreSelection(groupType) {
  //   switch (groupType) {
  //     case "customGroup":
  //       this.openCustomGroupModalEmitter.emit({ isEditMode: false, groupToEdit: null });
  //       break;
  //     case "savedGroup":
  //       this.handleCreateNewSavedGroup();
  //       break;
  //   }
  // }

  // handleCreateNewSavedGroup() {
  //   const modalRef = this.modalService.show(SavedGroupModalComponent, {
  //     backdrop: false,
  //     class: "saved-group-modal modal-dialog-centered"
  //   });

  //   modalRef.content.upsertResourceFilter.subscribe((group) => {
  //     this.upsertResourceFilterHandler(group);
  //   });
  // }

  upsertResourceFilterHandler(resourceFiltersData) {
    this.upsertResourceFilter.emit(resourceFiltersData)
  }

  // Handle group editing
  // handleEditGroupSelection(filterItemToEdit, secondPage: false) {
  //   if (filterItemToEdit.filterGroupId == this.CUSTOM_GROUP_FILTER_ENUM) {
  //     let groupToEdit = this.filteredGroups.find(x => x.id == filterItemToEdit.id);
  //     this.openCustomGroupModalEmitter.emit({ isEditMode: true, groupToEdit })
  //   } else {
  //     let groupToEdit = this.filteredItems.find(x => x.id == filterItemToEdit.id);
  //     this.openSavedGroupModal(true, groupToEdit, secondPage);
  //   }
  // }

  // openSavedGroupModal(isEditMode: boolean = false, groupToEdit: ResourceFilter = null, secondPage: boolean = false) {
  //   const modalRef = this.modalService.show(SavedGroupModalComponent, {
  //     backdrop: false,
  //     class: "saved-group-modal modal-dialog-centered",
  //     initialState: {
  //       isEditMode: isEditMode,
  //       groupToEdit: groupToEdit,
  //       onSecondPage: secondPage
  //     }
  //   });

  //   modalRef.content.upsertResourceFilter.subscribe((upsertedSavedGroupFilter) => {
  //     this.setChangesDone();
  //     this.upsertResourceFilterHandler(upsertedSavedGroupFilter);
  //   });
  // }

  setChangesDone() {
    this.areChangesMade = false;
  }

  // component unload
  ngOnDestroy() {
    // unsubscribe to ensure no memory leaks
    this.clearSearch.unsubscribe();
  }

}