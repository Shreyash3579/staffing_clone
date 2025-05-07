// -------------------Angular Operators---------------------------------------//
import { Component, OnInit, ViewChild, Output, EventEmitter, OnDestroy } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

// -------------------Components---------------------------------------//
import { DemandSettingsComponent } from './demand-settings/demand-settings.component';
import { SupplySettingsComponent } from './supply-settings/supply-settings.component';
import { CreateGroupComponent } from 'src/app/shared/staffing-settings/create-group/create-group.component';

// -------------------Interfaces---------------------------------------//
import { UserPreferences } from 'src/app/shared/interfaces/userPreferences.interface';

// -------------------Services---------------------------------------//
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { PopupDragService } from 'src/app/shared/services/popupDrag.service';
import { UserPreferenceSupplyGroup } from 'src/app/shared/interfaces/userPreferenceSupplyGroup';
import { UserPreferenceSupplyGroupViewModel } from 'src/app/shared/interfaces/userPreferenceSupplyGroupViewModel';
import { GroupSettingsComponent } from './group-settings/group-settings.component';
import { Subscription } from 'rxjs';
import { ShareGroupComponent } from './share-group/share-group.component';
import { UserPreferenceSupplyGroupSharedInfoViewModel } from 'src/app/shared/interfaces/userPrefernceSupplyGroupSharedInfoViewModel';
import { UserPreferenceSupplyGroupSharedInfo } from 'src/app/shared/interfaces/UserPreferenceSupplyGroupSharedInfo';
import { UserPreferencesService } from 'src/app/core/user-preferences.service';
import { CoreService } from 'src/app/core/core.service';
import { UserPreferencesMessageService } from 'src/app/core/user-preferences-message.service';
import { ThemePalette } from '@angular/material/core';

@Component({
  selector: 'app-staffing-settings',
  templateUrl: './staffing-settings.component.html',
  styleUrls: ['./staffing-settings.component.scss'],
  providers: [PopupDragService]
})
export class StaffingSettingsComponent implements OnInit, OnDestroy {

  // Mock groups arrays
  // Remove when data is ready
  allGroupsArray: UserPreferenceSupplyGroupViewModel[] = [];
  // -------------------Local variables---------------------------------------//

  showDemandSettings = false;
  showCreateGroup = false;
  showShareGroup = false;
  supplyIsDefault = true;
  groupIsDefault = false;
  groupToEdit: UserPreferenceSupplyGroupViewModel;
  groupToShare: UserPreferenceSupplyGroupViewModel;
  sharedWithMembers: UserPreferenceSupplyGroupSharedInfoViewModel[];

  title = '';
  saveLabel = 'Save Changes';
  saveTypeCode = 1;
  userPreferences: UserPreferences;
  supplyGroups: UserPreferenceSupplyGroupViewModel[] = [];
  supplyGroupSharedInfo: UserPreferenceSupplyGroupSharedInfo[] = [];
  supplyGroupsToDelete: string = "";
  loggedInUserHomeOffice;
  subscription: Subscription = new Subscription();
  errorMessage: string[] = [];
  color:ThemePalette=  "warn";
  // -------------------View Child Refrences---------------------------------------//

  @ViewChild('supplySettings', { static: false }) supplySettingsComponent: SupplySettingsComponent;
  @ViewChild('demandSettings', { static: false }) demandSettingsComponent: DemandSettingsComponent;
  @ViewChild('createGroup', { static: false }) createGroupComponent: CreateGroupComponent;
  @ViewChild('shareGroup', { static: false }) shareGroupComponent: ShareGroupComponent;
  @ViewChild('groupSettings', { static: false }) groupSettingsComponent: GroupSettingsComponent;

  // -------------------Output Events---------------------------------------//

  @Output() saveUserPreferences = new EventEmitter<any>();
  @Output() upsertUserPreferenceSupplyGroupSharedInfo = new EventEmitter<any>();
  @Output() upsertUserPreferenceSupplyGroups = new EventEmitter<any>();
  @Output() deleteUserPreferenceSupplyGroupByIds = new EventEmitter<any>();
  @Output() saveAllUserPreferences = new EventEmitter<any>();

  // -------------------Constructor---------------------------------------//

  constructor(
    private bsModalRef: BsModalRef,
    private coreService: CoreService,
    private localStorageService: LocalStorageService,
    private _popupDragService: PopupDragService,
    private userPreferencesMessageService: UserPreferencesMessageService,
    private userPreferencesService: UserPreferencesService) { }

  // -------------------Life Cycle Events---------------------------------------//

