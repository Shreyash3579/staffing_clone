import { Component, EventEmitter, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { UserPreferenceSupplyGroupViewModel } from 'src/app/shared/interfaces/userPreferenceSupplyGroupViewModel';
import { FilterByComponent } from '../filter-by/filter-by.component';
import { SortByComponent } from '../sort-by/sort-by.component';
import { UserPreferenceSupplyGroup } from 'src/app/shared/interfaces/userPreferenceSupplyGroup';
import { CoreService } from 'src/app/core/core.service';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { Subscription } from 'rxjs';
import { UserPreferencesMessageService } from 'src/app/core/user-preferences-message.service';
import { SortRow } from 'src/app/shared/interfaces/sort-row.interface';
import { ValidationService } from 'src/app/shared/validationService';
import { UserPreferenceGroupFilters } from 'src/app/shared/interfaces/UserPreferenceGroupFilters';
import { FilterRow } from 'src/app/shared/interfaces/filter-row.interface';
import { ShareGroupComponent } from 'src/app/shared/staffing-settings/share-group/share-group.component';
import { UserPreferenceSupplyGroupSharedInfoViewModel } from 'src/app/shared/interfaces/userPrefernceSupplyGroupSharedInfoViewModel';
import { CreateGroupComponent } from 'src/app/shared/staffing-settings/create-group/create-group.component';
import * as fromResources from 'src/app/resources/State/resources.reducer';
import { select, Store } from '@ngrx/store';
import { ResourceViewCD } from 'src/app/shared/interfaces/resource-view-cd.interface';
import { ResourceViewCommercialModel } from 'src/app/shared/interfaces/resource-view-commercial-model.interface';

@Component({
  selector: 'app-custom-group-modal',
  templateUrl: './custom-group-modal.component.html',
  styleUrls: ['./custom-group-modal.component.scss']
})
export class CustomGroupModalComponent implements OnInit, OnDestroy {

  // ViewChilds
  @ViewChild("customGroupFilterBy", { static: false }) filterByComponent!: FilterByComponent;
  @ViewChild("customGroupSortBy", { static: false }) sortByComponent!: SortByComponent;
  @ViewChild('createGroup', { static: false }) createGroupComponent: CreateGroupComponent;
  @ViewChild('shareGroup', { static: false }) shareGroupComponent: ShareGroupComponent; 

  // Outputs
  @Output() upsertUserPreferenceSupplyGroups = new EventEmitter<UserPreferenceSupplyGroup>();

  //--------------------Input Vairables passed from Parent------------------------------
  groupToEdit: any;
  isEditMode: boolean = false;
  
  // Variables
  modalTitle: string;
  isFormValid: boolean = true;
  sharedWithMembers: UserPreferenceSupplyGroupSharedInfoViewModel[];
  errorMessage: string[] = [];
  allGroupsArray: UserPreferenceSupplyGroupViewModel[] = [];
  customGroup: UserPreferenceSupplyGroupViewModel;
  subscription: Subscription = new Subscription();
  storeSub: Subscription = new Subscription();
  resourcesRecentCDList: ResourceViewCD[] = [];
  resourcesCommercialModelList:ResourceViewCommercialModel[] = [];
  constructor(
    private coreService: CoreService,
    public modalRef: BsModalRef,
    private store: Store<fromResources.State>,) { }

  ngOnInit(): void {
    // Set members
    this.customGroup = {} as UserPreferenceSupplyGroupViewModel;
    this.getResourcesRecentCDListFromStore();
    this.getResourcescommercialModelListFromStore();

    if (this.isEditMode == true) {
      this.customGroup = this.groupToEdit;
      this.modalTitle = this.customGroup.name ? `Edit "${this.customGroup.name}"` : "Create New Custom Group";
      this.customGroup.sortRows = this.getSortRowOptions(this.groupToEdit.sortBy);
      this.sharedWithMembers = this.customGroup.sharedWith;
    } else {
      this.modalTitle = "Create New Custom Group";
      this.customGroup.groupMembers = [];
      this.sharedWithMembers = [];
    }
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

  upsertPreferenceSupplyGroups() {
    if (!this.isGroupToUpsertValid()) {
      return;
    }

    //set sort by on selected group
    this.createGroupComponent.group.sortBy = this.convertSortByArrayToCommaSeparatedString(this.sortByComponent.sortRows);
    this.createGroupComponent.group.sortRows = this.sortByComponent.sortRows;

    this.createGroupComponent.group.filterBy = this.convertFilterByArrayToUserPreferenceGroupFiltersModel(this.filterByComponent.filterBy, this.createGroupComponent.group);
    this.createGroupComponent.group.sharedWith = this.shareGroupComponent.sharedWith;

    const supplyGroupDataToUpsert: UserPreferenceSupplyGroup = {
      id: this.createGroupComponent.group.id,
      name: this.createGroupComponent.group.name,
      description: this.createGroupComponent.group.description,
      isDefault: false,
      isDefaultForResourcesTab: this.createGroupComponent.group.isDefaultForResourcesTab ?? false,
      isShared: this.createGroupComponent.group.isShared ?? false,
      createdBy: this.createGroupComponent.group.id ? this.createGroupComponent.group.createdBy : this.coreService.loggedInUser.employeeCode,
      groupMemberCodes: this.createGroupComponent.group.groupMembers.map(x => x.employeeCode).join(","),
      sortBy: this.createGroupComponent.group.sortBy,
      filterBy: this.createGroupComponent.group.filterBy,
      sharedWith: this.createGroupComponent.group.sharedWith 
    }

    this.upsertUserPreferenceSupplyGroups.emit(supplyGroupDataToUpsert);

    this.modalRef.hide();
  }

  //--------------------Helper Functions------------------------------

  getSortRowOptions(sortBy = '') {
    const sortRows: SortRow[] = [];
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
          sortRows.push(option);
        }
      });
    }
    return sortRows;
  }

  isGroupToUpsertValid(){
    if(!this.createGroupComponent.validateGroup() || !this.isSortAndFilterByValid()){
      return false;
    }else{
      return true;
    }
  }

  isSortAndFilterByValid() {
    this.errorMessage = [];
    var isValid = true;
    
    if(this.sortByComponent.sortRows.some( x=> !x.field)){
      isValid = false;
      this.errorMessage.push(ValidationService.sortByValidationMessage);
    }

    if(this.filterByComponent.filterBy.some( x=> !x.filterField) || this.filterByComponent.filterBy.some( x=> ValidationService.isNullEmptyOrUndefined(x.filterValue)) ||  this.filterByComponent.filterBy.some( x=> !x.filterOperator)){
      isValid = false;
      this.errorMessage.push(ValidationService.filterByValidationMessage);
    }    
  
    return isValid;
  }

  convertSortByArrayToCommaSeparatedString(sortRows: SortRow[] = []) {
    let sortBy = '';
    sortRows.forEach(x => {
      sortBy += `${x.field}|${x.direction},`
    });
    sortBy = sortBy.slice(0, -1);

    return sortBy;
  }

  convertFilterByArrayToUserPreferenceGroupFiltersModel(filterRows: FilterRow[] = [], group) {
    let filterBy: UserPreferenceGroupFilters[] = [];
    filterRows.forEach(x => {
      const filterObj = {
        groupId: group.id,
        andOr: x.andOr,
        filterField: x.filterField,
        filterOperator: x.filterOperator,
        filterValue: x.filterValue
      } 
      filterBy.push(filterObj);
    })
    return filterBy;
  }

  //--------------------On Destroy------------------------------
  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}
