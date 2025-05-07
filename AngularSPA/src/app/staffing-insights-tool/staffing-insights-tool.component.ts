import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { PracticeArea } from '../shared/interfaces/practiceArea.interface';
import { ProfileImageComponent } from '../core/profile-image/profile-image.component';
import { EmployeeBasic } from '../shared/interfaces/employee.interface';
import { CoreService } from '../core/core.service';
import { StaffingPreferenceGroupEnum } from '../shared/constants/enumMaster';
import { StaffingPreferenceForInsightsTool } from '../shared/interfaces/staffingPreferenceForInsightsTool';
import { FormsModule } from '@angular/forms';
import { StaffingInsightsToolService } from './staffing-insights-tool.service';
import { StaffingPreferenceLookupOption } from '../shared/interfaces/staffingPreferenceLookupOption';
import { Subject, forkJoin, takeUntil } from 'rxjs';
import { NotificationService } from '../shared/notification.service';
import { SharedService } from '../shared/shared.service';
import { CommonModule } from '@angular/common';
import { SingleSelectDropdownComponent } from '../standalone-components/single-select-dropdown/single-select-dropdown.component';
import { MultiSelectDropdownComponent } from '../standalone-components/multi-select-dropdown/multi-select-dropdown.component';
import { ConstantsMaster } from '../shared/constants/constantsMaster';
import { CommonService } from '../shared/commonService';

@Component({
  selector: 'app-staffing-insights-tool',
  standalone: true,
  imports: [
    FormsModule,
    CommonModule,
    TabsModule, 
    MatTabsModule, 
    MatIconModule, 
    SingleSelectDropdownComponent, 
    MultiSelectDropdownComponent,
    ProfileImageComponent 
  ],
  providers: [StaffingInsightsToolService, SharedService],
  templateUrl: './staffing-insights-tool.component.html',
  styleUrl: './staffing-insights-tool.component.scss'
})
export class StaffingInsightsToolComponent implements OnInit, OnDestroy, OnChanges {
  @Input() employee: EmployeeBasic;
  irisUrl:string = 'https://iris.bain.com/people-finder/people-directory/results?name=';
  workdayLanguagesURL:string = 'https://wd5.myworkday.com/bain/d/task/2998$7001.htmld';
  workdayProfileURL:string = 'https://wd5.myworkday.com/bain/d/inst/13102!CK5mGhEKBggDEMenAhIHCgUI1A0QJQ~~*ONvdiMouhyk~/cacheable-task/2997$2151.htmld';
  
  StaffingPreferenceGroup = StaffingPreferenceGroupEnum;
  //-----------------Variables-----------------------------------
  public prioritiesLabel = [ 
    { text: 'First Priority', selectedDropdownValue: ''}, 
    { text: 'Second Priority', selectedDropdownValue : ''}, 
    { text: 'Third Priority', selectedDropdownValue : ''}
  ];

  public pdLabels = [ 
    { text: 'Are there specific PD focus areas that are important for your upcoming casework?', selectedDropdownValue: []}
  ];

  industryInterestList = [];
  capabilityInterestList = [];

  public travelPreferencesTabOptions = [ 
    { labelText: 'Travel availability', dropdownType: 'single', data: [], selectedDropdownValue: ''},
    { labelText: 'Which regions are you able to travel to?', dropdownType: 'multiple',data: {}, selectedDropdownValue: []},
    { labelText: 'Are you happy to fly long haul, short haul, or both?', dropdownType: 'single',data: [], selectedDropdownValue: ''},
    { labelText: 'Reason unable to travel', dropdownType: 'single', selectedDropdownValue: ''}
  ];
  
  //---------drop downs--------------------------------
  priorities = [];
  pdPreferencesDropdownList= [];
  travelInterestDropdownList = [];
  travelRegionDropDownList = [];
  travelDurationDropDownList = [];
  industryPracticeAreaDropdownList = [];
  capabilityPracticeAreaDropdownList = [];

