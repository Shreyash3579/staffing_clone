import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { forkJoin, Observable } from 'rxjs';
import { ConstantsMaster } from '../shared/constants/constantsMaster';
import { UserPreferences } from '../shared/interfaces/userPreferences.interface';
import { UserPreferenceSupplyGroup } from '../shared/interfaces/userPreferenceSupplyGroup';
import { UserPreferenceSupplyGroupSharedInfo } from '../shared/interfaces/UserPreferenceSupplyGroupSharedInfo';
import { UserPreferenceSupplyGroupViewModel } from '../shared/interfaces/userPreferenceSupplyGroupViewModel';
import { UserPreferenceSupplyGroupSharedInfoViewModel } from '../shared/interfaces/userPrefernceSupplyGroupSharedInfoViewModel';
import { CoreService } from './core.service';

@Injectable()
export class UserPreferencesService {

  constructor(private coreService : CoreService, 
      private http: HttpClient) { }


  saveAllUserPreferences(allUserPreferencesToSave): Observable<any>{
    return forkJoin([
      this.upsertUserPreferences(allUserPreferencesToSave.userPreferences),
      this.upsertUserPreferencesSupplyGroupsWithSharedInfo(allUserPreferencesToSave.userPreferencesSupplyGroups),
      this.deleteUserPreferenceSupplyGroupByIds(allUserPreferencesToSave.deletedGroupIds)
    ]);
  } 

  upsertUserPreferences(userPreferences: UserPreferences): Observable<UserPreferences> {
    userPreferences.employeeCode = this.coreService.loggedInUser.employeeCode;
    userPreferences.lastUpdatedBy = this.coreService.loggedInUser.employeeCode;

    return this.http.post<UserPreferences>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.upsertUserPreferences}`, userPreferences);
  }

  getUserPreferenceSupplyGroups(): Observable<UserPreferenceSupplyGroupViewModel[]> {
    const employeeCode = this.coreService.loggedInUser.employeeCode;
    return this.http.get<UserPreferenceSupplyGroupViewModel[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.getUserPreferenceSupplyGroups}?employeeCode=${employeeCode}`);
  }

  upsertUserPreferencesSupplyGroupsWithSharedInfo(supplyGroupsToUpsert: UserPreferenceSupplyGroup[] | UserPreferenceSupplyGroupViewModel[]): Observable<UserPreferenceSupplyGroupViewModel[]> {

    const dataToUpsert: UserPreferenceSupplyGroup[] = supplyGroupsToUpsert.map( x => {
      return {
        ...x,
        groupMemberCodes: x.groupMemberCodes || x.groupMembers?.map(x => x.employeeCode).join(",") || "",
        lastUpdatedBy : this.coreService.loggedInUser.employeeCode
      }
    }) ;

    return this.http.post<UserPreferenceSupplyGroupViewModel[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.upsertUserPreferencesSupplyGroupWithSharedInfo}`, dataToUpsert);
  }

  updateUserPreferenceSupplyGroupSharedInfo(dataToUpsert: UserPreferenceSupplyGroupSharedInfo[] | UserPreferenceSupplyGroupSharedInfoViewModel[]): Observable<UserPreferenceSupplyGroupSharedInfo[]>
  {
    return this.http.post<UserPreferenceSupplyGroupSharedInfo[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.updateUserPreferenceSupplyGroupSharedInfo}`, dataToUpsert);
  }

  upsertUserPreferencesSupplyGroupSharedInfo(supplyGroupSharedInfoToUpsert: UserPreferenceSupplyGroupSharedInfo[] | UserPreferenceSupplyGroupSharedInfoViewModel[]): Observable<UserPreferenceSupplyGroupSharedInfoViewModel[]>
  {
    const dataToUpsert: UserPreferenceSupplyGroupSharedInfo[] = supplyGroupSharedInfoToUpsert.map( x => {
      return {
        ...x,
        lastUpdatedBy : this.coreService.loggedInUser.employeeCode
      }
    }) ;
    return this.http.post<UserPreferenceSupplyGroupSharedInfoViewModel[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.upsertUserPreferenceSupplyGroupSharedInfo}`, dataToUpsert);
  }

  deleteUserPreferenceSupplyGroupByIds(listSupplyGroupIdsToDelete: string): Observable<string> {
    const lastUpdatedBy = this.coreService.loggedInUser.employeeCode;
    const requesParam = new HttpParams({
      fromObject: {
        'listSupplyGroupIdsToDelete': listSupplyGroupIdsToDelete,
        'lastUpdatedBy': lastUpdatedBy
      }
    });
    return this.http.delete<any>(`
          ${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.deleteUserPreferenceSupplyGroupByIds}`, { params: requesParam });
  }

  getUserPreferenceGroupSharedInfo(groupId: string) : Observable<UserPreferenceSupplyGroupSharedInfoViewModel[]>{
    return this.http.get<UserPreferenceSupplyGroupSharedInfoViewModel[]>(
      `${this.coreService.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.getUserPreferenceGroupSharedInfo}?groupId=${groupId}`);

  }
}
