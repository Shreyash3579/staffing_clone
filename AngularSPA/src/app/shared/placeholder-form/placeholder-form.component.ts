// ----------------------- Angular Package References ----------------------------------//
import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { Observable, Subject, concat, of } from 'rxjs';
import { distinctUntilChanged, tap, switchMap, catchError, ignoreElements } from 'rxjs/operators';

// ----------------------- External Library References ----------------------------------//
import { BsModalRef } from 'ngx-bootstrap/modal';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import * as moment from 'moment';

// --------------------------Interfaces -----------------------------------------//
import { CaseRoleType } from 'src/app/shared/interfaces/caseRoleType.interface';
import { DateService } from 'src/app/shared/dateService';
import { Employee } from 'src/app/shared/interfaces/employee.interface';
import { InvestmentCategory } from 'src/app/shared/interfaces/investmentCateogry.interface';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';
import { Office } from '../interfaces/office.interface';
import { ServiceLine } from '../interfaces/serviceLine.interface';
import { ResourceAllocation } from '../interfaces/resourceAllocation.interface';
import { PDGrade } from '../interfaces/pdGrade.interface';
import { Resource } from '../interfaces/resource.interface';

// ----------------------- Component/Service References ----------------------------------//
import { LocalStorageService } from '../local-storage.service';
import { PopupDragService } from '../services/popupDrag.service';
import { SharedService } from '../shared.service';
import { ValidationService } from 'src/app/shared/validationService';
import { ConstantsMaster } from '../constants/constantsMaster';
import { ResourceAllocationService } from '../services/resourceAllocation.service';

// ----------------------- System Constants ----------------------------------//
import { BS_DEFAULT_CONFIG } from '../constants/bsDatePickerConfig';
import { PlanningCard } from '../interfaces/planningCard.interface';
import { CommitmentType } from '../interfaces/commitmentType.interface';
import { PositionGroup } from '../interfaces/position-group.interface';
import { CaseType, ServiceLine as ServiceLineEnum } from '../constants/enumMaster';


@Component({
  selector: 'app-plcaeholder-form',
  templateUrl: './placeholder-form.component.html',
  styleUrls: ['./placeholder-form.component.scss'],
  providers: [PopupDragService]
})
export class PlaceholderFormComponent implements OnInit {
  // --------------------------local members--------------------------------//
  private radix = 10;
  private isConfirmingPlaceholder: boolean;
  offices: Office[];
  serviceLines: ServiceLine[];
  pdGrades: PDGrade[];
  positionGroups: PositionGroup[];
  ringfences: CommitmentType[];
  commitmentTypeGroup: any;
  projects: Observable<Project>;
  resources: Observable<Employee>;
  showCaseAndAllocation = false;
  showInvestmentAndCaseRole = false;
  showCommitmentAllocation = false;
  asyncProjectString: string;
  asyncResourceString = '';
  resourceInput$ = new Subject<string>();
  errorList = [];
  investmentCategories: InvestmentCategory[];
  investmentCategoriesToShow:InvestmentCategory[];
  caseRoleTypes: CaseRoleType[];
  modalTitle = '';
  startDateLabel: string;
  endDateLabel: string;
  isFormDirty = false;
  noResultsFoundMsg = ValidationService.noResultsFoundMsg;
  formModel = {
    id: null,
    office: { value: null, isInvalid: false },
    serviceLine: { value: null, isInvalid: false },
    pdGrade: { value: null, isInvalid: false },
    ringfence: { value: null, isInvalid: false },
    startDate: { value: null, isInvalid: false },
    endDate: { value: null, isInvalid: false },
    resource: { value: null, isInvalid: false },
    project: { value: null, isInvalid: false },
    allocation: { value: null, isInvalid: false },
    positionGroup: { value: null, isInvalid: false },
    notes: { value: null },
    role: { value: null },
    investmentCategory: { value: null }
  };
  bsConfig: Partial<BsDatepickerConfig>;
  resourcesData$: Observable<Resource[]>;
  selectedResource: Resource;
  isResourceSearchOpen = false;
  resourcesHavingJoiningDateGreaterThanStartDate = [];
  public notesAlertText = ConstantsMaster.NotesAlert;

  // ----------------- Properties Set By Modal Service Initial State ---------------------------//
  public placeholderAllocationData: PlaceholderAllocation;
  public isUpdateModal = false;
  public projectData: Project;
  public planningCardData: PlanningCard;

  // --------------------------Ouput Events--------------------------------//
  @Output() upsertPlaceholderAllocationsToProject = new EventEmitter<any>();
  @Output() deletePlaceholderAllocationByIds = new EventEmitter();
  @Output() upsertResourceAllocationsToProject = new EventEmitter();
  @Output() openBackFillPopUp = new EventEmitter<any>();

  // --------------------------constructor--------------------------------//
  constructor(public bsModalRef: BsModalRef,
    private localStorageService: LocalStorageService,
    private sharedService: SharedService,
    private _resourceAllocationService: ResourceAllocationService,
    private _popupDragService: PopupDragService) { }

