import { Component, EventEmitter, OnInit, Output, ViewChild } from '@angular/core';
import { SortByComponent } from '../sort-by/sort-by.component';
import { FilterByComponent } from '../filter-by/filter-by.component';
import { UserPreferences } from 'src/app/shared/interfaces/userPreferences.interface';
import { UserPreferenceSupplyGroupViewModel } from 'src/app/shared/interfaces/userPreferenceSupplyGroupViewModel';
import { ResourceFiltersBasicMenu } from 'src/app/shared/interfaces/resource-filters-basic-menu.interface';
import { ResourceFilter } from 'src/app/shared/interfaces/resource-filter.interface';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { CoreService } from 'src/app/core/core.service';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { SavedResourceFiltersComponent } from '../saved-resource-filters/saved-resource-filters.component';
import { SupplySettingsComponent } from 'src/app/shared/staffing-settings/supply-settings/supply-settings.component';
import { SortRow } from 'src/app/shared/interfaces/sort-row.interface';
import { ValidationService } from 'src/app/shared/validationService';
import { FilterRow } from 'src/app/shared/interfaces/filter-row.interface';
import { UserPreferenceGroupFilters } from 'src/app/shared/interfaces/UserPreferenceGroupFilters';
import { UserPreferencesService } from 'src/app/core/user-preferences.service';
import { UserPreferenceSupplyGroupSharedInfoViewModel } from 'src/app/shared/interfaces/userPrefernceSupplyGroupSharedInfoViewModel';
import { ShareGroupComponent } from 'src/app/shared/staffing-settings/share-group/share-group.component';
import * as fromResources from 'src/app/resources/State/resources.reducer';
import { select, Store } from '@ngrx/store';
import { ResourceViewCD } from 'src/app/shared/interfaces/resource-view-cd.interface';
import { Subscription } from 'rxjs';
import { ResourceViewCommercialModel } from 'src/app/shared/interfaces/resource-view-commercial-model.interface';

@Component({
  selector: 'app-saved-group-modal',
  templateUrl: './saved-group-modal.component.html',
  styleUrls: ['./saved-group-modal.component.scss']
})
export class SavedGroupModalComponent implements OnInit {

  @Output() upsertResourceFilter = new EventEmitter();

  // ViewChild(s)
  @ViewChild("savedGroupSortBy", { static: false }) sortByComponent!: SortByComponent;
  @ViewChild("savedGroupFilterBy", { static: false }) filterByComponent!: FilterByComponent;
  @ViewChild("savedGroupSupplySetting", { static: false }) supplySettingsComponent: SupplySettingsComponent;
  @ViewChild("savedGroupFilterSettings", { static: false }) savedResourceFiltersComponent!: SavedResourceFiltersComponent;
  @ViewChild('shareGroup', { static: false }) shareGroupComponent: ShareGroupComponent; 

  // Validation
  isPageOneValid: boolean = true;
  isPageTwoValid: boolean = true;
  isFormValid: boolean = true;

  // Variables
  modalTitle: string;
  errorMessage: string[] = [];
  groupToEdit: any;
  isEditMode: boolean = false;

  sharedWithMembers: UserPreferenceSupplyGroupSharedInfoViewModel[];
  userPreferences: UserPreferences;
  loggedInUserHomeOffice;
  storeSub: Subscription = new Subscription();
  resourcesRecentCDList: ResourceViewCD[] = [];
  resourcesCommercialModelList:ResourceViewCommercialModel[] = [];

  onSecondPage: boolean = false;
  showSpecificUserSearch: boolean = false;

  sharingMenuOptions: ResourceFiltersBasicMenu[] = [
    { label: "Everyone", value: 1, selected: false },
    { label: "Specific User", value: 2, selected: false },
    { label: "Private", value: 3, selected: false }
  ];

  // Temporary resource filters
  resourceFilterGroup: ResourceFilter;
  filteredItems: ResourceFilter[];

  sortRows: SortRow[] = [];
  filterBy: FilterRow[] = [];

  validationMessages = {
    title: 'Please add a Group Name',
    titleLength: 'Group Name cannot be more than 100 characters in length!',
    office: 'Please select atleast one office!',
    staffingTags: 'Please select atleast one staffing tag!'
  }

  constructor(public modalRef: BsModalRef, private coreService: CoreService, private localStorageService: LocalStorageService, private userPreferencesService: UserPreferencesService,
    private store: Store<fromResources.State>
   ) { }

