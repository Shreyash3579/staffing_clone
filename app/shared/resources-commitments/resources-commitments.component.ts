import { Component, EventEmitter, Output, ViewChild } from "@angular/core";
import { BsModalRef } from "ngx-bootstrap/modal";
import { forkJoin } from "rxjs/internal/observable/forkJoin";
import { OverlayService } from "src/app/overlay/overlay.service";
import { CommitmentsService } from "../commitments.service";
import { CommitmentType as CommitmentTypeEnum, NoteTypeCode} from "../constants/enumMaster";
import { DateService } from "../dateService";
import { HtmlGridComponent } from "../html-table-grid/html-table-grid.component";
import { ResourceCommitment } from "../interfaces/resourceCommitment";
import { PopupDragService } from "../services/popupDrag.service";
import { ConstantsMaster } from "../constants/constantsMaster";
import { environment } from "src/environments/environment";
import { DatePipe } from "@angular/common";
import { CommitmentType } from "../interfaces/commitmentType.interface";
import { LocalStorageService } from "../local-storage.service";
import { ResourcesCommitmentsDialogService } from "src/app/overlay/dialogHelperService/resourcesCommitmentsDialog.service";
import { CommonService } from "../commonService";
import { SharedService } from "../shared.service";
import { UserPreferences } from "../interfaces/userPreferences.interface";
import { ResourceService } from "../helperServices/resource.service";
import { Observable, of } from "rxjs";


@Component({
  selector: 'app-resources-commitments',
  templateUrl: './resources-commitments.component.html',
  styleUrls: ['./resources-commitments.component.scss']
})
export class ResourcesCommitmentsComponent {

  @ViewChild('htmlTableGridRef', {static: false}) htmlTableGridRef: HtmlGridComponent;
  employees: any;
  upcomingCommitments = [];
  resourcesData = [];
  recentCommitments = [];
  employeesWithLanguages = [];
  employeesTimeInLevel = [];
  combinedResourcesCommitmentsData = [];
  modalHeaderText: string;
  isDataLoading = true;
  isResourcesAvailibilityPreCalculated = false;
  public selectedDurationInWeeks  = 12;
  public commitmentTypesDropDownList;
  public commitmentTypes: CommitmentType[] = [];
  public userPreferences: UserPreferences;
  public selectedCommitmentTypeList = [];
  isCDvalueSelected = false;
  dataToExport: any[];
  resourcesObj : any[] = [];
  @Output() resourceToBeDeselectedEmitter = new EventEmitter();

  
  htmlTableDef = [
    {
      columnHeaderText: 'EmployeeCode',
      columnMappingValue: 'employeeCode',
      isVisible: false,
      class:'col-width-10',
      excludeFromMail: false
    },
    {
      columnHeaderText: 'Resource',
      columnMappingValue: 'employeeName',
      isVisible: true,
      class:'col-width-10',
      excludeFromMail: false
    },
    {
      columnHeaderText: 'LG',
      columnMappingValue: 'levelGrade',
      isVisible: true, 
      class:'col-width-5',
      excludeFromMail: false
    },
    {
      columnHeaderText: 'Time In Level',
      columnMappingValue: 'employeeTimeInLevel',
      toolTipText: ConstantsMaster.TimeInLevelDefination,
      isVisible: false, 
      class:'col-width-10',
      excludeFromMail: false
    },
    {
      columnHeaderText: 'Available as of',
      columnMappingValue: 'availableAsOf',
      isVisible: true, 
      class:'col-width-10',
      excludeFromMail: false
    },
    {
      columnHeaderText: 'Commitment (Next 12 Weeks)',
      columnMappingValue: 'upcomingCommitments',
      isVisible: true, 
      class:'col-width-20',
      excludeFromMail: false
    },
    {
      columnHeaderText: 'Previous Cases With Case Manager (Past 6 months)',
      columnMappingValue: 'recentCommitments',
      isVisible: true, 
      class:'col-width-20',
      excludeFromMail: false
    },
    {
      columnHeaderText: 'Language',
      columnMappingValue: 'employeeLanguages',
      isVisible: true, 
      class:'col-width-10',
      excludeFromMail: false
    },
    {
      columnHeaderText: 'Profile Note',
      columnMappingValue: 'profileNotes',
      isVisible: false, 
      class:'col-width-10',
      excludeFromMail: false
    },
    {
      columnHeaderText: 'Notes',
      columnMappingValue: 'notes',
      isVisible: false, 
      class:'col-width-10',
      excludeFromEmail: false
    },
    {
      columnHeaderText: 'Remove from List',
      columnMappingValue: 'delete',
      isVisible: true, 
      class:'col-width-5',
      columnType: 'icon',
      excludeFromMail: true
    },

  ];

