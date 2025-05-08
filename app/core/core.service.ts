// -------------------------Angular Components--------------------------------------------
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, Subject, forkJoin } from 'rxjs';
import { map, switchMap, tap } from 'rxjs/operators';
import { of } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { TreeviewItem, TreeviewHelper } from '@soy-andrey-semyonov/ngx-treeview';

// -------------------------Interfaces--------------------------------------------
import { AppSettings } from '../shared/interfaces/appSettings.interface';
import { CaseRoleType } from '../shared/interfaces/caseRoleType.interface';
import { CaseType } from '../shared/interfaces/caseType.interface';
import { CommitmentType } from '../shared/interfaces/commitmentType.interface';
import { ConstantsMaster } from '../shared/constants/constantsMaster';
import { Employee } from '../shared/interfaces/employee.interface';
import { InvestmentCategory } from '../shared/interfaces/investmentCateogry.interface';
import { LevelGrade } from '../shared/interfaces/levelGrade.interface';
import { Office } from '../shared/interfaces/office.interface';
import { OfficeHierarchy } from '../shared/interfaces/officeHierarchy.interface';
import { OpportunityStatusType } from '../shared/interfaces/opportunityStatusType';
import { ServiceLine } from '../shared/interfaces/serviceLine.interface';
import { ServiceLineHierarchy } from '../shared/interfaces/serviceLineHierarchy';
import { SkuTerm } from '../shared/interfaces/skuTerm.interface';
import { UserPreferences } from '../shared/interfaces/userPreferences.interface';
import { UserNotification } from '../shared/interfaces/userNotification.interface';

// -------------------------Utils/Others--------------------------------------------
import { environment } from '../../environments/environment';

// -------------------------Services--------------------------------------------
import { LocalStorageService } from '../shared/local-storage.service';

// -------------------------ENUMS--------------------------------------------
import { CaseType as CaseTypeEnum } from '../shared/constants/enumMaster';
import { GroupBy as GroupByEnum } from '../shared/constants/enumMaster';
import { SortBy as SortByEnum } from '../shared/constants/enumMaster';
import { OpportunityStatusType as OpportunityStatusTypeEnum } from '../shared/constants/enumMaster';
import { ServiceLine as ServiceLineEnum } from '../shared/constants/enumMaster';
import { Certificate } from '../shared/interfaces/certificate.interface';
import { Language } from '../shared/interfaces/language';
import { PDGrade } from '../shared/interfaces/pdGrade.interface';
import { PracticeArea } from '../shared/interfaces/practiceArea.interface';
import { PositionHierarchy } from '../shared/interfaces/positionHierarchy.interface';
import { StaffableAsType } from '../shared/interfaces/staffableAsType.interface';
import { UserClaim } from '../shared/interfaces/userClaim.interface';
import { RingfenceManagement } from '../shared/interfaces/ringfenceManagement.interface';
import { CommonService } from '../shared/commonService';
import { OfficeClosureCases } from '../shared/interfaces/office-closure-cases.interface';
import { CasePlanningBoardBucket } from '../shared/interfaces/case-planning-board-bucket.interface';
import { AffiliationRole } from '../shared/interfaces/affiliationRole.interface';
import { Position } from '../shared/interfaces/position.interface';
import { SecurityRole } from '../shared/interfaces/securityRole.interface';
import { UserPersonaType } from '../shared/interfaces/userPersonaType';
import { UserPreferenceSupplyGroupViewModel } from '../shared/interfaces/userPreferenceSupplyGroupViewModel';
import { PositionGroup } from '../shared/interfaces/position-group.interface';
import { CombinedUserPreferences } from '../shared/interfaces/combinedUserPreferences.interface';
import { StaffingPreferenceLookupOption } from '../shared/interfaces/staffingPreferenceLookupOption';
import { SecurityFeature } from '../shared/interfaces/securityFeature.interface';
import { CommitmentTypeReasons } from '../shared/interfaces/commitmentTypeReasons.interface';
import { CortexSkuMapping } from '../shared/interfaces/cortex-sku-mapping.interface';

@Injectable({
  providedIn: 'root'
})
export class CoreService {
  public appSettings: AppSettings;
  public loggedInUser: Employee;
  public loggedInUserClaims: UserClaim;
  public combinedUserPreferences: BehaviorSubject<any> = new BehaviorSubject(null);
  public userPreferences: BehaviorSubject<UserPreferences> = new BehaviorSubject({} as UserPreferences);
  public userPreferenceSupplyGroups: BehaviorSubject<UserPreferenceSupplyGroupViewModel> = new BehaviorSubject({} as UserPreferenceSupplyGroupViewModel);
  public userNotifications: BehaviorSubject<UserNotification[]> = new BehaviorSubject([] as UserNotification[]);
  public caseOppToOpenFromNotifications = new Subject<string>();
  public showHideNotification = new Subject();
  public routeUrl: string;
  public accessibleOfficeCodeListForUser: string[];

  constructor(private http: HttpClient, private localStorageService: LocalStorageService) {
    this.appSettings = environment.settings;
  }

  // ------------------------EMPLOYEE------------------------------------//

