// -------------------Angular Operators---------------------------------------//
import { Component, OnInit, Input } from '@angular/core';

// -------------------Constants----------------------------------------//
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';

// -------------------Interfaces---------------------------------------//
import { Office } from 'src/app/shared/interfaces/office.interface';
import { OfficeHierarchy } from 'src/app/shared/interfaces/officeHierarchy.interface';
import { ServiceLine } from 'src/app/shared/constants/enumMaster';
import { UserPreferences } from 'src/app/shared/interfaces/userPreferences.interface';

// -------------------Services-------------------------------------------//
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { LevelGrade } from 'src/app/shared/interfaces/levelGrade.interface';
import { ServiceLineHierarchy } from 'src/app/shared/interfaces/serviceLineHierarchy';
import { ValidationService } from 'src/app/shared/validationService';
import { PracticeArea } from 'src/app/shared/interfaces/practiceArea.interface';
import { PositionHierarchy } from 'src/app/shared/interfaces/positionHierarchy.interface';
import { AffiliationRole } from 'src/app/shared/interfaces/affiliationRole.interface';
import { CoreService } from 'src/app/core/core.service';
import { StaffableAsType } from '../../interfaces/staffableAsType.interface';

@Component({
  selector: 'app-supply-settings',
  templateUrl: './supply-settings.component.html',
  styleUrls: ['./supply-settings.component.scss']
})
export class SupplySettingsComponent implements OnInit {
  // -------------------Local variables-------------------------------------------//

  isDateIntervalDisabled = false;
  isVacationThresholdDisabled = false;
  isTrainingThresholdDisabled = false;
  showDemandSettingsForm = false;
  selectedSupplyViewOfficeList = [];
  selectedStaffingTagList = [];
  selectedPositionList = [];
  selectedLevelGradeList = [];
  selectedAvailabilityIncludesList = [];
  selectedGroupByList = [];
  selectedSortByList = [];
  serviceLineEnum: typeof ServiceLine = ServiceLine;
  selectedPracticeAreaCodes = [];
  selectedAffiliationRoleCodes = [];
  selectedStaffableAsList = [];
  isAffiliationRoleShown: boolean = false;

  // -------------------Variable for Master List from DB ---------------------------------------------------//

  groupsBy = [];
  levelGrades: LevelGrade[] = [];
  availabilityIncludes = [];
  officeHierarchy: OfficeHierarchy;
  staffingTags: ServiceLineHierarchy[] = [];
  sortsBy = [];
  practiceAreas: PracticeArea[] = [];
  affiliationRoles: AffiliationRole[] = [];
  positionsHierarchy: PositionHierarchy[] = [];
  staffableAsTypes: StaffableAsType[];

  // -------------------Data sources for Dropdowns-------------------------------------------//

  groupByDropdownList = {};
  levelGradeDropdownList = {};
  availabilityIncludesDropdownList = {};
  staffingTagDropdownList = {};
  sortByDropdownList = {};
  practiceAreaDropdownList = {};
  affiliationRoleDropdownList = {};
  positionDropdownList = {};
  staffableAsDropdownList = {};

  // -------------------Input Variables -------------------------------------------//

  @Input() userPreferences: UserPreferences;
  @Input() loggedInUserHomeOffice: Office;

  // -------------------Constructor -------------------------------------------//

  constructor(private localStorageService: LocalStorageService,
     private coreService: CoreService) { }

  // -------------------Show and Hide of Affiliation Roles Filter----------------//

     isStringNullOrEmpty(value) {
      if (value === null || value === undefined || value === '') {
        return true;
      } else {
        return false;
      }

    }

  showOrHideAffiliationRoleFilter(){
      this.isAffiliationRoleShown= this.selectedPracticeAreaCodes!==undefined && this.selectedPracticeAreaCodes.length > 0  ? true: false;

      if(!this.isAffiliationRoleShown){
          this.selectedAffiliationRoleCodes=[];
      }
  }

  // -------------------Life Cycle Events -------------------------------------------//