  ngOnInit(): void {
    // Set sharing
    this.resourceFilterGroup = {} as ResourceFilter;
    this.getResourcesRecentCDListFromStore();
    this.getResourcescommercialModelListFromStore();

    if (this.isEditMode == true) {

      this.resourceFilterGroup = this.groupToEdit;
      this.modalTitle = !!this.resourceFilterGroup.title ? `Edit "${this.resourceFilterGroup.title}"` : "Create New Saved Group";
      this.setSortRowOptions(this.groupToEdit.resourcesTabSortBy);
      this.sharedWithMembers = this.resourceFilterGroup.sharedWith;
    } else {
      this.modalTitle = "Create New Saved Group";
      this.sharedWithMembers = [];
    }

    this.loggedInUserHomeOffice = this.coreService.loggedInUser.office;
  }

  private getResourcesRecentCDListFromStore() {
    this.storeSub.add(
      this.store
        .pipe(select(fromResources.getResourcesRecentCDList))
        .subscribe((resourceRecentCDList: ResourceViewCD[]) => {
          this.resourcesRecentCDList = resourceRecentCDList;
        })
    );
  }
  private getResourcescommercialModelListFromStore() {
    this.storeSub.add(
      this.store
        .pipe(select(fromResources.getResourcesCommercialModelList))
        .subscribe((resourcesCommercialModelList: ResourceViewCommercialModel[]) => {
          this.resourcesCommercialModelList = resourcesCommercialModelList;
        })
    );
  }

