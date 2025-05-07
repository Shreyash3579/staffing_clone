import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ConstantsMaster } from '../constants/constantsMaster';
import { DateService } from '../dateService';
import { CaseRoleType } from '../interfaces/caseRoleType.interface';
import { Project } from '../interfaces/project.interface';
import { ResourceAllocation } from '../interfaces/resourceAllocation.interface';
import { LocalStorageService } from '../local-storage.service';
import { PopupDragService } from '../services/popupDrag.service';
import { ResourceAllocationService } from '../services/resourceAllocation.service';
import { SharedService } from '../shared.service';
import { ValidationService } from '../validationService';

@Component({
  selector: 'app-overlapped-teams-form',
  templateUrl: './overlapped-teams-form.component.html',
  styleUrls: ['./overlapped-teams-form.component.scss']
})
export class OverlappedTeamsFormComponent implements OnInit {

  // ----------------- Properties Set By Modal Service Initial State ---------------------------//
  public projectData: Project;
  public overlappedTeams: ResourceAllocation[];
  public allocation: ResourceAllocation;

  // --------------------------Local Variables---------------------------------//
  public distinctProjects = [];
  public isSelectAll = false;
  public gridData = [];
  public caseRoleTypes: CaseRoleType[];
  public caseRoleDropDownList: string[];
  public elementsWithError = [];
  public isAddButtonDisabled = true;
  // --------------------------Output Events---------------------------------//
  @Output() upsertResourceAllocationsToProject = new EventEmitter<any>();

  constructor(public bsModalRef: BsModalRef,
    private localStorageService: LocalStorageService,
    private _resourceAllocationService: ResourceAllocationService,
    private _popupDragService: PopupDragService,
    private sharedService: SharedService) { }

  // --------------------------Life Cycle Event handlers---------------------------------//
  ngOnInit(): void {

    if (!this.overlappedTeams?.length) {
      this.sharedService.getOverlappingTeamsInPreviousProjects(this.allocation.employeeCode, this.projectData.startDate).subscribe(allocations => {
        this.overlappedTeams = allocations;
        this.loadDataForPersistentTeams();
      });
    } else {
      this.loadDataForPersistentTeams();
    }
    this._popupDragService.dragEvents();
  }

