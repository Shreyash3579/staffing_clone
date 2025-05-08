import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { DateService } from 'src/app/shared/dateService';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { BS_DEFAULT_CONFIG } from 'src/app/shared/constants/bsDatePickerConfig';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { CommitmentType } from 'src/app/shared/interfaces/commitmentType.interface';
import { FilterRow } from 'src/app/shared/interfaces/filter-row.interface';
import { ResourceFiltersBasicMenu } from 'src/app/shared/interfaces/resource-filters-basic-menu.interface';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { NotificationService } from 'src/app/shared/notification.service';
import { Certificate } from 'src/app/shared/interfaces/certificate.interface';
import { Language } from 'src/app/shared/interfaces/language';
import { PracticeArea } from 'src/app/shared/interfaces/practiceArea.interface';
import { CommitmentTypeHierarchy as CommitmentTypeHierarchyEnum } from 'src/app/shared/constants/enumMaster';
import { CommitmentType as  CommitmentTypeEnum} from 'src/app/shared/constants/enumMaster';
import { ResourceViewCD } from 'src/app/shared/interfaces/resource-view-cd.interface';
import { ResourceViewCommercialModel } from 'src/app/shared/interfaces/resource-view-commercial-model.interface';

@Component({
  selector: 'app-filter-by',
  templateUrl: './filter-by.component.html',
  styleUrls: ['./filter-by.component.scss']
})
export class FilterByComponent implements OnInit {

  @Input() menuPosition: string = "down";
  @Input() showHeader: boolean = true;
  @Input() 
  set rowsToEdit(value: FilterRow[]) {
    this._rowsToEdit = value;
    this.filterBy = this._rowsToEdit;
    this.setFilteredFieldOptions();
  }
  get rowsToEdit() {
    return this._rowsToEdit;
  }

  @Input() resourcesRecentCDList:ResourceViewCD[];
  @Input() resourcesCommercialModelList:ResourceViewCommercialModel[];

  public _rowsToEdit: FilterRow[];
  

  filterBy: FilterRow[] = [];

  public readonly AND_OR_ENUM = {
    AND: "and",
    OR: "or"
  }

  andOrLabel: string = this.AND_OR_ENUM.AND;
  andOrOptions: ResourceFiltersBasicMenu[] = [
    { label: "and", value: this.AND_OR_ENUM.AND, selected: false },
    { label: "or", value: this.AND_OR_ENUM.OR, selected: false }
  ]

  public readonly FIELDOPTIONENUM = {
    AVAILABLE_PERCENTAGE: "availabilityPercentage",
    AVAILABLE_DATE: "availabilityDate",
    HIRE_DATE: "hireDate",
    LAST_DATE_STAFFED: "lastDateStaffed",
    COMMITMENT_TYPE: "commitment",
    CERTIFICATES: "certificates",
    LANGUAGES: "languages",
    INDUSTRY_AND_CAPABILITY:"industry/capability",
    RECENT_CD:"recentCD",
    COMMERCIAL_MODEL:"commercialModel"
  }
  public readonly FIELDOPERATORENUM = {
    GREATER_THAN: "greaterThan",
    LESSER_THAN: "lesserThan",
    BETWEEN: "between",
    EQUALS: "equals",
    NOT_EQUALS: "notEquals",
    INTERESTED: "interested",
    NOT_INTERESTED: "notInterested",
    INCLUDES : "Includes",
    COMMERCIAL_MODEL:"commercialModel"
  }