  constructor(public bsModalRef: BsModalRef,
    private commitmentService: CommitmentsService,
    private _popupDragService: PopupDragService,
    private localStorageService: LocalStorageService,
    private overlayService: OverlayService,
    private resourcesCommitmentsDialogService: ResourcesCommitmentsDialogService,
   private sharedService: SharedService) { }

  ngOnInit(): void {
    this.modalHeaderText = 'Resources Data';
    this._popupDragService.dragEvents();
    this.getLookupListFromLocalStorage();
    this.checkIfResourcesAvailibilityPreCalculated();
    if (!this.isResourcesAvailibilityPreCalculated) {
      this.updateAvailableAsOfColumnHeaderText();
    }

    this.fetchResourcesData();
    this.initializeCommitmentTypesFilter();
    this.initializeDataToExport();  
  }

  checkIfResourcesAvailibilityPreCalculated() {

    const firstEmployee = this.employees?.[0];

    this.isResourcesAvailibilityPreCalculated = firstEmployee
      ? firstEmployee.isNotAvailable || firstEmployee.dateFirstAvailable 
      : false;
  }

  getLookupListFromLocalStorage() {     
    this.commitmentTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.commitmentTypes);
    this.userPreferences = JSON.parse(this.localStorageService.get(ConstantsMaster.localStorageKeys.userPreferences));
  }


  sendMail() {
    if(this.htmlTableGridRef.tableRef){
      let tableRef = this.htmlTableGridRef.tableRef.nativeElement;
      this.includeAndExcludeColumnInExport(tableRef);
      window.getSelection().selectAllChildren(tableRef);
    }
    document.execCommand('copy');
    let emailSubject = 'Available Resources Data as of ' + DateService.convertDateInBainFormat(new Date());
    let emailBody = encodeURIComponent('NOTE: Press Ctrl+V to paste the copied data.')
    let emailString = 'mailto:?subject=' + emailSubject + '&body=' + emailBody;
    window.location.href = emailString;
    this.closeDialogHandler();
  }

  emailResources() {
    if (this.employees && this.employees.length > 0) {
  
      if (!this.isResourcesAvailibilityPreCalculated) {
        this.employees = this.employees.map(employee => {
          const resource = this.resourcesObj?.find(r => r.employeeCode === employee.employeeCode);
          if (resource) {
            employee.internetAddress = resource.internetAddress; 
          }
          return employee;
        });
      }
  
      CommonService.sendEmailToSelectedResources(this.employees);
      this.closeDialogHandler();
    }
  }

  includeAndExcludeColumnInExport(table) {
    for (let i = 0; i < table.rows.length; i++) {
      for (let j = 0; j < table.rows[i].cells.length; j++) {
        if (table.rows[i].cells[j].classList.value.indexOf('export-to-mail') > 0) {
          table.rows[i].cells[j].classList.remove('hide-column');
        }
        if (table.rows[i].cells[j].classList.value.indexOf('exclude-from-mail') >= 0) {
          table.rows[i].cells[j].classList.add('hide-column');
        }

      }
    }
  }

  onExportCheckboxChange(option) {
    this.htmlTableDef.forEach(item => {
      if (item.columnHeaderText === option.srcElement.label) {
        item.isVisible = option.target.checked;
      }
      if (item.columnMappingValue === option.srcElement.value) {
        item.isVisible = option.target.checked;
      }
    });
    this.dataToExport.forEach(item => {
      if (item.value === option.srcElement.value) {
        item.ischecked = option.target.checked;
      }
    });
  }

  isCDCasesSelected(): boolean {
    return this.isCDvalueSelected;
  }

  private isArrayEqual(array1, array2) {
    return JSON.stringify(array1) === JSON.stringify(array2);
  }

  private initializeCommitmentTypesFilter() {
    if (!this.commitmentTypes || this.commitmentTypes.length === 0) {
      this.setCommitmentTypeFromLocalStorage();
    }
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
  setCommitmentTypeFromLocalStorage() {
    const excludedCommitmentTypes = [CommitmentTypeEnum.DOWN_DAY,CommitmentTypeEnum.PLANNING_CARD,CommitmentTypeEnum.NAMED_PLACEHOLDER]
    //exclude (downday, planning card, named placeholder commitments), (peg and peg surge ringfences) from this master list
    this.commitmentTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.commitmentTypes).filter(x => !x.isStaffingTag && !excludedCommitmentTypes.includes(x.commitmentTypeCode));
  }

  private initializeDataToExport() {
    this.dataToExport = [
      {
        text: 'Time In Levels',
        value: 'employeeTimeInLevel',
        label: 'Time In level',
        ischecked: false,
      },
      {
        text: 'Available as of',
        value: 'availableAsOf',
        label: 'Available as of',
        ischecked: true,
      },
      {
        text: 'Languages',
        value: 'employeeLanguages',
        label: 'Language',
        ischecked: true,
      },
      {
        text: 'Profile Notes',
        value: 'profileNotes',
        label: 'Profile Note',
        ischecked: false,
      },
      {
        text: 'Upcoming Commitments',
        value: 'upcomingCommitments',
        label: 'Commitment (Next 12 Weeks)',
        ischecked: true,
        child: [ {
          type: 'app-multi-select-dropdown',
          dropdownList: this.commitmentTypesDropDownList,
          title: 'Commitment Type',
          selectedItems: this.selectedCommitmentTypeList,
          onChangeMethod: 'showResourcesCommitmentsBySelectedValue',
        },
        {
          type: 'Increment-decrement-button',
          title: 'Commitment Type',
          minValue: 1,
          maxValue: 52,
          selectedValue: this.selectedDurationInWeeks,
          onChangeMethod: 'showResourceCommitmentBasedOnSelectedWeek',
        },
      ],
      },
      {
        text: 'Recent Cases',
        value: 'recentCommitments',
        label: 'Previous Cases (Past 180 Days)',
        ischecked: true,
        child:[ {
          type: 'checkbox',
          label: 'Include CD',
          isChecked: this.isCDvalueSelected,
          onChangeMethod: 'onCDCasesCheckboxChange',
        }],
      },
    ];
  }

  private fetchResourcesData() {
    const employeeCodes = this.getEmployeeCodesString();

    const startDateForUpcomingCommitments = DateService.getBainFormattedToday();
    const endDateForUpcomingCommitments = DateService.addWeeks(startDateForUpcomingCommitments, 12);
    const startDateForRecentCommitments = DateService.addDays(startDateForUpcomingCommitments, -180);
    const endDateForRecentCommitments = startDateForUpcomingCommitments;

    forkJoin([
      this.commitmentService.getResourcesCommitmentsWithinDateRangeByCommitmentTypes(
        employeeCodes, startDateForRecentCommitments, endDateForRecentCommitments, 'allocation'),
      this.commitmentService.getResourcesCommitmentsWithinDateRangeByCommitmentTypes(
        employeeCodes, startDateForUpcomingCommitments, endDateForUpcomingCommitments),
      this.overlayService.getEmployeeLanguagesByEmployeeCodes(employeeCodes),
      this.overlayService.getEmployeeTimeInLevelByEmployeeCodes(employeeCodes),
      this.overlayService.getResourceNotes(employeeCodes, NoteTypeCode.RESOURCE_PROFILE_NOTE),
      !this.isResourcesAvailibilityPreCalculated
        ? this.sharedService.getResourceDetailsByEmployeeCodes(employeeCodes)
        : of(null)  
    ] as Observable<any>[]).subscribe(data => {
      this.deriveFetchedResourcesData(data);
      this.resourcesData = data;
    }); 
  }

  private deriveFetchedResourcesData(data: any[]) {
    this.getRecentResourcesAllocations(data[0]);
    this.getUpcomingResourcesCommitments(data[1]);
    this.getEmployeesLanguages(data[2]);
    this.getEmployeesTimeInLevel(data[3]);
    this.getProfileNotes(data[4]);
  
    if (!this.isResourcesAvailibilityPreCalculated && data[5]) {
      this.getResourceAvailibility(data[1], data[5]);
    }
  
    this.getCombinedData();
  }


  getResourceAvailibility(resourcesCommitments, resourcesObj) {
    resourcesCommitments.resources = resourcesObj;
    this.resourcesObj = resourcesObj;

    const startDateForUpcomingCommitments = DateService.getBainFormattedToday();
    const endDateForUpcomingCommitments = DateService.addWeeks(startDateForUpcomingCommitments, 12);
    const resourcesDataWithAvailability = ResourceService.createResourcesDataForStaffing(resourcesCommitments, startDateForUpcomingCommitments, endDateForUpcomingCommitments,
                    null, this.commitmentTypes, this.userPreferences, true);
        
                 this.employees = this.employees.map(employee => {
    
                    const matchingResource = resourcesDataWithAvailability.find(resource => resource.employeeCode === employee.employeeCode);
                    if (matchingResource) {
                        employee.dateFirstAvailable = matchingResource.dateFirstAvailable;
                        employee.percentAvailable = matchingResource.percentAvailable;
                    }
                    return employee;
                });
  }

  getCombinedData() {
    this.employees.forEach(employee => {
      let employeeRecentCommitments = this.recentCommitments.find(rc => rc.employeeCode === employee.employeeCode).commitments;
      let employeeUpcomingCommitments = this.upcomingCommitments.find(c => c.employeeCode === employee.employeeCode).commitments;
      let employeeLanguages = this.employeesWithLanguages.find(el => el.employeeCode === employee.employeeCode).employeeLanguages;
      let employeeTimeInLevel = this.employeesTimeInLevel.find(el => el.employeeCode === employee.employeeCode);
      let employeeIrisProfileUrl = environment.settings.irisProfileBaseUrl + '/' + employee.employeeCode;
      let availableAsOf = (employee.isNotAvailable || !employee.dateFirstAvailable)
        ? 'N/A' 
        : `${new DatePipe("en-US").transform(employee.dateFirstAvailable,'dd-MMM')} (${employee.percentAvailable}%)`;
      this.combinedResourcesCommitmentsData.push({
        employeeCode: employee.employeeCode,
        employeeName: employee.employeeName,
        levelGrade: employee.levelGrade,
        availableAsOf: availableAsOf,
        upcomingCommitments: employeeUpcomingCommitments,
        recentCommitments: employeeRecentCommitments,
        employeeLanguages: employeeLanguages,
        employeeTimeInLevel: employeeTimeInLevel?.timeInLevel.toFixed(2).toString(),
        notes: '',
        profileNotes: employee.profileNotes,
        irisProfileUrl: employeeIrisProfileUrl
      });
    });
    this.isDataLoading = false;
  }

  getEmployeesLanguages(employeesLanguages) {
    this.employees.forEach(employee => {
      const employeeLanguage = this.deriveResourcesLanagues(employee, employeesLanguages)
      if (employeeLanguage !== null) {
        this.employeesWithLanguages.push(employeeLanguage);
      }
    });
  }

  getEmployeesTimeInLevel(employeesTimeInLevel) {
    this.employeesTimeInLevel = employeesTimeInLevel;
  }

  getProfileNotes(profileNotes) {
    this.employees.forEach(employee => {
      const employeeProfileNotes = profileNotes
        .filter(note => note.employeeCode === employee.employeeCode)
        .sort((a, b) => new Date(b.lastUpdated).getTime() - new Date(a.lastUpdated).getTime())
        .slice(0, 3)
        .map(x => `- ${x.note}`);

      employee.profileNotes = employeeProfileNotes;
    });
  }

  deriveResourcesLanagues(employee, employeesLangauages) {
    const employeeLangauge = employeesLangauages.find(employeeLangauge => { return employeeLangauge.employeeCode === employee.employeeCode; });
    const languages = employeeLangauge?.languages?.map(language => {
      return language.name + ' - ' + language.proficiencyName;
    });
    return {
      employeeCode: employee.employeeCode,
      employeeName: employee.employeeName,
      employeeLanguages: languages
    };
  }

  getRecentResourcesAllocations(recentCommitments) {
    this.employees.forEach(employee => {
      const resourceCommitments = this.deriveResourceAllocations(employee, recentCommitments);
      if (resourceCommitments !== null) {
        this.recentCommitments.push(resourceCommitments);
      }
    });
  }

  getUpcomingResourcesCommitments(upcomingCommitments) {
    this.employees.forEach(employee => {
      const resourceCommitments = this.deriveResourceCommitments(employee, upcomingCommitments);
      if (resourceCommitments !== null) {
        this.upcomingCommitments.push(resourceCommitments);
      }
    });
  }

  onChangeValue(functionName: string, event: any): void {
    if (functionName && typeof this[functionName] === 'function') {
      this[functionName](event);
    } else {
      console.error(`Function ${functionName} is not defined or not a function.`);
    }
  }

  showResourcesCommitmentsBySelectedValue(commitmentTypeList) {
    if (this.isArrayEqual(this.selectedCommitmentTypeList.map(String), commitmentTypeList.split(','))) {
        return false;
    }
    this.selectedCommitmentTypeList = commitmentTypeList.split(',');
    this.upcomingCommitments = [];
    this.employees.forEach(employee => {
      const resourceCommitments = this.deriveResourceCommitments(employee, this.resourcesData[1]);
      if (resourceCommitments !== null) {
        this.upcomingCommitments.push(resourceCommitments);
      }
    });
    this.combinedResourcesCommitmentsData = [];
    this.getCombinedData();
  }

  updateUpcomingCommitmentColumnHeaderText(){
    const durationInWeeksText = `Commitment (Next ${this.selectedDurationInWeeks} Weeks)`;
    const commitmentColumnIndex = this.htmlTableDef.findIndex(column => column.columnMappingValue === 'upcomingCommitments');
    if (commitmentColumnIndex !== -1) {
      this.htmlTableDef[commitmentColumnIndex].columnHeaderText = durationInWeeksText;
  }
}