  ngOnInit() {
    // show supply settings on load
    this.showDemandSettings = false;
    this.title = 'Supply Group Default Settings';

    this.loggedInUserHomeOffice = this.coreService.loggedInUser.office;

    // this._popupDragService.dragEvents();
    this.loadUserPreferences(); //TODO: move this to header
    this.setDefaultHeader();
    this.subscribeEvents();
  }


  loadUserPreferences() {
    this.userPreferences = JSON.parse(this.localStorageService.get(ConstantsMaster.localStorageKeys.userPreferences, 'weekly'));
    this.allGroupsArray = JSON.parse(this.localStorageService.get(ConstantsMaster.localStorageKeys.userPreferenceSupplyGroups, 'weekly'));
  }

  setSupplyAsDefault() {
    if (this.userPreferences.isDefault) {
      this.allGroupsArray.map(x => x.isDefault = false);

      if (this.groupSettingsComponent)
        this.groupSettingsComponent.defaultGroupArray.splice(0, 1);
    }


    this.setDefaultHeader();
  }

  setDefaultHeader() {
    this.supplyIsDefault = false;
    this.groupIsDefault = false;
    this.userPreferences.isDefault = false;

    if (this.allGroupsArray.some(x => x.isDefault)) {
      this.groupIsDefault = true;
    }
    else {
      this.userPreferences.isDefault = true;
      this.supplyIsDefault = true;
    }
  }

  onTabClick(event) {
    if (event.target.className.includes('mat-tab')) {
      let textValue = event.target.textContent.trim();
      if (textValue === 'Filters' || textValue === 'Filters (default)')
        this.resetLabelForSuppySettingForm();
    }
  }

  subscribeEvents() {
    this.subscription.add(this.userPreferencesMessageService.refreshUserPreferencesSupplyGroups().subscribe(result => {
      if (result?.length) {

        if (this.allGroupsArray.some(x => !x.id)) {
          const updatedData = this.allGroupsArray.find(x => !x.id);
          updatedData.id = result[0].id;
          updatedData.createdBy = result[0].createdBy;
        }

        this.allGroupsArray.forEach(group => {
          if(result.find(x => x.id === group.id)) {
            group.sharedWith = result.find(y => y.id === group.id)?.sharedWith;
            group.isShared = result.find(x => x.id === group.id)?.isShared;
          }
        })

        this.allGroupsArray.forEach(group => {
          if(result.find(x => x.id === group.id)) {
            group.isShared = result.find(x => x.id === group.id)?.isShared;
          }
        });
      }
    }));

    this.subscription.add(this.userPreferencesMessageService.refreshUserPreferencesSupplyGroupsSharedInfo().subscribe(result => {
      if(result?.length) {     
        this.allGroupsArray.forEach(group => {
          if(result.find(x => x.userPreferenceGroupId === group.id)) {
            group.sharedWith = result.filter(x => x.userPreferenceGroupId === group.id);
            group.isShared = group.sharedWith.length > 1 ?? false;
          }
        })
      }
    }))
  }
  // -------------------Event Handlers---------------------------------------//

  closeForm() {
    this.bsModalRef.hide();
  }

  showSupplySettingsForm() {
    this.resetLabelForSuppySettingForm();
    this.saveUserPreferencesForDemand();
  }

  resetLabelForSuppySettingForm() {
    this.showDemandSettings = false;
    this.showCreateGroup = false;
    this.title = 'Supply Group Default Settings';
    this.saveLabel = 'Save Changes';
    this.saveTypeCode = 1;
  }

  showGroupSettings() {
    this.showDemandSettings = false;
    this.showCreateGroup = false;
    this.showShareGroup = false;
    this.title = 'Supply Group Default Settings';
    this.saveLabel = 'Save Changes';
    this.saveTypeCode = 1;
  }

  showDemandSettingsForm() {
    this.saveUserPreferencesForSupply();
    // show demand settings on next click
    this.showDemandSettings = true;
    this.showCreateGroup = false;
    this.title = 'Demand Default Settings';
  }

  showCreateGroupForm() {
    this.showCreateGroup = true;
    this.groupToEdit = null;
    this.saveLabel = 'Save New Group';
    this.saveTypeCode = 2;
  }

  editGroup(group) {
    this.groupToEdit = group;
    this.showCreateGroup = true;
    this.saveLabel = 'Save Group';
    this.saveTypeCode = 3;
  }

  shareGroup(group) {
    this.groupToShare = group;
    this.groupToEdit = null;
    this.showShareGroup = true;
    this.saveLabel = 'Share Group';
    this.saveTypeCode = 4;
    this.userPreferencesService.getUserPreferenceGroupSharedInfo(this.groupToShare.id).subscribe(result =>
      this.sharedWithMembers = result);
  }

