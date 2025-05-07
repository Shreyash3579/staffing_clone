import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { LocalStorageService } from '../../shared/local-storage.service';
import { Office } from '../../shared/interfaces/office.interface';
import { UserPreferences } from '../../shared/interfaces/userPreferences.interface';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { CoreService } from '../core.service';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { CaseType } from 'src/app/shared/interfaces/caseType.interface';
import { SystemconfirmationFormComponent } from 'src/app/shared/systemconfirmation-form/systemconfirmation-form.component';
import { DateService } from 'src/app/shared/dateService';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
import { ResourceAssignmentService } from 'src/app/overlay/behavioralSubjectService/resourceAssignment.service';
import { forkJoin } from 'rxjs';
import { BS_DEFAULT_CONFIG } from 'src/app/shared/constants/bsDatePickerConfig';
import { OfficeClosureCases } from 'src/app/shared/interfaces/office-closure-cases.interface';
import { OfficeHierarchy } from 'src/app/shared/interfaces/officeHierarchy.interface';
import { ServiceLineHierarchy } from 'src/app/shared/interfaces/serviceLineHierarchy';
import { CommonService } from 'src/app/shared/commonService';

@Component({
  selector: 'office-closure',
  templateUrl: './office-closure.component.html',
  styleUrls: ['./office-closure.component.scss']
})
export class OfficeClosureComponent implements OnInit {
  userPreferences: UserPreferences;
  selectedDateRange: [Date, Date];
  bsConfig: Partial<BsDatepickerConfig>;
  selectedCaseTypeList = [];
  selectedCaseList = [];
  caseTypeDropdownList;
  caseTypes: CaseType[];
  offices: Office[];
  loggedInUserOfficeCode;
  officeClosureValidationMessage: string = null;
  selectedOfficesCodesWithProcessedAllocationsList: any[] = [];
  selectedOfficesCodesWithProcessedAllocationsMsg: string = null;
  officesWithoutHolidaysMsg: string = null;
  loadingCasesData: boolean = false;
  selectedFieldsForClosure = undefined;
  distinctOldCasesCodes = [];
  officeHierarchy: OfficeHierarchy[] = [];
  serviceLines: ServiceLineHierarchy[] = [];
  public selectedOfficeList = [];
  public selectedServiceLineList = [];
  public serviceLinesDropdownList;

  constructor(private localStorageService: LocalStorageService, private coreService: CoreService,
    private bsModalRef: BsModalRef, private modalService: BsModalService,
    private resourceAssignmentService: ResourceAssignmentService) { }

  ngOnInit(): void {
    this.getDataFromLocalStorage();
    this.initializeFilters();

    this.bsConfig = BS_DEFAULT_CONFIG;
    this.setPredefinedValues();
  }

  closeForm() {
    this.bsModalRef.hide();
  }

  shiftDateRange(shiftDate) {
    if (this.selectedDateRange) {
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
    }
  }

  verifyOfficeClosureAndCases() {
    if (this.selectedOfficeList && this.selectedOfficeList.length > 0
      && this.selectedServiceLineList && this.selectedServiceLineList.length > 0
      && this.selectedDateRange
      && this.selectedCaseTypeList && this.selectedCaseTypeList.length > 0
      && !this.loadingCasesData) {
      this.officeClosureValidationMessage = null;
      this.loadingCasesData = true;
      let selectedFields = {
        "officeCodes": this.selectedOfficeList,
        "staffingTags": this.selectedServiceLineList,
        "startDate": DateService.getFormattedDate(this.selectedDateRange[0]),
        "endDate": DateService.getFormattedDate(this.selectedDateRange[1]),
        "caseTypeCodes": this.selectedCaseTypeList
      }

      this.coreService.getOfficeholidaysWithinDateRangeByOffices({
        ...selectedFields,
        officeCodes: this.selectedOfficeList.join(',')
      }).subscribe((officesHolidays) => {
        if (!this.validateHolidaysInDateRange(officesHolidays)) {
          this.officeClosureValidationMessage = 'Office closure (minimum of 5 days) has not been updated in Finance Systems & process can’t be triggered for the selected offices.';
          this.loadingCasesData = false;
          return;
        }
        const officeCodesWithHolidays = [...new Set(officesHolidays.map((holiday) => holiday.officeCode))];

        this.validateSelectedOfficesWithNoHolidays(officeCodesWithHolidays);

        this.selectedOfficeList = officeCodesWithHolidays;
        selectedFields.officeCodes = this.selectedOfficeList;

        this.getAllocationsDuringOfficeClosure(selectedFields);
      }, (error) => {
        this.loadingCasesData = false;
        this.officeClosureValidationMessage = 'Error while getting office closure data';
      }
      );

    }
  }