  // --------------------------Life Cycle Event handlers---------------------------------//

  ngOnInit() {
    this.isConfirmingPlaceholder = false;
    this.initialiseDatePicker();
    this.loadLookupListFromLocalStorage();
    this.initializeFormData();
    this.attachEventsToElements();
    this._popupDragService.dragEvents();
  }

  // --------------------------Initialization Functions--------------------------------//

  private initialiseDatePicker() {
    this.bsConfig = BS_DEFAULT_CONFIG;
  }

  private loadLookupListFromLocalStorage() {
    this.offices = this.localStorageService.get(ConstantsMaster.localStorageKeys.OfficeList);
    this.serviceLines = this.localStorageService.get(ConstantsMaster.localStorageKeys.serviceLines);
    this.investmentCategories = this.localStorageService.get(ConstantsMaster.localStorageKeys.investmentCategories);
    if(!this.placeholderAllocationData || this.placeholderAllocationData?.caseTypeCode != ConstantsMaster.InvestmentCategory.AdHoc.investmentCode){
      this.investmentCategoriesToShow = this.investmentCategories.filter(x => x.investmentCode === null);
    }
    this.caseRoleTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.caseRoleTypes);
    this.pdGrades = this.localStorageService.get(ConstantsMaster.localStorageKeys.levelGrades);
    this.positionGroups = this.localStorageService.get(ConstantsMaster.localStorageKeys.positionsGroups);
    this.ringfences = this.getRingfencesFromLocalStorage();;
  }

  getRingfencesFromLocalStorage() {
    const ringfences: CommitmentType[] = this.localStorageService.get(ConstantsMaster.localStorageKeys.ringfences);
    const dummyRingfence: CommitmentType = { commitmentTypeCode: null, commitmentTypeName: null, precedence: 0 }

    return [dummyRingfence, ...ringfences]

  }

  private initializeFormData() {
    this.setModalTitle();
    if (this.placeholderAllocationData) {
      this.loadDataToUpdate();
    } else {
      this.loadDefaultData();
    }

  }

  private attachEventsToElements() {
    this.attachEventForResourcesSearch();
  }

  // --------------------------Helper Functions--------------------------------//
  private setModalTitle() {
    if (this.isUpdateModal) {
      this.modalTitle = `Update/Confirm placeholder Allocation`;
    } else {
      this.modalTitle = `Add placeholder Allocation`;
    }
  }

  private attachEventForResourcesSearch() {
    this.resourcesData$ = concat(
      of([]), // default items
      this.resourceInput$.pipe(
        distinctUntilChanged(),
        switchMap(term => this.sharedService.getResourcesBySearchString(term).pipe(
          catchError(() => of([])), // empty list on error
        )),
        tap(() => {
          this.isResourceSearchOpen = true;
        })
      )
    );
  }

  private setCompliantFormDates(applyProjectDates = false) {
    const projectStartDate = this.formModel.project.value.caseStartDate ?? this.formModel.project.value.opportunityStartDate ?? this.formModel.project.value.planningCardStartDate;
    const projectEndDate = this.formModel.project.value.caseEndDate ?? this.formModel.project.value.opportunityEndDate ?? this.formModel.project.value.planningCardEndDate;
    if (projectStartDate && projectEndDate && applyProjectDates) {
      this.formModel.startDate.value = new Date(projectStartDate);
      this.formModel.endDate.value = new Date(projectEndDate);
    }

    this.alignDatesWithJoining();

    this.setDateLabels(projectStartDate, projectEndDate);
  }

  private setAllocationStartDate(projectStartDate) {
    const today = moment();
    const startDate = moment(projectStartDate);

    if (today.isBefore(startDate)) {
      return new Date(projectStartDate);
    }
    return new Date();
  }

  private setAllocationEndDate(allocationStartDate, projectEndDate) {
    const today = moment();

    if (today.isSameOrAfter(projectEndDate)) {
      return new Date();
    } else {
      const maxEndDate = DateService.getMaxEndDateForAllocation(allocationStartDate, new Date(projectEndDate));
      return maxEndDate;
    }
  }

  private alignDatesWithJoining() {
    if (this.formModel.resource.value?.startDate) {
      if (moment(this.formModel.resource.value.startDate).isAfter(this.formModel.startDate.value, 'day')) {
        this.formModel.startDate.value = new Date(this.formModel.resource.value.startDate);
      }
      if (moment(this.formModel.startDate.value).isAfter(this.formModel.endDate.value)) {
        this.formModel.endDate.value = this.formModel.startDate.value;
      }
    }
  }

  private loadDataToUpdate() {
    this.formModel.id = this.placeholderAllocationData.id;

    this.asyncResourceString = this.placeholderAllocationData.employeeName || '';

    this.asyncProjectString = this.placeholderAllocationData.caseName || this.placeholderAllocationData.opportunityName ||this.placeholderAllocationData.planningCardTitle|| '';

    this.formModel.allocation.value = this.placeholderAllocationData.allocation !== undefined ?
      parseInt(this.placeholderAllocationData.allocation.toString()) : null;

    this.formModel.startDate.value =
      this.placeholderAllocationData.startDate ? new Date(this.placeholderAllocationData.startDate) : null;

    this.formModel.endDate.value =
      this.placeholderAllocationData.endDate ? new Date(this.placeholderAllocationData.endDate) : null;

    this.formModel.investmentCategory.value =
      this.investmentCategories.find(x => x.investmentCode === (this.placeholderAllocationData.investmentCode || null));

    this.formModel.role.value =
      this.caseRoleTypes.find(x => x.caseRoleCode === (this.placeholderAllocationData.caseRoleCode || null));

    this.formModel.notes.value = this.placeholderAllocationData.notes;

    this.formModel.office.value =
      this.offices.find(x => x.officeCode === (this.placeholderAllocationData.operatingOfficeCode || null));

    this.formModel.serviceLine.value =
      this.serviceLines.find(x => x.serviceLineCode === (this.placeholderAllocationData.serviceLineCode || null));

    this.formModel.pdGrade.value =
      this.pdGrades.find(x => x.pdGradeName === (this.placeholderAllocationData.currentLevelGrade || null));

    this.formModel.ringfence.value =
      this.ringfences.find(x => x.commitmentTypeCode === (this.placeholderAllocationData.commitmentTypeCode || null));

    this.formModel.positionGroup.value = this.placeholderAllocationData.positionGroupCode;

    if (this.placeholderAllocationData.employeeCode) {
      this.formModel.resource.value = {
        employeeCode: this.placeholderAllocationData.employeeCode,
        employeeName: this.placeholderAllocationData.employeeName,
        currentLevelGrade: this.placeholderAllocationData.currentLevelGrade,
        operatingOfficeCode: this.placeholderAllocationData.operatingOfficeCode,
        operatingOfficeAbbreviation: this.placeholderAllocationData.operatingOfficeAbbreviation,
        serviceLineCode: this.placeholderAllocationData.serviceLineCode,
        serviceLineName: this.placeholderAllocationData.serviceLineName,
        startDate: this.placeholderAllocationData.joiningDate || null
      };
    }

    this.formModel.project.value = {
      oldCaseCode: this.placeholderAllocationData.oldCaseCode,
      caseName: this.placeholderAllocationData.caseName,
      clientName: this.placeholderAllocationData.clientName,
      caseTypeCode: this.placeholderAllocationData.caseTypeCode,
      pipelineId: this.placeholderAllocationData.pipelineId,
      opportunityName: this.placeholderAllocationData.opportunityName,
      caseStartDate: this.placeholderAllocationData.caseStartDate,
      caseEndDate: this.placeholderAllocationData.caseEndDate,
      opportunityStartDate: this.placeholderAllocationData.opportunityStartDate,
      opportunityEndDate: this.placeholderAllocationData.opportunityEndDate,
      planningCardStartDate: null, //NULL for now as we don't have a planning card details page from where we can edit allocations on planning card
      planningCardEndDate: null
    };

    this.setCompliantFormDates();
    this.setInvestmentCategoryDropdownBasedOnCaseType();

  }

  setInvestmentCategoryDropdownBasedOnCaseType(){
    const caseTypeCode = this.formModel.project.value.caseTypeCode;
    const isClientDevelopment = caseTypeCode && caseTypeCode.toString() === CaseType.ClientDevelopment;
      if(isClientDevelopment){
      this.formModel.investmentCategory.value = this.investmentCategories.find(x => x.investmentCode === ConstantsMaster.InvestmentCategory.AdHoc.investmentCode);
    }
    else{

    }
      this.investmentCategoriesToShow = this.investmentCategories.filter(x => x.applicableCaseTypeCodes == null || x.applicableCaseTypeCodes.includes(this.formModel.project.value.caseTypeCode));
  }

  private loadDefaultData() {
    this.formModel.investmentCategory.value = this.investmentCategories[0];
    this.formModel.role.value = this.caseRoleTypes[0];
    this.formModel.notes.value = '';
    this.formModel.allocation.value = 100;
    this.formModel.serviceLine.value = (this.projectData?.oldCaseCode || this.projectData?.pipelineId) ? this.serviceLines.find(x=>x.serviceLineCode == ServiceLineEnum.GeneralConsulting) : null;
    this.formModel.office.value = (this.projectData?.oldCaseCode || this.projectData?.pipelineId) ? this.offices.find(x=> x.officeAbbreviation === this.projectData?.managingOfficeAbbreviation) : null;

    this.asyncProjectString = this.projectData?.caseName ?? this.projectData?.opportunityName ?? this.planningCardData.name ?? '';
    this.formModel.project.value = {
      oldCaseCode: this.projectData?.oldCaseCode,
      caseName: this.projectData?.caseName,
      clientName: this.projectData?.clientName,
      pipelineId: this.projectData?.pipelineId,
      opportunityName: this.projectData?.opportunityName,
      caseStartDate: this.projectData?.startDate,
      caseEndDate: this.projectData?.endDate,
      opportunityStartDate: this.projectData?.pipelineId ? this.projectData.startDate : null,
      opportunityEndDate: this.projectData?.pipelineId ? this.projectData.endDate : null,
      planningCardStartDate: this.planningCardData?.startDate,
      planningCardEndDate: this.planningCardData?.endDate

    };
    this.setCompliantFormDates(true);
  }

  private setDateLabels(projectStartDate, projectEndDate) {

    this.startDateLabel = DateService.convertDateInBainFormat(projectStartDate);
    this.endDateLabel = DateService.convertDateInBainFormat(projectEndDate);

  }

  private upsertPlaceholderAllocations(placeholderAllocations: PlaceholderAllocation[]) {
    this.upsertPlaceholderAllocationsToProject.emit({
      placeholderAllocations: placeholderAllocations
    });
  }

  private prepareAndUpsertPlaceholderAllocations() {
    if (!this.selectedResource && !this.formModel.resource.value) {
      this.upsertGuessedPlaceholder();
      return;
    }
    this.upsertPlaceholderAllocationForResources();
  }

  private upsertGuessedPlaceholder() {
    const allocationsData: PlaceholderAllocation[] = [];
    const placeholderAllocation = this.populatePlacholderAllocationProperties();
    allocationsData.push(placeholderAllocation);
    this.upsertPlaceholders(allocationsData);
  }

  private upsertPlaceholderAllocationForResources() {
    const allocationsData: PlaceholderAllocation[] = [];
    const allocatedResource = this.formModel.resource.value;

    if (this.selectedResource) {
      const placeholderAllocation = this.populatePlacholderAllocationProperties(this.selectedResource, null);
      allocationsData.push(placeholderAllocation);
    } else {
      const placeholderAllocation = this.populatePlacholderAllocationProperties(null, allocatedResource);
      allocationsData.push(placeholderAllocation);
    }
    this.upsertPlaceholders(allocationsData);
  }
  private confirmPlaceholderAllocationOnPlanningCard() {
    const allocationsData: PlaceholderAllocation[] = [];
    const allocatedResource = this.formModel.resource.value;
      const placeholderAllocation = this.populateConfirmPlaceholderAllocationForPlanningCard(this.selectedResource,allocatedResource);
      allocationsData.push(placeholderAllocation);
    this.upsertPlaceholders(allocationsData);
  }

  private upsertPlaceholders(placeholderAllocations: PlaceholderAllocation[]) {
    //check if the new allocation falls in month close period
    const [isValidAllocation, validationMessage] =
      this._resourceAllocationService.validateMonthCloseForInsertAndDelete(placeholderAllocations);

    if (!isValidAllocation && !placeholderAllocations[0].planningCardId) {
      this.errorList.push(validationMessage);
      return;
    }

    this.upsertPlaceholderAllocations(placeholderAllocations);

    this.closeForm();
  }

  private populateConfirmPlaceholderAllocationForPlanningCard(selectedResource?: Resource, allocatedResource?: PlaceholderAllocation){
    let placeholderAllocation: PlaceholderAllocation = {
      id: this.placeholderAllocationData?.id,
      planningCardId: this.planningCardData?.id,
      caseName: this.formModel.project.value?.caseName,
      caseTypeCode: this.formModel.project.value?.caseTypeCode,
      clientName: this.formModel.project.value?.clientName,
      oldCaseCode: this.formModel.project.value?.oldCaseCode,
      employeeCode: selectedResource?.employeeCode ?? allocatedResource?.employeeCode,
      employeeName: selectedResource?.fullName ?? allocatedResource?.employeeName,
      currentLevelGrade: selectedResource?.levelGrade ?? allocatedResource?.currentLevelGrade,
      serviceLineCode: selectedResource?.serviceLine.serviceLineCode ?? allocatedResource?.serviceLineCode,
      serviceLineName: selectedResource?.serviceLine.serviceLineName ?? allocatedResource?.serviceLineName,
      operatingOfficeCode: selectedResource?.schedulingOffice.officeCode ?? allocatedResource?.operatingOfficeCode,
      operatingOfficeAbbreviation: selectedResource?.office.officeAbbreviation ?? allocatedResource?.operatingOfficeAbbreviation,
      pipelineId: this.formModel.project.value?.pipelineId,
      opportunityName: this.formModel.project.value?.opportunityName,
      investmentCode: this.formModel.investmentCategory.value?.investmentCode,
      investmentName: this.formModel.investmentCategory.value?.investmentName,
      caseRoleCode: this.formModel.role.value?.caseRoleCode,
      allocation: parseInt(this.formModel.allocation?.value, this.radix),
      startDate: DateService.getFormattedDate(this.formModel.startDate?.value),
      endDate: DateService.getFormattedDate(this.formModel.endDate?.value),
      caseStartDate: this.formModel.project.value?.caseStartDate,
      caseEndDate: this.formModel.project.value?.caseEndDate,
      opportunityStartDate: this.formModel.project.value?.opportunityStartDate,
      opportunityEndDate: this.formModel.project.value?.opportunityEndDate,
      lastUpdatedBy: null,
      notes: this.formModel.notes.value,
      isPlaceholderAllocation: false,
      positionGroupCode: this.formModel.positionGroup.value
    };
    return placeholderAllocation;
  }

  private populatePlacholderAllocationProperties(selectedResource?: Resource, allocatedResource?: PlaceholderAllocation) {
    let placeholderAllocation: PlaceholderAllocation = {
      id: this.placeholderAllocationData?.id,
      planningCardId: this.planningCardData?.id,
      caseName: this.formModel.project.value?.caseName,
      caseTypeCode: this.formModel.project.value?.caseTypeCode,
      clientName: this.formModel.project.value?.clientName,
      oldCaseCode: this.formModel.project.value?.oldCaseCode,
      employeeCode: selectedResource?.employeeCode ?? allocatedResource?.employeeCode,
      employeeName: selectedResource?.fullName ?? allocatedResource?.employeeName,
      currentLevelGrade: selectedResource?.levelGrade ?? allocatedResource?.currentLevelGrade ?? this.formModel.pdGrade.value?.pdGradeName,
      commitmentTypeCode: this.formModel.ringfence.value?.commitmentTypeCode,
      serviceLineCode: selectedResource?.serviceLine.serviceLineCode ?? allocatedResource?.serviceLineCode ?? this.formModel.serviceLine.value?.serviceLineCode,
      serviceLineName: selectedResource?.serviceLine.serviceLineName ?? allocatedResource?.serviceLineName ?? this.formModel.serviceLine.value?.serviceLineName,
      operatingOfficeCode: selectedResource?.schedulingOffice.officeCode ?? allocatedResource?.operatingOfficeCode ?? this.formModel.office.value?.officeCode,
      operatingOfficeAbbreviation: selectedResource?.schedulingOffice.officeAbbreviation ?? allocatedResource?.operatingOfficeAbbreviation ?? this.formModel.office.value?.officeAbbreviation,
      pipelineId: this.formModel.project.value?.pipelineId,
      opportunityName: this.formModel.project.value?.opportunityName,
      investmentCode: this.formModel.investmentCategory.value?.investmentCode,
      investmentName: this.formModel.investmentCategory.value?.investmentName,
      caseRoleCode: this.formModel.role.value?.caseRoleCode,
      allocation: parseInt(this.formModel.allocation?.value, this.radix),
      startDate: DateService.getFormattedDate(this.formModel.startDate?.value),
      endDate: DateService.getFormattedDate(this.formModel.endDate?.value),
      caseStartDate: this.formModel.project.value?.caseStartDate,
      caseEndDate: this.formModel.project.value?.caseEndDate,
      opportunityStartDate: this.formModel.project.value?.opportunityStartDate,
      opportunityEndDate: this.formModel.project.value?.opportunityEndDate,
      lastUpdatedBy: null,
      notes: this.formModel.notes.value,
      isPlaceholderAllocation: this.isConfirmingPlaceholder ? false : true,
      isConfirmed: this.isConfirmingPlaceholder,
      positionGroupCode: this.formModel.positionGroup.value
    };
    return placeholderAllocation;
  }

  private confirmPlaceholderAllocationOnCase() {
    const allocationsData: PlaceholderAllocation[] = [];
    const allocatedResource = this.formModel.resource.value;

    if (this.selectedResource) {
      const placeholderAllocation = this.populateResourceAllocationProperties(this.selectedResource, null);
      allocationsData.push(placeholderAllocation);
    } else {
      const placeholderAllocation = this.populateResourceAllocationProperties(null, allocatedResource);
      allocationsData.push(placeholderAllocation);
    }

    this.checkForMonthClosePrePostBackfillAndConfirm(allocationsData);

  }

  private populateResourceAllocationProperties(selectedResource: Resource, allocatedResource: ResourceAllocation) {
    const resourceAllocation: ResourceAllocation = {
      caseName: this.formModel.project.value.caseName,
      caseTypeCode: this.formModel.project.value.caseTypeCode,
      clientName: this.formModel.project.value.clientName,
      oldCaseCode: this.formModel.project.value.oldCaseCode,
      employeeCode: selectedResource?.employeeCode ?? allocatedResource?.employeeCode,
      employeeName: selectedResource?.fullName ?? allocatedResource?.employeeName,
      currentLevelGrade: selectedResource?.levelGrade ?? allocatedResource?.currentLevelGrade,
      serviceLineCode: selectedResource?.serviceLine.serviceLineCode ?? allocatedResource?.serviceLineCode,
      serviceLineName: selectedResource?.serviceLine.serviceLineName ?? allocatedResource?.serviceLineName,
      operatingOfficeCode: selectedResource?.schedulingOffice.officeCode ?? allocatedResource?.operatingOfficeCode,
      operatingOfficeAbbreviation: selectedResource?.office.officeAbbreviation ?? allocatedResource?.operatingOfficeAbbreviation,
      pipelineId: this.formModel.project.value.pipelineId,
      opportunityName: this.formModel.project.value.opportunityName,
      investmentCode: this.formModel.investmentCategory.value.investmentCode,
      investmentName: this.formModel.investmentCategory.value.investmentName,
      caseRoleCode: this.formModel.role.value.caseRoleCode,
      allocation: parseInt(this.formModel.allocation.value, this.radix),
      startDate: DateService.getFormattedDate(this.formModel.startDate.value),
      endDate: DateService.getFormattedDate(this.formModel.endDate.value),
      previousStartDate: null,
      previousEndDate: null,
      previousAllocation: null,
      caseStartDate: this.formModel.project.value.caseStartDate,
      caseEndDate: this.formModel.project.value.caseEndDate,
      opportunityStartDate: this.formModel.project.value.opportunityStartDate,
      opportunityEndDate: this.formModel.project.value.opportunityEndDate,
      lastUpdatedBy: null,
      notes: this.formModel.notes.value,
      isPlaceholderAllocation: false,
      positionGroupCode: this.formModel.positionGroup.value
    };
    return resourceAllocation;
  }

  private checkForMonthClosePrePostBackfillAndConfirm(placeholderAllocationsToConfirm: ResourceAllocation | ResourceAllocation[]) {
    const inputAllocationsData: ResourceAllocation[] = [].concat(placeholderAllocationsToConfirm);

    //check if the new allocation falls in month close period
    const [isValidAllocation, validationMessage] = this._resourceAllocationService.validateMonthCloseForInsertAndDelete(placeholderAllocationsToConfirm);
    if (!isValidAllocation) {
      this.errorList.push(validationMessage);
      return;
    }
    if (inputAllocationsData.length > 1) {
      this.checkForPrePostAndUPsertAllocations(inputAllocationsData);
    } else {
      let allocationToConfirm = inputAllocationsData[0];
      if (this.formModel.project.value.oldCaseCode) {
        this.sharedService.getCaseDetailsAndAllocations(this.formModel.project.value.oldCaseCode).subscribe(caseDetailsAndAllocations => {
          this.checkForBackfillAndUpsertResourceAllocation(caseDetailsAndAllocations, allocationToConfirm);
        });
      } else {
        this.sharedService.getOpportunityDetailsAndAllocations(this.formModel.project.value.pipelineId)
          .subscribe(opportunityDetailsAndAllocations => {
            this.checkForBackfillAndUpsertResourceAllocation(opportunityDetailsAndAllocations, allocationToConfirm);
          });
      }
    }

    this.closeForm();
  }

  private checkForPrePostAndUPsertAllocations(allocationsToConfirm: ResourceAllocation | ResourceAllocation[]) {
    const inputAllocationsData: ResourceAllocation[] = [].concat(allocationsToConfirm);
    let outputAllocationsData: ResourceAllocation[] = [];
    outputAllocationsData = this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(inputAllocationsData);
    this.upsertResourceAllocations(outputAllocationsData, inputAllocationsData);
  }

  private checkForBackfillAndUpsertResourceAllocation(projectData, resourceAllocation: ResourceAllocation) {

    if (this._resourceAllocationService.isBackFillRequiredOnProject(projectData.allocatedResources, projectData)) {
      // Adding id to the resource allocation object as backfill popup needs placeholder allocation Id to delete it
      resourceAllocation.id = this.placeholderAllocationData.id;

      const showMoreThanYearWarning = ValidationService.checkIfAllocationIsOfOneYear(new Date(projectData.startDate), new Date(projectData.endDate));

      this.openBackFillPopUp.emit({
        project: projectData,
        resourceAllocation: resourceAllocation,
        showMoreThanYearWarning: showMoreThanYearWarning,
        isPlaceholderAllocation: true,
        allocationDataBeforeSplitting: [].concat(resourceAllocation)
      });
    } else {
      this.checkForPrePostAndUPsertAllocations(resourceAllocation);
    }
  }

  private deletePlaceholders(notifyMessage?: string) {
    if (!notifyMessage) {
      notifyMessage = 'Assigment Deleted'; 
    }
    this.deletePlaceholderAllocationByIds.emit({
      placeholderAllocation: [].concat(this.placeholderAllocationData),
      placeholderIds: this.placeholderAllocationData.id,
      notifyMessage: notifyMessage
    });
    this.closeForm();
  }

  // --------------------------Event handlers---------------------------------//
  /// Multiselect TODO: use type-ahead instead of custom search function once we replace bootstrap typeahead with ng-select
  // we can't do it right now as [typeahead] directive creates conflict in bootstrap and ng-select
  onResourceSearchChange($event) {
    if ($event.term.length > 2) {
      this.resourceInput$.next($event.term);
    }

    // to reset search term if keyword's length is less than 3
    if ($event.term.length < 3) {
      this.resourceInput$.next(null);
    }

    // TODO: below condition should be removed once permanent solution is applied
    if ($event.term.length < 1) {
      this.isResourceSearchOpen = false;
    }
  }

  confirmPlaceholderAllocation() {
    this.isConfirmingPlaceholder = true;
    if (!this.isCommitmentDataValid()) {
      return false;
    }
    if(this.planningCardData){
     this.confirmPlaceholderAllocationOnPlanningCard();
    }
    else{
    this.confirmPlaceholderAllocationOnCase();
    }

  }


  addPlaceholderAllocations() {
    if (!this.isCommitmentDataValid()) {
      return false;
    }
    this.prepareAndUpsertPlaceholderAllocations();
  }

  updatePlaceholderAllocation() {
    if (!this.isConfirmingPlaceholder) {
      this.isConfirmingPlaceholder = false;
      if (!this.isCommitmentDataValid()) {
        return false;
      }
      if (this.isFormDirty || this.selectedResource) {
        this.prepareAndUpsertPlaceholderAllocations();
      }
    }
    else {
      this.prepareAndUpsertPlaceholderAllocations();
    }
    this.closeForm();
  }

  deletePlaceholderAllocation() {
    this.deletePlaceholders();
    this.closeForm();
  }

  upsertResourceAllocations(resourceAllocations: ResourceAllocation[], allocationDataBeforeSplitting: ResourceAllocation[]) {
    this.upsertResourceAllocationsToProject.emit({
      resourceAllocation: resourceAllocations,
      event: 'quickAdd',
      allocationDataBeforeSplitting: allocationDataBeforeSplitting
    });

    if (this.isConfirmingPlaceholder) {
      this.deletePlaceholderAllocation();
    }
    else {
      this.updatePlaceholderAllocation();
    }
  }

  selectedResourcesChange($event) {
    this.selectedResource = $event;
    this.isResourceSearchOpen = false;
    if (this.selectedResource) {
      this.formModel.allocation.value = parseInt((this.selectedResource.fte * 100).toString());
      this.formModel.positionGroup.value = this.positionGroups.find(x => x.positionGroupName === this.selectedResource.position.positionGroupName)?.positionGroupCode;
    } else {
      this.formModel.allocation.value = 100;
      this.formModel.positionGroup.value = this.placeholderAllocationData ? this.placeholderAllocationData.positionGroupCode : null;
    }
  }

  typeaheadOnSelect(data, type) {

    if (type === 'project') {
      this.formModel.project.value = {
        oldCaseCode: data.item.oldCaseCode,
        caseName: data.item.caseName,
        caseTypeCode: data.item.caseTypeCode,
        clientName: data.item.clientName,
        pipelineId: data.item.pipelineId,
        opportunityName: data.item.opportunityName,
        caseStartDate: data.item.oldCaseCode ? data.item.startDate : null,
        caseEndDate: data.item.oldCaseCode ? data.item.endDate : null,
        opportunityStartDate: !data.item.oldCaseCode ? data.item.startDate : null,
        opportunityEndDate: !data.item.oldCaseCode ? data.item.endDate : null
      };

      this.setCompliantFormDates();

    }
  }

  closeForm() {
    this.bsModalRef.hide();
  }

  onFormChange() {
    this.isFormDirty = true;
  }

  // ------------------------------Validations ---------------------------------------------
  addToErrorList(type) {
    switch (type) {
      case 'required': {
        if (this.errorList.indexOf(ValidationService.requiredMessage) === -1) {
          this.errorList.push(ValidationService.requiredMessage);
        }
        break;
      }
      case 'dateInvalid': {
        if (this.errorList.indexOf(ValidationService.dateInvalidMessage) === -1) {
          this.errorList.push(ValidationService.dateInvalidMessage);
        }
        break;
      }
      case 'numberInvalid': {
        if (this.errorList.indexOf(ValidationService.numberInvalidMessage) === -1) {
          this.errorList.push(ValidationService.numberInvalidMessage);
        }
        break;
      }
      case 'typeaheadInvalid': {
        if (this.errorList.indexOf(ValidationService.typeaheadInvalidMessage) === -1) {
          this.errorList.push(ValidationService.typeaheadInvalidMessage);
        }
        break;
      }
      case 'dateDiffInvalid': {
        if (this.errorList.indexOf(ValidationService.dateDiffInvalid) === -1) {
          this.errorList.push(ValidationService.dateDiffInvalid);
        }
        break;
      }
      case 'startDateGreaterThanEndDate': {
        if (this.errorList.indexOf(ValidationService.startDateGreaterThanEndDate) === -1) {
          this.errorList.push(ValidationService.startDateGreaterThanEndDate);
        }
        break;
      }
      case 'notYetStartedJoiningDateGreaterThanStartDate': {
        this.addErrorsForResourcesHavingJoiningdateGreaterThanStartDate();
        break;
      }
    }
  }

  private isCommitmentDataValid() {
    this.errorList = [];
    this.validateField('resource');
    this.validateField('startDate');
    this.validateField('endDate');
    this.validateField('office');
    this.validateField('serviceLine');
    this.validateField('pdGrade');
    this.validateField('allocation');
    if (this.formModel.office.isInvalid || this.formModel.resource.isInvalid
      || this.formModel.serviceLine.isInvalid || this.formModel.allocation.isInvalid
      || this.formModel.startDate.isInvalid || this.formModel.endDate.isInvalid
      || this.formModel.pdGrade.isInvalid || this.formModel.allocation.isInvalid) {
      return false;
    } else {
      return true;
    }
  }

  private validateField(fieldName) {
    switch (fieldName) {
      case 'startDate': {
        if (!this.formModel.startDate.value) {
          this.formModel.startDate.isInvalid = true;
          this.addToErrorList('required');
        } else if (this.formModel.startDate.value.toDateString() === undefined
          || this.formModel.startDate.value.toDateString() === 'Invalid Date') {
          this.formModel.startDate.isInvalid = true;
          this.addToErrorList('dateInvalid');
        } else {
          this.formModel.startDate.isInvalid = false;
        }
        break;
      }
      case 'endDate': {
        if (!this.formModel.endDate.value) {
          this.formModel.endDate.isInvalid = true;
          this.addToErrorList('required');
        } else if (this.formModel.endDate.value.toDateString() === undefined
          || this.formModel.endDate.value.toDateString() === 'Invalid Date') {
          this.formModel.endDate.isInvalid = true;
          this.addToErrorList('dateInvalid');
        } else if (Date.parse(this.formModel.endDate.value.toDateString()) < Date.parse(this.formModel.startDate.value.toDateString())) {
          this.formModel.endDate.isInvalid = true;
          this.addToErrorList('startDateGreaterThanEndDate');
        } else {
          this.formModel.endDate.isInvalid = false;
        }
        break;
      }
      case 'resource': {
        if (!this.isConfirmingPlaceholder) {
          this.formModel.resource.isInvalid = false;
        } else if (!this.selectedResource && !this.formModel.resource?.value) {
          this.formModel.resource.isInvalid = true;
          this.addToErrorList('required');
        } else if (this.isEmployeeJoiningDateGreaterThanStartDate()) {
          this.formModel.resource.isInvalid = true;
          this.addToErrorList('notYetStartedJoiningDateGreaterThanStartDate');
        } else {
          this.formModel.resource.isInvalid = false;
        }
        break;
      }
      case 'office': {
        if (!this.formModel.office.value && !this.selectedResource && !this.formModel.resource.value) {
          this.formModel.office.isInvalid = true;
          this.addToErrorList('required');
        } else {
          this.formModel.office.isInvalid = false;
        }
        break;
      }
      case 'serviceLine': {
        if (!this.formModel.serviceLine.value && !this.selectedResource && !this.formModel.resource.value) {
          this.formModel.serviceLine.isInvalid = true;
          this.addToErrorList('required');
        } else {
          this.formModel.serviceLine.isInvalid = false;
        }
        break;
      }
      case 'pdGrade': {
        if (!this.formModel.pdGrade.value && !this.selectedResource && !this.formModel.resource.value) {
          this.formModel.pdGrade.isInvalid = true;
          this.addToErrorList('required');
        } else {
          this.formModel.pdGrade.isInvalid = false;
        }
        break;
      }
      case 'allocation': {
        if (!ValidationService.isAllocationValid(this.formModel.allocation.value)) {
          this.formModel.allocation.isInvalid = true;
          this.addToErrorList('numberInvalid');
        } else {
          this.formModel.allocation.isInvalid = false;
        }
        break;
      }
    }
  }

  private addErrorsForResourcesHavingJoiningdateGreaterThanStartDate() {
    this.resourcesHavingJoiningDateGreaterThanStartDate.forEach(res => {
      this.errorList.push(`${res.fullName ?? res.employeeName} has Joining Date '${DateService.convertDateInBainFormat(res.startDate)}' greater than start Date`);
    });
  }

  private isEmployeeJoiningDateGreaterThanStartDate() {
    this.resourcesHavingJoiningDateGreaterThanStartDate = [];
    if (this.selectedResource) {
      if (moment(this.selectedResource.startDate).isAfter(this.formModel.startDate.value, 'day')) {
        this.resourcesHavingJoiningDateGreaterThanStartDate.push(this.selectedResource);
      }
    } else if (this.formModel.resource.value) {
      if (moment(this.formModel.resource.value.startDate).isAfter(this.formModel.startDate.value, 'day')) {
        this.resourcesHavingJoiningDateGreaterThanStartDate.push(this.formModel.resource.value);
      }
    }
    return this.resourcesHavingJoiningDateGreaterThanStartDate.length > 0;
  }

}
