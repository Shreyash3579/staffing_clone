import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { Observable } from 'rxjs';
import { debounceTime, mergeMap } from 'rxjs/operators';
import { BS_DEFAULT_CONFIG } from '../constants/bsDatePickerConfig';
import { DateService } from '../dateService';
import { Resource } from '../interfaces/resource.interface';
import { NotificationService } from '../notification.service';
import { SharedService } from '../shared.service';
import { ValidationService } from '../validationService';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { ConstantsMaster } from '../constants/constantsMaster';
import { OfficeHierarchy } from '../interfaces/officeHierarchy.interface';
import { Office } from '../interfaces/office.interface';
import { ServiceLineHierarchy } from '../interfaces/serviceLineHierarchy';
import { PracticeArea } from '../interfaces/practiceArea.interface';
import { PositionHierarchy } from '../interfaces/positionHierarchy.interface';
import { SecurityRole } from '../interfaces/securityRole.interface';
import { LevelGrade } from 'src/app/shared/interfaces/levelGrade.interface';
import { CommitmentType } from 'src/app/shared/interfaces/commitmentType.interface';
import { UserPersonaType } from 'src/app/shared/interfaces/userPersonaType';
import { SecurityTypeForAdminTabEnum, ServiceLine } from '../constants/enumMaster';
import { SecurityUserDetail } from '../interfaces/securityUserDetail';
import { SecurityGroupDetails } from '../interfaces/securityGroup';
import { SecurityFeature } from '../interfaces/securityFeature.interface';
import { BossSecurityRole } from '../constants/enumMaster';


@Component({
    selector: 'app-add-security-user-form',
    templateUrl: './add-security-user-form.component.html',
    styleUrls: ['./add-security-user-form.component.scss'],
})
export class AddSecurityUserFormComponent implements OnInit {
   //----------------Inputs from Parent--------------------------
    public existingUsersList: SecurityUserDetail[] = [];
    public existingGroupsList: SecurityGroupDetails[] = [];
    public headerText = '';
    public primaryBtnText = 'Select';
    selectedSecurityType: string =  '';
   
    //----------------variables--------------------------

    public users: Resource[];
    public asyncUsersSearchString: string;
    public selectedUserToAdd;
   
    public invalidAdminUser = false;
    public bsConfig: Partial<BsDatepickerConfig>;
    public isEndDateInvalid: boolean = false;
    public endDate = null;
    public adminNotes = '';
    public validationSummaryMsg = '';
    public notesAlertText = ConstantsMaster.NotesAlert;

    //data sources for dropdowns
    
    officeHierarchy: OfficeHierarchy[];
    officeDropdownList;
    selectedOfficeList = [];

    serviceLinesHierarchy: ServiceLineHierarchy[];
    serviceLines: any[];
    serviceLinesDropdownList;
    selectedServiceLinesList = [];
    
    positionsHierarchy: PositionHierarchy[] = [];
    positionsDropdownList;
    selectedPositionsList = [];

    levelGradesHierarchy: LevelGrade[];
    levelGradesDropdownList;
    selectedLevelGradesList = []; 

    ringfences: CommitmentType[];
    ringfenceDropdownList;
    selectedRingfenceDropdownList = [];
    
    practiceAreaDropDownList;
    practiceAreas: PracticeArea[] = [];
    selectedPracticeAreaList = [];

    securityRoles: SecurityRole[] = [];
    securityFeatures: SecurityFeature[] = [];
    visibleSecurityRoleIds: number[] = [1,2,3,10,11];
    securityRolesDropdownList;
    securityFeaturesDropDownList;
    selectedSecurityRoleList : any[] = [3];
    selectedSecurityFeaturesList = [];

    userPersonaTypesDropdownList;
    userPersonaTypes: UserPersonaType[] = [];
    selectedUserPersonaTypesList= [];

    geoTypelist = ConstantsMaster.UserGeotype;
    selectedGeoTypeList = '';

    securityUserTypeCode: string = 'U';
    groupName: string = '';
   
    validationMessage: string='';
    

