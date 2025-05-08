// ----------------------- Angular Package References ----------------------------------//
import { Component, OnInit, EventEmitter, Output, ChangeDetectionStrategy, ViewChild, ChangeDetectorRef, ElementRef } from '@angular/core';
import { Observable, Subject, concat, of } from 'rxjs';
import { mergeMap, debounceTime, distinctUntilChanged, tap, switchMap, catchError, ignoreElements } from 'rxjs/operators';

// ----------------------- External Library References ----------------------------------//
import { BsModalRef } from 'ngx-bootstrap/modal';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import * as moment from 'moment';

// --------------------------Interfaces -----------------------------------------//
import { Commitment } from 'src/app/shared/interfaces/commitment.interface';
import { CommitmentType } from 'src/app/shared/interfaces/commitmentType.interface';
import { CaseRoleType } from 'src/app/shared/interfaces/caseRoleType.interface';
import { DateService } from 'src/app/shared/dateService';
import { Employee } from 'src/app/shared/interfaces/employee.interface';
import { InvestmentCategory } from 'src/app/shared/interfaces/investmentCateogry.interface';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';


// ----------------------- Component/Service References ----------------------------------//
import { LocalStorageService } from '../local-storage.service';
import { PopupDragService } from '../services/popupDrag.service';
import { ResourceAllocationService } from '../services/resourceAllocation.service';
import { SharedService } from '../shared.service';
import { NotificationService } from 'src/app/shared/notification.service';
import { ValidationService } from 'src/app/shared/validationService';
import { ConstantsMaster } from '../constants/constantsMaster';
import { Resource } from '../interfaces/resource.interface';
import { OverlayMessageService } from 'src/app/overlay/behavioralSubjectService/overlayMessage.service';

// ----------------------- System Constants ----------------------------------//
import { BS_DEFAULT_CONFIG } from '../constants/bsDatePickerConfig';
import { CommitmentType as CommitmentTypeEnum } from '../constants/enumMaster';
import { CoreService } from 'src/app/core/core.service';
import { CommonService } from '../commonService';
import { PlaceholderAllocation } from '../interfaces/placeholderAllocation.interface';
import { PlaceholderAssignmentService } from 'src/app/overlay/behavioralSubjectService/placeholderAssignment.service';
import { PlanningCardTypeAheadComponent } from '../planning-card-typeahead/planning-card-typeahead.component';
import { CaseType } from '../constants/enumMaster';
import { CommitmentTypeReasons } from '../interfaces/commitmentTypeReasons.interface';
@Component({
  selector: 'app-quick-add-form',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './quick-add-form.component.html',
  styleUrls: ['./quick-add-form.component.scss'],
  providers: [PopupDragService]
})
export class QuickAddFormComponent implements OnInit {

  private radix = 10;
  public commitmentTypes: CommitmentType[];
  public commitmentTypeReasons: CommitmentTypeReasons[];
  public commitmentTypeGroup: any;
  public projects: Observable<Project>;
  public resources: Observable<Employee>;
  public showNonBillableReason = false;
  public showCaseAndAllocation = false;
  public showPlanningCardAndAllocation = false;
  public showInvestmentAndCaseRole = false;
  public showCommitmentAllocation = false;
  public asyncProjectString: string;
  public asyncResourceString = '';
  resourceInput$ = new Subject<string>();
  public errorList = [];
  public investmentCategories: InvestmentCategory[];
  public investmentCategoriesToShow: InvestmentCategory[];
  public caseRoleTypes: CaseRoleType[];
  modalTitle = '';
  startDateLabel: string;
  endDateLabel: string;
  isFormDirty = false;
  public noCaseOppFound = false;
  public noResultsFoundMsg = ValidationService.noResultsFoundMsg;
  public notesAlertText = ConstantsMaster.NotesAlert;


  public formModel = {
    id: null,
    type: { value: null, isInvalid: false },
    commitmentTypeReason: { value: null, isInvalid: false },
    startDate: { value: null, isInvalid: false, readonly: false },
    endDate: { value: null, isInvalid: false, readonly: false },
    resource: { value: null, isInvalid: false },
    project: { value: null, isInvalid: false },
    allocation: { value: null, isInvalid: false },
    notes: { value: null },
    role: { value: null },
    investmentCategory: { value: null }
  };

  public hasAccessToNonBillableReason;

  // public project: Project;
  bsConfig: Partial<BsDatepickerConfig>;
  // Multiselect
  resourcesData$: Observable<Resource[]>;
  selectedResources: Resource[] = [];
  isResourceSearchOpen = false;
  resourcesHavingJoiningDateGreaterThanStartDate = [];
  // ----------------- Properties Set By Modal Service Initial State ---------------------------//
  public commitmentTypeCode: string;
  public commitmentTypeReasonCode: string;
  public resourceAllocationData: ResourceAllocation;
  public isUpdateModal = false;
  public commitmentTypesToShow?: string;
  public employeeCode?: string = '';

  // --------------------------Ouput Events--------------------------------//

  @Output() upsertResourceAllocationsToProject = new EventEmitter<any>();
  @Output() insertResourcesCommitments = new EventEmitter<any>();
  @Output() deleteResourceAllocationFromCase = new EventEmitter();
  @Output() deletePlaceholderAllocationByIds = new EventEmitter();
  @Output() openBackFillPopUp = new EventEmitter<any>();
  @Output() openOverlappedTeamsPopup = new EventEmitter<any>();
  @Output() updateResourceCommitment = new EventEmitter();
  @Output() deleteResourceCommitment = new EventEmitter();
  @Output() upsertPlaceholderAllocationsToProject = new EventEmitter();

  //-----------------------------View Childs------------------------------//
  @ViewChild('planningCardTypeahead') planningCardTypeaheadComponentInstance: PlanningCardTypeAheadComponent;

  constructor(public bsModalRef: BsModalRef,
    private localStorageService: LocalStorageService,
    private notifyService: NotificationService,
    private sharedService: SharedService,
    private _resourceAllocationService: ResourceAllocationService,
    private _coreService: CoreService,
    private _popupDragService: PopupDragService,
    private readonly changeDetector: ChangeDetectorRef,
    private placeholderAssignmentService: PlaceholderAssignmentService,
    private overlayMessageService: OverlayMessageService) { }