  loadEmployee(loggedInEmployeeCode: string, currentUrlState: string = ""): Observable<void> {

    const impersonatedUser = this.getParamValueByName('employeecode', currentUrlState.toLowerCase());
    const userCode = impersonatedUser || loggedInEmployeeCode;

    //RESTRICT impersonation rights to select few
    if(this.appSettings.eCodesWithImpersonationAccess && impersonatedUser && !this.appSettings.eCodesWithImpersonationAccess.includes(loggedInEmployeeCode)){
      this.loggedInUser = {} as Employee;
      this.loggedInUserClaims = {} as UserClaim;
      return of(null);
    }

    const employeeApiEndPoint = userCode === null ?
      `${ConstantsMaster.apiEndPoints.loggedInUser}`
      : `${ConstantsMaster.apiEndPoints.impersonatedUser}?userCode=${userCode}`;
    return this.http.get<Employee>(this.appSettings.staffingAuthenticationApiBaseUrl + employeeApiEndPoint, { withCredentials: true })
      .pipe(
        map(result => {
          this.loggedInUser = result;
          // To-do : Clear only those values from local storage which are user specific to improve performance while impersonating
          this.clearLocalStorage(impersonatedUser);
          this.localStorageService.set(ConstantsMaster.localStorageKeys.loggedInUserHomeOffice,
            JSON.stringify(this.loggedInUser.office), 'monthly');

          if (this.loggedInUser.token) {
            sessionStorage.setItem('jwtBearerToken', this.loggedInUser.token);
            this.getLoggedInUserClaims();

          }
        })
      );
  }

  // ------------------------Helper Functions------------------------------------//

  clearLocalStorage(impersonatedUser) {
    const userPreferences = JSON.parse(this.localStorageService.get(ConstantsMaster.localStorageKeys.userPreferences));
    if (impersonatedUser != null || userPreferences?.employeeCode != this.loggedInUser.employeeCode || this.appSettings.clearLocalStorage) {
      localStorage.clear(); // Clear local storage while impersonating for testing purpose
    }
  }

  getLoggedInUserClaims() {
    const jwtBearerTokenPayload: any = jwtDecode(this.loggedInUser.token);
    this.loggedInUserClaims = jwtBearerTokenPayload;
    this.loggedInUserClaims.FeatureAccess = JSON.parse(jwtBearerTokenPayload.FeatureAccess);
    if(jwtBearerTokenPayload.Roles){
    this.loggedInUserClaims.HCPDAccess = JSON.parse(jwtBearerTokenPayload.HCPDAccess);
    this.loggedInUserClaims.OfficeAccess = JSON.parse(jwtBearerTokenPayload.OfficeAccess);
    this.loggedInUserClaims.Roles = JSON.parse(jwtBearerTokenPayload.Roles);
    this.loggedInUserClaims.DemandTypesAccess = JSON.parse(jwtBearerTokenPayload.DemandTypesAccess);
    this.loggedInUserClaims.PegC2CAccess = JSON.parse(jwtBearerTokenPayload.PegC2CAccess)
  }
  }

  // ------------------------USER PREFERENCES------------------------------------//

  getCombinedUserPreferences(): Observable<any> {
    return this.combinedUserPreferences.asObservable();
  }

  getCombinedUserPreferencesValue(): CombinedUserPreferences {
    return this.combinedUserPreferences.value;
  }

  getUserPreferences(): Observable<UserPreferences> {
    return this.userPreferences;
  }

  getUserPreferencesValue(): UserPreferences {
    return this.userPreferences.value;
  }

  getUserPreferencesSupplyGroups(): any {
    return this.userPreferenceSupplyGroups.value;
  }

  isStringNullOrEmpty(value) {
    if (value === null || value === undefined || value === '') {
      return true;
    } else {
      return false;
    }

  }

  isNumberNullOrUndefined(value) {

    if (value === null || value === undefined) {
      return true;
    } else {
      return false;
    }

  }

  getAllValuesForPropertyFromArrayOfObjects(arryaOfObjects, property) {
    return Array.prototype.map.call(arryaOfObjects, s => s[property]);
  }

  filterUpdatedDataByStaffingSettings(updatedData) {  
    let demandFilterCriteria = JSON.parse(sessionStorage.getItem('demandFilterCriteriaObj'));
    let today = new Date().toISOString().split('T')[0];

    if(updatedData.length > 0 && demandFilterCriteria) {  
      let demandStartDate = new Date(demandFilterCriteria.startDate).toISOString().split('T')[0];
      let demandEndDate = new Date(demandFilterCriteria.endDate).toISOString().split('T')[0];  
      
      if(demandStartDate < today) {
        demandStartDate = today;
      }
      updatedData = updatedData.filter(x => {
        let startDate = new Date(x.startDate).toISOString().split('T')[0];
        let endDate = new Date(x.endDate).toISOString().split('T')[0];
        return (startDate >= demandStartDate && startDate <= demandEndDate) ||
           (endDate >= demandStartDate && endDate <= demandEndDate) ||
           (startDate <= demandStartDate && endDate >= demandEndDate)
      })
    return updatedData;
    }
  }