updateAvailableAsOfColumnHeaderText(){
  const durationInWeeksText = `Available as of (Next ${this.selectedDurationInWeeks} Weeks)`;
  const availableAsOfColumnIndex = this.htmlTableDef.findIndex(column => column.columnMappingValue === 'availableAsOf');
  if (availableAsOfColumnIndex !== -1) {
    this.htmlTableDef[availableAsOfColumnIndex].columnHeaderText = durationInWeeksText;
}
}
 
showResourceCommitmentBasedOnSelectedWeek(event) {
  if (isNaN(event)) return;

  this.isDataLoading = true;
  this.selectedDurationInWeeks = event;
  this.updateUpcomingCommitmentColumnHeaderText();
  if (!this.isResourcesAvailibilityPreCalculated) {
    this.updateAvailableAsOfColumnHeaderText();
  }

  const employeeCodes = this.getEmployeeCodesString();
  const startDate = DateService.getBainFormattedToday();
  const endDate = DateService.addWeeks(startDate, this.selectedDurationInWeeks);

  this.commitmentService.getResourcesCommitmentsWithinDateRangeByCommitmentTypes(
    employeeCodes,
    startDate,
    endDate
  ).subscribe(resourcesCommitments => {
    if (!this.isResourcesAvailibilityPreCalculated) {
      resourcesCommitments.resources = this.resourcesObj;

      const enrichedResources = ResourceService.createResourcesDataForStaffing(
        resourcesCommitments,
        startDate,
        endDate,
        null,
        this.commitmentTypes,
        this.userPreferences,
        true
      );

      this.employees = this.employees.map(employee => {
        const matching = enrichedResources.find(r => r.employeeCode === employee.employeeCode);
        if (matching) {
          employee.dateFirstAvailable = matching.dateFirstAvailable;
          employee.percentAvailable = matching.percentAvailable;
        }
        return employee;
      });
    }
    this.processCommitmentsData(resourcesCommitments);
  });
}