  filterFieldOptions: ResourceFiltersBasicMenu[] = [
    { label: "Availability %", value: this.FIELDOPTIONENUM.AVAILABLE_PERCENTAGE, selected: false, isHidden: false },
    { label: "Availability Date", value: this.FIELDOPTIONENUM.AVAILABLE_DATE, selected: false, isHidden: false },
    { label: "Hire Date", value: this.FIELDOPTIONENUM.HIRE_DATE, selected: false, isHidden: false },
    { label: "Last date staffed on billable", value: this.FIELDOPTIONENUM.LAST_DATE_STAFFED, selected: false, isHidden: false },
    { label: "Commitment", value: this.FIELDOPTIONENUM.COMMITMENT_TYPE, selected: false,  isHidden: false },
    { label: "Certificates", value: this.FIELDOPTIONENUM.CERTIFICATES, selected: false,  isHidden: false },
    { label: "Languages", value: this.FIELDOPTIONENUM.LANGUAGES, selected: false,  isHidden: false },
    { label: "Industry/Capability Preference", value: this.FIELDOPTIONENUM.INDUSTRY_AND_CAPABILITY, selected: false,  isHidden: false },
    { label: "Recent CD", value: this.FIELDOPTIONENUM.RECENT_CD, selected: false,  isHidden: false },
    { label: "Commercial Model",value: this.FIELDOPTIONENUM.COMMERCIAL_MODEL, selected: false,  isHidden: false  }
  ];

  filterOperatorOptions: ResourceFiltersBasicMenu[] = [
    { label: ">", value: this.FIELDOPERATORENUM.GREATER_THAN, selected: false, isHidden: false },
    { label: "<", value: this.FIELDOPERATORENUM.LESSER_THAN, selected: false, isHidden: false },
    { label: "range (is between)", value: this.FIELDOPERATORENUM.BETWEEN, selected: false, isHidden: false },
    { label: "Is equal to", value: this.FIELDOPERATORENUM.EQUALS, selected: false, isHidden: false },
    { label: "Does not equal", value: this.FIELDOPERATORENUM.NOT_EQUALS, selected: false, isHidden: false },
    { label: "Has interest in", value: this.FIELDOPERATORENUM.INTERESTED, selected: false, isHidden: false },
    { label: "Not interested in", value: this.FIELDOPERATORENUM.NOT_INTERESTED, selected: false, isHidden: false },
    { label: "Includes", value: this.FIELDOPERATORENUM.INCLUDES, selected: false, isHidden: false },
    { label: "Commercial Model", value: this.FIELDOPERATORENUM.COMMERCIAL_MODEL, selected: false, isHidden: false}
  ];

  public commitmentTypesDropDownList;
  public certificatesDropDownList;
  public languagesDropDownList;
  public industryAndCapabilityDropDownList;
  public recentCdDropdownList;
  public commercialModelDropdownList;
  public selectedCommitmentTypeList = [];
  public commitmentTypes: CommitmentType[] = [];
  public certificates: Certificate[];
  public languages: Language[];
  public industryAndCapabilities :PracticeArea[];
  bsConfig: Partial<BsDatepickerConfig>;

  constructor(
    private notifyService: NotificationService,
    private localStorageService: LocalStorageService,
  ) { }

  ngOnInit(): void {
    this.getData();
    this.initializeDateConfig();
    this.initializeCommitmentTypesFilter();
    this.initializeCertificatesFilter();
    this.initializeLanguagesFilter();
    this.initializeIndustryAndCapabilityFilter();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.rowsToEdit && this.rowsToEdit) {
      this.mapSavedFiltersToModels();
    }
    if (changes.resourcesRecentCDList && this.resourcesRecentCDList){
      this.initializeRecentCDFilter();
    }