  saveUserPreferencesForSupply() {
    // set supply preferences in user preferences else it will be hidden on next click
    this.userPreferences.supplyViewOfficeCodes = this.supplySettingsComponent.selectedSupplyViewOfficeList.toString();
    this.userPreferences.supplyViewStaffingTags = this.supplySettingsComponent.selectedStaffingTagList.toString();
    this.userPreferences.levelGrades = this.supplySettingsComponent.selectedLevelGradeList.toString();
    this.userPreferences.availabilityIncludes = this.supplySettingsComponent.selectedAvailabilityIncludesList.toString();
    this.userPreferences.groupBy = this.supplySettingsComponent.selectedGroupByList.toString();
    this.userPreferences.sortBy = this.supplySettingsComponent.selectedSortByList.toString();
    this.userPreferences.practiceAreaCodes = this.supplySettingsComponent.selectedPracticeAreaCodes.toString();
    this.userPreferences.affiliationRoleCodes = this.supplySettingsComponent.selectedAffiliationRoleCodes.toString();
    this.userPreferences.positionCodes = this.supplySettingsComponent.selectedPositionList.toString();
    this.userPreferences.isDefault = this.userPreferences.isDefault;
    this.userPreferences.staffableAsTypeCodes = this.supplySettingsComponent.selectedStaffableAsList.toString();
  }

  saveUserPreferencesForDemand() {
    // set demand prefrences in user prefrences object
    this.userPreferences.demandViewOfficeCodes = this.demandSettingsComponent.selectedDemandViewOfficeList.toString();
    this.userPreferences.caseTypeCodes = this.demandSettingsComponent.selectedCaseTypeList.toString();
    this.userPreferences.caseAttributeNames = this.demandSettingsComponent.selectedStaffingTagDropdownList.toString();
    this.userPreferences.opportunityStatusTypeCodes = this.demandSettingsComponent.selectedOpportunityStatusTypeList.toString();
    this.userPreferences.demandTypes = this.demandSettingsComponent.selectedDemandTypeList.toString();
    this.userPreferences.caseAllocationsSortBy = this.demandSettingsComponent.selectedSortByItem.toString();
    this.userPreferences.industryPracticeAreaCodes = this.demandSettingsComponent.selectedIndustryPracticeAreaList.toString();
    this.userPreferences.capabilityPracticeAreaCodes = this.demandSettingsComponent.selectedCapabilityPracticeAreaList.toString();
  }

  saveUserPreferencesForSupplyGroups() {
    this.supplyGroups = [];

    this.groupSettingsComponent.allGroupsArray.forEach(x => {
      const supplyGroupsDataToUpsert: UserPreferenceSupplyGroupViewModel = {
        id: x.id,
        name: x.name,
        description: x.description,
        isDefault: x.isDefault,
        isDefaultForResourcesTab: x.isDefaultForResourcesTab,
        isShared: x.isShared ?? false,
        createdBy: x.id ? x.createdBy : this.coreService.loggedInUser.employeeCode,
        groupMembers: x.groupMembers,
        sharedWith: x.sharedWith,
        sortBy: x.sortBy,
        filterBy: x.filterBy
      }

      this.supplyGroups.push(supplyGroupsDataToUpsert);
    });

    if (this.groupSettingsComponent.deletedGroupIdArray) {
      this.supplyGroupsToDelete = this.groupSettingsComponent.deletedGroupIdArray.join(",");
    }
  }

  saveUserPreferenceSupplyGroupSharedInfo() {
    this.supplyGroupSharedInfo = [];
    var defaultGroup = this.groupSettingsComponent.allGroupsArray.find(x => x.isDefault);
    const groupsSharedInfoToUpsert: UserPreferenceSupplyGroupSharedInfo = {
      sharedWith: this.coreService.loggedInUser.employeeCode,
      userPreferenceGroupId: defaultGroup.id,
      lastUpdatedBy: this.coreService.loggedInUser.employeeCode
    }
    this.supplyGroupSharedInfo.push(groupsSharedInfoToUpsert);
  }

  saveSupplyAndGroups() {
    if (this.supplySettingsComponent) {
      this.saveUserPreferencesForSupply();
    }
    if (this.groupSettingsComponent) {
      this.saveUserPreferencesForSupplyGroups();
      if (this.groupSettingsComponent.allGroupsArray.some(x => x.isDefault)) {
        this.saveUserPreferenceSupplyGroupSharedInfo();
      }
    }
  }