  setDefaultUserPreferencesIfEmpty(userPrefrences: UserPreferences) {
    userPrefrences.supplyViewOfficeCodes = !this.isStringNullOrEmpty(userPrefrences.supplyViewOfficeCodes)
      ? userPrefrences.supplyViewOfficeCodes
      : this.loggedInUser.office.officeCode.toString();
    userPrefrences.groupBy = !this.isStringNullOrEmpty(userPrefrences.groupBy)
      ? userPrefrences.groupBy : GroupByEnum.ServiceLine;
    userPrefrences.sortBy = !this.isStringNullOrEmpty(userPrefrences.sortBy)
      ? userPrefrences.sortBy : SortByEnum.LevelGrade;
    userPrefrences.supplyWeeksThreshold = !this.isNumberNullOrUndefined(userPrefrences.supplyWeeksThreshold)
      ? userPrefrences.supplyWeeksThreshold : 2;
    userPrefrences.supplyViewStaffingTags = !this.isStringNullOrEmpty(userPrefrences.supplyViewStaffingTags)
      ? userPrefrences.supplyViewStaffingTags : ServiceLineEnum.GeneralConsulting;
    userPrefrences.vacationThreshold = !this.isNumberNullOrUndefined(userPrefrences.vacationThreshold)
      ? userPrefrences.vacationThreshold : 5;
    userPrefrences.trainingThreshold = !this.isNumberNullOrUndefined(userPrefrences.trainingThreshold)
      ? userPrefrences.trainingThreshold : 5;
    userPrefrences.levelGrades = !this.isStringNullOrEmpty(userPrefrences.levelGrades)
      ? userPrefrences.levelGrades : '';
    userPrefrences.practiceAreaCodes = !this.isStringNullOrEmpty(userPrefrences.practiceAreaCodes)
      ? userPrefrences.practiceAreaCodes : '';
    userPrefrences.planningCardsSortOrder = !this.isStringNullOrEmpty(userPrefrences.planningCardsSortOrder)
      ? userPrefrences.planningCardsSortOrder : '';
    userPrefrences.caseOppSortOrder = !this.isStringNullOrEmpty(userPrefrences.caseOppSortOrder)
      ? userPrefrences.caseOppSortOrder : '';
    userPrefrences.caseAttributeNames = !this.isStringNullOrEmpty(userPrefrences.caseAttributeNames)
      ? userPrefrences.caseAttributeNames : '';
    userPrefrences.positionCodes = !this.isStringNullOrEmpty(userPrefrences.positionCodes)
      ? userPrefrences.positionCodes : '';

    // Show billable by default in dropdown if no user preferences are set
    userPrefrences.caseTypeCodes = !this.isStringNullOrEmpty(userPrefrences.caseTypeCodes)
      ? userPrefrences.caseTypeCodes : CaseTypeEnum.Billable;
    userPrefrences.opportunityStatusTypeCodes = !this.isStringNullOrEmpty(userPrefrences.opportunityStatusTypeCodes)
      ? userPrefrences.opportunityStatusTypeCodes : OpportunityStatusTypeEnum.All;
    userPrefrences.demandTypes = !this.isStringNullOrEmpty(userPrefrences.demandTypes)
      ? userPrefrences.demandTypes : this.getAllValuesForPropertyFromArrayOfObjects(
        this.localStorageService.get(ConstantsMaster.localStorageKeys.demandTypes)
          .filter(item => item.type !== 'CasesStaffedBySupply'), 
        'type'
      ).toString();
    userPrefrences.demandViewOfficeCodes = !this.isStringNullOrEmpty(userPrefrences.demandViewOfficeCodes)
      ? userPrefrences.demandViewOfficeCodes : this.loggedInUser.office.officeCode.toString();
    userPrefrences.demandWeeksThreshold = !this.isNumberNullOrUndefined(userPrefrences.demandWeeksThreshold)
      ? userPrefrences.demandWeeksThreshold : 2;
    userPrefrences.minOpportunityProbability = !this.isNumberNullOrUndefined(userPrefrences.minOpportunityProbability)
      ? userPrefrences.minOpportunityProbability : 0;
    userPrefrences.caseAllocationsSortBy = !this.isStringNullOrEmpty(userPrefrences.caseAllocationsSortBy)
      ? userPrefrences.caseAllocationsSortBy : ConstantsMaster.CaseAllocationsSortByOptions[0].value;
    return userPrefrences;
  }

  setAllUserPreferences(userPreferences: UserPreferences, userPreferenceSupplyGroups: UserPreferenceSupplyGroupViewModel[]) {
    if (userPreferences) {
      this.setUserPreferences(userPreferences);
    } else {
      userPreferences = this.setUserPreferences({} as UserPreferences);
    }

    if (userPreferenceSupplyGroups) {
      this.setUserPreferenceSupplyGroupsInLocalStorage(userPreferenceSupplyGroups);
    } else {
      this.setUserPreferenceSupplyGroupsInLocalStorage([{} as UserPreferenceSupplyGroupViewModel]);
    }


    this.combinedUserPreferences.next({ userPreferences, userPreferenceSupplyGroups });
  }

  setUserPreferences(userPreferences: UserPreferences, reload = true): UserPreferences {
    // This is done to ensure consistency in showing and saving default settings like user's home office ,
    // billable cases are always shown irrespective of if the user unchecks all in user settings
    userPreferences = this.setDefaultUserPreferencesIfEmpty(userPreferences);

    //We still need to trigger this as Analytics depends on this user preferences
    if (reload) {
      this.userPreferences.next(userPreferences);
    }

    this.localStorageService.set(ConstantsMaster.localStorageKeys.userPreferences, JSON.stringify(userPreferences), 'weekly');
    return userPreferences;
  }

  setUserPreferenceSupplyGroupsInLocalStorage(userPreferenceSupplyGroups: UserPreferenceSupplyGroupViewModel[]){
    const existingdata = JSON.parse(this.localStorageService.get(ConstantsMaster.localStorageKeys.userPreferenceSupplyGroups));
    
    if(existingdata && existingdata.length > 0){
      //set all other defaults to false if default is present in upserted data as there can be only 1 default
      if(userPreferenceSupplyGroups.some(x => x.isDefaultForResourcesTab)){
        existingdata.map(x => x.isDefaultForResourcesTab = false);
      }

      //set all other defaults to false if default is present in upserted data as there can be only 1 default
      if(userPreferenceSupplyGroups.some(x => x.isDefault)){
        existingdata.map(x => x.isDefault = false);
      }

      //update local storage with upserted data
      userPreferenceSupplyGroups.forEach((item) => {
        const index = existingdata.findIndex(x => x.id === item.id);
        if(index > -1){
          existingdata[index] = item;
        }else{
          existingdata.push(item);
        }
      });
     
      this.localStorageService.set(ConstantsMaster.localStorageKeys.userPreferenceSupplyGroups, JSON.stringify(existingdata), 'weekly');
    }else{
      this.localStorageService.set(ConstantsMaster.localStorageKeys.userPreferenceSupplyGroups, JSON.stringify(userPreferenceSupplyGroups), 'weekly');
    }

  }