    @Input() loggedInUserHomeOffice: Office;
    @Output() addSelectedUserEventEmitter = new EventEmitter<any>();
    @Output() addSelectedGroupEventEmitter = new EventEmitter<any>();
    
    
    constructor(public bsModalRef: BsModalRef,
        private sharedService: SharedService,
        private notificationService: NotificationService,
        private localStorageService: LocalStorageService) { }

    ngOnInit() {
        this.setDataOnLoad()
        this.attachEventForUserSearch();
        this.initialiseDatePicker();
        this.getLookUpDataForDropdowns();
        this.intilializeDropDowns();    
    }

    setDataOnLoad(){
      this.securityUserTypeCode = this.selectedSecurityType;
      this.selectedSecurityRoleList = this.securityUserTypeCode ===  SecurityTypeForAdminTabEnum.USER ? [3] : [12];
    }

    getLookUpDataForDropdowns() {

    this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.officeHierarchy);
    this.serviceLinesHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.serviceLinesHierarchy);
    this.serviceLines = this.localStorageService.get(ConstantsMaster.localStorageKeys.serviceLines);
    this.positionsHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.positionsHierarchy);
    this.levelGradesHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.levelGradesHierarchy);
    this.practiceAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.capabilityPracticeAreas);
    this.ringfences = this.localStorageService.get(ConstantsMaster.localStorageKeys.ringfences).filter(x => x.commitmentTypeCode !== 'PS');
    this.userPersonaTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.userPersonaTypes);
    const securityRoles: SecurityRole[] = this.localStorageService.get(ConstantsMaster.localStorageKeys.securityRoles);
    this.securityFeatures = this.localStorageService.get(ConstantsMaster.localStorageKeys.securityFeatures);
    
    if(this.securityUserTypeCode ===  SecurityTypeForAdminTabEnum.USER){
      this.securityRoles = securityRoles.filter(x => this.visibleSecurityRoleIds.some(code => code === x.roleCode));
    }else{
      this.securityRoles = securityRoles;
    }
  }



 FilterAndUpdateServiceLineHierarchy(){
  const activeServiceLines = this.serviceLines.filter(sl => sl.inActive == true);

  activeServiceLines.forEach(serviceLine => {
    const hierarchyItem = this.serviceLinesHierarchy.find(h => h.value === serviceLine.serviceLineHierarchyCode);

    if (hierarchyItem) {
      hierarchyItem.children?.push({
        text: serviceLine.serviceLineName,
        value: serviceLine.serviceLineCode,
        children: null // No further hierarchy
      });
    } else {
      this.serviceLinesHierarchy.push({
        text: serviceLine.serviceLineHierarchyName,
        value: serviceLine.serviceLineHierarchyCode,
        children: [{
          text: serviceLine.serviceLineName,
          value: serviceLine.serviceLineCode,
          children: null // No further hierarchy
        }]
      });
    }
  });

  return;
}




  intilializeDropDowns() {

    this.FilterAndUpdateServiceLineHierarchy();

    this.serviceLinesDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.serviceLinesHierarchy.map((item) => {
        return {
          text: item.text,
          value: item.value,
          collapsed: true,
          children: item.children != null ? item.children.map(child => {
            return {
              text: child.text,
              value: child.value,
              checked: false
            };
          }) : null,
          checked: false
        };
      })
    };


    this.positionsDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.positionsHierarchy.map((item) => {
        return {
          text: item.text,
          value: item.value,
          collapsed: true,
          children: item.children != null ? item.children.map(child => {
            return {
              text: child.text,
              value: child.value,
              checked: false
            };
          }) : null,
          checked: false
        };
      })
    };


    this.levelGradesDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.levelGradesHierarchy.map((item) => {
        return {
          text: item.text,
          value: item.value,
          collapsed: true,
          children: item.children != null ? item.children.map(child => {
            return {
              text: child.text,
              value: child.value,
              checked: false
            };
          }) : null,
          checked: false
        };
      })
    };

    this.ringfences = this.ringfences.filter(type => type.commitmentTypeCode != '');
    this.ringfenceDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.ringfences.map(x => {
        return {
          text: x.commitmentTypeName,
          value: x.commitmentTypeCode,
          checked: false
        };
      })
    };

    this.practiceAreaDropDownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.practiceAreas.map(x => {
        return {
          text: x.practiceAreaName,
          value: x.practiceAreaCode,
          checked: false
        };
      })
    };


    this.securityRolesDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.securityRoles.map(x => {
        return {
          text: x.roleName,
          value: x.roleCode,
          checked: false
        };
      })
    };

    this.securityFeaturesDropDownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.securityFeatures
        .filter(x => !x.featureName.includes('/')) 
        .map(x => {
          const featurePrefix = x.featureName; 
    
          return {
            text: x.featureName,
            value: x.featureCode,
            checked: false,
            children: this.securityFeatures
              .filter(y => y.featureName.startsWith(`${featurePrefix}/`)) 
              .map(y => {
                return {
                  text: y.featureName.split('/')[1], 
                  value: y.featureCode,
                  checked: false
                };
              })
          };
        }),
    };

    this.userPersonaTypesDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.userPersonaTypes.map(x => {
        return {
          text: x.userTypeName,
          value: x.userTypeCode,
          checked: false
        };
      })
    };

    this.geoTypelist=this.geoTypelist.map(data =>{
      return{
        "text": data.text,
        "value": data.text
      }
    })

  }

  setSelectedOfficeList(officeDropdownList) {
    this.selectedOfficeList = officeDropdownList.split(',');
  }

  setSelectedServiceLinesDropdownList(serviceLinesDropdownList) {
    this.selectedServiceLinesList = serviceLinesDropdownList.split(',');
  }

  setSelectedPositionsDropdownList(positionsDropdownList) {
    this.selectedPositionsList = positionsDropdownList.split(',');
  }

  setSelectedLevelGradesDropdownList(levelGradesDropdownList) {
    this.selectedLevelGradesList = levelGradesDropdownList.split(',');
  }

  setSelectedRingfenceDropdownList(ringfenceDropdownList) {
    this.selectedRingfenceDropdownList = ringfenceDropdownList.split(',');
  }

  setSelectedPracticeAreaList(practiceAreaCodes) {
    this.selectedPracticeAreaList = practiceAreaCodes.split(',');
  }


  setSelectedSecurityList(securityRoles) {
    this.selectedSecurityRoleList = securityRoles ? securityRoles.split(',') : [];
  }

  setSelectedSecurityFeaturesList(securityFeatures) {
    this.selectedSecurityFeaturesList = securityFeatures ? securityFeatures.split(',') : [];
  }

  setSelectedUserPersonaTypesDropdownList(userPersonaTypesDropdownList) {
    this.selectedUserPersonaTypesList = userPersonaTypesDropdownList.split(',');
  }

  setSelectedGeoTypeList(geoTypelist) {
    this.selectedGeoTypeList = geoTypelist.value;
  }



    initialiseDatePicker() {
        this.bsConfig = BS_DEFAULT_CONFIG;
        this.bsConfig.containerClass = 'theme-red calendar-align-right';
    }

    typeaheadOnSelect(data) {
        if (this.existingUsersList?.length > 0 && this.isUserAlreadyAdded(data.item.employeeCode)) {
            this.notificationService.showWarning(ValidationService.selectedUserAlreadyExist, 'Warning');
            this.resetForm();
        } else {
            this.invalidAdminUser = false;
            this.selectedUserToAdd = data.item;
            this.resetValidationSummary();
        }
    }

    isValidUser() {
        if (this.isUserSelectedAndNotEmpty()) {
            this.invalidAdminUser = false;
        } else {
            this.invalidAdminUser = true;
        }
    }

    closeForm() {
        this.bsModalRef.hide();
    }

    resetForm() {
        this.asyncUsersSearchString = null;
        this.selectedUserToAdd = '';
    }

    onEndDateChange(date) {
        if (date === null || ValidationService.validateDate(date).isValid) {
            this.isEndDateInvalid = false;
            this.resetValidationSummary();
            this.endDate = date === null ? date : DateService.convertDateInBainFormat(date);
        } else {
            this.isEndDateInvalid = true;
        }
    }

    isWFPRoleSelected(){
      return this.selectedSecurityRoleList.includes(BossSecurityRole.WFP);
    }


    onAddUserClick() {
        if (this.securityUserTypeCode === 'U' && this.validateAddAdminUserObject()) {

          this.addSelectedUserEventEmitter.emit({
            user: this.selectedUserToAdd,  
            endDate: this.endDate, 
            notes: this.adminNotes,
            roleCodes: this.selectedSecurityRoleList?.join(),
            userTypeCode: this.selectedUserPersonaTypesList?.join(),
            geoType: this.selectedGeoTypeList,
            officeCodes: this.selectedOfficeList?.join(),
            serviceLineCodes: this.selectedServiceLinesList?.join(),
            positionGroupCodes: this.selectedPositionsList?.join(),
            levelGrades: this.selectedLevelGradesList?.join(),
            practiceAreaCodes: this.selectedPracticeAreaList?.join(),
            ringfenceCodes: this.selectedRingfenceDropdownList?.join()
          });
        
          this.closeForm();
        }
        else if(this.securityUserTypeCode === 'UG' && this.validateAddAdminGroupObject()){
          this.groupName = this.groupName.trim();
          this.addSelectedGroupEventEmitter.emit({
            groupName: this.groupName,  
            notes: this.adminNotes,
            roleCodes: this.selectedSecurityRoleList?.join(),
            featureCodes: this.selectedSecurityFeaturesList?.join()
          });
          this.closeForm();
        }
    }

    private validateAddAdminUserObject() {
        if (!this.isUserSelectedAndNotEmpty()) {
            this.validationSummaryMsg = ValidationService.invalidAdminUserMsg;
            this.resetForm();
            return false;
        }
        if (this.endDate !== null && !ValidationService.validateDate(this.endDate).isValid) {
            this.validationSummaryMsg = ValidationService.dateInvalidMessage;
            return false;
        }

        if (this.selectedSecurityRoleList.includes(BossSecurityRole.WFP)) {
          if ((!this.selectedOfficeList || this.selectedOfficeList.length === 0) || 
              (!this.selectedServiceLinesList || this.selectedServiceLinesList.length === 0)) {
              this.validationMessage = "Please select office and service line";
              return false;
          }
        }

        if(!this.selectedSecurityRoleList  || this.selectedSecurityRoleList.length === 0){
          this.validationMessage = "Please select a BOSS role";
          return false;
        }
        this.resetValidationSummary();
        return true;
    }

    private validateAddAdminGroupObject(){
      if(this.groupName === ''){
        this.validationMessage = "Please enter a group name";
        return false;
      }

      if(this.isGroupAlreadyAdded(this.groupName.trim())){
        this.validationMessage = "Group name already exists";
        return false;
      }
      
      if((!this.selectedSecurityRoleList  || this.selectedSecurityRoleList.length === 0) &&
      (!this.selectedSecurityFeaturesList || this.selectedSecurityFeaturesList.length === 0)){
        this.validationMessage = "Please select either a BOSS role or feature";
        return false;
      }

      this.resetValidationSummary();
      return true;
    }

    typeaheadNoResultsHandler(event: boolean): void {
        this.invalidAdminUser = event;
    }

    resetValidationSummary() {
        this.validationSummaryMsg = '';
    }

    private isUserAlreadyAdded(employeeCode: string) {
        return this.existingUsersList.some((securityUser) => securityUser.employeeCode.toLowerCase() === employeeCode.toLowerCase());
    }

    private isGroupAlreadyAdded(groupName: string) {
      return this.existingGroupsList.some((group) => group.groupName.toLowerCase() === groupName.toLowerCase());
  }

    private isUserSelectedAndNotEmpty() {
        return !!this.selectedUserToAdd && !!this.asyncUsersSearchString && !this.invalidAdminUser;
    }

    private attachEventForUserSearch() {
        this.users = Observable.create((observer: any) => {
            observer.next(this.asyncUsersSearchString);
        }).pipe(
            debounceTime(1000),
            mergeMap((token: string) => this.sharedService.getResourcesBySearchString(token))
        );
    }
}