  getAllocationsDuringOfficeClosure(selectedFields) {
    this.coreService.getAllocationsDuringOfficeClosure({
      ...selectedFields,
      officeCodes: this.selectedOfficeList.join(','),
      staffingTags: this.selectedServiceLineList.join(','),
      caseTypeCodes: this.selectedCaseTypeList.join(',')
    }).subscribe(allocations => {
      if (allocations?.length) {
        this.isAllocationsProcessedForAnyOffices(allocations);
        this.openAffectedCasesPopup(allocations, { ...selectedFields, startDate: this.selectedDateRange[0], endDate: this.selectedDateRange[1] });
      } else {
        this.loadingCasesData = false;
        this.officeClosureValidationMessage = 'No allocations will be affected for the selected parameters. This could happen if there are no allocations for the selected parameters or if the office closure process had already been triggered before';
      }
    });
  }

  isAllocationsProcessedForAnyOffices(allocations) {
    if (!!allocations && allocations.length > 0) {
      const allocationDistinctOffices = [...new Set(allocations.map((allocation) => allocation.operatingOfficeCode))];
      const selectedOfficesCodesWithProcessedAllocations = this.selectedOfficeList.filter(x => !allocationDistinctOffices.includes(Number(x)));
      if (!!selectedOfficesCodesWithProcessedAllocations && selectedOfficesCodesWithProcessedAllocations.length > 0) {
        this.selectedOfficesCodesWithProcessedAllocationsList = this.offices.filter(office =>
          selectedOfficesCodesWithProcessedAllocations.includes(office.officeCode))
          ?.map(office => office.officeName)
        const selectedOfficesCodesWithProcessedAllocationsNames = this.selectedOfficesCodesWithProcessedAllocationsList?.join(', ')
        this.selectedOfficesCodesWithProcessedAllocationsMsg = `Allocations from <a style='display: inline-block; cursor:pointer;' title='${selectedOfficesCodesWithProcessedAllocationsNames}' href='javascript:void(0)'>${this.selectedOfficesCodesWithProcessedAllocationsList.length}</a> office(s) have already been processed before.`
      }

    }
  }

  validateSelectedOfficesWithNoHolidays(officeCodesWithHolidays) {
    let selectedOfficesCodesWithNoHolidays = this.selectedOfficeList.filter(x => !officeCodesWithHolidays.includes(Number(x)));

    if (!!selectedOfficesCodesWithNoHolidays && selectedOfficesCodesWithNoHolidays.length > 0) {
      const selectedOfficesWithNoHolidaysList = this.offices.filter(office =>
        selectedOfficesCodesWithNoHolidays.includes(office.officeCode.toString()))
        ?.map(office => office.officeName)
      const selectedOfficesWithNoHolidays = selectedOfficesWithNoHolidaysList?.join(', ')
      this.officesWithoutHolidaysMsg = `The office closure (min of 5 days) has not been updated in Finance Systems for <a style='display: inline-block; cursor:pointer;' title='${selectedOfficesWithNoHolidays}' href='javascript:void(0)'>${selectedOfficesWithNoHolidaysList.length}</a> offices. Hence the process can’t be triggered for these offices.
      The process will not run until the data is updated.`;
    }
  }