  loadAllUserPreferences(): Observable<any> {
    return forkJoin([
      this.loadUserPreferences(),
      this.loadUserPreferenceSupplyGroups()
    ]).pipe(
      tap(result => {
        this.setAllUserPreferences(result[0], result[1]);
      })
    );

  }

  // used to load user preferences for the first time when application loads in app initializer.
  // After that get and set methods are used for accessing user preferences data
  loadUserPreferences(): Observable<UserPreferences> {
    this.loadLookupList();

    const params = new HttpParams({
      fromObject: {
        'employeeCode': this.loggedInUser.employeeCode
      }
    });

      return this.http.get<UserPreferences>(
        `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.getUserPreferences}`, { params: params });
  }

  // used to load user preferences supply groups for the first time when application loads in app initializer.
  // After that get and set methods are used for accessing user preferences supply groups data
  loadUserPreferenceSupplyGroups(): Observable<UserPreferenceSupplyGroupViewModel[]> {
    const params = new HttpParams({
      fromObject: {
        'employeeCode': this.loggedInUser.employeeCode
      }
    });

    // (02-08-2023) Made groups to load from backend, as there are some changes made in DB structure. Will revert it in next PROD release

    // const userPreferenceSupplyGroups = this.localStorageService.get(ConstantsMaster.localStorageKeys.userPreferenceSupplyGroups);
    // if (userPreferenceSupplyGroups) {
    //   return of(JSON.parse(userPreferenceSupplyGroups));
    // }
    // else {
    //   return this.http.get<UserPreferenceSupplyGroupViewModel[]>(
    //     `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.getUserPreferenceSupplyGroups}`, { params: params });
    // }

    return this.http.get<UserPreferenceSupplyGroupViewModel[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.getUserPreferenceSupplyGroups}`, { params: params });

  }

  insertUserPreferences(userPreferences: UserPreferences): Observable<UserPreferences> {
    userPreferences.employeeCode = this.loggedInUser.employeeCode;
    userPreferences.lastUpdatedBy = this.loggedInUser.employeeCode;

    return this.http.post<UserPreferences>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.upsertUserPreferences}`, userPreferences);
  }

  updateUserPreferences(userPreferences: UserPreferences): Observable<UserPreferences> {
    userPreferences.lastUpdatedBy = this.loggedInUser.employeeCode;

    return this.http.post<UserPreferences>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.upsertUserPreferences}`, userPreferences);
  }

  // ------------------------USER NOTIFICATIONS------------------------------------//

  setShowHideNotification(value) {
    this.showHideNotification.next(value);
  }

  getShowHideNotification() {
    return this.showHideNotification;
  }

  triggerOpenCaseOppOverlayFromNotifications(value) {
    this.caseOppToOpenFromNotifications.next(value);
  }

  openCaseOppOverlayFromNotifications(): Observable<string> {
    return this.caseOppToOpenFromNotifications.asObservable();
  }

  getUserNotifications(): Observable<UserNotification[]> {
    let userPreferredDemandViewOfficeCodes;
    this.userPreferences.subscribe(next => {
      if (next) {
        userPreferredDemandViewOfficeCodes = next.demandViewOfficeCodes;
      }
    });

    const params = new HttpParams({
      fromObject: {
        'employeeCode': this.loggedInUser.employeeCode,
        'officeCodes': userPreferredDemandViewOfficeCodes ? userPreferredDemandViewOfficeCodes : this.loggedInUser.office.officeCode
      }
    });

    return this.http.get<UserNotification[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.userNotification}`, { params: params });
  }

  updateUserNotificationStatus(notificationId, notificationStatus): Observable<void> {
    const employeeCode = this.loggedInUser.employeeCode;

    return this.http
      .put<void>(
        `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.userNotification}`,
        { notificationId, employeeCode, notificationStatus });
  }

  getPreponedCaseAllocationsAudit(payload) {
    return this.http.post<any>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.preponedCasesAllocationsAudit}`, payload);
  }

  // ------------------------Load local Storage ------------------------------------//
  loadLookupList() {
    //02-Jul-2024: Removing security roles lookup data s we are getting more roles
    this.localStorageService.removeItem(ConstantsMaster.localStorageKeys.securityRoles);
    
    const officeList = this.localStorageService.get(ConstantsMaster.localStorageKeys.OfficeList);
    const officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.officeHierarchy);
    const caseTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.caseTypes);
    const serviceLines = this.localStorageService.get(ConstantsMaster.localStorageKeys.serviceLines);
    const serviceLinesHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.serviceLinesHierarchy);
    const levelGradesHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.levelGradesHierarchy);
    const levelGrades = this.localStorageService.get(ConstantsMaster.localStorageKeys.levelGrades);
    const positions = this.localStorageService.get(ConstantsMaster.localStorageKeys.positions);
    const positionsHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.positionsHierarchy);
    const skuTermList = this.localStorageService.get(ConstantsMaster.localStorageKeys.skuTermList);
    const cortexSkuMappings = this.localStorageService.get(ConstantsMaster.localStorageKeys.cortexSkuMappings);
    const opportunityStatusTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.opportunityStatusTypes);
    const commitmentTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.commitmentTypes);
    const commitmentTypeReasons = this.localStorageService.get(ConstantsMaster.localStorageKeys.commitmentTypeReasons);
    const investmentCategories = this.localStorageService.get(ConstantsMaster.localStorageKeys.investmentCategories);
    const caseRoleTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.caseRoleTypes);
    const certificates = this.localStorageService.get(ConstantsMaster.localStorageKeys.certificates);
    const languages = this.localStorageService.get(ConstantsMaster.localStorageKeys.languages);
    const practiceAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.practiceAreas);
    const affiliationRoles = this.localStorageService.get(ConstantsMaster.localStorageKeys.affiliationRoles);
    const staffableAsTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffableAsTypes);
    const demandTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.demandTypes);
    const casePlanningBoardBuckets = this.localStorageService.get(ConstantsMaster.localStorageKeys.casePlanningBoardBuckets);
    const industryPracticeAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.industryPracticeAreas);
    const capabilityPracticeAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.capabilityPracticeAreas);
    const securityRoles = this.localStorageService.get(ConstantsMaster.localStorageKeys.securityRoles);
    const securityFeatures = this.localStorageService.get(ConstantsMaster.localStorageKeys.securityFeatures);
    const userPersonaTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.userPersonaTypes);
    const staffingPreferences = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffingPreferences);

    if(!officeList){
      this.getOfficeList().subscribe(offices => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.OfficeList, offices);
      });
    }

    if (!officeHierarchy) {
      this.getOfficeHierarchy().subscribe(officeHierarchy => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.officeHierarchy, officeHierarchy);

        //TODO: Hard coding for now to proivde EMEA practice staffing users access to only EMEA data.
        //DELETE entire block once integrated multiple-role based office security is implemented
        {
          let accessibleOfficeRegion = null;
  
          if (this.loggedInUserClaims.OfficeAccess) {
            accessibleOfficeRegion = this.getSelectedofficesRegion(officeHierarchy, [this.loggedInUserClaims.OfficeAccess[0].toString()]);
          }   
          
          let accessibleOfficeHierarchyForUser = accessibleOfficeRegion 
            ? officeHierarchy.children.filter(x => x.text.toUpperCase() === accessibleOfficeRegion.toUpperCase()) 
            : officeHierarchy.children;
          
          this.accessibleOfficeCodeListForUser = this.getOfficesFromChildren(accessibleOfficeHierarchyForUser, []);
          
          this.localStorageService.set(ConstantsMaster.localStorageKeys.accessibleOfficeHierarchyForUser, accessibleOfficeHierarchyForUser[0]);
          this.localStorageService.set(ConstantsMaster.localStorageKeys.accessibleOfficeCodeListForUser, this.accessibleOfficeCodeListForUser);
          
        }
      });
 }

    if (!caseTypes) {
      this.getCaseTypeList().subscribe(caseTypes => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.caseTypes, caseTypes, "weekly");
      });
    }
  //if (!serviceLines) {
      this.getServiceLineList().subscribe(serviceLineList => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.serviceLines, serviceLineList, "weekly");

        // Manually Adding PEG as it is not service line in workday
        serviceLineList.unshift({ serviceLineCode: 'PS', serviceLineName: 'PEG-Surge', inActive: false });
        serviceLineList.unshift({ serviceLineCode: 'P', serviceLineName: 'PEG', inActive: false });

        this.localStorageService.set(ConstantsMaster.localStorageKeys.staffingTags, serviceLineList, "weekly");
      });
   // }

    if (!serviceLinesHierarchy) {
      this.getServiceLineHierarchyList().subscribe(serviceLineList => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.serviceLinesHierarchy, serviceLineList);

        // Manually Adding PEG as it is not service line in workday
        serviceLineList.unshift({ value: 'PEG', text: 'PEG', children: [{ value: 'P', text: 'PEG', children: null }, { value: 'PS', text: 'PEG-Surge', children: null }] });
        this.localStorageService.set(ConstantsMaster.localStorageKeys.staffingTagsHierarchy, serviceLineList);
      });
    }

    if (!levelGradesHierarchy) {
      this.getLevelGradeHierarichalList().subscribe(levelGradeList => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.levelGradesHierarchy, levelGradeList);
      });
    }

    if (!levelGrades) {
      this.getLevelGradeList().subscribe(levelGradeList => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.levelGrades, levelGradeList);
      });
    }

    if (!positions) {
      this.getPositionsList().subscribe(positionsList => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.positions, positionsList);
      });
    }

    if (!positionsHierarchy) {
      this.getPositionsHierarchyList().subscribe(positionsHierarchy => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.positionsHierarchy, positionsHierarchy);

        this.setPositionsGroupFlatList(positionsHierarchy);
      });
    }

    if (!skuTermList) {
      this.getSKUTermList().subscribe(skuTermList => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.skuTermList, skuTermList, "weekly");
      });
    }

    //if(!cortexSkuMappings) {
      this.getCortexSkuList().subscribe(cortexSkuMappings => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.cortexSkuMappings, cortexSkuMappings, "weekly");
      });
    //}

    if (!opportunityStatusTypes) {
      this.getOpportunityStatusTypeList().subscribe(statusTypeList => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.opportunityStatusTypes, statusTypeList, "weekly");
      });
    }

    //if (!commitmentTypes) {
      this.getCommitmentTypes().subscribe(commitmentTypes => {
        // Filter commitments accessible to user role
        let accessibleCommitmentTypes = CommonService.getAccessibleCommitmentTypes(this.loggedInUserClaims);
        commitmentTypes = commitmentTypes.filter(x => accessibleCommitmentTypes?.includes(x.commitmentTypeCode) ?? true);
        // add a "select type" item to the list
        const dummyCategory = { commitmentTypeCode: '', commitmentTypeName: 'Select Type', precedence: 0, isStaffingTag: false };
        commitmentTypes.splice(0, 0, dummyCategory);
        this.localStorageService.set(ConstantsMaster.localStorageKeys.commitmentTypes, commitmentTypes, "weekly");

        // set ringfences in Local Storage
        this.localStorageService.set(ConstantsMaster.localStorageKeys.ringfences, commitmentTypes.filter(x => x.isStaffingTag), "weekly");
      });
    //}

    //if(!commitmentTypeReasons)
    //{
      this.getCommitmentTypeReasons().subscribe(commitmentTypeReasons => {
        //Prepend 'Select Type' to show select type as default category
        const dummyCommitmentTypeReason = { commitmentTypeReasonCode: null, commitmentTypeReasonName: 'Select Type' };
        commitmentTypeReasons.splice(0, 0, dummyCommitmentTypeReason);
          
        this.localStorageService.set(ConstantsMaster.localStorageKeys.commitmentTypeReasons, commitmentTypeReasons, "weekly");
      });
    //}

      this.getInvestmentCategories().subscribe(investmentCategories => {
        // Prepend 'Select Type' to show select type as default category
        const dummyCategory = { investmentCode: null, investmentName: 'Select Type', investmentDescription: '', precedence: 0,applicableCaseTypeCodes:null };
        investmentCategories.splice(0, 0, dummyCategory);
        this.localStorageService.set(ConstantsMaster.localStorageKeys.investmentCategories, investmentCategories, "weekly");
      });

    if (!caseRoleTypes) {
        this.getCaseRoleTypes().subscribe(caseRoleTypes => {
          //Prepend 'Select Type' to show select type as default category
          const dummyCaseRoleType = { caseRoleCode: null, caseRoleName: 'Select Type' };
          caseRoleTypes.splice(0, 0, dummyCaseRoleType);
          this.localStorageService.set(ConstantsMaster.localStorageKeys.caseRoleTypes, caseRoleTypes, "weekly");
        });
    }

    if (!certificates) {
      this.getCertificatesList().subscribe(certificates => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.certificates, certificates, "weekly");
      });
    }

    if (!languages) {
      this.getLanguagesList().subscribe(languages => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.languages, languages, "weekly");
      });
    }

    //Added on 27thJanuary2025 to ensure latest taxonomy changes are picked up
    //if (!practiceAreas) {
      this.getPracticeArea().subscribe(practiceAreas => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.practiceAreas, practiceAreas, "weekly");
      });
    //}

    if (!affiliationRoles) {
      this.getRoleName().subscribe(roleNames => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.affiliationRoles, roleNames, "weekly")
      });
    }

    if (!staffableAsTypes) {
      this.getStaffableAsTypes().subscribe(staffableAsTypes => {
        staffableAsTypes.unshift({ staffableAsTypeCode: -1, staffableAsTypeName: 'Select Role' });
        this.localStorageService.set(ConstantsMaster.localStorageKeys.staffableAsTypes, staffableAsTypes, "weekly");
      });
    }

    if (!demandTypes) {
      this.getDemandTypes().subscribe(demandTypes => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.demandTypes, demandTypes, "weekly");
      });
    }

    if (!casePlanningBoardBuckets) {
      this.getCasePlanningBoardBucketList().subscribe(bucketList => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.casePlanningBoardBuckets, bucketList, "weekly");
      });
    }

    if (!industryPracticeAreas) {
      this.getIndustryPracticeAreaList().subscribe(industryPracticeAreas => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.industryPracticeAreas, industryPracticeAreas, "weekly");
      });
    }

    if (!capabilityPracticeAreas) {
      this.getCapabilityPracticeAreaList().subscribe(capabilityPracticeAreas => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.capabilityPracticeAreas, capabilityPracticeAreas, "weekly");
      });
    }

    if (!securityRoles) {
      this.getSecurityRolesList().subscribe(securityRoles => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.securityRoles, securityRoles, "weekly");
      });
    }

    if (!securityFeatures) {
      this.getSecurityFeaturesList().subscribe(securityFeatures => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.securityFeatures, securityFeatures, "weekly");
      });
    }

    if (!userPersonaTypes) {
      this.getUserPersonaTypesList().subscribe(userPersonaTypes => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.userPersonaTypes, userPersonaTypes, "weekly");
      });
    }
  }



   loadLookupListForStaffingPreferences(): Observable<any> {
    const staffingPreferences = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffingPreferences);
  
    if (staffingPreferences) {
      return of(staffingPreferences);
    }
  
    return this.getStaffingPreferencesLookUpLists().pipe(
      tap(staffingPreferences => {
        this.localStorageService.set(ConstantsMaster.localStorageKeys.staffingPreferences, staffingPreferences, "weekly");
      })
    );
  }

  loadLookupListForStaffingInsightsTool(): Observable<any> {
    const industryPracticeAreas: PracticeArea[] = this.localStorageService.get(ConstantsMaster.localStorageKeys.industryPracticeAreas);
    const capabilityPracticeAreas: PracticeArea[] = this.localStorageService.get(ConstantsMaster.localStorageKeys.capabilityPracticeAreas);
    //const staffingPreferences: StaffingPreferenceLookupOption[] = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffingPreferences); //commented as we want to update local storage for staffing preferences lookup data
    const staffingPreferences = null; 
  
    let industryPracticeArea$;
    let capabilityPracticeArea$;
    let staffingPreferencesLookUp$;
  
    if (!industryPracticeAreas) {
      industryPracticeArea$ = (this.getIndustryPracticeAreaList().pipe(
        switchMap(industryPracticeAreas => {
          this.localStorageService.set(ConstantsMaster.localStorageKeys.industryPracticeAreas, industryPracticeAreas, "weekly");
          return of(industryPracticeAreas);
        })
      ));
    } else {
      industryPracticeArea$ = (of(industryPracticeAreas));
    }
  
    if (!capabilityPracticeAreas) {
      capabilityPracticeArea$ =(this.getCapabilityPracticeAreaList().pipe(
        switchMap(capabilityPracticeAreas => {
          this.localStorageService.set(ConstantsMaster.localStorageKeys.capabilityPracticeAreas, capabilityPracticeAreas, "weekly");
          return of(capabilityPracticeAreas);
        })
      ));
    } else {
      capabilityPracticeArea$ = (of(capabilityPracticeAreas));
    }
  
    if (!staffingPreferences) {
      staffingPreferencesLookUp$ = (this.getStaffingPreferencesLookUpLists().pipe(
        switchMap(staffingPreferences => {
          this.localStorageService.set(ConstantsMaster.localStorageKeys.staffingPreferences, staffingPreferences, "weekly");
          return of(staffingPreferences);
        })
      ));
    } else {
      staffingPreferencesLookUp$ = (of(staffingPreferences));
    }
    
    return forkJoin({industryPracticeAreas: industryPracticeArea$, capabilityPracticeAreas: capabilityPracticeArea$, staffingPreferences : staffingPreferencesLookUp$});
  }
  

  getSelectedofficesRegion(officeHierarchy, selectedOffices) {
    const treeViewOfficeitems = [new TreeviewItem(officeHierarchy)];
    const selectedRegions = [];
    selectedOffices.forEach(officeCode => {
      const office = TreeviewHelper.findItem(treeViewOfficeitems[0], officeCode.toString());
      if(office){
        this.findParentItem(treeViewOfficeitems[0], office, selectedRegions);
      }
    });
    return selectedRegions.toString();
  }

  findParentItem(treeViewOfficeitems, office, selectedRegions) {
    const parentOffice = TreeviewHelper.findParent(treeViewOfficeitems, office);
    const isParentRegion = ['Americas', 'Asia/Pacific', 'EMEA', 'Corp/Adj'].some(x => x === parentOffice?.text);
    if (isParentRegion) {
      if (!selectedRegions.some(x => x === parentOffice.text)) {
        selectedRegions.push(parentOffice.text);
      }
    } else {
      this.findParentItem(treeViewOfficeitems, parentOffice, selectedRegions);
    }
  }

  setPositionsGroupFlatList(positionsHierarchy: PositionHierarchy[]) {
    let positionGroups: PositionGroup[] = positionsHierarchy.map(node => ({
      positionGroupName: node.text,
      positionGroupCode: node.value,
    }));

    // add a "select type" item to the list
    const dummyCategory: PositionGroup = { positionGroupCode: '', positionGroupName: '' };
    positionGroups.splice(0, 0, dummyCategory);

    this.localStorageService.set(ConstantsMaster.localStorageKeys.positionsGroups, positionGroups);
  }

  getDemandTypes(): Observable<any> {
    let accessibleDemandTypes = this.loggedInUserClaims.DemandTypesAccess;
    let demandTypes = ConstantsMaster.demandTypes.filter(x => accessibleDemandTypes?.includes(x.type));
    return of(demandTypes);
  }

  getOfficeList(): Observable<Office[]> {
    return this.http.get<Office[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.officeList}`);
  }

  getOfficeHierarchy(): Observable<OfficeHierarchy> {
    return this.http.get<OfficeHierarchy>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.officeHierarchy}`);
  }

  getCaseTypeList(): Observable<CaseType[]> {
    return this.http.get<CaseType[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.casetypes}`);
  }

  getServiceLineList(): Observable<ServiceLine[]> {
    return this.http.get<ServiceLine[]>(
      `${this.appSettings.resourcesApiBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.serviceLines}`);
  }

  getServiceLineHierarchyList(): Observable<ServiceLineHierarchy[]> {
    return this.http.get<ServiceLineHierarchy[]>(
      `${this.appSettings.resourcesApiBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.serviceLinesHierarchy}`);
  }

  getPositionsHierarchyList(): Observable<PositionHierarchy[]> {
    return this.http.get<PositionHierarchy[]>(
      `${this.appSettings.resourcesApiBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.positionHierarchy}`);
  }

  getLevelGradeHierarichalList(): Observable<LevelGrade[]> {
    return this.http.get<LevelGrade[]>(
      `${this.appSettings.resourcesApiBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.levelGradesHierarchy}`);
  }

  getLevelGradeList(): Observable<PDGrade[]> {
    return this.http.get<PDGrade[]>(
      `${this.appSettings.resourcesApiBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.levelGrades}`);
  }

  getPositionsList(): Observable<Position[]> {
    return this.http.get<Position[]>(
      `${this.appSettings.resourcesApiBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.positions}`);
  }

  getSKUTermList(): Observable<SkuTerm[]> {
    return this.http.get<SkuTerm[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.skuterms}`);

  }

  getCortexSkuList(): Observable<CortexSkuMapping[]> {
    return this.http.get<CortexSkuMapping[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.cortexSkuMappings}`);
  }

  getOpportunityStatusTypeList(): Observable<OpportunityStatusType[]> {
    return this.http.get<OpportunityStatusType[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.opportunityStatusTypes}`);

  }

  getCommitmentTypes(): Observable<CommitmentType[]> {
    const params = new HttpParams({
      fromObject: {
        'showHidden': false
      }
    });
    return this.http.get<CommitmentType[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.commitmentTypes}`, { params: params });
  }

  getCommitmentTypeReasons(): Observable<CommitmentTypeReasons[]> {
    return this.http.get<CommitmentTypeReasons[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.commitmentTypeReasonList}`);
  }

  getInvestmentCategories(): Observable<InvestmentCategory[]> {
    return this.http.get<InvestmentCategory[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.investmentTypes}`);
  }

  getCaseRoleTypes(): Observable<CaseRoleType[]> {
    return this.http.get<CaseRoleType[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.caseRoleTypes}`);
  }

  getCertificatesList(): Observable<Certificate[]> {
    return this.http.get<Certificate[]>(
      `${this.appSettings.resourcesApiBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.certificates}`);
  }

  getLanguagesList(): Observable<Language[]> {
    return this.http.get<Language[]>(
      `${this.appSettings.resourcesApiBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.languages}`);
  }

  getPracticeArea(): Observable<PracticeArea[]> {
    return this.http.get<PracticeArea[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.practiceArea}`);
  }

  getRoleName(): Observable<AffiliationRole[]> {
    return this.http.get<AffiliationRole[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.affiliationRoles}`);
  }

  getStaffableAsTypes(): Observable<StaffableAsType[]> {
    return this.http.get<StaffableAsType[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.staffableAsTypes}`);
  }

  getCasePlanningBoardBucketList(): Observable<CasePlanningBoardBucket[]> {
    const loggedInUserCode = this.loggedInUser.employeeCode;

    return this.http.get<CasePlanningBoardBucket[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.casePlanningBoardBucketsByEmployee}?employeeCode=${loggedInUserCode}`);
  }

  getIndustryPracticeAreaList(): Observable<PracticeArea[]> {
    return this.http.get<PracticeArea[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.industryPracticeArea}`);
  }

  getCapabilityPracticeAreaList(): Observable<PracticeArea[]> {
    return this.http.get<PracticeArea[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.capabilityPracticeArea}`);
  }

  getStaffingPreferencesLookUpLists(): Observable<StaffingPreferenceLookupOption[]> {
    return this.http.get<StaffingPreferenceLookupOption[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.StaffingPreferences}`);
  }

  getSecurityRolesList(): Observable<SecurityRole[]> {
    return this.http.get<SecurityRole[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.securityRoles}`);
  }

  getSecurityFeaturesList(): Observable<SecurityFeature[]> {
    return this.http.get<SecurityFeature[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.securityFeatures}`);
  }

  getUserPersonaTypesList(): Observable<UserPersonaType[]> {
    return this.http.get<UserPersonaType[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.lookup.userPersonaTypes}`);
  }
  // ------------------------RINGFENCE MANAGEMENT------------------------------------//
  getTotalResourcesByOfficesAndRingfences(officeCodes: string, commitmentTypeCodes: string): Observable<RingfenceManagement[]> {
    const payload = {
      'officeCodes': officeCodes,
      'commitmentTypeCodes': commitmentTypeCodes
    };

    return this.http.post<RingfenceManagement[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.totalResourcesByOfficesAndRingfences}`, payload);
  }

  getRingfenceAuditLogByOfficeAndCommitmentCode(officeCode: string, commitmentTypeCode: string): Observable<RingfenceManagement[]> {
    const params = new HttpParams({
      fromObject: {
        'officeCode': officeCode,
        'commitmentTypeCode': commitmentTypeCode
      }
    });

    return this.http.get<RingfenceManagement[]>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.ringfenceAuditLogByOfficeAndCommitmentCode}`, { params: params });
  }

  upsertRingfenceDetails(upsertedData: RingfenceManagement) {
    upsertedData.lastUpdatedBy = this.loggedInUser.employeeCode;

    return this.http.post<RingfenceManagement>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.upsertRingfenceDetails}`, upsertedData);
  }

  getOfficeholidaysWithinDateRangeByOffices(officeClosureData: any): Observable<any> {
    return this.http.post<any>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.officeHolidaysWithinDateRange}`, officeClosureData);
  }

  getAllocationsDuringOfficeClosure(officeClosureData: any): Observable<any> {
    return this.http.post<any>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.allocationsDuringOfficeClosure}`, officeClosureData);
  }

  upsertOfficeClosureCases(officeClosureData: OfficeClosureCases): Observable<OfficeClosureCases> {
    officeClosureData.lastUpdatedBy = this.loggedInUser.employeeCode;
    return this.http.post<OfficeClosureCases>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.upsertOfficeClosureCases}`, officeClosureData);
  }

  getMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode(): Observable<any>{
    const loggedInUser = this.loggedInUser.employeeCode;
    return this.http.get<any>(
      `${this.appSettings.ocelotApiGatewayBaseUrl}/${ConstantsMaster.apiEndPoints.mostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode}?employeeCode=${loggedInUser}`);
  }

  getOfficesFromChildren(child: OfficeHierarchy[], offices) {

    child.forEach(x => {
      if (!x.children.length) {
        offices.push(x.value);
        return offices;
      }
      return this.getOfficesFromChildren(x.children, offices);
    })

    return offices;
  }



  // ------------------------TESTING------------------------------------//

  /** HACK->
   * Get Employee Object by Employee Code passed in query params.
   * This is done only for testing PurPose
   * @param name : name of the query param
   */
  getParamValueByName(name, currentUrlState) {
    // const url = window.location.href.toLowerCase();
    name = name.replace(/[\[\]]/g, '\\$&');
    const regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'),
      results = regex.exec(currentUrlState);
    if (!results) { return null; }
    if (!results[2]) { return ''; }
    return decodeURIComponent(results[2].replace(/\+/g, ' '));
  }

}