  saveSettings() {
    this.setDefaultHeader();

    switch (this.saveTypeCode) {
      case 1: {
        if (!this.userPreferences.isDefault && !this.allGroupsArray.some(x => x.isDefault)) {
          this.errorMessage.push("Please select a default settings !");
          return;
        } else {
          this.errorMessage = [];
        }

        this.savePreferences();
        this.setStaffingSettingsLastUpdatedTimeStampInLocalStorage();
        break;
      }
      case 2: {
        //save new groups
        this.upsertPreferenceSupplyGroups();
        this.setStaffingSettingsLastUpdatedTimeStampInLocalStorage();
        break;
      }
      case 3: {
        //save edit groups
        this.upsertPreferenceSupplyGroups();
        this.setStaffingSettingsLastUpdatedTimeStampInLocalStorage();
        break;
      }
      case 4: {
        //share groups
        this.upsertPreferenceSupplyGroupSharedInfo();

      }
    }

  }

  setStaffingSettingsLastUpdatedTimeStampInLocalStorage() {
    //This is done so that tableau  reports load with latest staffing settings on load
    this.localStorageService.set(ConstantsMaster.localStorageKeys.userPreferencesLastUpdatedTimestamp, new Date().getTime());
  }

  savePreferences() {
    this.saveUserPreferencesForDemand();

    const allPreferencesToSave = {
      userPreferences: this.userPreferences,
      userPreferencesSupplyGroups: this.supplyGroups,
      userPreferenceSupplyGroupSharedInfo: this.supplyGroupSharedInfo,
      deletedGroupIds: this.supplyGroupsToDelete
    }

    this.saveAllUserPreferences.emit(allPreferencesToSave)
    this.closeForm();
  }

  upsertPreferenceSupplyGroupSharedInfo() {
    if (this.shareGroupComponent && this.shareGroupComponent.sharedWith.length > 0) {
      this.upsertUserPreferenceSupplyGroupSharedInfo.emit(this.shareGroupComponent.sharedWith);
    }
    else {
      this.deleteUserPreferenceSupplyGroupByIds.emit(this.groupToShare.id);
    }
    this.allGroupsArray.map(x => {
      if(x.id == this.shareGroupComponent.sharedWith[0].userPreferenceGroupId)
      {
        x.sharedWith = this.shareGroupComponent.sharedWith;
      }
    })
    this.showGroupSettings();
  }

  upsertPreferenceSupplyGroups() {
    if (!this.createGroupComponent.validateGroup()) {
      return;
    }

    if (this.createGroupComponent && this.createGroupComponent.group.isDefault) {
      if (this.groupSettingsComponent)
        this.groupSettingsComponent.defaultGroupArray.splice(0, 1, this.createGroupComponent.group);

      this.allGroupsArray.map(x => {
        if (x.id === this.createGroupComponent.group.id) {
          x.isDefault = this.createGroupComponent.group.isDefault;
        } else {
          x.isDefault = false;
        }
      });
    }

    if (this.groupSettingsComponent && this.groupSettingsComponent.defaultGroupArray
      && this.groupSettingsComponent.defaultGroupArray[0].id === this.createGroupComponent.group.id
      && this.createGroupComponent.group.isDefault) {

      this.groupSettingsComponent.defaultGroupArray.splice(0, 1);
    }

    if (this.createGroupComponent.group.id) {
      const indexOfGroup = this.allGroupsArray.findIndex(x => x.id === this.createGroupComponent.group.id);
      this.allGroupsArray.splice(
        indexOfGroup,
        1,
        this.createGroupComponent.group
      );
    } else {
      this.allGroupsArray.push(this.createGroupComponent.group);
    }

    const supplyGroupsDataToUpsert: UserPreferenceSupplyGroup = {
      id: this.createGroupComponent.group.id,
      name: this.createGroupComponent.group.name,
      description: this.createGroupComponent.group.description,
      isDefault: this.createGroupComponent.group.isDefault ?? false,
      isDefaultForResourcesTab: false,
      isShared: this.createGroupComponent.group.isShared ?? false,
      createdBy: this.createGroupComponent.group.id ? this.createGroupComponent.group.createdBy : this.coreService.loggedInUser.employeeCode,
      groupMemberCodes: this.createGroupComponent.group.groupMembers.map(x => x.employeeCode).join(","),
      sharedWith: this.createGroupComponent.group.sharedWith,
      sortBy: this.createGroupComponent.group.sortBy,
      filterBy: this.createGroupComponent.group.filterBy
    }

    this.setDefaultHeader();

    this.upsertUserPreferenceSupplyGroups.emit([].concat(supplyGroupsDataToUpsert));
    this.showGroupSettings();

  }

  defaultGroupHandler() {
    this.setDefaultHeader();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}
