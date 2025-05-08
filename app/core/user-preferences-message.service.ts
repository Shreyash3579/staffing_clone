import { Injectable } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
import { UserPreferenceSupplyGroupSharedInfoViewModel } from '../shared/interfaces/userPrefernceSupplyGroupSharedInfoViewModel';
import { UserPreferenceSupplyGroupViewModel } from '../shared/interfaces/userPreferenceSupplyGroupViewModel';

@Injectable({
  providedIn: 'root'
})
export class UserPreferencesMessageService {
  private userPreferencesSupplyGroups : Subject<UserPreferenceSupplyGroupViewModel[]> = new Subject(); // used as Subject since we don't need initial value
  private userPreferencesSupplyGroupsSharedInfo : BehaviorSubject<UserPreferenceSupplyGroupSharedInfoViewModel[]> = new BehaviorSubject([] as UserPreferenceSupplyGroupSharedInfoViewModel[]);
  constructor() { }

  triggerUserPreferencesSupplyGroupsRefresh(upsertedData: UserPreferenceSupplyGroupViewModel[]) {
    this.userPreferencesSupplyGroups.next(upsertedData);
  }

  refreshUserPreferencesSupplyGroups() {
    return this.userPreferencesSupplyGroups.asObservable();
  }

  triggerUserPreferencesSupplyGroupsSharedInfoRefresh(upsertedData: UserPreferenceSupplyGroupSharedInfoViewModel[]) {
    this.userPreferencesSupplyGroupsSharedInfo.next(upsertedData);
  }

  refreshUserPreferencesSupplyGroupsSharedInfo() {
    return this.userPreferencesSupplyGroupsSharedInfo.asObservable();
  }
}