  // --------------------------Life Cycle Event handlers---------------------------------//

  ngOnInit() {
    this.initialiseDatePicker();
    this.loadLookupListFromLocalStorage();
    this.checkAccessForCommitmentTypeReasonField();
    this.initializeFormData();
    this.initializeCommitmentTypes();
    this.attachEventsToElements();
    this._popupDragService.dragEvents();
  }

  // --------------------------Helper Functions--------------------------------//

  initialiseDatePicker() {

    this.bsConfig = BS_DEFAULT_CONFIG;

  }

  getEditableCommitments() {
    const commitmentTypes = (this.localStorageService.get(ConstantsMaster.localStorageKeys.commitmentTypes));
    
    return CommonService.getEditableCommitmentTypes(this._coreService.loggedInUserClaims, commitmentTypes);
  }

  checkAccessForCommitmentTypeReasonField()
  {
    this.hasAccessToNonBillableReason = this._coreService.loggedInUserClaims.FeatureAccess
    .some(x => x.FeatureName === 'commitmentTypeReason');

  }

  loadLookupListFromLocalStorage() {
    this.commitmentTypes = this.getEditableCommitments();
    this.commitmentTypeReasons = this.localStorageService.get(ConstantsMaster.localStorageKeys.commitmentTypeReasons); 
    this.investmentCategories = this.localStorageService.get(ConstantsMaster.localStorageKeys.investmentCategories);
    this.investmentCategoriesToShow = this.investmentCategories.filter(x => x.applicableCaseTypeCodes == null);
    this.caseRoleTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.caseRoleTypes);
  }

  initializeCommitmentTypes() {
    if (this.commitmentTypesToShow) {
      this.commitmentTypes = this.commitmentTypes.filter(
        commitmentType => this.commitmentTypesToShow.includes(commitmentType.commitmentTypeCode));
    }
    
    let commitmentGroups = [];
    ConstantsMaster.commitmentTypeGroups
      .sort((prev, next) => prev.order - next.order)
      .forEach(grp => {
        commitmentGroups.push({
          group: grp.name,
          items: this.commitmentTypes.filter( x => x.isStaffingTag === grp.isStaffingTag)
        });
      });

    this.commitmentTypeGroup = commitmentGroups.filter(x => x.items.length > 0);
  }

  attachEventsToElements() {
    this.attachEventForProjectSearch();
    this.attachEventForResourcesSearch();
  }

  attachEventForProjectSearch() {

    this.projects = Observable.create((observer: any) => {
      // Runs on every search
      observer.next(this.asyncProjectString);
    }).pipe(
      debounceTime(1000),
      mergeMap((token: string) => this.sharedService.getProjectsBySearchString(token))
    );

  }

  attachEventForResourcesSearch() {
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

  initializeFormData() {
    this.setModalTitle();

    if (this.resourceAllocationData) {
      this.loadEditedCommitmentData();

    } else if (this.employeeCode) {
      this.setResourceDataByECode();
    } else {

      // Called when opened through Quick-Add button
      this.loadDefaultData();

    }
  }

  setResourceDataByECode() {
    if (this.employeeCode) {
      this.sharedService.getResourcesBySearchString(this.employeeCode).subscribe(employeesData => {
        var employeeData = employeesData?.find(x => x.employeeCode === this.employeeCode);
        this.resourceAllocationData = this.setResourceDataInUpsertedCommitment({} as ResourceAllocation, employeeData);

        this.loadEditedCommitmentData();
        this.changeDetector.detectChanges();
      });
    }
  }

  setModalTitle() {
    if (this.isUpdateModal) {
      this.modalTitle = 'Save Resource Commitment';
    } else {
      this.modalTitle = 'Add Resource Commitment';
    }
  }

  setInitialDatesFromProjectData() {
    let projectStartDate;
    let projectEndDate;

    if (this.formModel.type.value.commitmentTypeCode === 'PC' && this.formModel.project.value) {
      projectStartDate = this.formModel.project.value.startDate;
      projectEndDate = this.formModel.project.value.endDate;     
    }

    else if(this.formModel.type.value.commitmentTypeCode === 'C' && this.formModel.project.value) {
      projectStartDate = this.formModel.project.value.oldCaseCode ?
        this.formModel.project.value.caseStartDate : this.formModel.project.value.opportunityStartDate;
      projectEndDate = this.formModel.project.value.oldCaseCode ?
        this.formModel.project.value.caseEndDate : this.formModel.project.value.opportunityEndDate;
    }
      if (projectStartDate && projectEndDate) {
        if(this.formModel.type.value.commitmentTypeCode === 'PC'){
        this.formModel.startDate.value = new Date(projectStartDate);
        this.formModel.endDate.value = new Date(projectEndDate);
        }
        else{
          this.formModel.startDate.value = this.formModel.startDate.value ?? this.setAllocationStartDate(projectStartDate);
          this.formModel.endDate.value =
          this.formModel.endDate.value ?? this.setAllocationEndDate(this.formModel.startDate.value, projectEndDate);
        }
      }

      this.setInitialDatesCompliantWithResourceJoiningDate();
      this.setDateLabels(projectStartDate, projectEndDate);
  }

  setInitialDatesCompliantWithResourceJoiningDate() {
    if (this.formModel.resource.value?.startDate) {
      if (moment(this.formModel.resource.value.startDate).isAfter(this.formModel.startDate.value, 'day')) {
        this.formModel.startDate.value = new Date(this.formModel.resource.value.startDate);
      }
      if (moment(this.formModel.startDate.value).isAfter(this.formModel.endDate.value)) {
        this.formModel.endDate.value = this.formModel.startDate.value;
      }
    }
  }

  loadEditedCommitmentData() {
    
    this.showHideFieldsForCommitments(this.commitmentTypeCode);

    this.formModel.id = this.resourceAllocationData.id;
    this.formModel.type.value =
      this.commitmentTypes.find(x => x.commitmentTypeCode === (this.commitmentTypeCode || ''));

    this.formModel.commitmentTypeReason.value = this.commitmentTypeReasons.find(
        x => x.commitmentTypeReasonCode === (this.resourceAllocationData.commitmentTypeReasonCode || null) // Use null instead of ''
      );
    this.asyncResourceString = this.resourceAllocationData.employeeName || '';

    this.asyncProjectString = this.resourceAllocationData.caseName || this.resourceAllocationData.opportunityName || this.resourceAllocationData.planningCardTitle;

    this.formModel.allocation.value = this.resourceAllocationData.allocation !== undefined ?
      parseInt(this.resourceAllocationData.allocation.toString()) : null;
    this.formModel.startDate.value =
      this.resourceAllocationData.startDate ? new Date(this.resourceAllocationData.startDate) : null;
    this.formModel.endDate.value =
      this.resourceAllocationData.endDate ? new Date(this.resourceAllocationData.endDate) : null;
    this.formModel.investmentCategory.value =
      this.investmentCategories.find(x => x.investmentCode === (this.resourceAllocationData.investmentCode || null));
    this.formModel.role.value =
      this.caseRoleTypes.find(x => x.caseRoleCode === (this.resourceAllocationData.caseRoleCode || null));
    this.formModel.notes.value = this.resourceAllocationData.notes;
  
  //Disable dates based on the month close period if the commitment type is not a planning card.
   if(this.formModel.type.value.commitmentTypeCode != CommitmentTypeEnum.PLANNING_CARD){
      this.disableDatesPerMonthClose();
    }
    
    this.formModel.resource.value = {
      employeeCode: this.resourceAllocationData.employeeCode,
      employeeName: this.resourceAllocationData.employeeName,
      currentLevelGrade: this.resourceAllocationData.currentLevelGrade,
      operatingOfficeCode: this.resourceAllocationData.operatingOfficeCode,
      operatingOfficeAbbreviation: this.resourceAllocationData.operatingOfficeAbbreviation,
      serviceLineCode: this.resourceAllocationData.serviceLineCode,
      serviceLineName: this.resourceAllocationData.serviceLineName,
      startDate: this.resourceAllocationData.joiningDate
    };

    this.formModel.project.value = {
      oldCaseCode: this.resourceAllocationData.oldCaseCode,
      caseName: this.resourceAllocationData.caseName,
      caseTypeCode: this.resourceAllocationData.caseTypeCode,
      clientName: this.resourceAllocationData.clientName,
      pipelineId: this.resourceAllocationData.pipelineId,
      planningCardId: this.resourceAllocationData.planningCardId,
      planningCardName: this.resourceAllocationData.planningCardTitle,
      opportunityName: this.resourceAllocationData.opportunityName,
      caseStartDate: this.resourceAllocationData.caseStartDate,
      caseEndDate: this.resourceAllocationData.caseEndDate,
      opportunityStartDate: this.resourceAllocationData.opportunityStartDate,
      opportunityEndDate: this.resourceAllocationData.opportunityEndDate
    };
    this.setInvestmentCategoryDropdownBasedOnCaseType();
    this.setInitialDatesFromProjectData();

  }

  private disableDatesPerMonthClose() {
    const [isStartDateInMonthClose, startDateValidationMessage] = this._resourceAllocationService.isDateFallsInMonthClose(this.formModel.startDate.value);
    const [isEndDateInMonthClose, endDatevalidationMessage] = this._resourceAllocationService.isDateFallsInMonthClose(this.formModel.endDate.value);

    if (isStartDateInMonthClose ) {
      this.formModel.startDate.readonly = true;
    }

    if (isEndDateInMonthClose ) {
      this.formModel.endDate.readonly = true;
    }
  }

  loadDefaultData() {

    this.formModel.type.value = this.commitmentTypes[0];
    this.formModel.investmentCategory.value = this.investmentCategories[0];
    this.formModel.role.value = this.caseRoleTypes[0];
    this.formModel.notes.value = '';

  }

  showHideFieldsForCommitments(commitmentTypeCode) {
    if (commitmentTypeCode === 'C') {
      this.showCaseAndAllocation = true;
      this.showPlanningCardAndAllocation = false;
      this.showInvestmentAndCaseRole = true;

    } else if(commitmentTypeCode === 'PC'){
      this.showCaseAndAllocation = false;
      this.showPlanningCardAndAllocation = true;
      this.showInvestmentAndCaseRole = false;
    } 

    else if(commitmentTypeCode === 'NB'){
      this.showNonBillableReason = true;
      this.showCaseAndAllocation = false;
      this.showPlanningCardAndAllocation = false;
      this.showInvestmentAndCaseRole = false;
    }
    
    else {
      this.showNonBillableReason = false;
      this.showCaseAndAllocation = false;
      this.showPlanningCardAndAllocation = false;
      this.showInvestmentAndCaseRole = false;
    }

    if (this.isRingFenceCommitment(commitmentTypeCode)) {
      this.showCommitmentAllocation = true;
    } else {
      this.showCommitmentAllocation = false;
    }
  }

  isRingFenceCommitment(commitmentTypeCode) {
    return commitmentTypeCode === CommitmentTypeEnum.PEG
      || commitmentTypeCode === CommitmentTypeEnum.PEG_Surge
      || commitmentTypeCode === CommitmentTypeEnum.AAG
      || commitmentTypeCode === CommitmentTypeEnum.ADAPT
      || commitmentTypeCode === CommitmentTypeEnum.FRWD;
  }

  setDateLabels(projectStartDate, projectEndDate) {

    this.startDateLabel = DateService.convertDateInBainFormat(projectStartDate);
    this.endDateLabel = DateService.convertDateInBainFormat(projectEndDate);

  }

  upsertResourceAllocations(resourceAllocations: ResourceAllocation[], successMessage?, allocationDataBeforeSplitting?: ResourceAllocation[]) {

    this.upsertResourceAllocationsToProject.emit({
      resourceAllocation: resourceAllocations,
      event: 'quickAdd',
      splitSuccessMessage: successMessage,
      allocationDataBeforeSplitting: allocationDataBeforeSplitting
    });

  }

  upsertPlaceholderAllocations(placeholderAllocations: PlaceholderAllocation[]) {

    this.upsertPlaceholderAllocationsToProject.emit({
      placeholderAllocations
    });

    // this.placeholderAssignmentService.upsertPlcaeholderAllocations(placeholderAllocations).subscribe(
    //   (updatedData) => {
    //       this.notifyService.showSuccess(`Placeholder assignment for ${updatedData[0].employeeName} is created/updated`);
    //       this.overlayMessageService.triggerCaseAndOpportunityRefresh();
    //   });
  }

  checkForPrePostAndUpsertResourceAllocation(resourceAllocationData, allocationDataBeforeSplitting?: ResourceAllocation[], successMessage?) {
    let allocationsData: ResourceAllocation[] = [];
    const projectStartDate = DateService.convertDateInBainFormat(
      this.formModel.project.value.oldCaseCode
        ? this.formModel.project.value.caseStartDate
        : this.formModel.project.value.opportunityStartDate);
    const projectEndDate = DateService.convertDateInBainFormat(
      this.formModel.project.value.oldCaseCode
        ? this.formModel.project.value.caseEndDate
        : this.formModel.project.value.opportunityEndDate);

    // If project has start and end date then, split else directly upsert the allocation data
    if (projectStartDate && projectEndDate) {

      if (Array.isArray(resourceAllocationData)) {

        resourceAllocationData.forEach(resourceAllocation => {
          allocationsData = allocationsData.concat(
            this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(resourceAllocation));
        });

      } else {
        allocationsData = allocationsData.concat(this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(
          resourceAllocationData));

      }

    } else {
      allocationsData = allocationsData.concat(resourceAllocationData);

    }

    this.upsertResourceAllocations(allocationsData, successMessage, allocationDataBeforeSplitting);

  }

  checkForBackfillAndUpsertResourceAllocation(projectData, resourceAllocation: ResourceAllocation, allocationDataBeforeSplitting: ResourceAllocation[]) {

    if (this._resourceAllocationService.isBackFillRequiredOnProject(projectData.allocatedResources, projectData)) {
      const showMoreThanYearWarning = ValidationService.checkIfAllocationIsOfOneYear(new Date(projectData.startDate), new Date(projectData.endDate));
      this.openBackFillPopUp.emit({
        project: projectData,
        resourceAllocation: resourceAllocation,
        showMoreThanYearWarning: showMoreThanYearWarning,
        isPlaceholderAllocation: false,
        allocationDataBeforeSplitting: allocationDataBeforeSplitting
      });

    } else {
      this.checkForPrePostAndUpsertResourceAllocation(resourceAllocation, allocationDataBeforeSplitting);
    }
  }

  addPlaceholderAllocationForResources() {
    const placeholderAllocationData: PlaceholderAllocation[] = [];
    if (this.selectedResources.length > 0) {
      this.selectedResources.forEach((selectedResource) => {
        const placeholderAllocation: PlaceholderAllocation = {
          id: null,
          planningCardId: this.formModel.project.value.id ||this.formModel.project.value.planningCardId ,
          oldCaseCode: null,
          caseName: null,
          clientName: null,
          pipelineId: null,
          caseTypeCode: null,
          opportunityName: null,
          employeeCode: selectedResource.employeeCode,
          employeeName: selectedResource.fullName,
          internetAddress: selectedResource.internetAddress,
          operatingOfficeCode: selectedResource.schedulingOffice.officeCode,
          operatingOfficeAbbreviation: selectedResource.schedulingOffice.officeAbbreviation,
          currentLevelGrade: selectedResource.levelGrade,
          serviceLineCode: selectedResource.serviceLine.serviceLineCode,
          serviceLineName: selectedResource.serviceLine.serviceLineName,
          allocation: parseInt(this.formModel.allocation.value, this.radix),
          startDate: DateService.getFormattedDate(this.formModel.startDate.value),
          endDate: DateService.getFormattedDate(this.formModel.endDate.value),
          investmentCode: null,
          investmentName: null,
          caseRoleCode: null,
          isPlaceholderAllocation: false,
          lastUpdatedBy: null
      };
      placeholderAllocationData.push(placeholderAllocation);
      });
    }
      else {
        // If new resource added then take that, else use the existing resource data
        const selectedResource = this.formModel.resource.value;
        const placeholderAllocation: PlaceholderAllocation = {
          id: null,
          planningCardId: this.formModel.project.value.id,
          oldCaseCode: null,
          caseName: null,
          clientName: null,
          pipelineId: null,
          caseTypeCode: null,
          opportunityName: null,
          employeeCode: selectedResource.employeeCode,
          employeeName: selectedResource.fullName,
          internetAddress: selectedResource.internetAddress,
          operatingOfficeCode: selectedResource.operatingOfficeCode,
          operatingOfficeAbbreviation: selectedResource.operatingOfficeAbbreviation,
          currentLevelGrade: selectedResource.currentLevelGrade,
          serviceLineCode: selectedResource.serviceLineCode,
          serviceLineName: selectedResource.serviceLineName,
          allocation: parseInt(this.formModel.allocation.value, this.radix),
          startDate: DateService.getFormattedDate(this.formModel.startDate.value),
          endDate: DateService.getFormattedDate(this.formModel.endDate.value),
          investmentCode: null,
          investmentName: null,
          caseRoleCode: null,
          isPlaceholderAllocation: false,
          lastUpdatedBy: null
      };       
        placeholderAllocationData.push(placeholderAllocation);
      }
      this.upsertPlaceholderAllocations(placeholderAllocationData);
      this.closeForm();
  }

  addAllocationsForResources() {
    const allocationsData: ResourceAllocation[] = [];
    // TODO: try to minimise if-else
    if (this.selectedResources.length > 0) {

      this.selectedResources.forEach((selectedResource) => {
        const resourceAllocation: ResourceAllocation = {
          caseName: this.formModel.project.value.caseName,
          clientName: this.formModel.project.value.clientName,
          oldCaseCode: this.formModel.project.value.oldCaseCode,
          caseTypeCode: this.formModel.project.value.caseTypeCode,
          employeeCode: selectedResource.employeeCode,
          employeeName: selectedResource.fullName,
          currentLevelGrade: selectedResource.levelGrade,
          serviceLineCode: selectedResource.serviceLine.serviceLineCode,
          serviceLineName: selectedResource.serviceLine.serviceLineName,
          operatingOfficeCode: selectedResource.schedulingOffice.officeCode,
          operatingOfficeAbbreviation: selectedResource.schedulingOffice.officeAbbreviation,
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
          previousInvestmentCode: null,
          caseStartDate: this.formModel.project.value.caseStartDate,
          caseEndDate: this.formModel.project.value.caseEndDate,
          opportunityStartDate: this.formModel.project.value.opportunityStartDate,
          opportunityEndDate: this.formModel.project.value.opportunityEndDate,
          lastUpdatedBy: null,
          notes: this.formModel.notes.value
        };

        allocationsData.push(resourceAllocation);
      });

    } else {
      // If new resource added then take that, else use the existing resource data
      const selectedResource = this.formModel.resource.value;

      const resourceAllocation: ResourceAllocation = {
        caseName: this.formModel.project.value.caseName,
        clientName: this.formModel.project.value.clientName,
        oldCaseCode: this.formModel.project.value.oldCaseCode,
        caseTypeCode: this.formModel.project.value.caseTypeCode,
        employeeCode: selectedResource.employeeCode,
        employeeName: selectedResource.employeeName,
        currentLevelGrade: selectedResource.currentLevelGrade,
        serviceLineCode: selectedResource.serviceLineCode,
        serviceLineName: selectedResource.serviceLineName,
        operatingOfficeCode: selectedResource.operatingOfficeCode,
        operatingOfficeAbbreviation: selectedResource.operatingOfficeAbbreviation,
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
        previousInvestmentCode: null,
        caseStartDate: this.formModel.project.value.caseStartDate,
        caseEndDate: this.formModel.project.value.caseEndDate,
        opportunityStartDate: this.formModel.project.value.opportunityStartDate,
        opportunityEndDate: this.formModel.project.value.opportunityEndDate,
        lastUpdatedBy: null,
        notes: this.formModel.notes.value
      };

      allocationsData.push(resourceAllocation);

    }

    //check if the new allocation falls in month close period
    const [isValidAllocation, validationMessage] = this._resourceAllocationService.validateMonthCloseForInsertAndDelete(allocationsData);
    if (!isValidAllocation) {
      this.errorList.push(validationMessage);
      return;
    }

    if (allocationsData.length > 1) {
      this.checkForPrePostAndUpsertResourceAllocation(allocationsData, allocationsData);
    } else {

      if (this.formModel.project.value.oldCaseCode) {
        this.sharedService.getCaseDetailsAndAllocations(this.formModel.project.value.oldCaseCode).subscribe(caseDetailsAndAllocations => {
          this.checkForBackfillAndUpsertResourceAllocation(caseDetailsAndAllocations, allocationsData[0], allocationsData);
          if (!(caseDetailsAndAllocations.allocatedResources?.length > 0) && caseDetailsAndAllocations.startDate) {
            this.sharedService.getOverlappingTeamsInPreviousProjects(allocationsData[0].employeeCode, caseDetailsAndAllocations.startDate).subscribe(allocations => {
              if (allocations.length > 0) {
                this.openOverlappedTeamsForm(caseDetailsAndAllocations, allocations, allocationsData[0]);
              }
            });
          }
        });
      } else {
        this.sharedService.getOpportunityDetailsAndAllocations(this.formModel.project.value.pipelineId)
          .subscribe(opportunityDetailsAndAllocations => {
            this.checkForBackfillAndUpsertResourceAllocation(opportunityDetailsAndAllocations, allocationsData[0], allocationsData);
            if (!(opportunityDetailsAndAllocations.allocatedResources?.length > 0) && opportunityDetailsAndAllocations.startDate) {
              this.sharedService.getOverlappingTeamsInPreviousProjects(allocationsData[0].employeeCode, opportunityDetailsAndAllocations.startDate).subscribe(allocations => {
                if (allocations.length > 0) {
                  this.openOverlappedTeamsForm(opportunityDetailsAndAllocations, allocations, allocationsData[0]);
                }
              });
            }
          });
      }

    }

    this.closeForm();
  }

  openOverlappedTeamsForm(projectData: Project, overlappedTeams: ResourceAllocation[], allocation: ResourceAllocation) {
    this.openOverlappedTeamsPopup.emit({
      projectData: projectData,
      overlappedTeams: overlappedTeams,
      allocation: allocation
    });
  }

  // --------------------------Event handlers---------------------------------//

  onCommitmentTypeChange() {
    this.showHideFieldsForCommitments(this.formModel.type.value.commitmentTypeCode);

    if (this.formModel.type.value.commitmentTypeCode !== 'C') {
      this.asyncProjectString = null;
      this.startDateLabel = null;
      this.endDateLabel = null;
    }

  }

  addCommitmentForResources() {
    const commitmentForEmployees: Commitment[] = [];

    // TODO: improve this process
    if (this.selectedResources && this.selectedResources.length > 0) {
      this.selectedResources.forEach((selectedResource) => {

        const commitment: Commitment = {
          id: '00000000-0000-0000-0000-000000000000', // adding empty Id is must as null ID fails in conversion on backend
          commitmentType: this.formModel.type.value,
          commitmentTypeReasonCode: this.formModel.commitmentTypeReason?.value?.commitmentTypeReasonCode,
          employeeCode: selectedResource.employeeCode,
          startDate: DateService.getFormattedDate(this.formModel.startDate.value),
          endDate: DateService.getFormattedDate(this.formModel.endDate.value),
          notes: this.formModel.notes.value,
          allocation: this.isRingFenceCommitment(this.formModel.type.value.commitmentTypeCode) ? parseInt(this.formModel.allocation.value, this.radix) : null, //we don't show allocation percent for other commitments hence setting it null. Remove this check if we allow allocation entry
          lastUpdatedBy: null
        };

        commitmentForEmployees.push(commitment);

      });
    }
     else {

      const commitment: Commitment = {
        id: '00000000-0000-0000-0000-000000000000', // adding empty Id is must as null ID fails in conversion on backend
        commitmentType: this.formModel.type.value,
        commitmentTypeReasonCode: this.formModel.commitmentTypeReason?.value?.commitmentTypeReasonCode,
        employeeCode: this.formModel.resource.value.employeeCode,
        startDate: DateService.getFormattedDate(this.formModel.startDate.value),
        endDate: DateService.getFormattedDate(this.formModel.endDate.value),
        notes: this.formModel.notes.value,
        allocation: parseInt(this.formModel.allocation.value, this.radix),
        lastUpdatedBy: null
      };

      commitmentForEmployees.push(commitment);

    }

    this.insertResourcesCommitments.emit(commitmentForEmployees);
    this.closeForm();
  }

  addCommitment() {

    if (!this.isCommitmentDataValid()) {
      return false;
    }

    if (this.formModel.type.value.commitmentTypeCode === 'C') {
      this.addAllocationsForResources();
    } else if(this.formModel.type.value.commitmentTypeCode === 'PC') {
      this.addPlaceholderAllocationForResources();
    } else {
      this.addCommitmentForResources();
    }

  }

  updateCommitment() {

    if (!this.isCommitmentDataValid()) {
      return false;
    }

    if (this.isFormDirty) {
      if (this.formModel.type.value.commitmentTypeCode !== 'C' && this.formModel.type.value.commitmentTypeCode !== 'PC' ) {

        const updatedUserCommitment: Commitment = {
          id: this.formModel.id,
          employeeCode: this.formModel.resource.value.employeeCode,
          startDate: DateService.getFormattedDate(this.formModel.startDate.value),
          endDate: DateService.getFormattedDate(this.formModel.endDate.value),
          notes: this.formModel.notes.value,
          commitmentType: {
            commitmentTypeCode: this.formModel.type.value.commitmentTypeCode,
            commitmentTypeName: this.formModel.type.value.commitmentTypeName,
            precedence: this.formModel.type.value.precedence
          },
          commitmentTypeReasonCode: this.formModel.commitmentTypeReason.value?.commitmentTypeReasonCode || null, // Ensure NULL instead of empty string
          allocation: this.isRingFenceCommitment(this.formModel.type.value.commitmentTypeCode) ? parseInt(this.formModel.allocation.value, this.radix) : null, //we don't show allocation percent for other commitments hence setting it null. Remove this check if we allow allocation entry
          lastUpdatedBy: null
        };

        this.updateResourceCommitment.emit({ resourceAllocation: updatedUserCommitment, event: 'ganttResource' });

      }
      else if(this.formModel.type.value.commitmentTypeCode == 'PC'){
        const placeholderAllocationData: PlaceholderAllocation[] = [];
        const selectedResource = this.formModel.resource.value;
        const placeholderAllocation: PlaceholderAllocation = {
          id: this.formModel.id,
          planningCardId: this.formModel.project.value.id ||this.formModel.project.value.planningCardId ,
          oldCaseCode: null,
          caseName: null,
          clientName: null,
          pipelineId: null,
          caseTypeCode: null,
          opportunityName: null,
          employeeCode: selectedResource.employeeCode,
          employeeName: selectedResource.fullName,
          internetAddress: selectedResource.internetAddress,
          operatingOfficeCode: selectedResource.operatingOfficeCode,
          operatingOfficeAbbreviation: selectedResource.operatingOfficeAbbreviation,
          currentLevelGrade: selectedResource.currentLevelGrade,
          serviceLineCode: selectedResource.serviceLineCode,
          serviceLineName: selectedResource.serviceLineName,
          allocation: parseInt(this.formModel.allocation.value, this.radix),
          startDate: DateService.getFormattedDate(this.formModel.startDate.value),
          endDate: DateService.getFormattedDate(this.formModel.endDate.value),
          investmentCode: null,
          investmentName: null,
          caseRoleCode: null,
          isPlaceholderAllocation: false,
          notes: this.formModel.notes.value,
          lastUpdatedBy: null
      };       
        placeholderAllocationData.push(placeholderAllocation);
        this.upsertPlaceholderAllocations(placeholderAllocationData);
      }
      
      else {

        const updatedUserAllocationOnCase: ResourceAllocation = {
          id: this.formModel.id,
          caseName: this.formModel.project.value.caseName,
          clientName: this.formModel.project.value.clientName,
          oldCaseCode: this.formModel.project.value.oldCaseCode,
          employeeCode: this.formModel.resource.value.employeeCode,
          employeeName: this.formModel.resource.value.employeeName,
          currentLevelGrade: this.formModel.resource.value.currentLevelGrade,
          serviceLineCode: this.formModel.resource.value.serviceLineCode,
          serviceLineName: this.formModel.resource.value.serviceLineName,
          operatingOfficeCode: this.formModel.resource.value.operatingOfficeCode,
          operatingOfficeAbbreviation: this.formModel.resource.value.operatingOfficeAbbreviation,
          pipelineId: this.formModel.project.value.pipelineId,
          opportunityName: this.formModel.project.value.opportunityName,
          investmentCode: this.formModel.investmentCategory.value.investmentCode,
          investmentName: this.formModel.investmentCategory.value.investmentName,
          caseRoleCode: this.formModel.role.value.caseRoleCode,
          allocation: parseInt(this.formModel.allocation.value, this.radix),
          startDate: DateService.getFormattedDate(this.formModel.startDate.value),
          endDate: DateService.getFormattedDate(this.formModel.endDate.value),
          previousStartDate: this.resourceAllocationData.startDate,
          previousEndDate: this.resourceAllocationData.endDate,
          previousAllocation: this.resourceAllocationData.allocation,
          previousInvestmentCode: this.resourceAllocationData.investmentCode,
          caseStartDate: this.formModel.project.value.caseStartDate,
          caseEndDate: this.formModel.project.value.caseEndDate,
          opportunityStartDate: this.formModel.project.value.opportunityStartDate,
          opportunityEndDate: this.formModel.project.value.opportunityEndDate,
          lastUpdatedBy: null,
          notes: this.formModel.notes.value
        };

        let allocationsToUpdate: ResourceAllocation[] = [];
        let successMessage = null;
        let existingAllocation = this.resourceAllocationData;
        let isValidAllocation = true;
        const currentAllocationDecidingParamsForSplit = {
          allocation: this.formModel.allocation.value,
          investmentCode: this.formModel.investmentCategory.value.investmentCode,
          caseRoleCode: this.formModel.role.value.caseRoleCode,
          startDate: this.formModel.startDate.value,
          endDate: this.formModel.endDate.value
        };

        let [canSplitForMonthClose, validationMessage] = this._resourceAllocationService.canSplitForMonthClose(existingAllocation, currentAllocationDecidingParamsForSplit);


        if (canSplitForMonthClose) {
          [allocationsToUpdate, successMessage] = this._resourceAllocationService.splitAlloctionForMonthClose(existingAllocation, updatedUserAllocationOnCase)
        } else {
          [isValidAllocation, validationMessage] = this._resourceAllocationService.validateMonthCloseForUpdates(updatedUserAllocationOnCase, this.resourceAllocationData);
          allocationsToUpdate.push(updatedUserAllocationOnCase);
        }

        if (validationMessage) {
          this.errorList.push(validationMessage);
          return;
        }
        this.checkForPrePostAndUpsertResourceAllocation(allocationsToUpdate, null, successMessage);
      }
    }

    this.closeForm();
  }

  deleteCommitment() {
    if (this.formModel.type.value.commitmentTypeCode === 'C') {
      //check if the deleted allocation falls in month close period
      const [isValidAllocation, validationMessage] = this._resourceAllocationService.validateMonthCloseForInsertAndDelete(this.resourceAllocationData);
      if (!isValidAllocation) {
        this.errorList.push(validationMessage);
        return;
      }

      this.deleteResourceAllocationFromCase.emit({
          allocationId: this.formModel.id, 
          resourceAllocation: [].concat(this.resourceAllocationData)
        });
    }
    else if(this.formModel.type.value.commitmentTypeCode === 'PC'){
      this.deletePlaceholderAllocationByIds.emit({
        placeholderAllocation: [].concat(this.resourceAllocationData),
        placeholderIds: this.resourceAllocationData.id
      });
    }

     else {
      this.deleteResourceCommitment.emit({
        commitmentId: this.formModel.id,
        resourceAllocation: [].concat(this.resourceAllocationData)
      });
    }

    this.closeForm();

  }

  selectedResourcesChange($event) {
    this.selectedResources = $event;
    this.isResourceSearchOpen = false;
    if (this.selectedResources.length === 1) {
      // this.formModel.allocation.value = parseInt((this.selectedResources[0].fte * 100).toString());
      this.formModel.allocation.value = !parseInt(this.formModel.allocation.value)
        ? parseInt((this.selectedResources[0].fte * 100).toString())
        : parseInt(this.formModel.allocation.value);
    } else if (this.selectedResources.length > 1) {
      this.formModel.allocation.value = !parseInt(this.formModel.allocation.value)
        ? 100
        : parseInt(this.formModel.allocation.value);
    }
  }

  typeaheadOnSelect(data, type) {

    if (type === 'project') {
      this.formModel.project.value = {
        oldCaseCode: data.item.oldCaseCode,
        caseName: data.item.caseName,
        clientName: data.item.clientName,
        caseTypeCode: data.item.caseTypeCode,
        pipelineId: data.item.pipelineId,
        opportunityName: data.item.opportunityName,
        caseStartDate: data.item.oldCaseCode ? data.item.startDate : null,
        caseEndDate: data.item.oldCaseCode ? data.item.endDate : null,
        opportunityStartDate: !data.item.oldCaseCode ? data.item.startDate : null,
        opportunityEndDate: !data.item.oldCaseCode ? data.item.endDate : null
      };

      this.setInitialDatesFromProjectData();
      this.setInvestmentCategoryDropdownBasedOnCaseType();

    }
  }
  setInvestmentCategoryDropdownBasedOnCaseType(){
    const isAddingNewResource = !this.resourceAllocationData?.employeeCode;
    const hasNoInvestmentCode = !this.formModel.investmentCategory.value.investmentCode;
    const caseTypeCode = this.formModel.project.value.caseTypeCode;
    const isClientDevelopment = caseTypeCode && caseTypeCode.toString() === CaseType.ClientDevelopment;

    if (hasNoInvestmentCode && isClientDevelopment) {
        this.formModel.investmentCategory.value = this.investmentCategories.find(
            x => x.investmentCode === ConstantsMaster.InvestmentCategory.AdHoc.investmentCode
        );
    }

    this.investmentCategoriesToShow = this.investmentCategories.filter(
        x => x.applicableCaseTypeCodes == null || x.applicableCaseTypeCodes.includes(caseTypeCode)
    );
  }

  onPlanningCardSearchItemSelectHandler(selectedPlanningCard) {
    
    this.formModel.project.value = {
      id: selectedPlanningCard.id,
      name: selectedPlanningCard.name,
      startDate: selectedPlanningCard.startDate,
      endDate: selectedPlanningCard.endDate,
      isShared: selectedPlanningCard.isShared,
      office: selectedPlanningCard.office,
      sharedOfficeCodes: selectedPlanningCard.sharedOfficeCodes,
      sharedOfficeNames: selectedPlanningCard.sharedOfficeNames,
      sharedStaffingTagNames: selectedPlanningCard.sharedStaffingTagNames,
      sharedStaffingTags: selectedPlanningCard.sharedStaffingTags
    };
    this.setInitialDatesFromProjectData();
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


  validateField(fieldName) {
    switch (fieldName) {
      case 'type': {
        if (this.formModel.type.value.commitmentTypeCode === '') {
          this.formModel.type.isInvalid = true;
          this.addToErrorList('required');
        } else {
          this.formModel.type.isInvalid = false;
        }
        break;
      }
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
        } else if (!this.formModel.startDate.isInvalid
          && Date.parse(this.formModel.endDate.value.toDateString()) < Date.parse(this.formModel.startDate.value.toDateString())) {
          this.formModel.endDate.isInvalid = true;
          this.addToErrorList('startDateGreaterThanEndDate');
        } else {
          this.formModel.endDate.isInvalid = false;
        }
        break;
      }
      case 'resource': {
        if (!this.selectedResources) {
          this.formModel.resource.isInvalid = true;
          this.addToErrorList('required');
        } else if (this.asyncResourceString === '' && this.selectedResources.length === 0) {
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
      case 'case': {
        const selectedProjectName = (this.formModel.project.value?.caseName || this.formModel.project.value?.opportunityName);
        if (!this.asyncProjectString) {
          this.formModel.project.isInvalid = true;
          this.addToErrorList('required');
        } else if (!this.formModel.project.value || (selectedProjectName !== this.asyncProjectString)) {
          this.formModel.project.isInvalid = true;
          this.addToErrorList('typeaheadInvalid');
        } else {
          this.formModel.project.isInvalid = false;
        }
        break;
      }
      case 'planningCard': {
        const selectedProjectName = (this.formModel.project.value?.planningCardName) ||(this.formModel.project.value?.name) ;
        const inputStringInTextBox = this.planningCardTypeaheadComponentInstance.asyncProjectString; //Since the typeahead is in another component, asyncPorjectString does not update when the user empties the input box or changes value in it.
        if (!inputStringInTextBox && !this.formModel.project.value.planningCardId) {
          this.formModel.project.isInvalid = true;
          this.addToErrorList('required');
        } else if (!this.formModel.project.value || (selectedProjectName !== inputStringInTextBox)) {
            if(selectedProjectName && (selectedProjectName !== inputStringInTextBox))
            {
              this.formModel.project.isInvalid = true;
              this.addToErrorList('typeaheadInvalid');
            }
        } else {
          this.formModel.project.isInvalid = false;
        }
        break;
      }
      case 'allocation': {
        if (!ValidationService.isAllocationValid(this.formModel.allocation.value,
          !this.isRingFenceCommitment(this.formModel.type.value.commitmentTypeCode))) {
          this.formModel.allocation.isInvalid = true;
          this.addToErrorList('numberInvalid');
        } else {
          this.formModel.allocation.isInvalid = false;
        }
        break;
      }
    }
  }

  isCommitmentDataValid() {
    this.errorList = [];

    this.validateField('type');
    this.validateField('resource');
    this.validateField('startDate');
    this.validateField('endDate');
    if (this.formModel.type.value.commitmentTypeCode === 'C') {
      this.validateField('case');
    }
    if (this.formModel.type.value.commitmentTypeCode === 'PC') {
      this.validateField('planningCard');
    }
    if (this.formModel.type.value.commitmentTypeCode === 'C' ||  this.formModel.type.value.commitmentTypeCode === 'PC' ||
      this.isRingFenceCommitment(this.formModel.type.value.commitmentTypeCode)) {
      this.validateField('allocation');
    }

    if (this.formModel.type.isInvalid || this.formModel.resource.isInvalid
      || this.formModel.project.isInvalid || this.formModel.allocation.isInvalid
      || this.formModel.startDate.isInvalid || this.formModel.endDate.isInvalid) {
      return false;
    } else {
      return true;
    }
  }

  setAllocationStartDate(projectStartDate) {
    const today = moment();
    const startDate = moment(projectStartDate);
    const thresholdStartDate = moment(projectStartDate).add(8, 'd');

    if (today.isBetween(startDate, thresholdStartDate)) {
      return new Date(projectStartDate);
    }
    return new Date();
  }

  setAllocationEndDate(allocationStartDate, projectEndDate) {
    const today = moment();

    if (today.isSameOrAfter(projectEndDate)) {
      return new Date();
    } else {
      const maxEndDate = DateService.getMaxEndDateForAllocation(allocationStartDate, new Date(projectEndDate));
      return maxEndDate;
    }
  }

  typeaheadNoResultsHandler(event: boolean): void {
    this.noCaseOppFound = event;
  }

  private addErrorsForResourcesHavingJoiningdateGreaterThanStartDate() {
    this.resourcesHavingJoiningDateGreaterThanStartDate.forEach(res => {
      this.errorList.push(`${res.fullName ?? res.employeeName} has Joining Date '${DateService.convertDateInBainFormat(res.startDate)}' greater than start Date`);
    });
  }



  private isEmployeeJoiningDateGreaterThanStartDate() {
    this.resourcesHavingJoiningDateGreaterThanStartDate = [];
    if (this.selectedResources.length > 0) {
      this.selectedResources.forEach(x => {
        if (moment(x.startDate).isAfter(this.formModel.startDate.value, 'day')) {
          this.resourcesHavingJoiningDateGreaterThanStartDate.push(x);
        }
      })
    } else if (this.formModel.resource.value) {
      if (moment(this.formModel.resource.value.startDate).isAfter(this.formModel.startDate.value, 'day')) {
        this.resourcesHavingJoiningDateGreaterThanStartDate.push(this.formModel.resource.value);
      }
    }
    return this.resourcesHavingJoiningDateGreaterThanStartDate.length > 0;
  }

  private setResourceDataInUpsertedCommitment(resourceAllocationObj: ResourceAllocation, employee: Resource): ResourceAllocation {
    resourceAllocationObj.employeeCode =
      resourceAllocationObj.employeeCode || employee.employeeCode;
    resourceAllocationObj.employeeName =
      resourceAllocationObj.employeeName || employee.fullName;
    resourceAllocationObj.currentLevelGrade =
      resourceAllocationObj.currentLevelGrade || employee.levelGrade;
    resourceAllocationObj.operatingOfficeCode =
      resourceAllocationObj.operatingOfficeCode || employee.schedulingOffice.officeCode;
    resourceAllocationObj.operatingOfficeAbbreviation =
      resourceAllocationObj.operatingOfficeAbbreviation || employee.schedulingOffice.officeAbbreviation;
    resourceAllocationObj.serviceLineCode =
      resourceAllocationObj.serviceLineCode || employee.serviceLine.serviceLineCode;
    resourceAllocationObj.serviceLineName =
      resourceAllocationObj.serviceLineName || employee.serviceLine.serviceLineName;
    // because !!0 will return false and we're allowing allocation percentage to be 0
    resourceAllocationObj.allocation =
      resourceAllocationObj.allocation !== undefined ?
        resourceAllocationObj.allocation : (employee.fte * 100);
    resourceAllocationObj.joiningDate = employee.startDate;

    return resourceAllocationObj;
  }

}
