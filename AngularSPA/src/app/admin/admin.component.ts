import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { AppInsightsService } from '../app-insights.service';
import { CoreService } from '../core/core.service';
import { ConstantsMaster } from '../shared/constants/constantsMaster';
import { GoogleAnalytics } from '../shared/google-analytics/googleAnalytics';
import { SecurityTypeForAdminTabEnum } from '../shared/constants/enumMaster';
import { Subject, debounceTime } from 'rxjs';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { AddSecurityUserFormComponent } from '../shared/add-security-user-form/add-security-user-form.component';
import { SecurityGroup } from '../shared/interfaces/securityGroup';
import { UserListComponent } from './users-list/users-list.component';
import { GroupListComponent } from './groups-list/groups-list.component';
import { PracticeRingfencesComponent } from './practice-ringfences/practice-ringfences.component';
import { SystemconfirmationFormComponent } from '../shared/systemconfirmation-form/systemconfirmation-form.component';
import { SharedNotesService } from '../core/services/shared-notes-info.service';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss']
})
export class AdminComponent implements OnInit, OnDestroy {
  private searchFilter$: Subject<string> = new Subject();
  public bsModalRef: BsModalRef;
  selectedSecurityType: string = SecurityTypeForAdminTabEnum.USER;
  sharedNotes = [];
  caseIntakeAlerts=[];

  
  @ViewChild(UserListComponent, { static: false }) usersListComponent: UserListComponent;
  @ViewChild(GroupListComponent, { static: false }) groupsListComponent: GroupListComponent;
  @ViewChild('staffingUsersSearchInput', { static: false }) staffingUsersSearchInput: ElementRef;
  
  constructor(private modalService: BsModalService, private coreService: CoreService, private appInsightsService: AppInsightsService, private sharedNotesService: SharedNotesService) { }
  
  ngOnInit() {
    GoogleAnalytics.staffingTrackPageView(this.coreService.loggedInUser.employeeCode, ConstantsMaster.appScreens.page.admin, '');
    this.appInsightsService.logPageView(this.coreService.loggedInUser.employeeCode, ConstantsMaster.appScreens.page.admin);

    this.attachSearchFilterSubsciption();
    this.getSharedNotesInfo(this.coreService.loggedInUser.employeeCode);
          
  }

  private getSharedNotesInfo(loggedInEmployeeCode){
    this.sharedNotesService.getSharedNotes(loggedInEmployeeCode).subscribe(notes => {
      this.sharedNotes = notes;
    });
  }

  onSecurityTypeToggled(selectedValue: string){
    this.selectedSecurityType = selectedValue;
    this.searchFilter$.next('');
    this.staffingUsersSearchInput.nativeElement.value = '';
  }

  searchFilterHandler(event) {
    this.searchFilter$.next(event.target.value);
  }

  private attachSearchFilterSubsciption() {
    this.searchFilter$.pipe(
      debounceTime(500)
    ).subscribe(searchTextValue => {
      this.filterUsersBySearchString(searchTextValue);
    });
  }

  private filterUsersBySearchString(searchTextValue) {
    if(this.selectedSecurityType === SecurityTypeForAdminTabEnum.USER){
      this.usersListComponent.filterUsersBySearchString(searchTextValue);
    }else{
      this.groupsListComponent.filterGroupsBySearchString(searchTextValue);
    }
  }

  onAddButtonClick() {
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        existingUsersList: this.usersListComponent.securityUsersDetails,
        existingGroupsList: this.groupsListComponent.securityGroups,
        headerText: this.selectedSecurityType === SecurityTypeForAdminTabEnum.USER ? 'Add Security User' : 'Add Security Group',
        primaryBtnText: 'Add',
        selectedSecurityType: this.selectedSecurityType
      }
    };

    this.bsModalRef = this.modalService.show(AddSecurityUserFormComponent, config);
    this.bsModalRef.content.addSelectedUserEventEmitter.subscribe((userData) => {
      this.usersListComponent.addUserHandler(userData);
    });

    this.bsModalRef.content.addSelectedGroupEventEmitter.subscribe((groupData: SecurityGroup) => {
      this.groupsListComponent.addGroupHandler(groupData);
    });
  }

  openAddRingfenceModal() {
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true
    };

    this.bsModalRef = this.modalService.show(PracticeRingfencesComponent, config);
    this.bsModalRef.content.upsertPracticeBasedRingfenceEmitter.subscribe((selectedPracticeBaseRinfence) => {
      this.usersListComponent.upsertPracticeBasedRingfence(selectedPracticeBaseRinfence);
    });
  }

  openDeleteConfirmationPopUp(deleteObj) {
    const {deleteConfirmationMessage, idToDelete}: {deleteConfirmationMessage: string, idToDelete: string} = deleteObj;
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        confirmationPopUpBodyMessage: deleteConfirmationMessage
      }
    };

    this.bsModalRef = this.modalService.show(SystemconfirmationFormComponent, config);
    this.bsModalRef.content.deleteResourceNote.subscribe(() => {
      if(this.selectedSecurityType === SecurityTypeForAdminTabEnum.USER){
        this.usersListComponent.confirmDeleteSecurityUser(idToDelete);
      }else{
        this.groupsListComponent.deleteStaffingGroup(idToDelete);
      }
      
    });
  }


  //------------------------ Destroy ------------------------
  ngOnDestroy() {
    this.searchFilter$.complete();
    // this.storeSub.unsubscribe();
  }
}
