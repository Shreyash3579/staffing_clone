import { Component, EventEmitter, OnDestroy, OnInit, Output } from '@angular/core';
import { Store, select } from '@ngrx/store';
// --------------------------Redux Component -----------------------------------------//
import * as adminActions from '../State/admin.actions';
import * as fromAdmin from '../State/admin.selectors';
import * as adminState from '../State/admin.state';
import { SecurityGroup, SecurityGroupDetails } from 'src/app/shared/interfaces/securityGroup';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { SecurityRole } from 'src/app/shared/interfaces/securityRole.interface';
import { Subscription, filter } from 'rxjs';

@Component({
  selector: 'app-groups-list',
  templateUrl: './groups-list.component.html',
  styleUrls: ['./groups-list.component.scss']
})
export class GroupListComponent implements OnInit, OnDestroy{
  securityRoles: SecurityRole[] = [];
  public storeSub: Subscription = new Subscription();
  public securityGroups: SecurityGroupDetails[] = [];
  public filteredGroups: SecurityGroupDetails[] = [];
  
  @Output() deleteGroupEmitter = new EventEmitter();

  constructor(private store: Store<adminState.State>,
    private localStorageService: LocalStorageService
  ) { }

  //-------------------------Life Cycle Events----------------------------------------------------------//
  ngOnInit() {
    this.getLookUpdataFromLocalStorage();
    // this.loadGridConfigurations();
    this.setStoreSubscription();
    this.getSecurityGroupDetails();
  }

  getLookUpdataFromLocalStorage() {
    this.securityRoles = this.localStorageService.get(ConstantsMaster.localStorageKeys.securityRoles);
  }

  private getSecurityGroupDetails() {
    // this.showHideStaffingUsersLoader(true);
    this.store.dispatch(
      new adminActions.LoadStaffingGroups()
    );
  }

  private setStoreSubscription() {
    this.setLoadStaffingGroupsSusbscription();
    // this.setLoaderSubscription();
  }

  private setLoadStaffingGroupsSusbscription() {
    this.storeSub.add(this.store.pipe(
      select(fromAdmin.getStaffingGroups),
      filter((staffingGroups: SecurityGroup[]) => staffingGroups !== null)
    ).subscribe((staffingGroups: SecurityGroupDetails[]) => {
      this.securityGroups = this.sortSecurityGroupsByDate(staffingGroups);
      this.securityGroups = this.mapRoleNamesToRoleCodes(this.securityGroups);

      this.filteredGroups = this.securityGroups;
      // this.rowData=this.filteredUsers;
    }));

  }

  mapRoleNamesToRoleCodes(securityGroups: SecurityGroupDetails[]){
    securityGroups.forEach(group => {
      if(group.roleCodes != null && group.roleCodes != ""){
      group.roleNames = group.roleCodes.split(",").map(roleCode => {
        return  this.securityRoles.find(role => role.roleCode === parseInt(roleCode)).roleName;
      }).join(", ");
    }
    });

    return securityGroups;
  }

  // private setLoaderSubscription() {
  //   this.storeSub.add(this.store.pipe(
  //     select(fromAdmin.staffingUsersLoader)
  //   ).subscribe((staffingUsersLoader: boolean) => {
  //     this.showProgressBar = staffingUsersLoader;
  //   }));
  // }
  
 
 

  private addNewStaffingGroup(staffingGroupToUpsert: SecurityGroup) {
    this.store.dispatch(
      new adminActions.UpsertSecurityGroup(staffingGroupToUpsert)
    );

  }

  onDeleteStaffingGroupClick(groupTodelete: SecurityGroupDetails) {
    const confirmationPopUpBodyMessage = 'Are you sure you want to delete group "' + groupTodelete.groupName + '" from the database ?';
    this.deleteGroupEmitter.emit({deleteConfirmationMessage: confirmationPopUpBodyMessage, idToDelete: groupTodelete.id});

  }

  private sortSecurityGroupsByDate(securityGroups: SecurityGroupDetails[]) {
    return securityGroups.sort((a, b) => <any>new Date(b.lastUpdated) - <any>new Date(a.lastUpdated));
  }

  //---------------------------Methods called directly from parent-------------------------------------------
  public filterGroupsBySearchString(searchTextValue) {
    if (searchTextValue.length < 1) {
      this.filteredGroups = this.securityGroups;
    }else{
      this.filteredGroups = this.securityGroups.filter(group => {
        if (!!group.groupName) {
          return group.groupName.toLowerCase().includes(searchTextValue.trim().toLowerCase());
        }
      });
    }

    // this.rowData = this.filteredUsers;
  }

  public addGroupHandler(securityGroupData: SecurityGroup) {
    this.addNewStaffingGroup(securityGroupData);
  }
  
  public deleteStaffingGroup(idOfGroupTodelete: string) {
    this.store.dispatch(
      new adminActions.DeleteSaffingGroup(idOfGroupTodelete)
    );

  }
  //-------------------------Life Cycle Events----------------------------------------------------------//
  ngOnDestroy() {
    this.storeSub.unsubscribe();
  }
}
