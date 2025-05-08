import { Component, Input, OnInit, OnChanges, SimpleChanges, Output, EventEmitter } from "@angular/core";
import { BsDatepickerConfig } from "ngx-bootstrap/datepicker";
import { BS_DEFAULT_CONFIG } from "../constants/bsDatePickerConfig";
import { ConstantsMaster } from "../constants/constantsMaster";
import { ServiceLine } from "../constants/enumMaster";
import { Certificate } from "../interfaces/certificate.interface";
import { CommitmentType } from "../interfaces/commitmentType.interface";
import { Language } from "../interfaces/language";
import { LevelGrade } from "../interfaces/levelGrade.interface";
import { Office } from "../interfaces/office.interface";
import { PositionHierarchy } from "../interfaces/positionHierarchy.interface";
import { PracticeArea } from "../interfaces/practiceArea.interface";
import { ResourceFilter } from "../interfaces/resource-filter.interface";
import { AffiliationRole } from "../interfaces/affiliationRole.interface";
import { ServiceLineHierarchy } from "../interfaces/serviceLineHierarchy";
import { StaffableAsType } from "../interfaces/staffableAsType.interface";
import { UserPreferences } from "../interfaces/userPreferences.interface";


@Component({
    selector: 'app-side-bar-filters',
    templateUrl: 'side-bar-filters.component.html',
    styleUrls: ['./side-bar-filters.component.scss']
})
export class SidebarFiltersComponent implements OnInit, OnChanges {
    @Output() closeFilterEmitter = new EventEmitter();
    @Output() updateThresholdRangeEmitter = new EventEmitter();
    @Output() getResources = new EventEmitter();
    @Output() getResourcesSortBySelectedValues = new EventEmitter();
    @Output() showCommitmentBySelectedValues = new EventEmitter();
    @Output() upsertResourceFilter = new EventEmitter();
    @Output() deleteSavedFilter = new EventEmitter<string>();

    @Input() officeHierarchy;
    @Input() officeFlatList: Office[];
    @Input() staffingTagsHierarchy: ServiceLineHierarchy[];
    @Input() staffingTags: ServiceLine[];
    @Input() levelGrades: LevelGrade[];
    @Input() commitmentTypes: CommitmentType[];
    @Input() certificates: Certificate[];
    @Input() languages: Language[];
    @Input() practiceAreas: PracticeArea[];
    @Input() affiliationRoles:AffiliationRole[];
    @Input() dateRange: [Date, Date];
    @Input() updateFiltersWithUserPreferences: boolean;
    @Input() filterConfig;
    @Input() userPreferences: UserPreferences;
    @Input() savedResourceFilters: ResourceFilter[];
    @Input() staffableAsTypes: StaffableAsType[];
    @Input() positionsHierarchy: PositionHierarchy[];
    @Input() sortsBy;

    public readonly resourcesFilters = ConstantsMaster.resourcesFilter;
    public serviceLineEnum: typeof ServiceLine = ServiceLine;
    public selectedOfficeList = [];
    public selectedLevelGradeList = [];
    public selectedPositionList = [];
    public levelGradeDropdownList;
    public staffingTagDropdownList;
    public sortByDropdownList;
    public roleNameDropdownList;
    public selectedStaffingTagList = [];
    public selectedSortByList = [];
    public employeeStatus = [];
    public commitmentTypesDropDownList;
    public selectedCommitmentTypeList = [];
    public certificatesDropDownList;
    public selectedCertificatesList = [];
    public languagesDropDownList;
    public practiceAreaDropDownList;
    public positionDropdownList;
    public staffableAsDropdownList;
    public selectedLanguagesList = [];
    public selectedRoleNameList=[];
    public selectedPracticeAreaList = [];
    public employeeStatusDropDownList;
    public selectedEmployeeStatusList = [];
    public bsConfig: Partial<BsDatepickerConfig>;
    public selectedDateRange: any;
    public selectedFiltersTitle: string;
    public selectedSavedFilter: ResourceFilter;
    public updateFilters = false;
    public showErrorMessage = false;
    public errorMessage = '*Title is Mandatory!';
    public selectedStaffableAsList = [];
    public isDefault = false;
    public readonly filter = ConstantsMaster.appScreens.resourcesFilter;
    public isAffiliationRoleShown: boolean = false;

    constructor() { }

    isStringNullOrEmpty(value) {
        if (value === null || value === undefined || value === '') {
          return true;
        } else {
          return false;
        }

      }

    showOrHideAffiliationRoleFilter(){
        this.isAffiliationRoleShown= this.selectedPracticeAreaList!==undefined && this.selectedPracticeAreaList.length > 0  ? true: false;

        if(!this.isAffiliationRoleShown){
            this.selectedRoleNameList=[];
        }
    }