  //-------------------------Selecetd Values----------------------------
  selectedTravelInterest = '';
  selectedTravelRegions = [];
  selectedTravelDuration = '';
  
  //-----------------Variables-----------------------------------
  selectedSortByItem= "";
  activeTab: string;
  isPreferencesDataLoaded = false;
  savedPreferencesObj : StaffingPreferenceForInsightsTool = {} as StaffingPreferenceForInsightsTool;
  private unsubscribe$: Subject<void> = new Subject<void>();
  accessibleFeatures = ConstantsMaster.appScreens.feature;
  tabs = [
    {name: 'staffingInsights', displayName: 'Staffing Insights', visible: false},
    {name: 'staffingInsightsPriorities', displayName: 'Priorities', visible: false},
    {name: 'staffingInsightsPD', displayName: 'PD', visible: false},
    {name: 'staffingInsightsIndustries', displayName: 'Industries', visible: false},
    {name: 'staffingInsightsCapabilities', displayName: 'Capabilities', visible: false},
    {name: 'staffingInsightsExperience', displayName: 'Experience', visible: false},
    {name: 'staffingInsightsTravel', displayName: 'Travel', visible: false}
  ]
  
  constructor(
    private coreService: CoreService,
    private staffingInsightsToolService: StaffingInsightsToolService,
    private notificationService: NotificationService,
    private sharedService: SharedService) { 
  }

  ngOnInit(){
    this.showAccessibleFeatures();
    this.setActiveTab();

  }

  ngOnChanges(changes: SimpleChanges): void {
    if(changes.employee){
      this.loadEmployeeDetails();
      this.getEmployeePreferencesAndLookUpData();
    }
  }

  setActiveTab() {
    const nextVisibleTab = this.tabs.find(tab => tab.visible);
    if (nextVisibleTab) {
      this.activeTab = nextVisibleTab.displayName;
    }
  }

  showAccessibleFeatures(){    
    if (this.coreService.loggedInUser.hasAccessToStaffingInsightsTool) {
      this.tabs.forEach(tab => {
        tab.visible = true;
      });
    } else {
      let staffingInsightsAccess = false;
      const accessibleFeaturesForUser = this.coreService.loggedInUserClaims.FeatureAccess;

      this.tabs.forEach(tab => {
        const featureName = this.accessibleFeatures[tab.name];
        const isFeatureAccessible = CommonService.isAccessToFeatureReadOnlyOrNone(featureName, accessibleFeaturesForUser); 
        if(featureName === 'staffingInsightsTool' && isFeatureAccessible){
          staffingInsightsAccess = true;
        }
        tab.visible = staffingInsightsAccess || isFeatureAccessible;
      });
    }
  }

  isFeatureAccessible(feature: string) {
    return this.tabs.find(tab => tab.name === feature).visible;
  }

  loadEmployeeDetails(){
    this.employee = this.employee ?? this.coreService.loggedInUser;
    this.irisUrl = `${this.irisUrl}${this.employee.firstName} ${this.employee.lastName}`;
  }

  getEmployeePreferencesAndLookUpData(){

    forkJoin([
      this.staffingInsightsToolService.getEmployeeStaffingPreferences(this.employee.employeeCode), 
      this.coreService.loadLookupListForStaffingInsightsTool()
    ])
    .pipe(takeUntil(this.unsubscribe$))
    .subscribe(([savedPreferences, {industryPracticeAreas, capabilityPracticeAreas, staffingPreferences}]) => {
    
      this.initializeDropDownsWithLookUpData(industryPracticeAreas, capabilityPracticeAreas, staffingPreferences);
      
      this.setEmployeeStaffingPreferences(savedPreferences?.[0]);
      
    });
  }