  setSortRowOptions(sortBy = '') {
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
          this.sortRows.push(option);
        }
      });
    }
  }

  // setFilterRowOptions(filterBy = '') {
  //   if (!!filterBy) {
  //     const options: string[] = filterBy.split(',');

  //     options.forEach(x => {
  //       if (!!x) {
  //         const field = x.split('|')[0];
  //         const operator = x.split('|')[1];
  //         const value = x.split('|')[2];
  //         const andOr = x.split('|')[3];
  //         const option: FilterRow = {
  //           filterField: field,
  //           filterOperator: operator,
  //           andOr: andOr,
  //           filterValue: value
  //         }
  //         this.filterBy.push(option);
  //       }
  //     });
  //   }
  // }

  // Go to next page
  nextPage() {
    this.errorMessage = [];
    this.isTitleValid();

    if (this.errorMessage.length == 0) {
      this.isPageOneValid = true;
      this.onSecondPage = true;
    }
    else {
      this.isPageOneValid = false;
      this.onSecondPage = false;
    }
  }

  // NOTE: Commented as this code is used for sharing the saved group, i.e., a functionality that we will work on in the future PBIs
  // Add users to members or sharing list
  // onSearchItemSelectHandler(selectedUser: Resource) {
  //   this.errorMessage = [];

  //   const groupMemberToAdd: UserPreferenceSupplyGroupMember = {
  //     employeeCode: selectedUser.employeeCode,
  //     employeeName: selectedUser.fullName,
  //     currentLevelGrade: selectedUser.levelGrade,
  //     positionName: selectedUser.position?.positionName,
  //     operatingOfficeAbbreviation: selectedUser.schedulingOffice?.officeAbbreviation
  //   }

  //   if (!(this.savedGroup.sharedMembers.some(x => x.employeeCode == selectedUser.employeeCode))) {
  //     this.savedGroup.sharedMembers.push(groupMemberToAdd);
  //   }
  //   else {
  //     this.errorMessage.push(`"${selectedUser.fullName}" is already present in the shared list.`);
  //   }
  // }

  // Remove user from list
  // deleteMemberFromGroupHandler(userToRemove: UserPreferenceSupplyGroupMember) {
  //   this.savedGroup.sharedMembers.splice(
  //     this.savedGroup.sharedMembers.findIndex(X => X.employeeCode === userToRemove.employeeCode),
  //     1
  //   );
  // }

  // Select sharing option
  // onSharingSelectionHandler(selectedSharingOption: ResourceFiltersBasicMenu) {
  //   this.sharingMenuOptions.forEach((option) => {
  //     if (option.value == selectedSharingOption.value) {
  //       option.selected = true;
  //       this.savedGroup.sharingOption = option.value;
  //     } else {
  //       option.selected = false;
  //     }
  //   });

  //   if (selectedSharingOption.value == 2) {
  //     this.showSpecificUserSearch = true;
  //   } else {
  //     this.showSpecificUserSearch = false;
  //   }
  // }

  isFormValidated() {
    this.isTitleValid();
    this.isOfficesValid();
    this.isStaffingTagsValid();
    this.checkSortAndFilterValidations();

    return this.errorMessage.length === 0;
  }

  isTitleValid() {
    if (!this.resourceFilterGroup.title) {
      this.errorMessage.push(this.validationMessages.title);
    }
    else {
      this.errorMessage = this.errorMessage.filter(x => x !== this.validationMessages.title)
    }
    if (this.resourceFilterGroup.title && this.resourceFilterGroup.title.length > 100) {
      this.errorMessage.push(this.validationMessages.titleLength);
    }
    else {
      this.errorMessage = this.errorMessage.filter(x => x !== this.validationMessages.titleLength)
    }
  }

  isOfficesValid() {
    if (!this.savedResourceFiltersComponent.selectedOfficeList.toString()) {
      this.errorMessage.push(this.validationMessages.office);
    }
    else {
      this.errorMessage = this.errorMessage.filter(x => x !== this.validationMessages.office)
    }
  }

  isStaffingTagsValid() {
    if (!this.savedResourceFiltersComponent.selectedStaffingTagList.toString()) {
      this.errorMessage.push(this.validationMessages.staffingTags);
    }
    else {
      this.errorMessage = this.errorMessage.filter(x => x !== this.validationMessages.staffingTags);
    }
  }

  checkSortAndFilterValidations() {
    if(this.sortByComponent.sortRows.some( x=> !x.field)){
      this.errorMessage.push(ValidationService.sortByValidationMessage);
    }else{
      this.errorMessage = this.errorMessage.filter(x => x !== ValidationService.sortByValidationMessage);
    }

    if(this.filterByComponent.filterBy.some( x=> !x.filterField) || this.filterByComponent.filterBy.some( x=> ValidationService.isNullEmptyOrUndefined(x.filterValue)) ||  this.filterByComponent.filterBy.some( x=> !x.filterOperator)){
      this.errorMessage.push(ValidationService.filterByValidationMessage);
    }else{
      this.errorMessage = this.errorMessage.filter(x => x !== ValidationService.filterByValidationMessage);
    }
  
  }

  upsertFiltersForLoggedInEmployee(saveNew = true) {
    if (!this.isFormValidated()) {
      return;
    }
    if (this.resourceFilterGroup.title !== "") {
      const resourceFiltersData: ResourceFilter[] = [];

      const resourceFilter: ResourceFilter = {
        id: saveNew ? null : this.resourceFilterGroup?.id,
        title: this.resourceFilterGroup.title || '',
        description: this.resourceFilterGroup.description,
        isDefault: this.resourceFilterGroup.isDefault == true ? true : false,
        officeCodes: this.savedResourceFiltersComponent.selectedOfficeList.toString(),
        staffingTags: this.savedResourceFiltersComponent.selectedStaffingTagList.toString(),
        levelGrades: this.savedResourceFiltersComponent.selectedLevelGradeList.toString(),
        employeeStatuses: this.savedResourceFiltersComponent.selectedEmployeeStatusList.toString(),
        practiceAreaCodes: this.savedResourceFiltersComponent.selectedPracticeAreaList.toString(),
        resourcesTabSortBy: this.setResourcesTabSortByValue(this.sortByComponent.sortRows),
        filterBy: this.setResourcesTabFilterByValue(this.filterByComponent.filterBy, this.resourceFilterGroup),
        lastUpdatedBy: null,
        staffableAsTypeCodes: this.savedResourceFiltersComponent.selectedStaffableAsList.toString(),
        positionCodes: this.savedResourceFiltersComponent.selectedPositionList.toString(),
        affiliationRoleCodes: this.savedResourceFiltersComponent.selectedRoleNameList.toString(),
        sharedWith: this.shareGroupComponent.sharedWith
      }

      resourceFiltersData.push(resourceFilter);
      this.upsertResourceFilters(resourceFiltersData);
      this.modalRef.hide();
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

  upsertResourceFilters(resourceFiltersData: ResourceFilter[]) {
    this.upsertResourceFilter.emit(resourceFiltersData);
  }
}