  initializeCaseTypes() {
    if (this.caseTypes != null && this.userPreferences) {
      const caseTypeChildrenList = this.caseTypes.map(item => {
        return {
          text: item.caseTypeName,
          value: item.caseTypeCode,
          checked: false,
          children: []
        };
      });

      this.caseTypeDropdownList = {
        text: 'All',
        value: 0,
        children: caseTypeChildrenList
      };

      this.selectedCaseTypeList = [];
    }

  }

  setSelectedDateRange(selectedDateRange) {
    if (!selectedDateRange || this.selectedDateRange?.toString() === selectedDateRange.toString()) {
      return;
    }
    this.selectedDateRange = selectedDateRange;
  }

  openAffectedCasesPopup(casesData, selectedFields) {
    let officeNameList = this.offices.filter(office => selectedFields.officeCodes.includes(office.officeCode))?.map(office => office.officeName);
    if(!!this.selectedOfficesCodesWithProcessedAllocationsList && this.selectedOfficesCodesWithProcessedAllocationsList.length > 0){
      officeNameList = officeNameList.filter(x => !this.selectedOfficesCodesWithProcessedAllocationsList.includes(x))
    }
    const officeNames: any = officeNameList.join(', ');
    this.bsModalRef.hide();

    let confirmationPopUpBodyMessage = !!this.officesWithoutHolidaysMsg ? "<p> " + this.officesWithoutHolidaysMsg + "</p></n>" : "";

    confirmationPopUpBodyMessage += !!this.selectedOfficesCodesWithProcessedAllocationsMsg ? "<p> " + this.selectedOfficesCodesWithProcessedAllocationsMsg + "</p></n>" : "";

    confirmationPopUpBodyMessage += "<p> Resources from <a style='display: inline-block; cursor:pointer;' title='"
      + officeNames + "' href='javascript:void(0)'>" + officeNameList.length +
      "</a> office(s) will be impacted by this change: </p></n> <ul>" +
      this.getAffectedCasesInARow(casesData) + "</ul>";

    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        templateConfirmationPopUpBodyMessage: confirmationPopUpBodyMessage,
        splitAllocationForClosure: true
      }
    };
    this.bsModalRef = this.modalService.show(SystemconfirmationFormComponent, config);

    this.bsModalRef.content.splitAllocationDuringOfficeClosure.subscribe(splitConfirmation => {
      if (splitConfirmation) {
        const affectedAllocationdata = this.updateAllocationsForOfficeClosure(casesData);

        if (affectedAllocationdata.allocationsToUpsert?.length) {
          let allocationData = {
            "resourceAllocation": affectedAllocationdata.allocationsToUpsert
          }
          // This code needs refactoring
          this.resourceAssignmentService.upsertResourceAllocationsToProject(allocationData, null, null, 'Office-ClosureAPI');
          this.resourceAssignmentService.upsertPlaygroundAllocationsForCasePlanningMetrics(allocationData.resourceAllocation);
        }

        if (affectedAllocationdata.allocationsToDelete?.length) {
          const deletedAllocationIds = affectedAllocationdata.allocationsToDelete.map(x => x.id).join(",");
          this.resourceAssignmentService.deleteResourcesAssignmentsFromCase(deletedAllocationIds, null, null, 'Office-ClosureAPI');
        }

        const officeClosureData: OfficeClosureCases = {
          id: null,
          officeCode: this.loggedInUserOfficeCode,
          oldCaseCodes: this.distinctOldCasesCodes.join(','),
          caseTypeCodes: this.selectedCaseTypeList.join(','),
          officeClosureStartDate: DateService.getFormattedDate(new Date(this.selectedDateRange[0])),
          officeClosureEndDate: DateService.getFormattedDate(new Date(this.selectedDateRange[1])),
          lastUpdated: null,
          lastUpdatedBy: null
        }

        this.coreService.upsertOfficeClosureCases(officeClosureData).subscribe(result => {
          console.log(result);
        });

        this.bsModalRef.hide();

      } else {
        this.bsModalRef.hide();
        const config = {
          ignoreBackdropClick: true,
          class: 'modal-dialog-centered',
          initialState: {
            selectedFieldsForClosure: selectedFields
          }
        };
        this.bsModalRef = this.modalService.show(OfficeClosureComponent, config);
      }
    });

  }

  validateHolidaysInDateRange(holidayData): boolean {
    if (holidayData && holidayData.length > 0) {
      let startDate: Date = this.selectedDateRange[0];
      let endDate: Date = this.selectedDateRange[1];
      var isOfficeClosureDateMatchWithCCM = true;

      let dateDifference = DateService.getDatesDifferenceInDays(startDate, endDate) + 1;

      for (var i = 0; i < dateDifference; i++) {
        const officeClosureDateToTestWithCCM = new Date(new Date(startDate).setDate(startDate.getDate() + i));
        var isDateExistInCCM = holidayData.filter(x => DateService.isSame(officeClosureDateToTestWithCCM, x.startDate)).length;

        if (DateService.isWeekend(officeClosureDateToTestWithCCM) || isDateExistInCCM) {
          continue;
        } else {
          isOfficeClosureDateMatchWithCCM = false;
          break;
        }
      }

      if (dateDifference < 5 || holidayData.length < 5) {
        isOfficeClosureDateMatchWithCCM = false;
      }

      return isOfficeClosureDateMatchWithCCM;
    }
    return false;
  }

  validateHolidaysForSelectedOffices(holidayData): boolean {
    if (holidayData && holidayData.length > 0) {
      const officeCodes = [...new Set(holidayData.map((holiday) => holiday.officeCode))];
      return CommonService.isArraysEqual(officeCodes, this.selectedOfficeList);
    }
    return false;
  }

  updateAllocationsForOfficeClosure(allocationData) {
    let updatedAllocationData: ResourceAllocation[] = [];
    let deletedAllocationsData: ResourceAllocation[] = [];
    const selectedStartDate = new Date(this.selectedDateRange[0]);
    const selectedEndDate = new Date(this.selectedDateRange[1]);

    allocationData.forEach(allocation => {
      const allocationStartDate = new Date(allocation.startDate);
      const allocationEndDate = new Date(allocation.endDate);

      if (!this.distinctOldCasesCodes.includes(allocation.oldCaseCode)) {
        this.distinctOldCasesCodes.push(allocation.oldCaseCode);
      }

      if (allocationStartDate < selectedStartDate && allocationEndDate > selectedEndDate) {
        let newAllocation = Object.assign({}, allocation);

        allocation.endDate = DateService.getFormattedDate(new Date(DateService.subtractDays(selectedStartDate, 1)));

        newAllocation.id = null;
        newAllocation.startDate = DateService.getFormattedDate(new Date(DateService.addDays(selectedEndDate, 1)));

        updatedAllocationData.push(allocation);
        updatedAllocationData.push(newAllocation);

      } else if (allocationStartDate < selectedStartDate && (allocationEndDate >= selectedStartDate && allocationEndDate <= selectedEndDate)) {

        allocation.endDate = DateService.getFormattedDate(new Date(DateService.subtractDays(selectedStartDate, 1)));
        updatedAllocationData.push(allocation);

      } else if ((allocationStartDate >= selectedStartDate && allocationStartDate <= selectedEndDate) && allocationEndDate > selectedEndDate) {

        allocation.startDate = DateService.getFormattedDate(new Date(DateService.addDays(selectedEndDate, 1)));
        updatedAllocationData.push(allocation);

      } else if (allocationStartDate >= selectedStartDate && allocationEndDate <= selectedEndDate) {
        deletedAllocationsData.push(allocation);
      }

    });

    return {
      allocationsToUpsert: updatedAllocationData,
      allocationsToDelete: deletedAllocationsData
    };
  }

  getAffectedCasesInARow(allocationData) {
    let affectedAllocations = [];
    allocationData.forEach(allocation => {
      if (!affectedAllocations.includes('<b><li class="list-bold">' + allocation.caseName + ' (' + allocation.oldCaseCode + ') </li></b>')) {
        affectedAllocations.push('<b><li class="list-bold">' + allocation.caseName + ' (' + allocation.oldCaseCode + ') </li></b>');
        const resourcesInSameCase = allocationData.filter(x => x.oldCaseCode === allocation.oldCaseCode);
        if (!!resourcesInSameCase && resourcesInSameCase.length > 0) {
          affectedAllocations.push('<ul>');

          resourcesInSameCase.forEach(resource => {
            // if (!affectedAllocations.includes('<li>' + resource.employeeName + ' (' + allocation.employeeCode + ') </li>')) {
            affectedAllocations.push('<li>' + resource.employeeName + ' (' + resource.employeeCode + ') </li>');
            // }
          });

          affectedAllocations.push('</ul>');
        }
      }
    });
    return affectedAllocations.join(' ');
  }

  setPredefinedValues() {
    if (this.selectedFieldsForClosure) {
      this.selectedOfficeList = this.selectedFieldsForClosure.officeCodes;
      this.selectedServiceLineList = this.selectedFieldsForClosure.staffingTags;
      this.selectedDateRange = [this.selectedFieldsForClosure.startDate, this.selectedFieldsForClosure.endDate];
      this.selectedCaseTypeList = this.selectedFieldsForClosure.caseTypeCodes?.map(caseType => parseInt(caseType));
    }
  }

  setCaseType(caseTypeCodes: any) {
    this.selectedCaseTypeList = !caseTypeCodes ? [] : caseTypeCodes?.split(',');
  }

  setOfficesBySelectedValue(officeCodes) {
    this.selectedOfficeList = !officeCodes ? [] : officeCodes.split(',');
  }

  setServiceLinesBySelectedValue(staffingTags) {
    this.selectedServiceLineList = !staffingTags ? [] : staffingTags.split(',');
  }

  private getDataFromLocalStorage() {
    this.userPreferences = JSON.parse(this.localStorageService.get(ConstantsMaster.localStorageKeys.userPreferences, 'monthly'));
    this.loggedInUserOfficeCode = this.coreService.loggedInUser?.office?.officeCode;
    this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.officeHierarchy);
    this.serviceLines = this.localStorageService.get(ConstantsMaster.localStorageKeys.serviceLinesHierarchy);
    this.caseTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.caseTypes);
    this.offices = this.localStorageService.get(ConstantsMaster.localStorageKeys.OfficeList);
  }

  private initializeFilters() {
    this.initializeOfficesFilter();
    this.initializeStaffingTagsFilter();
    this.initializeCaseTypes();

    this.setDefaultDropdownFilterValues();
  }

  private initializeOfficesFilter() {
    if (this.officeHierarchy) {
      this.officeHierarchy = JSON.parse(JSON.stringify(this.officeHierarchy));
    }
    this.selectedOfficeList = [];
  }

  private setDefaultDropdownFilterValues() {
    this.selectedOfficeList.push(this.loggedInUserOfficeCode);
    this.selectedServiceLineList.push(ConstantsMaster.ServiceLine.GeneralConsulting);
    this.selectedCaseTypeList = this.caseTypes.filter(caseType => this.userPreferences.caseTypeCodes.indexOf(caseType.caseTypeCode.toString()) > -1)
      .map(caseType => caseType.caseTypeCode);
  }

  private initializeStaffingTagsFilter() {
    if (this.serviceLines) {
      this.serviceLinesDropdownList = {
        text: 'All',
        value: 0,
        checked: false,
        children: this.serviceLines.map((item) => {
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
    this.selectedServiceLineList = [];
  }

}