private processCommitmentsData(commitmentsData: any) {
  this.upcomingCommitments = [];

  this.employees.forEach(employee => {
    const resourceCommitments = this.deriveResourceCommitments(employee, commitmentsData);
    if (resourceCommitments !== null) {
      this.upcomingCommitments.push(resourceCommitments);
    }
  });

  this.combinedResourcesCommitmentsData = [];
  this.getCombinedData();
}



  onCDCasesCheckboxChange(event: any): void {
    this.isCDvalueSelected = event.target.checked;
    this.recentCommitments = [];
    this.employees.forEach(employee => {
      const resourceCommitments = this.deriveResourceAllocations(employee, this.resourcesData[0]);
      if (resourceCommitments !== null) {
        this.recentCommitments.push(resourceCommitments);
      }
    });
    this.combinedResourcesCommitmentsData = [];
    this.getCombinedData();
  }

  closeDialogHandler() {
    this.bsModalRef.hide();
  }

  private getEmployeeCodesString() {
    // NOTE: This is done as a resource can have mutiple active allocations in a case
    if (this.employees) {
      this.employees = this.employees.filter(
        (employee, index, employeeArrayCopy) => employeeArrayCopy
          .findIndex(employeeCopy => employeeCopy.employeeCode === employee.employeeCode) === index
      );
    }
    return this.employees?.map(x => x.employeeCode).toString();
  }

  private deriveResourceCommitments(employee, resourcesCommitments: ResourceCommitment) {
    let commitments = [];
    if (this.selectedCommitmentTypeList.includes(CommitmentTypeEnum.CASE_OPP))
      commitments = commitments.concat(this.getResourcesAssignmentsOnCase(employee, resourcesCommitments, true));
    if (this.selectedCommitmentTypeList.includes(CommitmentTypeEnum.LOA))
      commitments = commitments.concat(this.getResourcesLoAs(employee, resourcesCommitments));
    if (this.selectedCommitmentTypeList.includes(CommitmentTypeEnum.VACATION))
      commitments = commitments.concat(this.getResourcesVacations(employee, resourcesCommitments));
    if (this.selectedCommitmentTypeList.includes(CommitmentTypeEnum.TRAINING))
      commitments = commitments.concat(this.getResourcesTrainings(employee, resourcesCommitments));
    if (this.selectedCommitmentTypeList.includes(CommitmentTypeEnum.RECRUITING))
      commitments = commitments.concat(this.getResourcesRecruitments(employee, resourcesCommitments));
    if (this.selectedCommitmentTypeList.includes(CommitmentTypeEnum.SHORT_TERM_AVAILABLE))
      commitments = commitments.concat(this.getResourcesShortTermAvailability(employee, resourcesCommitments));
    if (this.selectedCommitmentTypeList.includes(CommitmentTypeEnum.LIMITED_AVAILABILITY))
      commitments = commitments.concat(this.getResourcesLimitedAvailability(employee, resourcesCommitments));
    if (this.selectedCommitmentTypeList.includes(CommitmentTypeEnum.NOT_AVAILABLE))
      commitments = commitments.concat(this.getResourcesNotAvailability(employee, resourcesCommitments));
    if (this.selectedCommitmentTypeList.includes(CommitmentTypeEnum.HOLIDAY))
      commitments = commitments.concat(this.getResourcesHolidays(employee, resourcesCommitments));

    return {
      employeeCode: employee.employeeCode,
      employeeName: employee.employeeName,
      levelGrade: employee.levelGrade,
      commitments: commitments,
      notes: '',
    };

  }

  private deriveResourceAllocations(employee, resourcesCommitments: ResourceCommitment) {
    let commitments = [];
    commitments = commitments.concat(this.getResourcesAssignmentsOnCase(employee, resourcesCommitments, this.isCDvalueSelected));
    return {
      employeeCode: employee.employeeCode,
      employeeName: employee.employeeName,
      levelGrade: employee.levelGrade,
      commitments: commitments,
      notes: '',
    };

  }

  private getResourcesLoAs(employee, resourcesCommitments) {
    let loAs = resourcesCommitments.loAs?.filter(x => x.employeeCode === employee.employeeCode)?.map(x => {
      return {
        type: x.type,
        startDate: DateService.convertDateInBainFormat(x.startDate),
        endDate: DateService.convertDateInBainFormat(x.endDate)
      };
    });

    const loASavedInBoss = resourcesCommitments.commitments
      ?.filter(x => x.employeeCode === employee.employeeCode && x.commitmentTypeCode === CommitmentTypeEnum.LOA)
      ?.map(x => {
        return {
          type: 'LOA',
          startDate: DateService.convertDateInBainFormat(x.startDate),
          endDate: DateService.convertDateInBainFormat(x.endDate)
        };
      });

    loAs = loAs.concat(loASavedInBoss);
    loAs = loAs.sort((first, second) => {
      return <any>new Date(first.startDate) - <any>new Date(second.startDate);
    });

    const mergedColumnValues = [];
    loAs.forEach(loa => {
      mergedColumnValues.push(loa.type + ': ' + loa.startDate + ' - ' + loa.endDate);
    });
    return mergedColumnValues;
  }


  private getResourcesVacations(employee, resourcesCommitments: ResourceCommitment) {
    let vacations = resourcesCommitments.vacations?.filter(x => x.employeeCode === employee.employeeCode)?.map(x => {
      return {
        type: x.type,
        startDate: DateService.convertDateInBainFormat(x.startDate),
        endDate: DateService.convertDateInBainFormat(x.endDate)
      };
    });

    const vacationsSavedInBoss = resourcesCommitments.commitments
      ?.filter(x => x.employeeCode === employee.employeeCode && x.commitmentTypeCode === CommitmentTypeEnum.VACATION)
      ?.map(x => {
        return {
          type: 'Vacation',
          startDate: DateService.convertDateInBainFormat(x.startDate),
          endDate: DateService.convertDateInBainFormat(x.endDate)
        };
      });

    vacations = vacations.concat(vacationsSavedInBoss);

    const vacationsSavedInWorkday = resourcesCommitments.timeOffs?.filter(x => x.employeeCode === employee.employeeCode)?.map(x => {
      return {
        type: x.type,
        startDate: DateService.convertDateInBainFormat(x.startDate),
        endDate: DateService.convertDateInBainFormat(x.endDate)
      };
    });
    vacations = vacations.concat(vacationsSavedInWorkday);
    vacations = vacations.sort((first, second) => {
      return <any>new Date(first.startDate) - <any>new Date(second.startDate);
    });

    const mergedColumnValues = [];
    vacations.forEach(vacation => {
      mergedColumnValues.push(vacation.type + ': ' + vacation.startDate + ' - ' + vacation.endDate);
    });
    return mergedColumnValues;
  }

  private getResourcesTrainings(employee, resourcesCommitments: ResourceCommitment) {
    let trainings = resourcesCommitments.trainings?.filter(x => x.employeeCode === employee.employeeCode)?.map(x => {
      return {
        type: x.type,
        startDate: DateService.convertDateInBainFormat(x.startDate),
        endDate: DateService.convertDateInBainFormat(x.endDate)
      };
    });

    const trainingsSavedInBoss = resourcesCommitments.commitments
      ?.filter(x => x.employeeCode === employee.employeeCode && x.commitmentTypeCode === CommitmentTypeEnum.TRAINING)
      ?.map(x => {
        return {
          type: 'Trainings',
          startDate: DateService.convertDateInBainFormat(x.startDate),
          endDate: DateService.convertDateInBainFormat(x.endDate)
        };
      });

    trainings = trainings.concat(trainingsSavedInBoss);

    trainings = trainings.sort((first, second) => {
      return <any>new Date(first.startDate) - <any>new Date(second.startDate);
    });

    const mergedColumnValues = [];
    trainings.forEach(training => {
      mergedColumnValues.push(training.type + ': ' + training.startDate + ' - ' + training.endDate);
    });
    return mergedColumnValues;
  }

  private getResourcesHolidays(employee, resourcesCommitments: ResourceCommitment) {
    let holidays = resourcesCommitments.holidays?.filter(x => x.employeeCode === employee.employeeCode)?.map(x => {
      return {
        type: x.type,
        startDate: DateService.convertDateInBainFormat(x.startDate),
        endDate: DateService.convertDateInBainFormat(x.endDate),
        description: x.description
      };
    });

    holidays = holidays.sort((first, second) => {
      return <any>new Date(first.startDate) - <any>new Date(second.startDate);
    });

    const mergedColumnValues = [];
    holidays.forEach(holidays => {
      mergedColumnValues.push(holidays.type + '- ' + holidays.description + ': ' + holidays.startDate + ' - ' + holidays.endDate);
    });
    return mergedColumnValues;
  }

  private getResourcesAssignmentsOnCase(employee, resourcesCommitments: ResourceCommitment, cdcasesSelected: boolean) {
    let assignments = resourcesCommitments.allocations?.filter(x => x.employeeCode === employee.employeeCode);
  
    // Filter out all other cases except CD cases if CD cases are not selected
    if (!cdcasesSelected) {
      assignments = assignments?.filter(x => x.caseTypeCode != 4);
    }
  
    let filteredAssignments = assignments?.map(x => {
      return {
        type: x.oldCaseCode ? `Case ${x.oldCaseCode} - ${x.caseName}` : `Opp ${x.opportunityName}`,
        startDate: DateService.convertDateInBainFormat(x.startDate),
        endDate: DateService.convertDateInBainFormat(x.endDate),
        caseManagerName: x.caseManagerName
      };
    });
  
    filteredAssignments = filteredAssignments?.sort((first, second) => {
      return <any>new Date(first.startDate) - <any>new Date(second.startDate);
    });
  
    const mergedColumnValues = [];
    filteredAssignments?.forEach(assignment => {
      let displayValue = assignment.type + ': ' + assignment.startDate + ' - ' + assignment.endDate;
      if (assignment.caseManagerName) {
        displayValue += ' (Case Manager: ' + assignment.caseManagerName + ')';
      }
      mergedColumnValues.push(displayValue);
    });
  
    return mergedColumnValues;
  }
  



  private getResourcesShortTermAvailability(employee, resourcesCommitments: ResourceCommitment) {
    let commitments = resourcesCommitments.commitments
      ?.filter(x => x.employeeCode === employee.employeeCode && x.commitmentTypeCode === CommitmentTypeEnum.SHORT_TERM_AVAILABLE)
      ?.map(x => {
        return {
          type: `Short Term Available`,
          startDate: DateService.convertDateInBainFormat(x.startDate),
          endDate: DateService.convertDateInBainFormat(x.endDate)
        };
      });

    commitments = commitments.sort((first, second) => {
      return <any>new Date(first.startDate) - <any>new Date(second.startDate);
    });

    const mergedColumnValues = [];
    commitments.forEach(commitment => {
      mergedColumnValues.push(commitment.type + ': ' + commitment.startDate + ' - ' + commitment.endDate);
    });
    return mergedColumnValues;
  }


  private getResourcesLimitedAvailability(employee, resourcesCommitments: ResourceCommitment) {
    let commitments = resourcesCommitments.commitments
      ?.filter(x => x.employeeCode === employee.employeeCode && x.commitmentTypeCode === CommitmentTypeEnum.LIMITED_AVAILABILITY)
      ?.map(x => {
        return {
          type: `Limited Available`,
          startDate: DateService.convertDateInBainFormat(x.startDate),
          endDate: DateService.convertDateInBainFormat(x.endDate)
        };
      });

    commitments = commitments.sort((first, second) => {
      return <any>new Date(first.startDate) - <any>new Date(second.startDate);
    });

    const mergedColumnValues = [];
    commitments.forEach(commitment => {
      mergedColumnValues.push(commitment.type + ': ' + commitment.startDate + ' - ' + commitment.endDate);
    });
    return mergedColumnValues;
  }


  private getResourcesNotAvailability(employee, resourcesCommitments: ResourceCommitment) {
    let commitments = resourcesCommitments.commitments
      ?.filter(x => x.employeeCode === employee.employeeCode && x.commitmentTypeCode === CommitmentTypeEnum.NOT_AVAILABLE)
      ?.map(x => {
        return {
          type: `Not Available`,
          startDate: DateService.convertDateInBainFormat(x.startDate),
          endDate: DateService.convertDateInBainFormat(x.endDate)
        };
      });

    commitments = commitments.sort((first, second) => {
      return <any>new Date(first.startDate) - <any>new Date(second.startDate);
    });

    const mergedColumnValues = [];
    commitments.forEach(commitment => {
      mergedColumnValues.push(commitment.type + ': ' + commitment.startDate + ' - ' + commitment.endDate);
    });
    return mergedColumnValues;
  }


  private getResourcesRecruitments(employee, resourcesCommitments: ResourceCommitment) {
    let commitments = resourcesCommitments.commitments
      ?.filter(x => x.employeeCode === employee.employeeCode && x.commitmentTypeCode === CommitmentTypeEnum.RECRUITING)
      ?.map(x => {
        return {
          type: `Recruiting`,
          startDate: DateService.convertDateInBainFormat(x.startDate),
          endDate: DateService.convertDateInBainFormat(x.endDate)
        };
      });

    commitments = commitments.sort((first, second) => {
      return <any>new Date(first.startDate) - <any>new Date(second.startDate);
    });

    const mergedColumnValues = [];
    commitments.forEach(commitment => {
      mergedColumnValues.push(commitment.type + ': ' + commitment.startDate + ' - ' + commitment.endDate);
    });
    return mergedColumnValues;
  }
  resourceToBeDeletedHandler(employeeCode){
    this.employees = this.employees.filter((employee) => employee.employeeCode !== employeeCode);
    this.combinedResourcesCommitmentsData = this.combinedResourcesCommitmentsData.filter((data) => data.employeeCode !== employeeCode);
    this.resourcesCommitmentsDialogService.resourceToBeDeselectedEmitter(employeeCode);
  }

}