  setEmployeeStaffingPreferences(data: StaffingPreferenceForInsightsTool)
  {
      this.savedPreferencesObj = data ?? {} as StaffingPreferenceForInsightsTool;
      this.isPreferencesDataLoaded = true;
      this.populateDropdownsWithSavedPreferencesData();
      
      if(this.savedPreferencesObj.lastUpdatedBy)
      this.sharedService.getResourceDetailsByEmployeeCode(this.savedPreferencesObj.lastUpdatedBy, 'employeeCode,fullName').subscribe({
        next: (employee: EmployeeBasic[]) => {
          if(employee?.length > 0){
            this.savedPreferencesObj.lastUpdatedByName = employee[0].fullName;
          }
        }
      });
  }

  //-----------------------Helper Methods -----------------------------------------
  initializeDropDownsWithLookUpData(industries: PracticeArea[], capabilities: PracticeArea[], staffingPreferences: StaffingPreferenceLookupOption[]){
    
    this.priorities = staffingPreferences
      .filter(x => x.preferenceGroupCode === StaffingPreferenceGroupEnum.PRIORITY)
      .map( (item : StaffingPreferenceLookupOption) => {
        return {
          text: item.preferenceTypeName,
          value: item.preferenceTypeCode
        }
    });

    this.industryInterestList = staffingPreferences
      .filter(x => x.preferenceGroupCode === StaffingPreferenceGroupEnum.INDUSTRY_CAPABILITY_INTEREST)
      .map( (item : StaffingPreferenceLookupOption) => {
        return {
          text: item.preferenceTypeName ,
          value: item.preferenceTypeCode,
          selectedDropdownValue: []
        }
    });

    this.capabilityInterestList = staffingPreferences
      .filter(x => x.preferenceGroupCode === StaffingPreferenceGroupEnum.INDUSTRY_CAPABILITY_INTEREST)
      .map( (item : StaffingPreferenceLookupOption) => {
        return {
          text: item.preferenceTypeName,
          value: item.preferenceTypeCode,
          selectedDropdownValue: []
        }
    });

    this.travelPreferencesTabOptions[0].data = staffingPreferences
      .filter(x => x.preferenceGroupCode === StaffingPreferenceGroupEnum.TRAVEL_INTEREST)
      .map( (item : StaffingPreferenceLookupOption) => {
        return {
          text: item.preferenceTypeName,
          value: item.preferenceTypeCode
        }
    });

    this.travelPreferencesTabOptions[1].data = {
      text: 'All',
      value: 0,
      checked: false,
      children: staffingPreferences
        .filter(x => x.preferenceGroupCode === StaffingPreferenceGroupEnum.TRAVEL_REGIONS)
        .map( (item : StaffingPreferenceLookupOption) => {
          return {
              text: item.preferenceTypeName,
              value: item.preferenceTypeCode,
              checked: false
          }
      })
    };


    this.travelPreferencesTabOptions[2].data = staffingPreferences
      .filter(x => x.preferenceGroupCode === StaffingPreferenceGroupEnum.TRAVEL_DURATION)
      .map( (item : StaffingPreferenceLookupOption) => {
        return {
          text: item.preferenceTypeName,
          value: item.preferenceTypeCode
        }
    });
    
    this.pdPreferencesDropdownList = staffingPreferences
      .filter(x => x.preferenceGroupCode === StaffingPreferenceGroupEnum.PD)
      .map( (item : StaffingPreferenceLookupOption) => {
        return {
          text: item.preferenceTypeName,
          value: item.preferenceTypeCode
        }
    });

    this.industryPracticeAreaDropdownList = industries.filter(x => x.practiceAreaCode !=16).map(item => { //exclude 'No Industry'
        return {
            text: `${item.practiceAreaName}`,// (${item.practiceAreaAbbreviation ?? ''})` ,
            value: item.practiceAreaCode
        }
    });

    this.capabilityPracticeAreaDropdownList = capabilities.filter(x => x.practiceAreaCode !=17).map(item => { //exclude 'No Capability'
        return {
            text: `${item.practiceAreaName}`,// (${item.practiceAreaAbbreviation ?? ''})` ,
            value: item.practiceAreaCode,
            checked: false
        }
    });

    this.travelInterestDropdownList = staffingPreferences
      .filter(x => x.preferenceGroupCode === StaffingPreferenceGroupEnum.TRAVEL_INTEREST)
      .map( (item : StaffingPreferenceLookupOption) => {
        return {
          text: item.preferenceTypeName,
          value: item.preferenceTypeCode
        }
    });

    this.travelDurationDropDownList = staffingPreferences
      .filter(x => x.preferenceGroupCode === StaffingPreferenceGroupEnum.TRAVEL_DURATION)
      .map( (item : StaffingPreferenceLookupOption) => {
        return {
          text: item.preferenceTypeName,
          value: item.preferenceTypeCode
        }
    });

    this.travelRegionDropDownList = staffingPreferences
      .filter(x => x.preferenceGroupCode === StaffingPreferenceGroupEnum.TRAVEL_REGIONS)
      .map( (item : StaffingPreferenceLookupOption) => {
        return {
            text: item.preferenceTypeName,
            value: item.preferenceTypeCode
        }
    });
  }