  ngOnInit() {

    this.getMasterDataForDropDowns();
    this.initializeDropDowns();
    this.setUserSettings();

  }

  // -------------------Helper Functions -------------------------------------------//

  getMasterDataForDropDowns() {
    //TODO: Hard coding for now to proivde EMEA practice staffing users access to only EMEA data.
    //DELETE once integrated multiple-role based office security is implemented
    if(this.coreService.loggedInUserClaims.Roles?.includes('PracticeStaffing')){
      this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.accessibleOfficeHierarchyForUser);
    }else{
      this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.officeHierarchy);
    }

    this.levelGrades = this.localStorageService.get(ConstantsMaster.localStorageKeys.levelGradesHierarchy);
    this.staffingTags = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffingTagsHierarchy);
    this.positionsHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.positionsHierarchy);
    this.practiceAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.practiceAreas);
    this.affiliationRoles=this.localStorageService.get(ConstantsMaster.localStorageKeys.affiliationRoles);
    this.staffableAsTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffableAsTypes);
    this.availabilityIncludes = ConstantsMaster.availabilityIncludes;
    this.groupsBy = ConstantsMaster.groupBy;
    this.sortsBy = ConstantsMaster.sortBy;


  }

  initializeDropDowns() {
    this.sortByDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.sortsBy.map(item => {
        return {
          text: item.name,
          value: item.code,
          checked: false
        };
      })
    };

    this.groupByDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.groupsBy.map(item => {
        return {
          text: item.name,
          value: item.code,
          checked: false
        };
      })
    };

    this.staffingTagDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.staffingTags.map((item) => {
        return {
          text: item.text,
          value: item.value,
          collapsed: true,
          children: item.children != null ? item.children.map(child => {
            return {
              text : child.text,
              value : child.value,
              checked: false
            };
          }) : null,
          checked: false
        };
      })
    };

    this.positionDropdownList = {
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
              text : child.text,
              value : child.value,
              checked: false
            };
          }) : null,
          checked: false
        };
      })
    };

    this.levelGradeDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.levelGrades.map((item) => {
        return {
          text: item.text,
          value: item.value,
          collapsed: true,
          children: item.children.map(child => {
            return {
              text : child.text,
              value : child.value,
              checked: false
            };
          }),
          checked: false
        };
      })
    };

    if (this.staffableAsTypes) {
      this.staffableAsTypes = this.staffableAsTypes.filter(x => x.staffableAsTypeCode > -1);
      this.staffableAsDropdownList = {
        text: 'All',
        value: 0,
        checked: false,
        children: this.staffableAsTypes.map(item => {
          return {
            text: item.staffableAsTypeName,
            value: item.staffableAsTypeCode.toString(),
            checked: false
          }
        })
      };
    }

    this.availabilityIncludesDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.availabilityIncludes.map(item => {
        return {
          text: item.name,
          value: item.code,
          checked: false
        };
      })
    };

    this.practiceAreaDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.practiceAreas.map(item => {
          return {
              text: item.practiceAreaName,
              value: item.practiceAreaCode,
              checked: false
          }
      })
  };

    this.affiliationRoleDropdownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.affiliationRoles.map(item => {
          return {
              text: item.roleName,
              value: item.roleCode,
              checked: false
          }
      })
  };


  }

  setUserSettings() {
    /*
      ** if user has saved some user prefrences, then set them else set defaults
      ** assigning the saved preferences to variables will bind/check the corresponding dropdown items
    */

    if (this.userPreferences && this.userPreferences.supplyViewOfficeCodes) {
      this.selectedSupplyViewOfficeList = this.userPreferences.supplyViewOfficeCodes.split(',');
    } else {
      this.selectedSupplyViewOfficeList.push(this.loggedInUserHomeOffice.officeCode.toString());
    }

    if (this.userPreferences && this.userPreferences.supplyViewStaffingTags) {
      this.selectedStaffingTagList = this.userPreferences.supplyViewStaffingTags.split(',');
    } else {
      this.selectedStaffingTagList = [this.serviceLineEnum.GeneralConsulting];
    }

    if (this.userPreferences && this.userPreferences.levelGrades) {
      this.selectedLevelGradeList = this.userPreferences.levelGrades.split(',');
    }

    if (this.userPreferences && this.userPreferences.staffableAsTypeCodes) {
      this.selectedStaffableAsList = this.userPreferences.staffableAsTypeCodes.split(',');
    }

    if (this.userPreferences && this.userPreferences.availabilityIncludes) {
      this.selectedAvailabilityIncludesList = this.userPreferences.availabilityIncludes.split(',');
    }

    if (this.userPreferences && this.userPreferences.groupBy) {
      this.selectedGroupByList = this.userPreferences.groupBy.split(',');
    } else {
      this.selectedGroupByList = ['serviceLine'];
    }

    if (this.userPreferences && this.userPreferences.sortBy) {
      this.selectedSortByList = this.userPreferences.sortBy.split(',');
    } else {
      this.selectedSortByList = ['levelGrade'];
    }

    if (this.userPreferences && this.userPreferences.practiceAreaCodes) {
      //this.selectedPracticeAreaCodes = this.userPreferences.practiceAreaCodes.split(',');
      this.selectedPracticeAreaCodes = !this.isStringNullOrEmpty(this.userPreferences.practiceAreaCodes)?this.userPreferences.practiceAreaCodes?.split(','): [];
      this.showOrHideAffiliationRoleFilter();
    }

    if (this.userPreferences && this.userPreferences.affiliationRoleCodes) {
      this.selectedAffiliationRoleCodes = this.userPreferences.affiliationRoleCodes.split(',');
    }

    if (this.userPreferences && this.userPreferences.positionCodes) {
      this.selectedPositionList = this.userPreferences.positionCodes.split(',');
    }

  }

  // -------------------Event Handlers -------------------------------------------//

  setSupplyWeekThreshold(supplyWeeksThreshold) {
    if (ValidationService.isInteger(supplyWeeksThreshold)) {
      this.userPreferences.supplyWeeksThreshold = supplyWeeksThreshold;
    }
  }

  setVacationThreshold(vacationThreshold) {
    if (ValidationService.isInteger(vacationThreshold)) {
      this.userPreferences.vacationThreshold = vacationThreshold;
    }
  }

  setTrainingThreshold(trainingThreshold) {
    if (ValidationService.isInteger(trainingThreshold)) {
      this.userPreferences.trainingThreshold = trainingThreshold;
    }
  }

  setSelectedSupplyViewOfficeList(officeCodes) {
    this.selectedSupplyViewOfficeList = officeCodes.split(',');
  }

  setSelectedSupplyViewLevelGradeList(levelGrades) {
    this.selectedLevelGradeList = levelGrades.split(',');
  }

  setSelectedSupplyViewStaffingTagList(staffingTags) {
    this.selectedStaffingTagList = staffingTags.split(',');
  }

  setSelectedSupplyViewPositionList(positions) {
    this.selectedPositionList = positions.split(',');
  }

  setSelectedAvailabilityIncludesList(availabilityIncludes){
    this.selectedAvailabilityIncludesList = availabilityIncludes.split(',');
  }

  setSelectedSupplyViewSortByList(sortByList) {
    this.selectedSortByList = sortByList.split(',')
  }

  setSelectedSupplyViewGroupByList(groupBy) {
    this.selectedGroupByList = groupBy.split(',');
  }

  setSelectedSupplyViewStaffableAsList(staffableAsTypeCodes)
  {
    this.selectedStaffableAsList = staffableAsTypeCodes.split(',');
    
  }

  setSelectedPracticeAreaList(practiceAreas) {
    //this.selectedPracticeAreaCodes = practiceAreas.split(',');
    this.selectedPracticeAreaCodes = !this.isStringNullOrEmpty(practiceAreas)?practiceAreas?.split(','): [];
    this.showOrHideAffiliationRoleFilter();
  }

  setSelectedAffiliationRolesList(affiliationRoleCodes) {
    this.selectedAffiliationRoleCodes = affiliationRoleCodes.split(',');
  }
}