    if(changes.resourcesCommercialModelList && this.resourcesCommercialModelList){
      this.initializeCommercialModelFilter();
    }
  }

  getData() {
    if (this.rowsToEdit) {
      this.filterBy = this.rowsToEdit;
    } else {
      this.filterBy = [];
    }
  }

  initializeDateConfig() {
    this.bsConfig = Object.assign({}, BS_DEFAULT_CONFIG);
    this.bsConfig.containerClass = "theme-dark-blue calendar-dropdown calendar-align-left";
  }


  mapSavedFiltersToModels() {
    this.filterBy.forEach(filterRow => {
      if (filterRow.filterField == this.FIELDOPTIONENUM.COMMITMENT_TYPE || filterRow.filterField == this.FIELDOPTIONENUM.LANGUAGES || filterRow.filterField == this.FIELDOPTIONENUM.CERTIFICATES || filterRow.filterField == this.FIELDOPTIONENUM.INDUSTRY_AND_CAPABILITY ||
        filterRow.filterField == this.FIELDOPTIONENUM.RECENT_CD ||  filterRow.filterField == this.FIELDOPTIONENUM.COMMERCIAL_MODEL
       ) {
          filterRow.parsedValue = filterRow.filterValue.split(',');
      } else if(filterRow.filterField == this.FIELDOPTIONENUM.AVAILABLE_DATE || filterRow.filterField == this.FIELDOPTIONENUM.HIRE_DATE || filterRow.filterField == this.FIELDOPTIONENUM.LAST_DATE_STAFFED) {
          if (filterRow.filterOperator == this.FIELDOPERATORENUM.BETWEEN) {
            filterRow.parsedValue = this.ConvertStringToDateRange(filterRow.filterValue);
          } else{
            filterRow.filterValue = DateService.convertDateInBainFormat(filterRow.filterValue);
          }
      } else if(filterRow.filterField == this.FIELDOPTIONENUM.AVAILABLE_PERCENTAGE && filterRow.filterOperator == this.FIELDOPERATORENUM.BETWEEN) {
        filterRow.parsedValue = this.ConvertAvailabilityPercentInRangeFormat(filterRow);
      }
    })

    this.andOrLabel = this.filterBy[0]?.andOr;
  }

  ConvertStringToDateRange(selectedDate) {    
    if(!Array.isArray(selectedDate)) {
      let selectedDateArray = selectedDate.split(',');
      selectedDateArray =  [new Date(selectedDateArray[0]?.toString()),
        new Date(selectedDateArray[1]?.toString())];      
      return selectedDateArray;
    } else {
      return selectedDate;
    }
  }

  ConvertAvailabilityPercentInRangeFormat(filterRow: FilterRow) {
    let array = filterRow.filterValue.split(",");
    return {
      min: Number(array[0]),
      max: Number(array[1]),
      isFilterApplied: true
    }
  }

  //---------------------------------------Event handlers --------------------------------------------//

  handleMenuSelection(event: ResourceFiltersBasicMenu, index: number, type: string) {
    let dataToParse = [];
    let updatedFilterRow: FilterRow;
        
    switch (type) {
      case "andOr":
        dataToParse = this.andOrOptions;
        this.andOrLabel = event.value;
        this.filterBy.forEach((row) => {
          row.andOr = event.value
        });
        break;
      case "field":
        dataToParse = this.filterFieldOptions;
        updatedFilterRow = {
          andOr: this.andOrLabel,
          filterField: event.value,
          filterOperator: "",
          filterValue: "",
          parsedValue: ""
        }  
        if(event.value === this.FIELDOPTIONENUM.COMMITMENT_TYPE ||
          event.value === this.FIELDOPTIONENUM.CERTIFICATES ||
          event.value === this.FIELDOPTIONENUM.LANGUAGES ||
          event.value === this.FIELDOPTIONENUM.INDUSTRY_AND_CAPABILITY ||
          event.value === this.FIELDOPTIONENUM.RECENT_CD ||
          event.value === this.FIELDOPTIONENUM.COMMERCIAL_MODEL) {
          updatedFilterRow.parsedValue = [];
        } 

        this.filterBy.splice(index, 1, updatedFilterRow);
        this.setFilteredFieldOptions();
        this.toggleOperatorsVisiblityByField(event.value);
        break;
      case "operator":
        dataToParse = this.filterOperatorOptions;
        updatedFilterRow = {
          andOr: this.andOrLabel,
          filterField: this.filterBy[index].filterField,
          filterOperator: event.value,
          filterValue: "",
          parsedValue: ""
        }  

        if(this.filterBy[index].filterField == this.FIELDOPTIONENUM.COMMITMENT_TYPE ||
          this.filterBy[index].filterField == this.FIELDOPTIONENUM.CERTIFICATES  ||
          this.filterBy[index].filterField == this.FIELDOPTIONENUM.LANGUAGES ||
          this.filterBy[index].filterField == this.FIELDOPTIONENUM.INDUSTRY_AND_CAPABILITY ||
          this.filterBy[index].filterField == this.FIELDOPTIONENUM.RECENT_CD ||
          this.filterBy[index].filterField == this.FIELDOPTIONENUM.COMMERCIAL_MODEL
          ) 
        {
          updatedFilterRow.parsedValue = [];        
        } 
        
        this.filterBy.splice(index, 1, updatedFilterRow);
        break;
      case "value":
        this.filterBy[index].filterValue = event.value;
        break;
    };

    dataToParse.forEach((option) => {
      if (option.value == event.value) {
        option.selected = true;
      } else {
        option.selected = false;
      }
    });
  }

  setFilteredFieldOptions() {
    if (!!this.filterBy) {
      this.filterFieldOptions.forEach(option => {
        option.isHidden = this.filterBy.some(y => y.filterField == option.value);
        return option;
      });
    }
  }

  toggleOperatorsVisiblityByField(selectedField) {
    let operators = '';
    switch (selectedField) {
      case this.FIELDOPTIONENUM.AVAILABLE_PERCENTAGE:
      case this.FIELDOPTIONENUM.AVAILABLE_DATE:
      case this.FIELDOPTIONENUM.HIRE_DATE:
      case this.FIELDOPTIONENUM.LAST_DATE_STAFFED:
        operators = `${this.FIELDOPERATORENUM.GREATER_THAN},${this.FIELDOPERATORENUM.LESSER_THAN},${this.FIELDOPERATORENUM.BETWEEN},${this.FIELDOPERATORENUM.EQUALS},${this.FIELDOPERATORENUM.NOT_EQUALS}`;
        break;
      case this.FIELDOPTIONENUM.COMMITMENT_TYPE:
      case this.FIELDOPTIONENUM.CERTIFICATES:
      case this.FIELDOPTIONENUM.LANGUAGES:
  
        operators = `${this.FIELDOPERATORENUM.EQUALS},${this.FIELDOPERATORENUM.NOT_EQUALS}`;
        break;
      case this.FIELDOPTIONENUM.INDUSTRY_AND_CAPABILITY:
        operators = `${this.FIELDOPERATORENUM.INTERESTED},${this.FIELDOPERATORENUM.NOT_INTERESTED}`;
        break;

        case this.FIELDOPTIONENUM.RECENT_CD:
        case this.FIELDOPTIONENUM.COMMERCIAL_MODEL:
           operators = `${this.FIELDOPERATORENUM.INCLUDES}`;
        break;
    }
   
    let values = operators.split(',');
    this.filterOperatorOptions.forEach(x => {
      x.isHidden = !values.includes(x.value);
      return x;
    });

  }

  onDateChange(selectedDate, index) {
    // To avoid API call during initialization we check for non nullable start and end dates
    if (!selectedDate || this.filterBy[index].filterValue.toString() == selectedDate.toString()) {
      return;
    }

    this.filterBy[index].filterValue = DateService.convertDateInBainFormat(selectedDate);
    }

  onDateRangeChange(selectedDateRange, index) {
    // To avoid API call during initialization we check for non nullable start and end dates
    if (!selectedDateRange || this.filterBy[index].filterValue.toString()  === selectedDateRange.toString()) {
      return;
    }

    this.filterBy[index].filterValue = this.ConvertDateRangeToString(selectedDateRange);
  }

  ConvertDateRangeToString(selectedDateRange) {
    let selectedDateRangeInStringFormat = ''
    selectedDateRange.forEach(date => {
      selectedDateRangeInStringFormat = selectedDateRangeInStringFormat == '' ? 
        DateService.convertDateInBainFormat(date) : 
        selectedDateRangeInStringFormat + "," +DateService.convertDateInBainFormat(date);
      })
    return selectedDateRangeInStringFormat;
  }

  updateThresholdRangeHandler(data, index) {
    this.filterBy[index].filterValue = data.min + "," + data.max;
  }

  onAvailabilityPercentChange(data, index) {
    this.filterBy[index].filterValue = data;
  }


  onResourcesCommitmentsChange(commitmentTypeList, index) {
    this.filterBy[index].parsedValue = commitmentTypeList.split(',');
    this.filterBy[index].filterValue = commitmentTypeList;
  }

  OnResourcesCertificatesChange(certificatesList, index) {
    this.filterBy[index].parsedValue = certificatesList.split(',');
    this.filterBy[index].filterValue = certificatesList;
  }

  OnResourcesLanguagesChange(languagesList, index) {
    this.filterBy[index].parsedValue = languagesList.split(',');
    this.filterBy[index].filterValue = languagesList;
  }

  OnResourcesRecentCDChange(recentCDList, index) {
    this.filterBy[index].parsedValue = recentCDList.split(',');
    this.filterBy[index].filterValue = recentCDList;
  }

  OnResourcesCommercialModelChange(commercialModelList, index) {
    this.filterBy[index].parsedValue = commercialModelList.split(',');
    this.filterBy[index].filterValue = commercialModelList;
  }



  

  OnResourcesPreferencesChange(industryAndCapabilityList, index) {
    this.filterBy[index].parsedValue = industryAndCapabilityList.split(',');
    this.filterBy[index].filterValue = industryAndCapabilityList;
  }



  // Add new filter row
  addFilterRow() {
    if (this.filterBy.length >= 1) {
      this.filterBy.push(
        { andOr: this.andOrLabel, filterField: "", filterOperator: "", filterValue: "" }
      );
    } else {
      this.filterBy.push(
        { andOr: "", filterField: "", filterOperator: "", filterValue: "" }
      );
    }
  }

  // Delete single or all filter row(s)
  deleteRow(index) {
    this.filterBy.splice(index, 1);
    this.setFilteredFieldOptions();
  }
  deleteAll() {
    this.filterBy = [];
    this.setFilteredFieldOptions();
    this.toggleToastNotification("All filter conditions have been deleted.");
  }

  toggleToastNotification(message: string) {
    this.notifyService.showSuccess(message);
  }

  onResourceMenuToggle(selectedField){
    this.toggleOperatorsVisiblityByField(selectedField);
  }

  //---------------------------------------Commitment Type Filter --------------------------------------------//
  private initializeCommitmentTypesFilter() {
    if (!this.commitmentTypes || this.commitmentTypes.length === 0) {
      this.setCommitmentTypeFromLocalStorage();
    }
    // Todo: remove this NB check once we implement filtering on Resources tab for Not billable
    this.commitmentTypes = this.commitmentTypes.filter(type => type.commitmentTypeCode != '' && type.commitmentTypeCode !="NB");


    //Updating commitment type name from LOA(Planned) to LOA
    this.commitmentTypes.forEach((item) => {
      if (item.commitmentTypeCode === CommitmentTypeEnum.LOA) {
        item.commitmentTypeName = 'LOA';
      }
    });

    this.initializeCommitmentTypeChildren();

    this.commitmentTypesDropDownList = {
      text: 'All',
      value: 0,
      checked: false,
      children: this.commitmentTypes.map(item => {
        return {
          text: item.commitmentTypeName,
          value: item.commitmentTypeCode,
          collapsed: true,
          children: item.children && item.children.length > 0 ? item.children.map(child => {
            return {
              text: child.commitmentTypeName,
              value: item.commitmentTypeCode + '_'+ child.commitmentTypeCode,
              checked: false
            };
          }) : [],
          checked: false
        };
      })
    };
  }
    

  
  initializeCommitmentTypeChildren() {
    let trainingChildArray: CommitmentType[] = [
      { commitmentTypeCode: CommitmentTypeHierarchyEnum.PLANNED, commitmentTypeName: 'Training(Planned)', precedence: 1},
      { commitmentTypeCode: CommitmentTypeHierarchyEnum.APPROVED, commitmentTypeName: 'Training(Approved)', precedence: 2 }
    ];

    let vacationChildArray: CommitmentType[] = [
      { commitmentTypeCode: CommitmentTypeHierarchyEnum.PLANNED, commitmentTypeName: 'Vacation(Planned)', precedence: 1},
      { commitmentTypeCode: CommitmentTypeHierarchyEnum.PENDING, commitmentTypeName: 'Vacation(Pending)', precedence: 2 },
      { commitmentTypeCode: CommitmentTypeHierarchyEnum.APPROVED, commitmentTypeName: 'Vacation(Approved)', precedence: 3 }
    ];

    let loaChildArray: CommitmentType[] = [
      { commitmentTypeCode: CommitmentTypeHierarchyEnum.PAID, commitmentTypeName: 'LOA(Paid)', precedence: 1 },
      { commitmentTypeCode: CommitmentTypeHierarchyEnum.UNPAID, commitmentTypeName: 'LOA(Unpaid)', precedence: 2 },
      { commitmentTypeCode: CommitmentTypeHierarchyEnum.PLANNED, commitmentTypeName: 'LOA(Planned)', precedence: 3 }
    ];

    this.commitmentTypes.forEach((item) => {
      if (item.commitmentTypeCode === CommitmentTypeEnum.TRAINING) {
        item.children = trainingChildArray;
      } else if (item.commitmentTypeCode === CommitmentTypeEnum.VACATION) {
        item.children = vacationChildArray;
      } else if (item.commitmentTypeCode === CommitmentTypeEnum.LOA) {
        item.children = loaChildArray;
      }
    });
  }


  setCommitmentTypeFromLocalStorage() {
    this.commitmentTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.commitmentTypes).filter(x => !x.isStaffingTag);
  }

  private initializeCertificatesFilter() {
    if (!this.certificates || this.certificates.length === 0) {
      this.setCertificatesFromLocalStorage();
    }
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
  }


  setCertificatesFromLocalStorage() {
    this.certificates = this.localStorageService.get(ConstantsMaster.localStorageKeys.certificates);
  }

  private initializeLanguagesFilter() {
    if (!this.languages || this.languages.length === 0) {
      this.setLanguagesFromLocalStorage();
    }
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
  }
  
  private initializeRecentCDFilter() {
    if (!this.resourcesRecentCDList || this.resourcesRecentCDList.length === 0 || this.recentCdDropdownList ) {
      return;
    }

    const uniqueRecentCDList = this.getUniqueResourcesRecentCDList();
      this.recentCdDropdownList = {
        text: 'All',
        value: 0,
        checked: false,
        children: uniqueRecentCDList.map(item => {
          return {
            text: item.recentCD,
            value: item.recentCD,
            checked: false
          };
        })
      };
  }

  private initializeCommercialModelFilter() {
    if (!this.resourcesCommercialModelList || this.resourcesCommercialModelList.length === 0 || this.commercialModelDropdownList ) {
      return;
    }

    const uniqueCommercialModelList = this.getUniqueResourcesCommercialModelList();
      this.commercialModelDropdownList = {
        text: 'All',
        value: 0,
        checked: false,
        children: uniqueCommercialModelList.map(item => {
          return {
            text: item.commercialModel,
            value: item.commercialModel,
            checked: false
          };
        })
      };
  }

  private getUniqueResourcesRecentCDList(): { recentCD: string }[] {
    const uniqueKeys  = new Set<string>();
    return this.resourcesRecentCDList.filter(item => {
      if (uniqueKeys.has(item.recentCD)) {
        return false;
      }
      uniqueKeys.add(item.recentCD);
      return true;
    });
  }

  private getUniqueResourcesCommercialModelList(): { commercialModel: string }[] {
    const uniqueKeys  = new Set<string>();
    return this.resourcesCommercialModelList.filter(item => {
      if (uniqueKeys.has(item.commercialModel)) {
        return false;
      }
      uniqueKeys.add(item.commercialModel);
      return true;
    });
  }

  setLanguagesFromLocalStorage() {
    this.languages = this.localStorageService.get(ConstantsMaster.localStorageKeys.languages);
  }

  initializeIndustryAndCapabilityFilter() {
    if (!this.industryAndCapabilities || this.industryAndCapabilities.length === 0) {
      this.setIndustryAndCapabilityPracticeAreaFromLocalStorage();
    }
      this.industryAndCapabilityDropDownList = {
        text: 'All',
        value: 0,
        checked: false,
        children: this.industryAndCapabilities.map(item => {
          return {
            text: item.practiceAreaName,
            value: item.practiceAreaCode,
            checked: false
          };
        })
      };

  }
  setIndustryAndCapabilityPracticeAreaFromLocalStorage() {
    const industryPracticeAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.industryPracticeAreas) || [];
    const capabilityPracticeAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.capabilityPracticeAreas) || [];
    this.industryAndCapabilities = [...industryPracticeAreas, ...capabilityPracticeAreas];
  }

  
}