  populateDropdownsWithSavedPreferencesData(){
    this.prioritiesLabel[0].selectedDropdownValue = this.savedPreferencesObj.firstPriority ?? '';
    this.prioritiesLabel[1].selectedDropdownValue = this.savedPreferencesObj.secondPriority ?? '';
    this.prioritiesLabel[2].selectedDropdownValue = this.savedPreferencesObj.thirdPriority ?? '';

    this.pdLabels[0].selectedDropdownValue = this.savedPreferencesObj.pdFocusAreas?.split(",") ?? [];
    this.industryInterestList[0].selectedDropdownValue = this.savedPreferencesObj.industryCodesHappyToWorkIn?.split(",") ?? [];
    this.industryInterestList[1].selectedDropdownValue = this.savedPreferencesObj.industryCodesNotInterestedToWorkIn?.split(",") ?? [];
    this.industryInterestList[2].selectedDropdownValue = this.savedPreferencesObj.industryCodesExcitedToWorkIn?.split(",") ?? [];
    this.capabilityInterestList[0].selectedDropdownValue = this.savedPreferencesObj.capabilityCodesHappyToWorkIn?.split(",") ?? [];
    this.capabilityInterestList[1].selectedDropdownValue = this.savedPreferencesObj.capabilityCodesNotInterestedToWorkIn?.split(",") ?? [];
    this.capabilityInterestList[2].selectedDropdownValue = this.savedPreferencesObj.capabilityCodesExcitedToWorkIn?.split(",") ?? [];

    this.selectedTravelInterest = this.savedPreferencesObj.travelInterest ?? '';
    this.selectedTravelRegions = this.savedPreferencesObj.travelRegions?.split(",") ?? [];
    this.selectedTravelDuration = this.savedPreferencesObj.travelDuration ?? '';
    
  }

  setPiorityPreferences(data, index){
    if( this.prioritiesLabel[index].selectedDropdownValue !== data.value){
      this.prioritiesLabel[index].selectedDropdownValue = data.value;
    }

    // switch(index){
    //   case 0:
    //     if( this.savedPreferencesObj.firstPriority !== data.value){
    //       this.savedPreferencesObj.firstPriority = data.value;
    //     }
        
    //     break;
    //   case 1:
    //     if( this.savedPreferencesObj.secondPriority !== data.value){
    //       this.savedPreferencesObj.secondPriority = data.value;
    //     }
        
    //     break;
    //   case 2:
    //     if( this.savedPreferencesObj.thirdPriority !== data.value){
    //       this.savedPreferencesObj.thirdPriority = data.value;
    //     }
        
    //     break;
    // }
  }