    ngOnInit() {
        this.initializeDateConfig();
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes.dateRange && this.dateRange) {
            this.selectedDateRange = this.dateRange;
        }
        // if (changes.levelGrades && this.levelGrades) {
        //     this.selectAllLevelGrades();
        // }
        const isDefaultSavedFilterExist = this.savedResourceFilters?.some(f => f.isDefault);

        if (changes.userPreferences && this.userPreferences && !isDefaultSavedFilterExist) {
            this.updateFiltersOnUserPrefrencesChange();
        }

        if (isDefaultSavedFilterExist) {
            this.setDropdownFilterValues(this.savedResourceFilters.find(f => f.isDefault));
        }
    }

    closeFilters() {
        this.closeFilterEmitter.emit();
    }

    getResourcesBySelectedOfficeHandler(officeCodes) {
        if (officeCodes && this.isArrayEqual(this.selectedOfficeList.map(String), officeCodes.split(','))) {
            return false;
        }
        this.selectedOfficeList = officeCodes.split(',');
        this.getFilteredResources();
    }

    getResourcesBySelectedLevelGrades(levelGrades) {
        if (this.isArrayEqual(this.selectedLevelGradeList.map(String), levelGrades.split(','))) {
            return false;
        }
        this.selectedLevelGradeList = levelGrades.split(',');
        this.getFilteredResources();
    }

    getResourcesBySelectedPositionList(position) {
        if (this.isArrayEqual(this.selectedPositionList.map(String), position.split(','))) {
            return false;
        }
        this.selectedPositionList = position.split(',');
        this.getFilteredResources();
    }

    getResourcesBySelectedStaffableAs(staffableAsTypeCodes) {
        if (this.isArrayEqual(this.selectedStaffableAsList.map(String), staffableAsTypeCodes.split(','))) {
            return false;
        }
        this.selectedStaffableAsList = staffableAsTypeCodes.split(',');
        this.getFilteredResources();
    }

    getResourcesBySelectedStaffingTags(staffingTagCodes) {
        if (this.isArrayEqual(this.selectedStaffingTagList.map(String), staffingTagCodes.split(','))) {
            return false;
        }
        this.selectedStaffingTagList = staffingTagCodes.split(',');
        this.getFilteredResources();
    }

    getResourcesSortBySelectedValue(sortByList) {
        if (this.isArrayEqual(this.selectedSortByList.map(String), sortByList.split(','))) {
            return false;
        }

        this.selectedSortByList = sortByList.split(',');
        const sortBy = this.selectedSortByList.toString();

        this.getResourcesSortBySelectedValues.emit({ sortBy , direction : 'asc'});

    }

    getResourcesByRoleNameSelectedValue(roleNameList)
    {
        if (this.isArrayEqual(this.selectedRoleNameList.map(String), roleNameList.split(','))) {
            return false;
        }
        this.selectedRoleNameList = roleNameList.split(',');
        this.getFilteredResources();


    }

    showResourcesCommitmentsBySelectedValue(commitmentTypeList) {
        if (this.isArrayEqual(this.selectedCommitmentTypeList.map(String), commitmentTypeList.split(','))) {
            return false;
        }
        this.selectedCommitmentTypeList = commitmentTypeList.split(',');
        this.showCommitmentBySelectedValues.emit({ commitmentTypes: this.selectedCommitmentTypeList });
    }

    getResourcesEmployeeStatusBySelectedValue(employeeStatusList) {
        if (this.isArrayEqual(this.selectedEmployeeStatusList.map(String), employeeStatusList.split(','))) {
            return false;
        }
        this.selectedEmployeeStatusList = employeeStatusList.split(',');
        this.getFilteredResources();
    }

    shiftDateRange(shiftDate) {
        if (shiftDate === 'left') {
            const startDate = this.selectedDateRange[0];
            const endDate = this.selectedDateRange[1];

            startDate.setDate(startDate.getDate() - 7);
            endDate.setDate(endDate.getDate() - 7);

            this.selectedDateRange = [startDate, endDate];

        } else {

            const startDate = this.selectedDateRange[0];
            const endDate = this.selectedDateRange[1];

            startDate.setDate(startDate.getDate() + 7);
            endDate.setDate(endDate.getDate() + 7);

            this.selectedDateRange = [startDate, endDate];
        }

        this.getFilteredResources();
    }

    getResourcesforSelectedDateRange(selectedDateRange) {
        // To avoid API call during initialization we check for non nullable start and end dates
        if (!selectedDateRange || this.selectedDateRange.toString() === selectedDateRange.toString()) {
            return;
        }

        this.selectedDateRange = selectedDateRange;

        this.getFilteredResources();
    }

    setSelectedSavedResourceFilters(selectedSavedFilter: ResourceFilter) {
        this.updateFiltersOnSelectingSavedFilter(selectedSavedFilter);
    }

    deleteSavedFilterHandler(filterIdToDelete: string) {
        this.resetSavedFilterSettings();
        this.deleteSavedFilter.emit(filterIdToDelete);
    }

    private getFilteredResources() {
        const dateRange = this.selectedDateRange;
        const officeCodes = this.selectedOfficeList?.toString();
        const levelGrades = this.selectedLevelGradeList?.toString();
        const staffingTags = this.selectedStaffingTagList?.toString();
        const sortBy = this.selectedSortByList?.toString();
        const certificates = this.selectedCertificatesList?.toString();
        const languages = this.selectedLanguagesList?.toString();
        const employeeStatuses = this.selectedEmployeeStatusList?.toString();
        const practiceAreas = this.selectedPracticeAreaList?.toString();
        const roleCodes=this.selectedRoleNameList?.toString();
        const staffableAsTypeCodes = this.selectedStaffableAsList?.toString();
        const positionCodes = this.selectedPositionList?.toString();
        this.getResources.emit({ dateRange, officeCodes, levelGrades, staffingTags, sortBy,certificates, languages, employeeStatuses, practiceAreas, staffableAsTypeCodes, positionCodes, roleCodes });
    }

    private updateFiltersOnSelectingSavedFilter(selectedSavedFilter: ResourceFilter) {
        this.setSelectedSavedFilterValues(selectedSavedFilter);
        this.getFilteredResources();
        this.showCommitmentBySelectedValues.emit({ commitmentTypes: this.selectedCommitmentTypeList });
    }

    private setSelectedSavedFilterValues(savedFilter: ResourceFilter) {
        this.selectedSavedFilter = savedFilter;
        this.setDropdownFilterValues(savedFilter);
        this.selectedFiltersTitle = savedFilter.title;
        this.isDefault = savedFilter.isDefault;
        this.updateFilters = true;
    }

    private setDropdownFilterValues(resourceFilter: ResourceFilter) {
        this.initializeFilters(); // re-rendering of filters is required to show the user updated/selected labels on the dropdown
        this.selectedOfficeList = resourceFilter.officeCodes?.split(',') || [];
        this.selectedStaffingTagList = resourceFilter.staffingTags?.split(',') || [];
        this.selectedLevelGradeList = resourceFilter.levelGrades?.split(',') || [];
        this.selectedPracticeAreaList = resourceFilter.practiceAreaCodes?.split(',') || [];
        this.selectedRoleNameList=resourceFilter.affiliationRoleCodes?.split(',')||[];
        this.selectedEmployeeStatusList = resourceFilter.employeeStatuses?.split(',') || [];
        this.selectedStaffableAsList = resourceFilter.staffableAsTypeCodes?.split(',')?.map(x =>  !isNaN(parseInt(x)) ? parseInt(x) : '') || [];
        this.selectedPositionList = resourceFilter.positionCodes?.split(',') || [];
        this.showCommitmentBySelectedValues.emit({ commitmentTypes: this.selectedCommitmentTypeList });
    }

    private updateFiltersOnUserPrefrencesChange() {
        this.initializeFilters(); // re-rendering of filters is required to show the user updated/selected labels on the dropdown
        this.selectedOfficeList = this.userPreferences.supplyViewOfficeCodes?.split(',');
        this.selectedStaffingTagList = this.userPreferences.supplyViewStaffingTags?.split(',');
        this.selectedLevelGradeList = this.userPreferences.levelGrades?.split(',');
        this.selectedPositionList = this.userPreferences.positionCodes?.split(',');
        this.selectedSortByList = this.userPreferences.sortBy?.split(',');
        this.selectedRoleNameList = this.userPreferences.affiliationRoleCodes?.split(',') ?? [];
        this.selectedPracticeAreaList = !this.isStringNullOrEmpty(this.userPreferences?.practiceAreaCodes)?this.userPreferences.practiceAreaCodes?.split(','): [];
        this.resetSavedFilterSettings();
        this.showOrHideAffiliationRoleFilter();
        this.showCommitmentBySelectedValues.emit({ commitmentTypes: this.selectedCommitmentTypeList });
        //TODO: since sortBy filed is common on supply & resurces tab, but supply is driven by userSettings while resources sort by is not, so think about making a combined logic when redoing th esupply side filetrs design
        //Make sure prod bug: 71921 is tested for when redesign happens on supply
    }

    resetSavedFilterSettings() {
        this.updateFilters = false;
        this.selectedFiltersTitle = '';
        this.isDefault = false;
    }

    updateThresholdRangeHandler(data) {
        this.updateThresholdRangeEmitter.emit(data);
    }

    showResourcesCertificatesBySelectedValue(certificatesList) {
        if (this.isArrayEqual(this.selectedCertificatesList.map(String), certificatesList.split(','))) {
            return false;
        }
        this.selectedCertificatesList = certificatesList.split(',');
        this.getFilteredResources();
    }

    showResourcesLanguagesBySelectedValue(languagesList) {
        if (this.isArrayEqual(this.selectedLanguagesList.map(String), languagesList.split(','))) {
            return false;
        }
        this.selectedLanguagesList = languagesList.split(',');
        this.getFilteredResources();
    }

    showResourcesPracticeAreaBySelectedValue(practiceAreaList) {
        if (this.isArrayEqual(this.selectedPracticeAreaList.map(String), practiceAreaList.split(','))) {
            return false;
        }
        this.selectedPracticeAreaList = !this.isStringNullOrEmpty(practiceAreaList)?practiceAreaList?.split(','): [];

        this.showOrHideAffiliationRoleFilter();
        this.getFilteredResources();
    }


    showResourcesRoleNameBySelectedValue(roleNameList) {
        if (this.isArrayEqual(this.selectedRoleNameList.map(String), roleNameList.split(','))) {
            return false;
        }
        this.selectedRoleNameList = roleNameList.split(',');
        this.getFilteredResources();
    }



    upsertFiltersForLoggedInEmployee(saveNew = true) {
        if (this.validateInput()) {
            const resourceFiltersData: ResourceFilter[] = [];

            const resourceFilter: ResourceFilter = {
                id: saveNew ? null : this.selectedSavedFilter?.id,
                title: this.selectedFiltersTitle || '',
                isDefault: this.isDefault,
                officeCodes: this.selectedOfficeList.toString(),
                staffingTags: this.selectedStaffingTagList.toString(),
                levelGrades: this.selectedLevelGradeList.toString(),
                employeeStatuses: this.selectedEmployeeStatusList.toString(),
                practiceAreaCodes: this.selectedPracticeAreaList.toString(),
                lastUpdatedBy: null,
                staffableAsTypeCodes: this.selectedStaffableAsList.toString(),
                positionCodes: this.selectedPositionList.toString(),
                affiliationRoleCodes:this.selectedRoleNameList.toString(),
                sharedWith: null
            }
            resourceFiltersData.push(resourceFilter);
            this.upsertResourceFilters(resourceFiltersData);
            this.resetSavedFilterSettings();
        }
    }

    upsertResourceFilters(resourceFiltersData: ResourceFilter[]) {
        this.upsertResourceFilter.emit(resourceFiltersData);
    }

    onSavedFiltersTabClick() {
        this.showErrorMessage = false;
        if (!this.updateFilters) {
            this.resetSavedFilterSettings();
        }
    }

    setAsDefaultFilterHandler(savedFilters: ResourceFilter[]) {
        this.upsertResourceFilters(savedFilters);
    }

    private initializeDateConfig() {
        this.bsConfig = BS_DEFAULT_CONFIG;
        this.bsConfig.containerClass = 'theme-red calendar-dropdown calendar-align-right';
    }

    private initializeFilters() {
        this.initializeOfficesFilter();
        this.initializeStaffingTagsFilter();
        this.initializeLevelGradesFilter();
        this.initializeCommitmentTypesFilter();
        this.initializeCertificatesFilter();
        this.initializeLanguagesFilter();
        this.initializeSortByFilter();
        this.initializeRoleNameFilter();
        this.initializePracticeAreaFilter();
        this.initializeEmployeeStatusFilter();
        this.initializeStaffableAsFilter();
        this.initializePositionFilter();
    }

    private selectAllLevelGrades() {
        if (this.levelGrades) {
            const levelGradeChildrenList = this.levelGrades.map(item => {
                return {
                    text: item.text,
                    value: item.value,
                    collapsed: true,
                    children: item.children.map(child => {
                        return {
                            text: child.text,
                            value: child.value,
                            checked: true
                        };
                    }),
                    checked: true
                };
            });
            this.levelGradeDropdownList = {
                text: 'All',
                value: 0,
                checked: true,
                children: levelGradeChildrenList
            };
        }
    }

    private initializeOfficesFilter() {
        if (this.officeHierarchy) {
            this.officeHierarchy = JSON.parse(JSON.stringify(this.officeHierarchy));
        }
        this.selectedOfficeList = [];
    }

    private initializeStaffableAsFilter() {
        if (this.staffableAsTypes) {
            this.staffableAsTypes = this.staffableAsTypes.filter(x => x.staffableAsTypeCode > -1);
            this.staffableAsDropdownList = {
                text: 'All',
                value: 0,
                checked: false,
                children: this.staffableAsTypes.map(item => {
                    return {
                        text: item.staffableAsTypeName,
                        value: item.staffableAsTypeCode,
                        checked: false
                    }
                })
            };
        }
        this.selectedStaffableAsList = [];
    }

    private initializePositionFilter() {
        if (this.positionsHierarchy && this.userPreferences) {
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
                                text: child.text,
                                value: child.value,
                                checked: false
                            };
                        }) : null,
                        checked: false
                    };
                })
            };

            this.selectedPositionList = this.userPreferences.positionCodes ? this.userPreferences.positionCodes.split(',') : [];
        }
    }

    private initializePracticeAreaFilter() {
        if (this.practiceAreas) {
            this.practiceAreaDropDownList = {
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
        }
        this.selectedPracticeAreaList = [];
    }

    private initializeStaffingTagsFilter() {
        if (this.staffingTagsHierarchy) {
            this.staffingTagDropdownList = {
                text: 'All',
                value: 0,
                checked: false,
                children: this.staffingTagsHierarchy.map((item) => {
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
        }
        this.selectedStaffingTagList = [];
    }

    private initializeLevelGradesFilter() {
        if (this.levelGrades) {
            const levelGradeChildrenList = this.levelGrades.map(item => {
                return {
                    text: item.text,
                    value: item.value,
                    collapsed: true,
                    children: item.children.map(child => {
                        return {
                            text: child.text,
                            value: child.value,
                            checked: false
                        };
                    }),
                    checked: false
                };
            });
            this.levelGradeDropdownList = {
                text: 'All',
                value: 0,
                checked: false,
                children: levelGradeChildrenList
            };
            this.selectedLevelGradeList = [];
        }
    }

    private initializeCommitmentTypesFilter() {
        this.commitmentTypes = this.commitmentTypes.filter(type => type.commitmentTypeCode != '');
        this.commitmentTypesDropDownList = {
            text: 'All',
            value: 0,
            checked: false,
            children: this.commitmentTypes.map(item => {
                return {
                    text: item.commitmentTypeName,
                    value: item.commitmentTypeCode,
                    checked: false
                };
            })
        };
        this.selectedCommitmentTypeList = this.commitmentTypes.map(item => item.commitmentTypeCode); // Select ALL as the default option for Commitment Type
    }

    private initializeCertificatesFilter() {
        if (this.certificates) {
            this.certificatesDropDownList = {
                text: 'All',
                value: 0,
                checked: false,
                children: this.certificates.map(item => {
                    return {
                        text: item.name,
                        value: item.name,
                        checked: false
                    };
                })
            };
            this.selectedCertificatesList = [];
        }
    }

    private initializeLanguagesFilter() {
        if (this.languages) {
            this.languagesDropDownList = {
                text: 'All',
                value: 0,
                checked: false,
                children: this.languages.map(item => {
                    return {
                        text: item.name,
                        value: item.name,
                        checked: false
                    };
                })
            };
            this.selectedLanguagesList = [];
        }
    }

    private initializeSortByFilter() {
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
        this.selectedSortByList = [];
    }

    private initializeRoleNameFilter(){
        this.roleNameDropdownList={
            text:'All',
            value:0,
            checked:false,
            children: this.affiliationRoles.map(item=>{
                return{
                    text:item.roleName,
                    value:item.roleCode,
                    checked:false
                };
            })
        };
        this.selectedRoleNameList=[];
    }

    private initializeEmployeeStatusFilter() {
        this.employeeStatus = ConstantsMaster.employeeStatus;
        this.employeeStatusDropDownList = {
            text: 'All',
            value: 0,
            checked: false,
            children: this.employeeStatus.map(item => {
                return {
                    text: item.name,
                    value: item.code,
                    checked: false
                };
            })
        };
        this.selectedEmployeeStatusList = this.employeeStatus.map(item => item.code); // Select ALL as the default option for Commitment Type
    }

    private isArrayEqual(array1, array2) {
        return JSON.stringify(array1) === JSON.stringify(array2);
    }

    private validateInput() {
        if (!this.selectedFiltersTitle) {
            this.showErrorMessage = true;
        }
        else {
            this.showErrorMessage = false;
        }
        return !this.showErrorMessage;
    }
}