  loadDataForPersistentTeams() {
    this.setDistinctAllocations();
    this.setDistinctProjects();
    this.caseRoleTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.caseRoleTypes);
    this.caseRoleDropDownList = this.caseRoleTypes.filter(role => !!role.caseRoleCode).map(role => role.caseRoleName);
    this.setGridData();
  }

  // --------------------------Event handlers---------------------------------//
  addOverlappedTeams() {
    if (this.elementsWithError.length > 0) return;

    if(!this.gridData.filter(row => row.checked).length){
      return;
    }

    let allocationsToInsert: ResourceAllocation[] = [];

    this.gridData.filter(row => row.checked).forEach(row => {
      const resourceAllocation: ResourceAllocation = {
        caseName: this.allocation.caseName ?? null,
        caseTypeCode: this.allocation.caseTypeCode ?? null,
        clientName: this.allocation.clientName ?? null,
        oldCaseCode: this.allocation.oldCaseCode ?? null,
        employeeCode: row.data.employeeCode,
        employeeName: row.data.employeeName,
        currentLevelGrade: row.data.currentLevelGrade,
        serviceLineCode: row.data.serviceLineCode,
        serviceLineName: row.data.serviceLineName,
        operatingOfficeCode: row.data.operatingOfficeCode,
        operatingOfficeAbbreviation: row.data.operatingOfficeAbbreviation,
        pipelineId: this.allocation.pipelineId ?? null,
        opportunityName: this.allocation.opportunityName ?? null,
        investmentCode: row.data.investmentCode,
        investmentName: row.data.investmentName,
        caseRoleCode: row.data.caseRoleCode,
        allocation: row.data.allocation,
        startDate: DateService.getFormattedDate(new Date(row.data.startDate)),
        endDate: DateService.getFormattedDate(new Date(row.data.endDate)),
        previousStartDate: null,
        previousEndDate: null,
        previousAllocation: null,
        caseStartDate: this.allocation.caseStartDate,
        caseEndDate: this.allocation.caseEndDate,
        opportunityStartDate: this.allocation.opportunityStartDate,
        opportunityEndDate: this.allocation.opportunityEndDate,
        lastUpdatedBy: null,
        notes: ''
      };

      allocationsToInsert.push(resourceAllocation);
    });

    if (allocationsToInsert.length > 0) {
      this.checkForPrePostAndUpsertResourceAllocation(allocationsToInsert, allocationsToInsert);
    }
    this.closeForm();
  }

  updateGridData(rowData: ResourceAllocation, element, field) {
    const newValue = element.value === "null" ? null : element.value;
    switch (field) {
      case 'allocation': {
        if (ValidationService.isAllocationValid(newValue) && ValidationService.isValidNumberBetween(newValue, 0, 999)) {
          this.removeElementToErrorList(element);
          rowData.allocation = parseInt(newValue);
        }
        else {
          this.addElementToErrorList(element);
        }
        break;
      }
      case 'role': {
        rowData.caseRoleName = newValue;
        rowData.caseRoleCode = !!newValue ? this.caseRoleTypes.find(role => role.caseRoleName === newValue).caseRoleCode : newValue;
        break;
      }
    }
  }

  OnSelectAllChanged(project: string, isChecked) {
    this.gridData.forEach(row => {
      if (this.checkProject(row.data, project)) {
        row.checked = isChecked;
      }
    });

    this.isAddButtonDisabled = !isChecked;
  }

  OnSelectRowChanged(row, project: string) {
    row.checked = !row.checked;
    this.isProjectSelectAllChecked(project);

    this.isAddButtonDisabled = !this.gridData.filter(row => row.checked).length;
  }

  checkProject(rowData, project) {
    return project.includes(rowData.caseName ?? rowData.opportunityName);
  }

  isProjectSelectAllChecked(project: string) {
    let isSelected = true;
    this.gridData.forEach(row => {
      if (this.checkProject(row.data, project)) {
        if (!row.checked) {
          isSelected = false;
        }
      }
    });
    return isSelected;
  }

  closeForm() {
    this.bsModalRef.hide();
  }

  // --------------------------Private methods---------------------------------//
  private setDistinctProjects() {
    this.distinctProjects = [...new Set(this.overlappedTeams.map(allocation => (allocation.oldCaseCode + ' - ' + allocation.caseName)))];
    this.distinctProjects = this.distinctProjects.concat([...new Set(this.overlappedTeams.map(allocation => allocation.opportunityName))]);
    this.distinctProjects = this.distinctProjects.filter(project => !!project);
  }

  private setGridData() {
    this.overlappedTeams.forEach((allocation, index) => {
      const row = {
        id: 'row-' + index,
        data: { ...allocation },
        checked: this.isSelectAll
      };

      row.data.id = null;
      row.data.startDate = DateService.convertDateInBainFormat(this.allocation.startDate);
      row.data.endDate = DateService.convertDateInBainFormat(this.allocation.endDate);

      this.gridData.push(row);
    });
  }

  private setDistinctAllocations() {
    let alloc: ResourceAllocation[] = [];
    this.overlappedTeams.forEach(x => {
      let employeeAllocations = alloc.filter(p => p.employeeCode == x.employeeCode);
      x.id = x.startDate = x.endDate = x.notes = x.lastUpdatedBy = null;

      if (employeeAllocations.length > 0) {
        for (let a of employeeAllocations) {
          if (!(a.allocation == x.allocation && a.investmentCode == x.investmentCode && a.caseRoleCode == x.caseRoleCode)) {
            alloc.push(x);
            break;
          }
        }
      }
      else {
        alloc.push(x);
      }
    });

    this.overlappedTeams = alloc;
  }

  private checkForPrePostAndUpsertResourceAllocation(resourceAllocationData, allocationDataBeforeSplitting?,successMessage?) {
    let allocationsData: ResourceAllocation[] = [];

    const projectStartDate = DateService.convertDateInBainFormat(this.projectData.startDate);
    const projectEndDate = DateService.convertDateInBainFormat(this.projectData.endDate);

    // If project has start and end date then, split else directly upsert the allocation data
    if (projectStartDate && projectEndDate) {

      if (Array.isArray(resourceAllocationData)) {

        resourceAllocationData.forEach(resourceAllocation => {
          allocationsData = allocationsData.concat(
            this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(resourceAllocation));
        });

      } else {
        allocationsData = this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(
          resourceAllocationData);
      }

    } else {
      allocationsData = resourceAllocationData;

    }

    this.upsertResourceAllocations(allocationsData, allocationDataBeforeSplitting, successMessage);

  }

  private upsertResourceAllocations(resourceAllocations: ResourceAllocation[], allocationDataBeforeSplitting: ResourceAllocation[], successMessage?) {

    this.upsertResourceAllocationsToProject.emit({
      resourceAllocation: resourceAllocations,
      event: 'quickAdd',
      splitSuccessMessage: successMessage,
      allocationDataBeforeSplitting: allocationDataBeforeSplitting
    });
  }

  private addElementToErrorList(element) {
    element.classList.add('is-invalid');
    if (!this.elementsWithError.includes(element.id)) {
      this.elementsWithError.push(element.id);
    }
  }

  private removeElementToErrorList(element) {
    element.classList.remove('is-invalid');
    if (this.elementsWithError.includes(element.id)) {
      this.elementsWithError.splice(this.elementsWithError.indexOf(element.id), 1);
    }
  }

}