  setPDPreferences(data, index){
    if( !data.isArrayEqual(this.pdLabels[index].selectedDropdownValue.map(String))){
      this.pdLabels[index].selectedDropdownValue = data;
    }
  }

  setIndustryPreferences(data, index){
    if( !data.isArrayEqual(this.industryInterestList[index].selectedDropdownValue.map(String))){
      this.industryInterestList[index].selectedDropdownValue = data;
    }
  }

  setCapabilityPreferences(data, index){
    if( !data.isArrayEqual(this.capabilityInterestList[index].selectedDropdownValue.map(String))){
      this.capabilityInterestList[index].selectedDropdownValue = data;
    }
  }

  setTravelPreferences(data, option){
    switch(option){
      case StaffingPreferenceGroupEnum.TRAVEL_INTEREST:
        if( this.selectedTravelInterest !== data.value){
          if(data.value === 'TI0010'){ //if new value is 'Unable to travel', then clear data for travel regions and duration
            this.selectedTravelRegions = [];
            this.selectedTravelDuration = '';
          }

          this.selectedTravelInterest = data.value;
        }
        break;
      case StaffingPreferenceGroupEnum.TRAVEL_REGIONS:
        if( !data.isArrayEqual(this.selectedTravelRegions.map(String))){
          this.selectedTravelRegions = data;
        }
        break;
      case StaffingPreferenceGroupEnum.TRAVEL_DURATION:
        if( this.selectedTravelDuration !== data.value){
          this.selectedTravelDuration = data.value;
        }
        break;


    }
  }
  
  savePreferences(){
    this.savedPreferencesObj.employeeCode = this.employee.employeeCode;
    
    this.savedPreferencesObj.firstPriority = this.prioritiesLabel[0].selectedDropdownValue;
    this.savedPreferencesObj.secondPriority = this.prioritiesLabel[1].selectedDropdownValue;
    this.savedPreferencesObj.thirdPriority = this.prioritiesLabel[2].selectedDropdownValue;
    
    this.savedPreferencesObj.pdFocusAreas = this.pdLabels[0].selectedDropdownValue.join(",");
    
    this.savedPreferencesObj.industryCodesHappyToWorkIn = this.industryInterestList[0].selectedDropdownValue.join(",");
    this.savedPreferencesObj.industryCodesNotInterestedToWorkIn = this.industryInterestList[1].selectedDropdownValue.join(",");
    this.savedPreferencesObj.industryCodesExcitedToWorkIn = this.industryInterestList[2].selectedDropdownValue.join(",");
    
    this.savedPreferencesObj.capabilityCodesHappyToWorkIn = this.capabilityInterestList[0].selectedDropdownValue.join(",");
    this.savedPreferencesObj.capabilityCodesNotInterestedToWorkIn = this.capabilityInterestList[1].selectedDropdownValue.join(",");
    this.savedPreferencesObj.capabilityCodesExcitedToWorkIn = this.capabilityInterestList[2].selectedDropdownValue.join(",");
    
    this.savedPreferencesObj.travelInterest = this.selectedTravelInterest;
    this.savedPreferencesObj.travelRegions = this.selectedTravelRegions.join(",");
    this.savedPreferencesObj.travelDuration = this.selectedTravelDuration;
    this.savedPreferencesObj.lastUpdatedByName = this.coreService.loggedInUser.fullName;
    this.savedPreferencesObj.lastUpdated = new Date();
    this.staffingInsightsToolService.upsertEmployeePreferences(this.savedPreferencesObj).subscribe({
      next : () => this.notificationService.showSuccess('Preferences Saved successfully!'),
      error: () => this.notificationService.showError('Error while saving preferences!')
    });
  }

  goToLink(url: string){
    window.open(url, "_blank"); 
  }

  //------------------------------Clean up on destroy----------------------------------
  ngOnDestroy() {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }
}
