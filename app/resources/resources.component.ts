// ----------------------- Angular Package References ----------------------------------//
import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { Store, select } from '@ngrx/store';
import { Subject, Subscription } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';

// ----------------------- Component References ----------------------------------//
import { BackfillFormComponent } from '../shared/backfill-form/backfill-form.component';
import { QuickAddFormComponent } from '../shared/quick-add-form/quick-add-form.component';
import { ResourceOverlayComponent } from '../overlay/resource-overlay/resource-overlay.component';
import { ProjectOverlayComponent } from '../overlay/project-overlay/project-overlay.component';
import { AgGridNotesComponent } from '../overlay/ag-grid-notes/ag-grid-notes.component';
import { CaseRollFormComponent } from '../shared/case-roll-form/case-roll-form.component';
import { AgGridSplitAllocationPopUpComponent } from '../overlay/ag-grid-split-allocation-pop-up/ag-grid-split-allocation-pop-up.component';
import { GanttComponent } from './gantt/gantt.component';

// -----------------------Service References ----------------------------------//
import { CoreService } from '../core/core.service';
import { LocalStorageService } from '../shared/local-storage.service';
import { DateService } from '../shared/dateService';
import { SkuCaseTermService } from '../overlay/behavioralSubjectService/skuCaseTerm.service';
import { UserPreferenceService } from '../overlay/behavioralSubjectService/userPreference.service';
import { CaseRollService } from '../overlay/behavioralSubjectService/caseRoll.service';
import { OpportunityService } from '../overlay/behavioralSubjectService/opportunity.service';
import { ResourceService } from '../shared/helperServices/resource.service';
import { OverlayDialogService } from '../overlay/dialogHelperService/overlayDialog.service';
import { staCommitmentDialogService } from '../overlay/dialogHelperService/staCommitmentCaseOppDialog.service';

// --------------------------Redux Component -----------------------------------------//
import * as resourcesActions from './State/resources.actions';
import * as fromResources from './State/resources.reducer';

// --------------------------Interfaces -----------------------------------------//
import { UserPreferences } from '../shared/interfaces/userPreferences.interface';
import { Office } from '../shared/interfaces/office.interface';
import { SupplyFilterCriteria } from '../shared/interfaces/supplyFilterCriteria.interface';
import { ResourceStaffing } from '../shared/interfaces/resourceStaffing.interface';
import { InvestmentCategory } from '../shared/interfaces/investmentCateogry.interface';
import { CaseRoleType } from '../shared/interfaces/caseRoleType.interface';
import { CommitmentType } from '../shared/interfaces/commitmentType.interface';
import { OfficeHierarchy } from '../shared/interfaces/officeHierarchy.interface';
import { ServiceLineHierarchy } from '../shared/interfaces/serviceLineHierarchy';
import { LevelGrade } from '../shared/interfaces/levelGrade.interface';

// ------------------------Constants/Enums---------------------------------------
import { ConstantsMaster } from '../shared/constants/constantsMaster';
import { ServiceLine as ServiceLineCodeEnum, ResourcesSupplyFilterGroupEnum, EmployeeCaseGroupingEnum, CommitmentType as CommitmentTypeEnum } from '../shared/constants/enumMaster';

// --------------------------utilities -----------------------------------------//
import { MatDialogRef, MatDialog } from '@angular/material/dialog';
import { LoaderComponent } from '../shared/loader/loader.component';
import { GoogleAnalytics } from '../shared/google-analytics/googleAnalytics';
import { ActivatedRoute } from '@angular/router';
import { Certificate } from '../shared/interfaces/certificate.interface';
import { Language } from '../shared/interfaces/language';
import { PracticeArea } from '../shared/interfaces/practiceArea.interface';
import { ResourceFilter } from '../shared/interfaces/resource-filter.interface';
import { ServiceLine } from '../shared/interfaces/serviceLine.interface';
import { PlaceholderAssignmentService } from '../overlay/behavioralSubjectService/placeholderAssignment.service';
import { StaffableAsType } from '../shared/interfaces/staffableAsType.interface';
import { PlaceholderFormComponent } from '../shared/placeholder-form/placeholder-form.component';
import { ResourceAssignmentService } from '../overlay/behavioralSubjectService/resourceAssignment.service';
import { BackfillDialogService } from '../overlay/dialogHelperService/backFillDialog.service';
import { OverlappedTeamsFormComponent } from '../shared/overlapped-teams-form/overlapped-teams-form.component';
import { AffiliationRole } from '../shared/interfaces/affiliationRole.interface';
import { PositionHierarchy } from '../shared/interfaces/positionHierarchy.interface';
import { InfoIconModalComponent } from './info-icon-modal/info-icon-modal.component';
import { SupplyGroupFilterCriteria } from '../shared/interfaces/supplyGroupFilterCriteria.interface';
import { UserPreferenceSupplyGroupViewModel } from '../shared/interfaces/userPreferenceSupplyGroupViewModel';
import { CombinedUserPreferences } from '../shared/interfaces/combinedUserPreferences.interface';
import { ResourceOrCasePlanningViewNote } from '../shared/interfaces/resource-or-case-planning-view-note.interface';
import { DropdownFilterOption } from '../shared/interfaces/dropdown-filter-option';
import { CommonService } from '../shared/commonService';
import { ProjectBasic } from '../shared/interfaces/project.interface';
import { ResourceCaseGroup } from '../shared/interfaces/resourceCaseGroup';
import { SortRow } from '../shared/interfaces/sort-row.interface';
import { FilterRow } from '../shared/interfaces/filter-row.interface';
import { Resource } from '../shared/interfaces/resource.interface';
import { FilterObject } from '../shared/interfaces/filter-object.interface';
import { CustomGroupModalComponent } from './resources-filter/custom-group-modal/custom-group-modal.component';
import { UserPreferenceSupplyGroup } from '../shared/interfaces/userPreferenceSupplyGroup';
import { UserPreferencesMessageService } from '../core/user-preferences-message.service';
import { SharedNotesService } from '../core/services/shared-notes-info.service';
import { ResourceViewCD } from '../shared/interfaces/resource-view-cd.interface';
import { ResourceViewSelectedTab } from '../shared/interfaces/resource-view-selected-tab.interface';
import { ResourceViewCommercialModel } from '../shared/interfaces/resource-view-commercial-model.interface';

@Component({
  selector: 'app-resources',
  templateUrl: './resources.component.html',
  styleUrls: ['./resources.component.scss']
})
export class ResourcesComponent implements OnInit, OnDestroy {
  // -----------------------Local Variables--------------------------------------------//
  destroy$: Subject<void> = new Subject<void>();
  webWorker: Worker;
  // public highlightResource = false;
  public resourcesCountOnCaseOpp: any;
  public resources: ResourceStaffing[] | ResourceCaseGroup[];
  public allresourcesAfterSorting: ResourceStaffing[];
  public allCaseGroupsAfterSorting: ResourceCaseGroup[];
  public savedResourceFilters: ResourceFilter[];
  userPreferences: UserPreferences;
  supplyGroupPreferences: UserPreferenceSupplyGroupViewModel[] = [];
  supplyFilterCriteriaObj: SupplyFilterCriteria = {} as SupplyFilterCriteria;
  supplyGroupFilterCriteriaObj: SupplyGroupFilterCriteria = {} as SupplyGroupFilterCriteria;
  pageNumber = 1;
  resourcesPerPage;
  dateRange: [Date, Date];
  storeSub: Subscription = new Subscription();
  showProgressBar = true;
  investmentCategories: InvestmentCategory[];
  caseRoleTypes: CaseRoleType[];
  commitmentTypes: CommitmentType[];
  bsModalRef: BsModalRef;
  officeHierarchy: OfficeHierarchy;
  officeFlatList: Office[];
  staffingTagsHierarchy: ServiceLineHierarchy[];
  staffingTags: ServiceLine[];
  levelGrades: LevelGrade[];
  certificates: Certificate[];
  languages: Language[];
  practiceAreas: PracticeArea[];
  affiliationRoles: AffiliationRole[];
  staffableAsTypes: StaffableAsType[];
  positionsHierarchy: PositionHierarchy[];
  sortsBy = [];
  dialogRef: MatDialogRef<ResourceOverlayComponent, any>;
  projectDialogRef: MatDialogRef<ProjectOverlayComponent, any>;
  agGridNotesDialogRef: MatDialogRef<AgGridNotesComponent, any>;
  serviceLineCodeEnum: typeof ServiceLineCodeEnum = ServiceLineCodeEnum;
  officeList: Office[];
  currRoute = '';
  public selectedCommitmentTypes: string[];
  private isSearchStringExist = false;
  public isPdfExport = false;
  public thresholdRangeValue = '';
  public allSupplyDropdownOptions: DropdownFilterOption[] = [];
  public sortingDirection = '';
  public selectedEmployeeCaseGroupingOption = '';
  public selectedWeeklyMonthlyGroupingOption = '';
  public isSelectedPracticeView = false;
  resourceViewSelectedTabs: ResourceViewSelectedTab[] = [];
  resourcesRecentCDList: ResourceViewCD[] = [];
  resourcesCommercialModelList: ResourceViewCommercialModel[] = [];
  sharedNotes = [];
  caseIntakeAlerts = [];

  //TODO: to be removed with app-side-bar-filters
  //public showFilters = false;
  // public filterConfig = {
  //   filtersToShow: [
  //     ConstantsMaster.resourcesFilter.Offices,
  //     ConstantsMaster.resourcesFilter.StaffingTags,
  //     ConstantsMaster.resourcesFilter.LevelGrades,
  //     ConstantsMaster.resourcesFilter.PositionCodes,
  //     ConstantsMaster.resourcesFilter.CommitmentTypes,
  //     ConstantsMaster.resourcesFilter.SortBy,
  //     ConstantsMaster.resourcesFilter.Languages,
  //     ConstantsMaster.resourcesFilter.Certificates,
  //     ConstantsMaster.resourcesFilter.RangeThreshold,
  //     ConstantsMaster.resourcesFilter.PracticeArea,
  //     ConstantsMaster.resourcesFilter.EmployeeStatus,
  //     ConstantsMaster.resourcesFilter.StaffableAs,
  //     ConstantsMaster.resourcesFilter.AffiliationRole
  //   ]
  // };
  searchString = '';
  // selectedFilterName = '';

  //used to identify which type of filter is currently applied
  private selectedFilterType: string;
  private SELECTEDFILTERTYPENUM = {
    SUPPLY_GROUPS: "supplyGroups",
    RESOURCES: "resources",
    SEARCH_STRING: "seaechString"
  }

  currentSelectedDisplayingFilter: ResourceFilter | UserPreferences | UserPreferenceSupplyGroupViewModel;
  currentSelectedDisplayingSortAndFilterBy = {
    sortRows: [] as string | SortRow[],
    filterRows: [] as FilterRow[]
  }

  @ViewChild('resourcesGantt', { static: false }) resourcesGantt: GanttComponent; //used for scrolling
  // ----------------------- Notifiers ------------------------------------------------//
  clearEmployeeSearch: Subject<boolean> = new Subject();

  // -----------------------Constructor--------------------------------------------//
  constructor(
    private store: Store<fromResources.State>,
    private modalService: BsModalService,
    private localStorageService: LocalStorageService,
    private coreService: CoreService,
    public dialog: MatDialog,
    private skuCaseTermService: SkuCaseTermService,
    private userpreferencesService: UserPreferenceService,
    private caseRollService: CaseRollService,
    private opportunityService: OpportunityService,
    private router: Router,
    private placeholderAssignmentService: PlaceholderAssignmentService,
    private resourceAssignmentService: ResourceAssignmentService,
    private backfillDialogService: BackfillDialogService,
    private activatedRoute: ActivatedRoute,
    private userPreferenceMessageService: UserPreferencesMessageService,
    private overlayDialogService: OverlayDialogService,
    private sharedNotesService: SharedNotesService,
    private staCommitmentDialogService : staCommitmentDialogService,
  ) { }

  // -------------------Component LifeCycle Events and Functions----------------------//

  ngOnInit() {
    GoogleAnalytics.staffingTrackPageView(this.coreService.loggedInUser.employeeCode, 'resources', '');
    this.officeList = this.localStorageService.get(ConstantsMaster.localStorageKeys.OfficeList);
    this.activatedRoute.queryParams.subscribe(params => {
      this.isPdfExport = params['export'];
      this.getLookupListFromLocalStorage();

      if (this.isPdfExport) {
        // resources per page has been set to max limit of 300 because pdf export gives issues for more resources
        this.resourcesPerPage = 300;
        this.getDataForExport();
        this.showExportPdfDownloadScreen();
        //this.setDefaultSelectedFilter();
        this.getActiveResourcesFromStore();
        this.getActiveResourcesFromStoreOnSearch();
      } else {
        this.resourcesPerPage = this.coreService.appSettings.resourcesPerPage;
        this.currRoute = this.router.url;
        this.getSavedFiltersForLoggedInResource();
        this.selectedCommitmentTypes = this.commitmentTypes.map(item => item.commitmentTypeCode);
        this.coreService.openCaseOppOverlayFromNotifications().pipe(takeUntil(this.destroy$)).subscribe(value => {
          if (value && this.currRoute.includes('resources')) {
            this.openProjectDetailsDialogHandler(value);
          }
        });
        this.getResourcesRecentCDList();
        this.getResourcesCommercialModelList();
        this.setStoreSuscriptions();
        this.getSharedNotesInfo(this.coreService.loggedInUser.employeeCode);
      }
    });
  }



  getDataForExport() {
    this.selectedFilterType = this.localStorageService.get(ConstantsMaster.localStorageKeys.selectedFilterType);
    this.currentSelectedDisplayingFilter = this.localStorageService.get(ConstantsMaster.localStorageKeys.currentSelectedDisplayingFilter);
    this.supplyFilterCriteriaObj = this.localStorageService.get(ConstantsMaster.localStorageKeys.supplyFilterCriteriaObj);
    this.supplyGroupFilterCriteriaObj = this.localStorageService.get(ConstantsMaster.localStorageKeys.supplyGroupFilterCriteriaObj);
    this.thresholdRangeValue = this.localStorageService.get(ConstantsMaster.localStorageKeys.availabilityThreshold);
    this.selectedCommitmentTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.selectedCommitmentTypes);
    this.currentSelectedDisplayingSortAndFilterBy = this.localStorageService.get(ConstantsMaster.localStorageKeys.currentSelectedDisplayingSortAndFilterBy);
    this.selectedWeeklyMonthlyGroupingOption = this.localStorageService.get(ConstantsMaster.localStorageKeys.selectedWeeklyMonthlyGroupingOption);
    this.isSelectedPracticeView = this.localStorageService.get(ConstantsMaster.localStorageKeys.isSelectedPracticeView);
    const startDate = DateService.parseDateInLocalTimeZone(this.supplyFilterCriteriaObj.startDate ?? this.supplyGroupFilterCriteriaObj.startDate);
    const endDate = DateService.parseDateInLocalTimeZone(this.supplyFilterCriteriaObj.endDate ?? this.supplyGroupFilterCriteriaObj.endDate);
    this.dateRange = [startDate, endDate];

    if (this.selectedFilterType === this.SELECTEDFILTERTYPENUM.RESOURCES) {
      this.getActiveResources(this.supplyFilterCriteriaObj);
    } else if (this.selectedFilterType === this.SELECTEDFILTERTYPENUM.SUPPLY_GROUPS) {
      this.getResourcesByGroup(this.supplyGroupFilterCriteriaObj);
    } else if (this.selectedFilterType === this.SELECTEDFILTERTYPENUM.SEARCH_STRING) {
      this.getResourcesIncludingTerminatedBySearchStringHandler({
        typeahead: this.localStorageService.get(ConstantsMaster.localStorageKeys.searchString)
      });
    }
  }

  onToggleEmployeeCaseGroupHandler(selectedGroupingOption: string) {
    this.selectedEmployeeCaseGroupingOption = selectedGroupingOption;
    this.allCaseGroupsAfterSorting = [];
    this.resetScroll();
    this.clearEmployeeSearch.next(true);
    this.isSearchStringExist = false;

    if (this.selectedEmployeeCaseGroupingOption === EmployeeCaseGroupingEnum.CASES) {
      this.allCaseGroupsAfterSorting = this.getResourcesGroupByCase(this.allresourcesAfterSorting);
    }

    const sortRows = this.getCurrentSelectedSortByValue();
    const filterBy = this.getCurrentSelectedFilterByValue();
    this.filterResourcesBySelectedValuesHandler({ filterBy, sortRows });

  }

  onToggleWeeklyMonthlyViewHandler(selectedGroupingOption: string) {
    this.selectedWeeklyMonthlyGroupingOption = selectedGroupingOption
    console.log(this.selectedWeeklyMonthlyGroupingOption);
  }

  onTogglePracticeViewHandler(isSelectedPracticeView: boolean) {
    this.isSelectedPracticeView = isSelectedPracticeView;
    this.resourceViewSelectedTabs = [];
  }

  //Export functionality: future requirements, should not be removed
  showExportPdfDownloadScreen() {
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        loaderMessage: 'Your file is being downloaded'
      }
    };
    this.bsModalRef = this.modalService.show(LoaderComponent, config);
  }

  ganttBodyLoadedEmitterHandler() {
    if (this.isPdfExport) {
      this.startExportingPdf();
    }
  }

  private startExportingPdf() {
    const elementId = 'ganttContainerDiv';
    const pdfFilename = 'resources-commitments.pdf';

    CommonService.generatePdf(elementId, pdfFilename);
  }

  printPdfHandler() {
    this.localStorageService.set(ConstantsMaster.localStorageKeys.supplyFilterCriteriaObj, this.supplyFilterCriteriaObj);
    this.localStorageService.set(ConstantsMaster.localStorageKeys.supplyGroupFilterCriteriaObj, this.supplyGroupFilterCriteriaObj);
    this.localStorageService.set(ConstantsMaster.localStorageKeys.searchString, this.searchString);
    this.localStorageService.set(ConstantsMaster.localStorageKeys.selectedFilterType, this.selectedFilterType);
    this.localStorageService.set(ConstantsMaster.localStorageKeys.currentSelectedDisplayingFilter, this.currentSelectedDisplayingFilter);
    this.localStorageService.set(ConstantsMaster.localStorageKeys.availabilityThreshold, this.thresholdRangeValue);
    this.localStorageService.set(ConstantsMaster.localStorageKeys.selectedCommitmentTypes, this.selectedCommitmentTypes);
    this.localStorageService.set(ConstantsMaster.localStorageKeys.currentSelectedDisplayingSortAndFilterBy, this.currentSelectedDisplayingSortAndFilterBy);
    this.localStorageService.set(ConstantsMaster.localStorageKeys.selectedWeeklyMonthlyGroupingOption, this.selectedWeeklyMonthlyGroupingOption);
    this.localStorageService.set(ConstantsMaster.localStorageKeys.isSelectedPracticeView, this.isSelectedPracticeView);
    this.localStorageService.set(ConstantsMaster.localStorageKeys.resourceViewSelectedTabs, this.resourceViewSelectedTabs);
    //this.localStorageService.set(ConstantsMaster.localStorageKeys.selectedFilterName, this.selectedFilterName);
    const queryParam = window.location.href.indexOf('?') > 0 ? '&' : '?';
    const pdfExportUrl = window.location.href + queryParam + 'export=true';
    window.open(pdfExportUrl);
  }

  private setStoreSuscriptions() {
    this.getSavedResourceFiltersFromStore();
    // if(this.isPdfExport) {
    //   this.resources = this.localStorageService.get(ConstantsMaster.localStorageKeys.allResources);
    // } else {
    //   this.getActiveResourcesFromStore();
    // }
    this.getActiveResourcesFromStore();
    this.getActiveResourcesFromStoreOnSearch();
    this.refreshCaseAndResourceOverlayListener();
    this.refreshLastBillableDateListener();
    this.resourcesLoaderListener();
    this.refreshUserPreferencesSupplyGroupsListener()
    this.getResourcesRecentCDListFromStore();
    this.getResourcesCommercialModelListFromStore();
  }

  private refreshCaseAndResourceOverlayListener() {
    this.storeSub.add(this.store
      .pipe(select(fromResources.refreshCaseAndResourceOverlay))
      .subscribe((refreshNeeded: boolean) => {
        if (!refreshNeeded) {
          return;
        }
        this.refreshCaseAndResourceOverlay();
        this.dispatchRefreshCaseAndResourceOverlayAction(false);
      }));

  }

  private refreshLastBillableDateListener() {
    this.storeSub.add(this.store
      .pipe(select(fromResources.refreshLastBillableDate))
      .subscribe((refreshNeeded: boolean) => {
        if (refreshNeeded && this.resources?.length > 0) {
          let employeeCodes = "";
          if (this.selectedEmployeeCaseGroupingOption === EmployeeCaseGroupingEnum.CASES) {
            let tempECodes = [];
            this.resources.map(x => x.members.map(y => tempECodes.push(y.resource.employeeCode)));
            employeeCodes = [... new Set(tempECodes)].join(',');
          } else {
            employeeCodes = this.resources.map(x => x.resource.employeeCode).join(',');
          }

          this.refreshLastBillableDate(employeeCodes);
          this.dispatchRefreshLastBillableDateAction(false);
        }
      }));

  }

  private resourcesLoaderListener() {
    this.storeSub.add(this.store
      .pipe(select(fromResources.resourcesLoader))
      .subscribe((isLoader: boolean) => {
        if (isLoader === false) {
          this.showProgressBar = false;
        }
      }));
  }

  private refreshUserPreferencesSupplyGroupsListener() {
    this.userPreferenceMessageService.refreshUserPreferencesSupplyGroups().pipe(takeUntil(this.destroy$)).subscribe(upsertedSupplyGroups => {

      //check if default exists in upsertedsupply group. If yes, then set savedfilters defautl and any prexisting custom group deafults to false
      if (upsertedSupplyGroups.find(x => x.isDefaultForResourcesTab)) {
        this.clearDefaultForSavedGroups();
        this.clearDefaultForCustomGroups();
      }

      // remove existing supply group from supplyGroupPreferences and insert upsertedSupplyGroups
      upsertedSupplyGroups.forEach(upsertedSupplyGroup => {
        const index = this.supplyGroupPreferences.findIndex(x => x.id === upsertedSupplyGroup.id);
        if (index > -1) {
          this.supplyGroupPreferences.splice(index, 1);
        }
        this.supplyGroupPreferences.push(upsertedSupplyGroup);
      });

      this.loadViewingFilterDropdown();

      if (this.currentSelectedDisplayingFilter["id"] === upsertedSupplyGroups[0].id) {
        this.onSupplyGroupFilterChangedHandler(upsertedSupplyGroups[0].id);
      }

    });
  }

  ngOnDestroy() {
    this.store.dispatch(new resourcesActions.ClearResourcesStaffingData());
    this.storeSub.unsubscribe();
    this.destroy$.next();
    this.destroy$.complete();
    this.currRoute = '';
  }

  // -------------------------------------Component Events----------------------------------//

  //TODO: to be removed with app-side-bar-filters
  // public getResourcesHandler(event) {
  //   this.setRequestParamsOnFilterChange(event);
  //   this.clearEmployeeSearch.next(true);
  //   this.isSearchStringExist = false;
  //   this.getActiveResources(this.supplyFilterCriteriaObj);
  // }

  getResourcesByDateRangeHandler(event) {
    event.dateRange[0] = DateService.getStartOfWeek(event.dateRange[0]);
    event.dateRange[1] = DateService.getEndOfWeek(true, event.dateRange[1]);
    const startDate = DateService.getFormattedDate(event.dateRange[0]);
    const endDate = DateService.getFormattedDate(event.dateRange[1]);

    this.supplyFilterCriteriaObj.startDate = startDate;
    this.supplyFilterCriteriaObj.endDate = endDate;
    this.supplyGroupFilterCriteriaObj.startDate = startDate;
    this.supplyGroupFilterCriteriaObj.endDate = endDate;
    this.dateRange = event.dateRange;

    this.clearEmployeeSearch.next(true);
    this.isSearchStringExist = false;

    if (this.selectedFilterType == this.SELECTEDFILTERTYPENUM.SUPPLY_GROUPS) {
      this.getResourcesByGroup(this.supplyGroupFilterCriteriaObj);
    } else {
      this.getActiveResources(this.supplyFilterCriteriaObj);
    }
  }

  getResourcesIncludingTerminatedBySearchStringHandler(event) {
    this.selectedFilterType = this.SELECTEDFILTERTYPENUM.SEARCH_STRING;
    this.searchString = event.typeahead.trim();
    const startDate = DateService.getFormattedDate(this.dateRange[0]);
    const endDate = DateService.getFormattedDate(this.dateRange[1]);
    if (this.searchString.length > 2) {
      this.isSearchStringExist = true;
      this.dispatchResourcesLoaderAction(true);
      this.store.dispatch(
        new resourcesActions.LoadResourcesStaffingBySearchString({
          searchString: this.searchString,
          startDate,
          endDate
        })
      );
    } else {
      this.clearSearchData();
    }
  }



  showCommitmentBySelectedValuesHandler(event) {
    this.selectedCommitmentTypes = event.commitmentTypes;
  }


  // -----------------------Resource Allocations Handlers-----------------------------

  public updateResourceAssignmentToProjectHandler(resourceAllocation) {
    resourceAllocation.allocation = parseInt(resourceAllocation.allocation, 10);
    this.dispatchUpdateResourceAction(resourceAllocation);
  }

  public upsertResourceAllocationsToProjectHandler(resourceAllocation, splitSuccessMessage?, allocationDataBeforeSplitting?) {
    let addedResourceAsArray = [];
    const addedResource = resourceAllocation;
    if (Array.isArray(addedResource)) {
      addedResourceAsArray = addedResource;
    } else {
      addedResourceAsArray.push(addedResource);
    }
    this.dispatchUpsertResourceAction(addedResourceAsArray, splitSuccessMessage, allocationDataBeforeSplitting);
  }

  public upsertPlaceholderAllocationsToProjectHandler(placeholderAllocation) {
    this.dispatchUpsertPlaceholderAction(placeholderAllocation);
  }

  public upsertResourceViewNoteHandler(resourceViewNote: ResourceOrCasePlanningViewNote) {
    if (resourceViewNote) {
      this.dispatchUpsertResourceViewNoteAction(resourceViewNote);
    }
  }

  public deleteResourceViewNotesHandler(idsToBeDeleted: string) {
    if (idsToBeDeleted.length > 0) {
      this.dispatchDeleteResourceViewNotesAction(idsToBeDeleted);
    }
  }



  public upsertResourceRecentCDHandler(resourceViewCD: ResourceViewCD) {
    if (resourceViewCD) {
      this.dispatchUpsertResourceRecentCDAction(resourceViewCD);
    }
  }

  public upsertResourceCommercialModelHandler(resourceViewCommercialModel: ResourceViewCommercialModel) {
    if (resourceViewCommercialModel) {
      this.dispatchUpsertResourceCommercialModelAction(resourceViewCommercialModel);
    }
  }


  public deleteResourceRecentCDHandler(idsToBeDeleted: string) {
    if (idsToBeDeleted.length > 0) {
      this.dispatchDeleteResourceRecentCDAction(idsToBeDeleted);
    }
  }

  public deleteResourceCommercialModelHandler(idsToBeDeleted: string) {
    if (idsToBeDeleted.length > 0) {
      this.dispatchDeleteResourceCommercialModelAction(idsToBeDeleted);
    }
  }

  public selectedResourceViewTabHandler(event) {
    const index = this.resourceViewSelectedTabs.findIndex(r => r.employeeCode === event.employeeCode);

    if (index !== -1) {
      this.resourceViewSelectedTabs[index].selectedTab = event.selectedTab;
    } else {
      this.resourceViewSelectedTabs.push(event);
    }
  }

  // -----------------------End Resource Allocations Handlers-----------------------------

  // -----------------------Resource Commitments-----------------------------

  public addResourceCommitmentHandler(resourceCommitment) {
    this.dispatchAddResourceCommitmentAction(resourceCommitment);
  }

  public updateResourceCommitmentHandler(updatedResourceCommitment) {
    this.dispatchUpdateResourceCommitmentAction(updatedResourceCommitment);
  }

  private deleteResourceCommitmentHandler(deletedCommitmentId) {
    this.dispatchDeleteResourceCommitmentAction(deletedCommitmentId);
  }
  // -----------------------End Resource Commitments Handlers-----------------------------

  // ---------------------------------Redux dispatch/Subscribe------------------------------------------//
  private getActiveResources(supplyFilterCriteriaObj: SupplyFilterCriteria) {
    this.selectedFilterType = this.SELECTEDFILTERTYPENUM.RESOURCES;

    if (this.supplyFilterCriteriaObj.officeCodes === '' ||
      this.supplyFilterCriteriaObj.staffingTags === '' || this.supplyFilterCriteriaObj.employeeStatuses === '') {
      this.resources = [];
      return;
    }
    this.dispatchResourcesLoaderAction(true);

    this.store.dispatch(
      new resourcesActions.LoadResourcesStaffing({
        supplyFilterCriteriaObj,
        pageNumber: null, //passing null so as to fetch all resources data and apply pagination at front end
        resourcesPerPage: null,
      })
    );
  }

  private getResourcesRecentCDList() {

    this.store.dispatch(
      new resourcesActions.LoadResourcesRecentCDList({
      })
    );
  }
  private getResourcesCommercialModelList() {

    this.store.dispatch(
      new resourcesActions.LoadResourcesCommercialModelList({
      })
    );
  }

  private getResourcesByGroup(supplyGroupFilterCriteriaObj: SupplyGroupFilterCriteria) {
    this.selectedFilterType = this.SELECTEDFILTERTYPENUM.SUPPLY_GROUPS;

    if (!supplyGroupFilterCriteriaObj.employeeCodes) {
      this.resources = [];
      return;
    }

    this.dispatchResourcesLoaderAction(true);

    this.store.dispatch(
      new resourcesActions.LoadGroupedResourcesStaffing({
        supplyGroupFilterCriteriaObj,
        pageNumber: null, //passing null so as to fetch all resources data and apply pagination at front end
        resourcesPerPage: null,
      })
    );
  }

  private getSavedResourceFiltersFromStore() {
    this.storeSub.add(this.store
      .pipe(
        select(fromResources.getSavedResourceFilters),
        filter(savedFilters => savedFilters != null)
      )
      .subscribe((resourceFilters: ResourceFilter[]) => {
        if (resourceFilters) {

          if (!this.savedResourceFilters) {
            // initial load
            this.savedResourceFilters = resourceFilters;
            this.initializeAndSubscribeUserPreferences();

          } else {
            // filter change
            const upsertedSavedFilter = resourceFilters.find(x => x.id === this.currentSelectedDisplayingFilter["id"]);
            this.savedResourceFilters = resourceFilters;

            //check if default exists in upsertedData, then set defaults from supply groups to False
            if (resourceFilters.find(x => x.isDefault)) {
              this.clearDefaultForCustomGroups();
            }

            this.loadViewingFilterDropdown();

            if (upsertedSavedFilter) {
              this.currentSelectedDisplayingFilter = upsertedSavedFilter;
              this.updateSupplyFilterCriteriaForSavedFilters(upsertedSavedFilter);
              this.getActiveResources(this.supplyFilterCriteriaObj);
            }
          }

        }
      }));
  }

  private initializeAndSubscribeUserPreferences() {
    const combinedUserPreferences = this.coreService.getCombinedUserPreferencesValue();
    this.userPreferences = combinedUserPreferences.userPreferences;
    this.supplyGroupPreferences = combinedUserPreferences.userPreferenceSupplyGroups;
    this.subscibeToUserPreferences();
  }

  private getDataBasedOnDefaultSettings() {
    if (this.isSearchStringExist) {
      this.clearEmployeeSearch.next(true);
      this.clearSearchData();
    }

    this.updateDefaultFilterCriteria();
    // load resources data
    this.getResourcesByDefaultSettings();
    this.loadViewingFilterDropdown();
  }


  private getSharedNotesInfo(loggedInEmployeeCode) {
    this.sharedNotesService.getSharedNotes(loggedInEmployeeCode).subscribe(notes => {
      this.sharedNotes = notes;
    });
  }

  updateDefaultFilterCriteria() {
    if (this.savedResourceFilters.some(x => x.isDefault)) {
      this.updateSupplyFilterCriteriaForSavedFilters(this.savedResourceFilters.find(x => x.isDefault));
    } else if (this.supplyGroupPreferences.some(x => x.isDefaultForResourcesTab || x.isDefault)) {
      const defaultFilter = this.supplyGroupPreferences.find(x => x.isDefaultForResourcesTab) ?? this.supplyGroupPreferences.find(x => x.isDefault); // done to ensure isDefaultForResourcesTab takes precedence ove isDefault
      this.updateSupplyFilterCriteriaForCustomGroup(defaultFilter);
    } else {
      this.updateSupplyFilterCriteriaForUserPreferences(this.userPreferences);
    }
  }

  private updateSupplyFilterCriteriaForUserPreferences(userPreferences: UserPreferences) {
    // const userPreferences = data ?? this.userPreferences;
    const dateRangeForResourcesTab = this.getDateRangeForResourcesTab();
    this.dateRange = [DateService.parseDateInLocalTimeZone(dateRangeForResourcesTab.startDate), DateService.parseDateInLocalTimeZone(dateRangeForResourcesTab.endDate)];

    this.supplyFilterCriteriaObj.startDate = dateRangeForResourcesTab.startDate;
    this.supplyFilterCriteriaObj.endDate = dateRangeForResourcesTab.endDate;

    if (userPreferences && typeof userPreferences === 'object') {
      this.supplyFilterCriteriaObj.officeCodes = userPreferences.supplyViewOfficeCodes;
      this.supplyFilterCriteriaObj.levelGrades = userPreferences.levelGrades;
      this.supplyFilterCriteriaObj.positionCodes = userPreferences.positionCodes;
      this.supplyFilterCriteriaObj.staffingTags =
        userPreferences.supplyViewStaffingTags || this.serviceLineCodeEnum.GeneralConsulting;
      this.supplyFilterCriteriaObj.sortBy = userPreferences.sortBy;
      this.supplyFilterCriteriaObj.practiceAreaCodes = userPreferences.practiceAreaCodes || '';
      this.supplyFilterCriteriaObj.affiliationRoleCodes = userPreferences.affiliationRoleCodes || '';
      this.supplyFilterCriteriaObj.availabilityIncludes = userPreferences.availabilityIncludes;
      this.supplyFilterCriteriaObj.staffableAsTypeCodes = userPreferences.staffableAsTypeCodes;
    } else {
      /*-------------- Set default search criteria for Supply Side ---------------------*/
      this.supplyFilterCriteriaObj.officeCodes = this.coreService.loggedInUser.office.officeCode.toString(); //home office
      this.supplyFilterCriteriaObj.staffingTags = this.serviceLineCodeEnum.GeneralConsulting;
      this.supplyFilterCriteriaObj.sortBy = 'fullName';
    }

    //below supply properties are not driven by userSettings. They have fixed defaults
    this.supplyFilterCriteriaObj.employeeStatuses = ConstantsMaster.employeeStatus.map(x => x.code).join(",");// show all status by default

  }

  private updateSupplyFilterCriteriaForCustomGroup(data: UserPreferenceSupplyGroupViewModel) {
    const dateRangeForResourcesTab = this.getDateRangeForResourcesTab();
    this.dateRange = [DateService.parseDateInLocalTimeZone(dateRangeForResourcesTab.startDate), DateService.parseDateInLocalTimeZone(dateRangeForResourcesTab.endDate)];

    this.supplyGroupFilterCriteriaObj.employeeCodes = data.groupMembers?.map(x => x.employeeCode).join(',') || '';
    this.supplyGroupFilterCriteriaObj.startDate = dateRangeForResourcesTab.startDate;
    this.supplyGroupFilterCriteriaObj.endDate = dateRangeForResourcesTab.endDate;
    this.supplyGroupFilterCriteriaObj.availabilityIncludes = this.supplyFilterCriteriaObj.availabilityIncludes;
  }

  private updateSupplyFilterCriteriaForSavedFilters(data: ResourceFilter) {
    const dateRangeForResourcesTab = this.getDateRangeForResourcesTab();
    this.dateRange = [DateService.parseDateInLocalTimeZone(dateRangeForResourcesTab.startDate), DateService.parseDateInLocalTimeZone(dateRangeForResourcesTab.endDate)];

    this.supplyFilterCriteriaObj.startDate = dateRangeForResourcesTab.startDate;
    this.supplyFilterCriteriaObj.endDate = dateRangeForResourcesTab.endDate;
    this.supplyFilterCriteriaObj.officeCodes = data.officeCodes || this.coreService.loggedInUser.office.officeCode.toString(); //home office;
    this.supplyFilterCriteriaObj.levelGrades = data.levelGrades;
    this.supplyFilterCriteriaObj.positionCodes = data.positionCodes;
    this.supplyFilterCriteriaObj.staffingTags = data.staffingTags || this.serviceLineCodeEnum.GeneralConsulting;;
    this.supplyFilterCriteriaObj.employeeStatuses = data.employeeStatuses || ConstantsMaster.employeeStatus.map(x => x.code).join(",");// show all status by default;
    this.supplyFilterCriteriaObj.practiceAreaCodes = data.practiceAreaCodes || '';
    this.supplyFilterCriteriaObj.affiliationRoleCodes = data.affiliationRoleCodes || '';
    this.supplyFilterCriteriaObj.staffableAsTypeCodes = data.staffableAsTypeCodes;
    this.supplyFilterCriteriaObj.sortBy = data.resourcesTabSortBy || 'fullName';

    // this.supplyFilterCriteriaObj.availabilityIncludes = this.userPreferences.availabilityIncludes;
  }

  private getDateRangeForResourcesTab() {
    const startDate = DateService.getStartOfWeek(new Date());
    let endDate = new Date();
    endDate.setDate(endDate.getDate() + 40);
    endDate = DateService.getEndOfWeek(true, endDate);

    // Default date range will be today + 40 days on load
    const defaultDateRange = DateService.getFormattedDateRange({
      startDate: startDate,
      endDate: endDate,
    });

    if (this.userPreferences && typeof this.userPreferences === 'object') {
      const today = new Date();
      let dateRangeForResources: { startDate: any; endDate: any };

      if (this.userPreferences.supplyWeeksThreshold) {
        const userSettingsEndDate = DateService.getEndOfWeek(true, new Date(
          today.setDate(
            today.getDate() + this.userPreferences.supplyWeeksThreshold * 7
          )
        ));

        const differenceInDays = DateService.getDatesDifferenceInDays(
          startDate,
          userSettingsEndDate
        );
        const minDaysForProperRenderingOfResourcesGantt = 40;
        if (differenceInDays > minDaysForProperRenderingOfResourcesGantt) {
          endDate = userSettingsEndDate;
        }
      }
      const date = { startDate: startDate, endDate: endDate };
      dateRangeForResources = DateService.getFormattedDateRange(date);

      return dateRangeForResources;
    }

    return defaultDateRange;
  }

  private clearDefaultForCustomGroups() {
    this.supplyGroupPreferences.forEach(x => {
      if (x.isDefaultForResourcesTab) {
        x.isDefaultForResourcesTab = false;
      }
    })
  }

  private clearDefaultForSavedGroups() {
    this.savedResourceFilters.forEach(x => {
      if (x.isDefault) {
        x.isDefault = false;
      }
    })
  }

  getCurrentSelectedSortByValue(): string | SortRow[] {
    let sortBy = this.currentSelectedDisplayingSortAndFilterBy.sortRows;
    return sortBy;
  }

  getCurrentSelectedFilterByValue(): FilterRow[] {
    let filterBy = this.currentSelectedDisplayingSortAndFilterBy.filterRows
    return filterBy;
  }

  private getActiveResourcesFromStoreOnSearch() {
    this.storeSub.add(this.store
      .pipe(select(fromResources.getSearchedResourcesStaffing))
      .subscribe((resourcesData: ResourceStaffing[]) => {
        if (this.isSearchStringExist) {
          if (typeof Worker !== 'undefined') {
            this.runWorkerForSearch(resourcesData);
          } else {
            let resourcesDataWithAvailability = ResourceService.createResourcesDataForResourcesTab(resourcesData, this.supplyFilterCriteriaObj.startDate, this.supplyFilterCriteriaObj.endDate,
              this.supplyFilterCriteriaObj, this.commitmentTypes, this.coreService.getUserPreferencesValue(), false);

            const sortBy = this.getCurrentSelectedSortByValue();
            resourcesDataWithAvailability = this.getResourcesSortBySelectedValues(resourcesDataWithAvailability, sortBy);
            //get the API call for getting resources count here
            if (this.selectedEmployeeCaseGroupingOption === EmployeeCaseGroupingEnum.CASES) {
              this.resources = this.getResourcesGroupByCase(resourcesDataWithAvailability);
            } else {
              this.resources = resourcesDataWithAvailability;
            }

            this.resetScroll();
          }
        }
      }));
  }


  private runWorkerForSearch(resourcesData: ResourceStaffing[]) {
    this.initializeWorker(resourcesData);
    this.webWorker.onmessage = (response) => {
      let resourcesDataWithAvailability = response.data;
      const sortBy = this.getCurrentSelectedSortByValue();
      resourcesDataWithAvailability = this.getResourcesSortBySelectedValues(resourcesDataWithAvailability, sortBy);

      if (this.selectedEmployeeCaseGroupingOption === EmployeeCaseGroupingEnum.CASES) {
        this.resources = this.getResourcesGroupByCase(resourcesDataWithAvailability);
      } else {
        this.resources = resourcesDataWithAvailability;
      }

      // this.resetScroll();
    };
  }

  private getActiveResourcesFromStore() {
    this.storeSub.add(this.store
      .pipe(
        select(fromResources.getResourcesStaffing),
        filter(resources => resources != null))
      .subscribe((resourcesData: ResourceStaffing[]) => {
        if ((!this.isSearchStringExist)) {
          if (typeof Worker !== 'undefined') {
            this.runWorker(resourcesData);

          } else {
            this.allresourcesAfterSorting = ResourceService.createResourcesDataForResourcesTab(resourcesData, this.supplyFilterCriteriaObj.startDate, this.supplyFilterCriteriaObj.endDate,
              this.supplyFilterCriteriaObj, this.commitmentTypes, this.coreService.getUserPreferencesValue(), false);

            const sortRows = this.getCurrentSelectedSortByValue();
            const filterBy = this.getCurrentSelectedFilterByValue();

            if (this.selectedEmployeeCaseGroupingOption === EmployeeCaseGroupingEnum.CASES) {
              this.allCaseGroupsAfterSorting = this.getResourcesGroupByCase(this.allresourcesAfterSorting);
              this.filterResourcesBySelectedValuesHandler({ filterBy, sortRows });
            } else {
              this.filterResourcesBySelectedValuesHandler({ filterBy, sortRows });
            }

          }

          if (resourcesData.length > 0) {
            let caseOppCodes = "";
            let includedKeys: string[] = [];
            resourcesData.forEach((item) => {
              if (item.allocations.length) {
                item.allocations.forEach(alloc => {
                  const key = alloc.oldCaseCode || alloc.pipelineId;
                  if (!includedKeys.includes(key)) {
                    includedKeys.push(key);
                  }
                })
              }
            })
            caseOppCodes = includedKeys.join(',')
            this.getResourcesCountOnCase(caseOppCodes)
          }

        }
      }));
  }

  private getResourcesRecentCDListFromStore() {
    this.storeSub.add(
      this.store
        .pipe(select(fromResources.getResourcesRecentCDList))
        .subscribe((resourceRecentCDList: ResourceViewCD[]) => {
          this.resourcesRecentCDList = resourceRecentCDList;
        })
    );
  }

  private getResourcesCommercialModelListFromStore() {
    this.storeSub.add(
      this.store
        .pipe(select(fromResources.getResourcesCommercialModelList))
        .subscribe((resourcesCommercialModelList: ResourceViewCommercialModel[]) => {
          this.resourcesCommercialModelList = resourcesCommercialModelList;
        })
    );
  }

  private initializeWorker(resourcesData: ResourceStaffing[]) {
    this.webWorker = new Worker(new URL('../shared/web-workers/resource-view-data.worker', import.meta.url), { type: "module" });
    this.webWorker.postMessage(
      JSON.stringify({
        resourcesData: resourcesData,
        searchStartDate: this.supplyFilterCriteriaObj.startDate,
        searchEndDate: this.supplyFilterCriteriaObj.endDate,
        supplyFilterCriteriaObj: this.supplyFilterCriteriaObj,
        commitmentTypes: this.commitmentTypes,
        userPreferences: this.coreService.getUserPreferencesValue(),
        isTriggeredFromSearch: false
      }));
  }

  private runWorker(resourcesData: ResourceStaffing[]) {
    this.initializeWorker(resourcesData);
    this.webWorker.onmessage = (response) => {
      this.allresourcesAfterSorting = response.data;

      const sortRows = this.getCurrentSelectedSortByValue();
      const filterBy = this.getCurrentSelectedFilterByValue();
      this.filterResourcesBySelectedValuesHandler({ filterBy, sortRows });

      if (this.selectedEmployeeCaseGroupingOption === EmployeeCaseGroupingEnum.CASES) {
        this.allCaseGroupsAfterSorting = this.getResourcesGroupByCase(this.allresourcesAfterSorting);
        this.filterResourcesBySelectedValuesHandler({ filterBy, sortRows });
      } else {
        this.filterResourcesBySelectedValuesHandler({ filterBy, sortRows });
      }
    };
  }

  private getResourcesCountOnCase(oldCaseCodes: string) {

    if (oldCaseCodes != "") {
      this.store.dispatch(
        new resourcesActions.LoadResourcesCountOnCaseOpp({
          oldCaseCodes
        })
      );

      this.storeSub.add(this.store
        .pipe(select(fromResources.getResourcesCountOnCaseOpp))
        .subscribe((data) => {
          this.resourcesCountOnCaseOpp = data;
        }));
    }

  }
 
  private loadResourcesData(resourcesData: ResourceStaffing[] | ResourceCaseGroup[]) {

    if (this.selectedEmployeeCaseGroupingOption === EmployeeCaseGroupingEnum.CASES) {
      this.resources = [...resourcesData as ResourceCaseGroup[]];
    } else {
      this.resources = [...resourcesData as ResourceStaffing[]];
    }
  }

  sortResourcesBySelectedValuesHandler(event: SortRow[]) {
    this.currentSelectedDisplayingSortAndFilterBy.sortRows = event;
    this.resetScroll();
    this.loadResourcesData(this.getResourcesSortBySelectedValues(this.resources, event));
  }

  getResourcesSortBySelectedValues(resources, sortBy: string | SortRow[]) {
    let sortRows: SortRow[] = [];

    if (!sortBy?.length) {
      sortRows.push({ field: 'fullName', direction: 'asc' });
    }
    else {
      sortRows = typeof sortBy === 'string' ? this.getSortByArray(sortBy) : sortBy;
    }

    if (this.selectedEmployeeCaseGroupingOption === EmployeeCaseGroupingEnum.CASES) {
      resources = this.sortResourcesForCasesView(resources, sortRows);
    } else {
      resources = this.sortResourcesForResourcesView(resources, sortRows);
    }

    return resources
  }

  sortResourcesForResourcesView(resources, sortRows) {
    return CommonService.getResourcesSortBySelectedValues(resources, sortRows, this.officeList);
  }

  sortResourcesForCasesView(resources, sortRows) {
    resources.forEach(x => {
      x.members = CommonService.getResourcesSortBySelectedValues(x.members, sortRows, this.officeList)
    });
    return resources;
  }

  filterResourcesBySelectedValuesHandler({ filterBy, sortRows }: { filterBy: FilterRow[], sortRows: string | SortRow[] }) {
    this.currentSelectedDisplayingSortAndFilterBy.filterRows = JSON.parse(JSON.stringify(filterBy));

    if (!sortRows?.length) {
      sortRows = this.getCurrentSelectedSortByValue();
    }

    let filterAndSortResourcesData;

    if (this.selectedEmployeeCaseGroupingOption === EmployeeCaseGroupingEnum.CASES) {


      filterAndSortResourcesData = this.filterResourcesByAdvancedFilter([...this.allCaseGroupsAfterSorting], filterBy);
    } else {
      filterAndSortResourcesData = this.filterResourcesByAdvancedFilter([...this.allresourcesAfterSorting], filterBy);
    }

    filterAndSortResourcesData = this.getResourcesSortBySelectedValues(filterAndSortResourcesData, sortRows);

    this.loadResourcesData(filterAndSortResourcesData);
  }

  filterResourcesByAdvancedFilter(resourcesToBeFilter, filterBy: FilterRow[] = []) {
    if (!filterBy?.length) {
      return resourcesToBeFilter;
    }

    let resources = JSON.parse(JSON.stringify(resourcesToBeFilter)); //done to avoid manipulation of original data
    if (this.selectedEmployeeCaseGroupingOption === EmployeeCaseGroupingEnum.CASES) {

      resources.forEach(x => {
        x.members = this.filterResourcesByFilterRows(x.members, filterBy);
      })
      // remove cases that don't have any resources after filtering
      resources = resources.filter(x => x.members.length > 0);

    } else {
      resources = this.filterResourcesByFilterRows(resources, filterBy);
    }

    return resources
  }

  filterResourcesByFilterRows(resourcesData: ResourceStaffing[], filterBy: FilterRow[]) {
    if (!resourcesData || resourcesData.length == 0) {
      return resourcesData;
    }
    resourcesData = resourcesData.filter((resourceData) => {
      var resource: Resource = resourceData.resource;
      var resourcesAllocationsAndCommitments = this.getAllResourceCommitments(resourceData);
      var certificates = this.getCertificatesForResource(resourceData);
      var recentCD = this.getRecentCDForResource(resourceData);
      var commercialModel = this.getCommercialModelForResource(resourceData);
      var languages = this.getLanguagesForResource(resourceData);
      var industryCapabilityInterested = this.getIndustryCapabilityPreferencesForResource(resourceData, true);
      var industryCapabilityNotInterested = this.getIndustryCapabilityPreferencesForResource(resourceData, false);
      let filterConditions: FilterObject[] = [];
      filterBy.forEach(filter => {
        switch (filter.filterField) {
          case 'availabilityPercentage':
            const valueToFilter = resource.percentAvailable ?? 0; // null is being considered as 0 for avail percentage
            let filterResourceByAvailabilityPercentage: FilterObject = {
              isFilterData: CommonService.validateUserInputByOperatorAndType(valueToFilter, filter.filterValue, filter.filterOperator, 'number'),
              operator: filter.andOr
            };
            filterConditions.push(filterResourceByAvailabilityPercentage);
            break;
          case 'availabilityDate':
            let filterResourceByAvailabilityDate: FilterObject = {
              isFilterData: CommonService.validateUserInputByOperatorAndType(resource.dateFirstAvailable, filter.filterValue, filter.filterOperator, 'date'),
              operator: filter.andOr
            };
            filterConditions.push(filterResourceByAvailabilityDate);
            break;
          case 'hireDate':
            let filterResourceByHireDate: FilterObject = {
              isFilterData: CommonService.validateUserInputByOperatorAndType(resource.startDate, filter.filterValue, filter.filterOperator, 'date'),
              operator: filter.andOr
            };
            filterConditions.push(filterResourceByHireDate);
            break;
          case 'lastDateStaffed':
            let filterResourceByLastDateStaffed: FilterObject = {
              isFilterData: CommonService.validateUserInputByOperatorAndType(resource.lastBillable?.lastBillableDate, filter.filterValue, filter.filterOperator, 'date'),
              operator: filter.andOr
            };
            filterConditions.push(filterResourceByLastDateStaffed);
            break;
          case 'commitment':
            let filterResourceByCommitments: FilterObject = {
              isFilterData: CommonService.validateUserInputByOperatorAndType(resourcesAllocationsAndCommitments, filter.filterValue, filter.filterOperator, 'array'),
              operator: filter.andOr
            };
            filterConditions.push(filterResourceByCommitments);
            break;
          case 'certificates':
            let filterResourceByCertificates: FilterObject = {
              isFilterData: CommonService.validateUserInputByOperatorAndType(certificates, filter.filterValue, filter.filterOperator, 'array'),
              operator: filter.andOr
            };
            filterConditions.push(filterResourceByCertificates);
            break;

          case 'languages':
            let filterResourceByLanguages: FilterObject = {
              isFilterData: CommonService.validateUserInputByOperatorAndType(languages, filter.filterValue, filter.filterOperator, 'array'),
              operator: filter.andOr
            };
            filterConditions.push(filterResourceByLanguages);
            break;

          case 'recentCD':
            const equalfilterOperator = 'equals';
            let filterResourceByRecentCD: FilterObject = {
              isFilterData: CommonService.validateUserInputByOperatorAndType(recentCD, filter.filterValue, equalfilterOperator, 'array'),
              operator: filter.andOr
            };
            filterConditions.push(filterResourceByRecentCD);
            break;

          case 'commercialModel':
            const equalFilterOperator = 'equals';
            let filterResourceByCommercialModel: FilterObject = {
              isFilterData: CommonService.validateUserInputByOperatorAndType(commercialModel, filter.filterValue, equalFilterOperator, 'array'),
              operator: filter.andOr
            };
            filterConditions.push(filterResourceByCommercialModel);
            break;



          case 'industry/capability':
            let filterData;

            if (filter.filterOperator === 'interested') {
              filterData = industryCapabilityInterested;
            } else {
              filterData = industryCapabilityNotInterested;
            }
            const filterType = 'equals';
            let filterResourceByPreferences: FilterObject = {
              isFilterData: CommonService.validateUserInputByOperatorAndType(filterData, filter.filterValue, filterType, 'array'),
              operator: filter.andOr
            };
            filterConditions.push(filterResourceByPreferences);
            break;
        }
        return filter;
      });
      return this.checkResourceVisibility(filterConditions);
    });

    return resourcesData;
  }

  getAllResourceCommitments(resourceData: ResourceStaffing) {
    const commitmentArray = [];
    if (resourceData.allocations.length > 0) {
      commitmentArray.push(CommitmentTypeEnum.CASE_OPP);
    }

    if (resourceData.loAs.length > 0) {
      resourceData.loAs.forEach(loa => {

        // Using 'includes' here because the LOA description might change in the future
        if (loa.description.includes("Unpaid")) {
          commitmentArray.push(CommitmentTypeEnum.LOA_UNPAID);
        }
        else if (loa.description.includes("Paid")) {
          commitmentArray.push(CommitmentTypeEnum.LOA_PAID);
        }
      });
    }

    if (this.getIfCommitmentExistsForResource(resourceData, CommitmentTypeEnum.LOA)) {
      commitmentArray.push(CommitmentTypeEnum.LOA_PLANNED);
    }

    if (this.getIfCommitmentExistsForResource(resourceData, CommitmentTypeEnum.PEG)) {
      commitmentArray.push(CommitmentTypeEnum.PEG);
    }
    if (this.getIfCommitmentExistsForResource(resourceData, CommitmentTypeEnum.PEG_Surge)) {
      commitmentArray.push(CommitmentTypeEnum.PEG_Surge);
    }
    if (resourceData.vacations.length > 0) {
      commitmentArray.push(CommitmentTypeEnum.VACATION_APPROVED);
    }
    if (resourceData.timeOffs.length > 0) {
      resourceData.timeOffs.forEach(vacation => {
        if (vacation.status == 'Approved') {
          commitmentArray.push(CommitmentTypeEnum.VACATION_APPROVED);
        }
        else if (vacation.status == 'Submitted') {
          commitmentArray.push(CommitmentTypeEnum.VACATION_PENDING);
        }
      });
    }

    if (this.getIfCommitmentExistsForResource(resourceData, CommitmentTypeEnum.VACATION)) {
      commitmentArray.push(CommitmentTypeEnum.VACATION_PLANNED);
    }

    if (resourceData.trainings.length > 0) {
      commitmentArray.push(CommitmentTypeEnum.TRAINING_APPROVED);
    }
    if (this.getIfCommitmentExistsForResource(resourceData, CommitmentTypeEnum.TRAINING)) {
      commitmentArray.push(CommitmentTypeEnum.TRAINING_PLANNED);
    }
    if (this.getIfCommitmentExistsForResource(resourceData, CommitmentTypeEnum.RECRUITING)) {
      commitmentArray.push(CommitmentTypeEnum.RECRUITING);
    }
    if (this.getIfCommitmentExistsForResource(resourceData, CommitmentTypeEnum.SHORT_TERM_AVAILABLE)) {
      commitmentArray.push(CommitmentTypeEnum.SHORT_TERM_AVAILABLE);
    }
    if (this.getIfCommitmentExistsForResource(resourceData, CommitmentTypeEnum.NOT_AVAILABLE)) {
      commitmentArray.push(CommitmentTypeEnum.NOT_AVAILABLE);
    }
    if (this.getIfCommitmentExistsForResource(resourceData, CommitmentTypeEnum.LIMITED_AVAILABILITY)) {
      commitmentArray.push(CommitmentTypeEnum.LIMITED_AVAILABILITY);
    }
    if (resourceData.holidays.length > 0) {
      commitmentArray.push(CommitmentTypeEnum.HOLIDAY);
    }
    if (resourceData.placeholderAllocations.length > 0 && resourceData.placeholderAllocations.filter(x => !!x.planningCardId).length > 0) {
      commitmentArray.push(CommitmentTypeEnum.PLANNING_CARD);
    }
    if (this.getIfCommitmentExistsForResource(resourceData, CommitmentTypeEnum.DOWN_DAY)) {
      commitmentArray.push(CommitmentTypeEnum.DOWN_DAY);
    }
    if (resourceData.placeholderAllocations.length > 0 && resourceData.placeholderAllocations.filter(x => !!x.employeeCode).length > 0) {
      commitmentArray.push(CommitmentTypeEnum.NAMED_PLACEHOLDER);
    }

    return commitmentArray.join(',');
  }

  getCertificatesForResource(resourceData: ResourceStaffing) {
    if (!resourceData.certificates) {
      return [];
    }
    return resourceData.certificates.map(certificate => certificate.name);
  }

  getRecentCDForResource(resourceData: ResourceStaffing) {
    if (!resourceData.resourceCD && resourceData.resourceCD.length > 0) {
      return [];
    }
    return resourceData.resourceCD.map(resourceCD => resourceCD.recentCD);
  }

  getCommercialModelForResource(resourceData: ResourceStaffing) {
    if (!resourceData.resourceCommercialModel && resourceData.resourceCommercialModel.length > 0) {
      return [];
    }
    return resourceData.resourceCommercialModel.map(resourceCommercialModel => resourceCommercialModel.commercialModel);
  }


  getLanguagesForResource(resourceData: ResourceStaffing) {
    if (!resourceData.languages) {
      return [];
    }
    return resourceData.languages.map(language => language.name);
  }

  getIndustryCapabilityPreferencesForResource(resourceData: ResourceStaffing, isInterested: boolean) {
    if (!resourceData.preferences) {
      return [];
    }

    if (isInterested) {
      return resourceData.preferences
        .filter(preferences => preferences.interest)
        .map(preferences => preferences.staffingPreference);
    } else {
      return resourceData.preferences
        .filter(preferences => preferences.noInterest)
        .map(preferences => preferences.staffingPreference);
    }
  }

  getIfCommitmentExistsForResource(resourceData: ResourceStaffing, commitmentTypeCode) {
    return resourceData.commitments.filter(x => x.commitmentTypeCode === commitmentTypeCode).length > 0;
  }

  public readonly AND_OR_ENUM = {
    AND: "and",
    OR: "or"
  }

  checkResourceVisibility(filterArray: FilterObject[]): boolean {
    if (filterArray.length === 0) {
      return;
    }

    let result = filterArray[0].isFilterData;
    for (let i = 1; i < filterArray.length; i++) {
      const { isFilterData, operator } = filterArray[i];

      if (operator === this.AND_OR_ENUM.AND) {
        result = result && isFilterData;
      } else if (operator === this.AND_OR_ENUM.OR) {
        result = result || isFilterData;
      }
    }

    return result;
  }

  isResourceFiltered(field, operator, value, type) {
    if (!type) {
      type = typeof field;
    }

    let filterResource = false;
    switch (operator) {
      case 'greaterThan':
        filterResource = CommonService.validateUserInputByOperatorAndType(field, value, operator, type);
        break;
      case 'lesserThan':
        filterResource = field < value;
        break;
      case 'equals':
        filterResource = field == value;
        break;
      case 'notEquals':
        filterResource = field != value;
        break;
      case 'between':
        filterResource = field >= value && field <= value;
        break;
    }
    return filterResource;
  }

  getResourcesGroupByCase(resourcesAfterSort) {
    let allCaseGroupsAfterSorting = [];

    Object.entries(this.groupResourcesByCases(resourcesAfterSort)).forEach(([key, value]) => {
      allCaseGroupsAfterSorting.push(value);
    });
    allCaseGroupsAfterSorting = this.getCaseGroupsSortBySelectedValues(allCaseGroupsAfterSorting, "clientName");

    return allCaseGroupsAfterSorting;
  }


  // getResourcesSortAndGroupBySelectedValues(resources, sortBy = 'fullName', sortDirection = 'asc') {
  //   sortDirection = sortDirection ?? 'asc';
  //   sortBy = sortBy ?? 'fullName';

  //   CommonService.getResourcesSortAndGroupBySelectedValues(resources, sortBy, sortDirection)
  //   return resources;
  // }

  groupResourcesByCases(resources) {
    let groups = [];
    resources.forEach((item) => {
      let includedKeys: string[] = [];

      const filteredPlaceholderAllocations = item.placeholderAllocations.filter((obj) => {
        return !obj.planningCardId || (obj.isPlanningCardShared === true && obj.startDate !== undefined && obj.endDate !== undefined && obj.planningCardTitle !== null && obj.planningCardId !== null)
      });

      const mergedData = [...item.allocations, ...filteredPlaceholderAllocations];

      if (mergedData.length) {

        mergedData.forEach(alloc => {
          const key = alloc.oldCaseCode || alloc.pipelineId || alloc.planningCardId;;
          if (!includedKeys.includes(key)) {
            includedKeys.push(key);
          } else
            return;

          if (groups[key]) {
            groups[key]["members"].push(item);
          } else {
            let caseDetails: ProjectBasic = {
              caseCode: alloc.caseCode,
              planningCardId: alloc.planningCardId,
              clientCode: alloc.clientCode,
              caseTypeCode: alloc.caseTypeCode,
              caseType: alloc.caseType,
              oldCaseCode: null,
              clientName: null,
              startDate: null,
              endDate: null,
              caseName: null,
              pipelineId: alloc.pipelineId,
              opportunityStatus: alloc.opportunityStatus,
              opportunityName: alloc.opportunityName,
              probabilityPercent: alloc.probabilityPercent
            }

            if (alloc.oldCaseCode) {
              caseDetails.oldCaseCode = alloc.oldCaseCode;
              caseDetails.clientName = alloc.clientName;
              caseDetails.startDate = alloc.caseStartDate;
              caseDetails.endDate = alloc.caseEndDate;
              caseDetails.caseName = alloc.caseName;
            }
            else if (alloc.pipelineId) {
              caseDetails.oldCaseCode = "Opp";
              caseDetails.clientName = alloc.clientName;
              caseDetails.startDate = alloc.opportunityStartDate;
              caseDetails.endDate = alloc.opportunityEndDate;
              caseDetails.caseName = alloc.caseName;
            }
            else if (alloc.planningCardId) {
              caseDetails.oldCaseCode = "PC";
              caseDetails.clientName = alloc.planningCardTitle;
              caseDetails.startDate = alloc.startDate;
              caseDetails.endDate = alloc.endDate;
              caseDetails.caseName = alloc.planningCardTitle;
            }

            groups[key] = {};
            groups[key]["caseDetails"] = caseDetails;
            groups[key]["members"] = [].concat(item);
          }
        });

      }
      else {
        const key = "notAllocated";
        if (groups[key]) {
          if (!includedKeys.includes(key)) {
            includedKeys.push(key);

            groups[key]["members"].push(item);
          }
        } else {
          const caseDetails: ProjectBasic = {
            caseCode: null,
            planningCardId: null,
            clientCode: null,
            caseTypeCode: null,
            caseType: null,
            oldCaseCode: "NA",
            clientName: "Not Allocated",
            startDate: null,
            endDate: null,
            caseName: "Not Allocated",
            pipelineId: null,
            opportunityStatus: null,
            opportunityName: null,
            probabilityPercent: null
          }

          groups[key] = {};
          groups[key]["caseDetails"] = caseDetails;
          groups[key]["members"] = [].concat(item);
        }
      }
    });

    return groups;
  }

  getCaseGroupsSortBySelectedValues(caseGroups, sortBy = 'clientName') {
    caseGroups.sort((previousElement, nextElement) => {

      //this is done to push "Not Allocated" bucket to bottom
      if (previousElement.caseDetails[sortBy] === "Not Allocated"
      ) {
        return 1;
      }
      if (nextElement.caseDetails[sortBy] === "Not Allocated"
      ) {
        return -1;
      }

      if (
        previousElement.caseDetails[sortBy] >
        nextElement.caseDetails[sortBy]
      ) {
        return 1;
      }
      if (
        previousElement.caseDetails[sortBy] <
        nextElement.caseDetails[sortBy]
      ) {
        return -1;
      }
    });

    return caseGroups;
  }


  private resetPageNumber() {
    this.pageNumber = 1;
  }

  private resetScroll() {
    if (this.resourcesGantt) {
      this.resetPageNumber();

      // Parent Container
      this.resourcesGantt.ganttContainer.nativeElement.scrollTo(0, 0);

      // Virtual Scroll on Employees Tab
      if (this.resourcesGantt.viewport) {
        this.resourcesGantt.viewport.scrollToIndex(0, 'smooth');
      }
    }
  }

  // -------------------------------------Helper Functions----------------------------------//
  deleteSavedFilterHandler(filterIdToDelete: string) {
    this.deleteSavedFilter(filterIdToDelete);
  }

  private refreshCaseAndResourceOverlay() {
    if (this.projectDialogRef &&
      this.projectDialogRef.componentInstance &&
      this.projectDialogRef.componentInstance.project &&
      Object.keys(this.projectDialogRef.componentInstance.project.projectDetails).length > 0) {
      const projectData = this.projectDialogRef.componentInstance.project.projectDetails;
      this.projectDialogRef.componentInstance.getProjectDetails(projectData);
    }

    if (this.dialogRef && this.dialogRef.componentInstance) {
      const employeeCode = this.dialogRef.componentInstance.data.employeeCode;
      this.dialogRef.componentInstance.getDetailsForResource(employeeCode);
    }
  }



  private subscibeToUserPreferences() {
    // this is done so that whenever user changes their user settings, it reflects in the projects and resources data
    this.coreService.getCombinedUserPreferences()
      .pipe(takeUntil(this.destroy$))
      .subscribe((combinedUserPreferences: CombinedUserPreferences) => {
        this.userPreferences = combinedUserPreferences.userPreferences;
        this.supplyGroupPreferences = combinedUserPreferences.userPreferenceSupplyGroups;
        this.getDataBasedOnDefaultSettings();
      });
  }

  setDefaultSelectedFilter() {
    const selectedDisplayingFilter =
      this.savedResourceFilters.find(x => x.isDefault) ||
      this.supplyGroupPreferences.find(x => x.isDefaultForResourcesTab || x.isDefault) ||
      this.userPreferences;

    this.setCurrentSelectedSortAndFilter(selectedDisplayingFilter);
  }

  setCurrentSelectedSortAndFilter(currentSelectedFilter: ResourceFilter | UserPreferences | UserPreferenceSupplyGroupViewModel) {
    const sortByString = currentSelectedFilter["resourcesTabSortBy"] || currentSelectedFilter["sortBy"] || '';
    this.currentSelectedDisplayingFilter = currentSelectedFilter;

    this.currentSelectedDisplayingSortAndFilterBy.sortRows = this.getSortByArray(sortByString);
    this.currentSelectedDisplayingSortAndFilterBy.filterRows = currentSelectedFilter["filterBy"] ?? [];
  }

  getResourcesByDefaultSettings() {
    this.setDefaultSelectedFilter();

    if (this.savedResourceFilters.some(x => x.isDefault)) {
      this.getActiveResources(this.supplyFilterCriteriaObj);
    } else if (this.supplyGroupPreferences.some(x => x.isDefaultForResourcesTab || x.isDefault)) {
      this.getResourcesByGroup(this.supplyGroupFilterCriteriaObj);
    } else {
      this.getActiveResources(this.supplyFilterCriteriaObj);
    }
  }

  loadViewingFilterDropdown() {
    let savedFilters: DropdownFilterOption[] = this.savedResourceFilters?.map((data: ResourceFilter) => {
      return {
        text: data.title,
        value: data.id,
        filterGroupId: ResourcesSupplyFilterGroupEnum.SAVED_GROUP,
        id: data.id,
        selected: false,
        isDefault: false,
        isDefaultForResourcesTab: false
      };
    }).sort((a, b) => a.text.localeCompare(b.text));


    let supplygroups: DropdownFilterOption[] = this.supplyGroupPreferences?.map((data: UserPreferenceSupplyGroupViewModel) => {
      return {
        text: data.name,
        value: data.groupMembers.map(x => x.employeeCode).join(","),
        filterGroupId: ResourcesSupplyFilterGroupEnum.CUSTOM_GROUP,
        id: data.id,
        selected: false,
        isDefault: false,
        isDefaultForResourcesTab: false
      };
    }).sort((a, b) => a.text.localeCompare(b.text));

    let staffingSettings: DropdownFilterOption[] = [
      {
        text: "Staffing Settings",
        value: "Staffing Settings Value",
        filterGroupId: ResourcesSupplyFilterGroupEnum.STAFFING_SETTINGS,
        id: ResourcesSupplyFilterGroupEnum.STAFFING_SETTINGS,
        selected: false,
        isDefault: false,
        isDefaultForResourcesTab: false
      }];

    this.allSupplyDropdownOptions = [...staffingSettings, ...supplygroups, ...savedFilters];

    //setting default based on precedence
    if (this.supplyGroupPreferences.some(x => x.isDefaultForResourcesTab)) {
      const defaultSupplyGrpPreference: UserPreferenceSupplyGroupViewModel = this.supplyGroupPreferences.find(x => x.isDefaultForResourcesTab)

      this.allSupplyDropdownOptions.find(x => x.id == defaultSupplyGrpPreference.id).selected = true;
      this.allSupplyDropdownOptions.find(x => x.id == defaultSupplyGrpPreference.id).isDefault = true;
    }
    else if (this.savedResourceFilters.find(x => x.isDefault)) {
      const defaultSavedFilter: ResourceFilter = this.savedResourceFilters.find(x => x.isDefault);
      this.allSupplyDropdownOptions.find(x => x.id == defaultSavedFilter.id).selected = true;
      this.allSupplyDropdownOptions.find(x => x.id == defaultSavedFilter.id).isDefault = true;
    }
    else if (this.supplyGroupPreferences.some(x => x.isDefault)) {
      const defaultSupplyGrpPreference: UserPreferenceSupplyGroupViewModel = this.supplyGroupPreferences.find(x => x.isDefault);

      this.allSupplyDropdownOptions.find(x => x.id == defaultSupplyGrpPreference.id).selected = true;
      this.allSupplyDropdownOptions.find(x => x.id == defaultSupplyGrpPreference.id).isDefault = true;
    }
    else {
      this.allSupplyDropdownOptions.find(x => x.id == ResourcesSupplyFilterGroupEnum.STAFFING_SETTINGS).selected = true;
      this.allSupplyDropdownOptions.find(x => x.id == ResourcesSupplyFilterGroupEnum.STAFFING_SETTINGS).isDefault = true;
    }
  }

  private getLookupListFromLocalStorage() {
    this.commitmentTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.commitmentTypes);
    //This is sprecific to Resources tab. Hence adding it here.
    this.investmentCategories = this.localStorageService.get(ConstantsMaster.localStorageKeys.investmentCategories);
    this.caseRoleTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.caseRoleTypes);

    //TODO: Hard coding for now to proivde EMEA practice staffing users access to only EMEA data.
    //DELETE once integrated multiple-role based office security is implemented
    if (this.coreService.loggedInUserClaims.Roles?.includes('PracticeStaffing')) {
      this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.accessibleOfficeHierarchyForUser);
    } else {
      this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.officeHierarchy);
    }

    this.officeFlatList = this.localStorageService.get(ConstantsMaster.localStorageKeys.OfficeList);
    this.levelGrades = this.localStorageService.get(ConstantsMaster.localStorageKeys.levelGradesHierarchy);
    this.staffingTagsHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffingTagsHierarchy);
    this.staffingTags = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffingTags);
    this.certificates = this.localStorageService.get(ConstantsMaster.localStorageKeys.certificates);
    this.languages = this.localStorageService.get(ConstantsMaster.localStorageKeys.languages);
    this.practiceAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.practiceAreas);
    this.affiliationRoles = this.localStorageService.get(ConstantsMaster.localStorageKeys.affiliationRoles);
    this.staffableAsTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffableAsTypes);
    this.positionsHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.positionsHierarchy);
    this.sortsBy = ConstantsMaster.sortBy;
  }

  //  -------------------------------Overlay-------------------------------//
  openCaseDetailsDialogHandler(event) {
    if (event.planningCardId) {
      this.openPlanningCardOverlay(event.planningCardId);
    }
    else {
      this.openProjectDetailsDialogHandler(event);
    }
  }

  openResourceDetailsDialogHandler(employeeCode) {
    // if (this.createTeamDialogRef != null) {
    //   this.createTeamDialogRef.close();
    // }

    // close previous resource dialog & open new dialog
    if (this.dialogRef) {
      this.dialogRef.close('no null');
      this.dialogRef = null;
    }

    if (this.dialogRef != null) {
      this.storeSub.add(this.dialogRef.beforeClosed().subscribe((result) => {
        if (result !== 'no null') {
          this.dialogRef = null;
        }
      }));
    }

    if (this.dialogRef == null) {
      this.dialogRef = this.dialog.open(ResourceOverlayComponent, {
        closeOnNavigation: true,
        hasBackdrop: false,
        enterAnimationDuration: 0,
        data: {
          employeeCode: employeeCode,
          investmentCategories: this.investmentCategories,
          caseRoleTypes: this.caseRoleTypes,
          showOverlay: true
        }
      });
    }



    this.storeSub.add(this.dialogRef.componentInstance.openResourceDetailsFromProjectDialog.subscribe(empCode => {
      this.openResourceDetailsDialogHandler(empCode);
    }));

    // Listens for click on case name for opening the project details pop-up
    this.storeSub.add(this.dialogRef.componentInstance.openProjectDetailsFromResourceDialog.subscribe(projectData => {
      this.openProjectDetailsDialogHandler(projectData);
    }));

    // Listens for click on notes opening the ag-grid notes pop-up
    this.storeSub.add(this.dialogRef.componentInstance.openNotesDialog.subscribe(projectData => {
      this.openNotesDialogHandler(projectData);
    }));

    // Listens for click on split allocation in context menu of ag-grid
    this.storeSub.add(this.dialogRef.componentInstance.openSplitAllocationDialog.subscribe(projectData => {
      this.openSplitAllocationDialogHandler(projectData);
    }));

    this.storeSub.add(this.dialogRef.componentInstance.updateResourceCommitment.subscribe(updatedCommitment => {
      this.updateResourceCommitmentHandler(updatedCommitment.resourceAllocation);
    }));

    // this.dialogRef.componentInstance.deleteResourceCommitment.subscribe(deletedObj => {
    //   this.deleteResourceCommitmentHandler(deletedObj);
    // });

    // inserts & updates resource data when changes are made to resource
    this.storeSub.add(this.dialogRef.componentInstance.upsertResourceAllocationsToProject.subscribe(updatedData => {
      this.upsertResourceAllocationsToProjectHandler(updatedData.resourceAllocation, updatedData.splitSuccessMessage);
    }));

    this.storeSub.add(this.dialogRef.componentInstance.deleteResourceAllocationFromCase.subscribe(allocation => {
      this.deleteResourceAssignmentFromProjectHandler(allocation.allocationId);
      this.resourceAssignmentService.deletePlaygroundAllocationsForCasePlanningMetrics(allocation.resourceAllocation);
    }));

    this.storeSub.add(this.dialogRef.componentInstance.deleteResourceAllocationFromCases.subscribe(allocation => {
      this.deleteResourcesAssignmentsFromProjectHandler(allocation.allocationIds);
      this.resourceAssignmentService.deletePlaygroundAllocationsForCasePlanningMetrics(allocation.resourceAllocation);
    }));

    this.storeSub.add(this.dialogRef.componentInstance.deleteResourceAllocationsCommitmentsFromCase.subscribe(dataToDelete => {
      this.deleteResourcesAllocationsCommitments(dataToDelete);
      this.resourceAssignmentService.deletePlaygroundAllocationsForCasePlanningMetrics(dataToDelete.resourceAllocation);
    }));

    this.storeSub.add(this.dialogRef.componentInstance.openQuickAddForm.subscribe(event => {
      this.openQuickAddFormHandler(event);
    }));

    this.dialogRef.componentInstance.upsertStaffableAsRoleEmitter.subscribe(event => {
      this.upsertStaffableAsRole(event);
    });

    this.dialogRef.componentInstance.deleteStaffableAsRoleEmitter.subscribe(event => {
      this.deleteStaffableAsRole(event);
    });

    //// updates resource data when changes are made to resource
    // this.dialogRef.componentInstance.updateResourceDataForProject.subscribe(updatedData => {
    //  this.resourceAssignmentService.updateResourceAssignmentToCase(updatedData, this.dialogRef, this.projectDialogRef);
    // });

    // this.storeSub.add(this.dialogRef.beforeClosed().subscribe((result) => {
    //   if (result !== 'no null') {
    //     this.dialogRef = null;
    //   }
    // }));

  }

  openPlanningCardOverlay(planningCardId) {
    this.overlayDialogService.openPlanningCardDetailsDialogHandler(planningCardId);
  }

  openProjectDetailsDialogHandler(projectData) {
    // if (this.createTeamDialogRef != null) {
    //   this.createTeamDialogRef.close();
    // }

    // close previous project dialog & open new dialog
    if (this.projectDialogRef) {
      this.projectDialogRef.close('no null');
      this.projectDialogRef = null
    }

    this.storeSub.add(this.projectDialogRef?.beforeClosed().subscribe(result => {
      if (result !== 'no null') {
        this.projectDialogRef = null;
      }
    }));

    if (this.projectDialogRef == null) {
      this.projectDialogRef = this.dialog.open(ProjectOverlayComponent, {
        closeOnNavigation: true,
        hasBackdrop: false,
        enterAnimationDuration: 0,
        data: {
          projectData: projectData,
          investmentCategories: this.investmentCategories,
          caseRoleTypes: this.caseRoleTypes,
          showDialog: true
        }
      });
    }

    // Listens for click on resource name for opening the resource details pop-up
    this.storeSub.add(this.projectDialogRef.componentInstance.openResourceDetailsFromProjectDialog.subscribe(employeeCode => {
      this.openResourceDetailsDialogHandler(employeeCode);
    }));



    //// updates resource data when changes are made to resource
    // this.projectDialogRef.componentInstance.updateResourceAssignmentToProject.subscribe(updatedData => {
    //  this.resourceAssignmentService.updateResourceAssignmentToCase(updatedData, this.dialogRef, this.projectDialogRef);
    // });

    // Listens for click on notes opening the ag-grid notes pop-up
    this.storeSub.add(this.projectDialogRef.componentInstance.openNotesDialog.subscribe(projectData => {
      this.openNotesDialogHandler(projectData);
    }));

    // Listens for click on split allocation in context menu of ag-grid
    this.storeSub.add(this.projectDialogRef.componentInstance.openSplitAllocationDialog.subscribe(projectData => {
      this.openSplitAllocationDialogHandler(projectData);
    }));

    // inserts & updates resource data when changes are made to resource
    this.storeSub.add(this.projectDialogRef.componentInstance.upsertResourceAllocationsToProject.subscribe(updatedData => {
      this.upsertResourceAllocationsToProjectHandler(updatedData.resourceAllocation);
    }));

    // inserts & updates placeholder data when changes are made to placeholder
    this.storeSub.add(this.projectDialogRef.componentInstance.upsertPlaceholderAllocationsToProject.subscribe(updatedData => {
      this.placeholderAssignmentService.upsertPlcaeholderAllocations(updatedData, null, this.projectDialogRef);
    }));

    // deletes resources data
    this.storeSub.add(this.projectDialogRef.componentInstance.deleteResourcesFromProject.subscribe(updatedData => {
      //this.deleteResourcesAssignmentsFromProjectHandler(updatedData.allocationIds);
      //deletes allocations/commitments related to a resource
      this.deleteResourcesAllocationsCommitments(updatedData);
      this.resourceAssignmentService.deletePlaygroundAllocationsForCasePlanningMetrics(updatedData.resourceAllocation);
    }));

    //updates made to commitments from project overlay
    this.storeSub.add(this.projectDialogRef.componentInstance.updateResourceCommitment.subscribe(updatedCommitment => {
      this.updateResourceCommitmentHandler(updatedCommitment.resourceAllocation);
    }));

    // opens add resource popup
    this.storeSub.add(this.projectDialogRef.componentInstance.openQuickAddForm.subscribe(projectData => {
      this.openQuickAddFormHandler(projectData);
    }));



    this.storeSub.add(this.projectDialogRef.componentInstance.openPlaceholderForm.subscribe(projectData => {
      this.openPlaceholderFormHandler(projectData);
    }));

    // insert sku case term
    this.storeSub.add(this.projectDialogRef.componentInstance.insertSkuCaseTermsForProject.subscribe(skuTab => {
      this.skuCaseTermService.insertSKUCaseTerms(skuTab, this.dialogRef, this.projectDialogRef);
    }));

    // update sku case term
    this.storeSub.add(this.projectDialogRef.componentInstance.updateSkuCaseTermsForProject.subscribe(skuTab => {
      this.skuCaseTermService.updateSKUCaseTerms(skuTab, this.dialogRef, this.projectDialogRef);
    }));

    // delete sku case term
    this.storeSub.add(this.projectDialogRef.componentInstance.deleteSkuCaseTermsForProject.subscribe(skuTab => {
      this.skuCaseTermService.deleteSKUCaseTerms(skuTab, this.dialogRef, this.projectDialogRef);
    }));

    // add project to user settings show list
    this.storeSub.add(this.projectDialogRef.componentInstance.addProjectToUserExceptionShowList.subscribe(event => {
      this.userpreferencesService.addCaseOpportunityToUserExceptionShowList(event);
    }));

    // remove project to user settings show list
    this.storeSub.add(this.projectDialogRef.componentInstance.removeProjectFromUserExceptionShowList.subscribe(event => {
      this.userpreferencesService.removeCaseOpportunityFromUserExceptionShowList(event);
    }));

    // add project to user settings hide list
    this.storeSub.add(this.projectDialogRef.componentInstance.addProjectToUserExceptionHideList.subscribe(event => {
      this.userpreferencesService.addCaseOpportunityToUserExceptionHideList(event);
    }));

    // remove project from user settings hide list
    this.storeSub.add(this.projectDialogRef.componentInstance.removeProjectFromUserExceptionHideList.subscribe(event => {
      this.userpreferencesService.removeCaseOpportunityFromUserExceptionHideList(event, true);
    }));

    // open case roll pop-up
    this.storeSub.add(this.projectDialogRef.componentInstance.openCaseRollForm.subscribe(event => {
      this.openCaseRollFormHandler(event);
    }));

    // update pipeline data in staffing db
    this.storeSub.add(this.projectDialogRef.componentInstance.updateProjectChanges.subscribe(event => {
      this.opportunityService.updateProjectChangesHandler(event, this.projectDialogRef);
      this.getActiveResources(this.supplyFilterCriteriaObj);
    }));

    this.storeSub.add(this.projectDialogRef.componentInstance.openStaCommitmentForm.subscribe(event => {
      this.staCommitmentDialogService.projectDialogRef = this.projectDialogRef;
      this.staCommitmentDialogService.openSTACommitmentFormHandler(event);
    }));

    // this.storeSub.add(this.projectDialogRef.beforeClosed().subscribe(result => {
    //   if (result !== 'no null') {
    //     this.projectDialogRef = null;
    //   }
    // }));
    // } else {
    //   this.notifyService.showValidationMsg('Opening multiple cases/opporutnities not allowed.');
    // }
  }

  private openNotesDialogHandler(event) {
    // class is required to center align the modal on large screens
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        projectData: event.projectData,
        popupType: event.popupType
      }
    };
    this.bsModalRef = this.modalService.show(AgGridNotesComponent, config);

    // inserts & updates resource data when changes are made to notes of an allocation
    this.storeSub.add(this.bsModalRef.content.updateNotesForAllocation.subscribe(updatedData => {
      this.upsertResourceAllocationsToProjectHandler(updatedData.resourceAllocation);
    }));
  }

  // opens from - resource-overlay.component, project-overlay.component
  openSplitAllocationDialogHandler(event) {
    // check if the popup is already open
    // if (!this.bsModalRef) {
    // class is required to center align the modal on large screens
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        allocationData: event.allocationData,
        popupType: event.popupType
      }
    };
    this.bsModalRef = this.modalService.show(AgGridSplitAllocationPopUpComponent, config);

    // inserts & updates resource data when changes are made to notes of an allocation
    this.storeSub.add(this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe(upsertData => {
      this.upsertResourceAllocationsToProjectHandler(upsertData.resourceAllocation);
    }));
    this.storeSub.add(this.bsModalRef.content.upsertPlaceholderAllocationsToProject.subscribe((data) => {
      this.upsertPlaceholderAllocationsToProjectHandler(data);
    }));


    // clear bsModalRef value on closing modal
    this.storeSub.add(this.modalService.onHidden.subscribe(() => {
      this.bsModalRef = null;
    }));
    // }
  }


  private openCaseRollFormHandler(event) {
    // class is required to center align the modal on large screens
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        project: event.project
      }
    };
    this.bsModalRef = this.modalService.show(CaseRollFormComponent, config);

    this.storeSub.add(this.bsModalRef.content.upsertCaseRollAndAllocations.subscribe(response => {
      this.caseRollService.upsertCaseRollAndAllocationsHandler(response.caseRoll, response.resourceAllocations, this.projectDialogRef);
    }));

    this.storeSub.add(this.bsModalRef.content.revertCaseRollAndAllocations.subscribe(response => {
      this.caseRollService.revertCaseRollAndAllocationsHandler(response.caseRoll, response.resourceAllocations, this.projectDialogRef);
    }));
    this.storeSub.add(this.bsModalRef.content.upsertCaseRollAndPlaceholderAllocations.subscribe(response => {
      this.caseRollService.upsertCaseRollAndPlaceholderAllocationsHandler(response.caseRoll, response.resourceAllocations, this.projectDialogRef);
    }));
  }

  // ------------------Pop-Ups -----------------------------------------------------------
  public openInfoModal(data) {

    this.modalService.show(InfoIconModalComponent, {
      animated: true,
      backdrop: true,
      ignoreBackdropClick: false,
      initialState: {
      },
    });
  }

  public openQuickAddFormHandler(modalData) {
    // class is required to center align the modal on large screens
    if (modalData) {

      const config = {
        class: 'modal-dialog-centered',
        ignoreBackdropClick: true,
        initialState: {
          commitmentTypeCode: modalData.commitmentTypeCode,
          commitmentData: modalData.commitmentData,
          resourceAllocationData: modalData.resourceAllocationData,
          isUpdateModal: modalData.isUpdateModal
        }
      };
      this.bsModalRef = this.modalService.show(QuickAddFormComponent, config);

    } else {

      const config = {
        class: 'modal-dialog-centered',
        ignoreBackdropClick: true
      };

      this.bsModalRef = this.modalService.show(QuickAddFormComponent, config);

    }

    // TODO: Same functionality is available in QuickAddDialog.Service file
    // There should be one place to register all emitter events to prevent functionality split
    // from different parts of the screen like
    // open overlay from home vs resource tab
    this.storeSub.add(this.bsModalRef.content.insertResourcesCommitments.subscribe(
      (commitment) => {
        this.addResourceCommitmentHandler(commitment);
      }));

    this.storeSub.add(this.bsModalRef.content.updateResourceCommitment.subscribe(updatedCommitment => {
      this.updateResourceCommitmentHandler(updatedCommitment.resourceAllocation);
    }));


    this.storeSub.add(this.bsModalRef.content.deleteResourceCommitment.subscribe(deletedObjId => {
      this.deleteResourceCommitmentHandler(deletedObjId);
    }));

    // inserts & updates resource data when changes are made to resource
    this.storeSub.add(this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe(
      (upsertedAllocations) => {
        this.upsertResourceAllocationsToProjectHandler(
          upsertedAllocations.resourceAllocation, upsertedAllocations.splitSuccessMessage,
          upsertedAllocations.allocationDataBeforeSplitting
        );
      }
    ));

    // inserts & updates placeholder data when changes are made to resource
    this.storeSub.add(this.bsModalRef.content.upsertPlaceholderAllocationsToProject.subscribe(
      (upsertedAllocations) => {
        this.upsertPlaceholderAllocationsToProjectHandler(
          upsertedAllocations
        );
      }
    ));

    this.storeSub.add(this.bsModalRef.content.deletePlaceholderAllocationByIds.subscribe(
      (placeholderIds) => {
        this.dispatchDeleteResourcePlaceholderAllocation(
          placeholderIds
        );
      }
    ));

    this.storeSub.add(this.bsModalRef.content.deleteResourceAllocationFromCase.subscribe(
      (allocation) => {
        this.deleteResourceAssignmentFromProjectHandler(allocation.allocationId);
        this.resourceAssignmentService.deletePlaygroundAllocationsForCasePlanningMetrics(allocation.resourceAllocation);
      }
    ));

    this.storeSub.add(this.bsModalRef.content.openBackFillPopUp.subscribe((result) => {
      this.openBackFillFormHandler(result);
    }));

    this.storeSub.add(this.bsModalRef.content.openOverlappedTeamsPopup.subscribe((result) => {
      this.openOverlappedTeamsFormHandler(result);
    }));
  }

  public openPlaceholderFormHandler(modalData) {
    // class is required to center align the modal on large screens
    let initialState = null;

    if (modalData) {

      initialState = {
        projectData: modalData.project,
        placeholderAllocationData: modalData.placeholderAllocationData,
        isUpdateModal: modalData.isUpdateModal
      };

    }

    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: initialState
    };

    this.bsModalRef = this.modalService.show(PlaceholderFormComponent, config);

    // inserts & updates resource data when changes are made to resource
    this.bsModalRef.content.upsertPlaceholderAllocationsToProject.subscribe(updatedCommitment => {
      this.placeholderAssignmentService.upsertPlcaeholderAllocations(updatedCommitment, null, this.projectDialogRef);
    });

    this.bsModalRef.content.deletePlaceholderAllocationByIds.subscribe(event => {
      this.placeholderAssignmentService.deletePlaceHoldersByIds(event, this.projectDialogRef);
    });

    this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe(event => {
      this.resourceAssignmentService.upsertResourceAllocationsToProject(event, null, this.projectDialogRef);
      this.resourceAssignmentService.upsertPlaygroundAllocationsForCasePlanningMetrics(event.resourceAllocation);
    });

    this.bsModalRef.content.openBackFillPopUp.subscribe(result => {
      this.backfillDialogService.projectDialogRef = this.projectDialogRef;
      this.backfillDialogService.openBackFillFormHandler(result);
    });
  }

  private openBackFillFormHandler(event) {
    // class is required to center align the modal on large screens
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        project: event.project,
        resourceAllocation: event.resourceAllocation,
        showMoreThanYearWarning: event.showMoreThanYearWarning,
        allocationDataBeforeSplitting: event.allocationDataBeforeSplitting
      },
    };
    this.bsModalRef = this.modalService.show(BackfillFormComponent, config);

    this.storeSub.add(this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe((data) => {
      event.project.allocatedResources = event.project.allocatedResources.concat(
        data.resourceAllocation
      );
      this.upsertResourceAllocationsToProjectHandler(data.resourceAllocation, null, data.allocationDataBeforeSplitting);
    }));
  }


  private openOverlappedTeamsFormHandler(event) {
    // class is required to center align the modal on large screens
    const config = {
      class: 'custom-modal-large modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        projectData: event.projectData,
        overlappedTeams: event.overlappedTeams,
        allocation: event.allocation
      },
    };
    this.bsModalRef = this.modalService.show(OverlappedTeamsFormComponent, config);

    this.storeSub.add(this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe((data) => {
      this.upsertResourceAllocationsToProjectHandler(data.resourceAllocation);
    }));

  }

  openSplitAllocationPopupHandler(event) {
    this.openSplitAllocationDialogHandler(event);
  }

  openOverlappedTeamsPopupHandler(event) {
    this.openOverlappedTeamsFormHandler(event)
  }

  updateThresholdRangeHandler(event) {
    this.thresholdRangeValue = event
  }

  upsertResourceFilterHandler(resourceFiltersData: ResourceFilter[]) {
    this.dispatchResourcesLoaderAction(true);

    this.store.dispatch(
      new resourcesActions.UpsertResourceFilters(resourceFiltersData)
    );
  }

  upsertUserPreferencesSupplyGroupHandler(supplyGroupDataToUpsert: UserPreferenceSupplyGroup) {
    this.userpreferencesService.upsertUserPreferencesSupplyGroupsWithSharedInfo([].concat(supplyGroupDataToUpsert));
    //the upserted data is subscribed to in refreshUserPreferencesSupplyGroups subscription;
  }

  deleteSupplyGroupsRefreshHandler(groupIds: string[]) {
    if (groupIds?.length) {
      this.supplyGroupPreferences = this.supplyGroupPreferences.filter(x => !groupIds.includes(x.id));
      this.coreService.setUserPreferenceSupplyGroupsInLocalStorage(this.supplyGroupPreferences)
      this.getDataBasedOnDefaultSettings();
    }
  }

  onSupplyGroupFilterChangedHandler(selectedSupplyDropdownId) {
    let selectedFilterObj = this.allSupplyDropdownOptions.find(x => x.id == selectedSupplyDropdownId);
    selectedFilterObj.selected = true;
    // this.selectedFilterName = selectedFilterObj.text;
    this.isSearchStringExist = false;
    this.clearEmployeeSearch.next(true);

    switch (selectedFilterObj.filterGroupId) {
      case ResourcesSupplyFilterGroupEnum.CUSTOM_GROUP: {
        this.setCurrentSelectedSortAndFilter(this.supplyGroupPreferences.find(x => x.id === selectedFilterObj.id));
        this.supplyGroupFilterCriteriaObj.employeeCodes = selectedFilterObj.value;
        this.updateSupplyFilterCriteriaForCustomGroup(this.supplyGroupPreferences.find(x => x.id === selectedFilterObj.id));
        this.getResourcesByGroup(this.supplyGroupFilterCriteriaObj);
        break;
      }

      case ResourcesSupplyFilterGroupEnum.SAVED_GROUP: {
        const currentSelectedDisplayingFilter = this.savedResourceFilters.find(x => x.id == selectedFilterObj.id);
        this.setCurrentSelectedSortAndFilter(currentSelectedDisplayingFilter);

        this.updateSupplyFilterCriteriaForSavedFilters(currentSelectedDisplayingFilter);
        this.getActiveResources(this.supplyFilterCriteriaObj);

        break;
      }

      case ResourcesSupplyFilterGroupEnum.STAFFING_SETTINGS: {
        this.setCurrentSelectedSortAndFilter(this.userPreferences);
        this.updateSupplyFilterCriteriaForUserPreferences(this.userPreferences);
        this.getActiveResources(this.supplyFilterCriteriaObj);

        break;
      }

    }

    this.resetScroll();
  }

  getSortByArray(sortBy = '') {
    let sortRows: SortRow[] = [];
    sortBy.split(',').forEach(x => {
      const option: SortRow = {
        field: x.split('|')[0],
        direction: x.split('|')[1] ?? 'asc'
      }
      sortRows.push(option);
    });
    return sortRows;
  }

  openCustomGroupModalHandler(event) {
    const isEditMode: boolean = event.isEditMode;
    const groupToEdit: UserPreferenceSupplyGroup | UserPreferenceSupplyGroupViewModel = event.groupToEdit;
    const modalRef = this.modalService.show(CustomGroupModalComponent, {
      backdrop: false,
      class: "custom-group-modal modal-dialog-centered",
      initialState: {
        isEditMode: isEditMode,
        groupToEdit: groupToEdit,
      }
    });

    modalRef.content.upsertUserPreferenceSupplyGroups.subscribe((upsertedData: UserPreferenceSupplyGroup) => {
      this.upsertUserPreferencesSupplyGroupHandler(upsertedData);
    });
  }


  // -------------------------------------Action Dispatchers----------------------------------//
  private dispatchUpsertResourceAction(upsertedAllocations, splitSuccessMessage?, allocationDataBeforeSplitting?) {
    this.store.dispatch(
      new resourcesActions.UpsertResourceStaffing({ upsertedAllocations, splitSuccessMessage, allocationDataBeforeSplitting })
    );
  }

  private dispatchUpsertPlaceholderAction(upsertedAllocations) {
    this.store.dispatch(
      new resourcesActions.UpsertPlaceholderStaffing({ upsertedAllocations })
    );
  }

  private dispatchDeleteResourcePlaceholderAllocation(placeholderIds) {
    this.store.dispatch(
      new resourcesActions.DeleteResourcePlaceholderAllocation(placeholderIds)
    );
  }
  private dispatchUpsertResourceViewNoteAction(resourceViewNote) {
    this.store.dispatch(
      new resourcesActions.UpsertResourceViewNote(resourceViewNote)
    );
  }

  private dispatchDeleteResourceViewNotesAction(idsToBeDeleted) {
    this.store.dispatch(
      new resourcesActions.DeleteResourceViewNotes(idsToBeDeleted)
    );
  }
  private dispatchUpsertResourceRecentCDAction(resourceViewNote) {
    this.store.dispatch(
      new resourcesActions.UpsertResourceViewCD(resourceViewNote)
    );
  }
  private dispatchUpsertResourceCommercialModelAction(resourceViewCommercialModel) {
    this.store.dispatch(
      new resourcesActions.UpsertResourceViewCommercialModel(resourceViewCommercialModel)
    );
  }

  private dispatchDeleteResourceRecentCDAction(idsToBeDeleted) {
    this.store.dispatch(
      new resourcesActions.DeleteResourceViewCD(idsToBeDeleted)
    );
  }
  private dispatchDeleteResourceCommercialModelAction(idsToBeDeleted) {
    this.store.dispatch(
      new resourcesActions.DeleteResourceViewCommercialModel(idsToBeDeleted)
    );
  }

  private dispatchUpdateResourceAction(resourceAllocation) {
    this.store.dispatch(
      new resourcesActions.UpdateResource(resourceAllocation)
    );
  }

  private dispatchAddResourceCommitmentAction(resourceCommitment) {
    this.store.dispatch(
      new resourcesActions.AddResourceCommitment(resourceCommitment)
    );
  }

  private dispatchUpdateResourceCommitmentAction(updatedResourceCommitment) {
    this.store.dispatch(
      new resourcesActions.UpdateResourceCommitment([].concat(updatedResourceCommitment))
    );
  }

  private dispatchDeleteResourceCommitmentAction(deletedCommitmentId) {
    this.store.dispatch(
      new resourcesActions.DeleteResourceCommitment(deletedCommitmentId)
    );
  }

  private dispatchRefreshCaseAndResourceOverlayAction(refreshNeeded) {
    this.store.dispatch(
      new resourcesActions.RefreshCaseAndResourceOverlay(refreshNeeded)
    );
  }

  private dispatchRefreshLastBillableDateAction(refreshNeeded) {
    this.store.dispatch(
      new resourcesActions.RefreshLastBillableDate(refreshNeeded)
    );
  }

  private upsertStaffableAsRole(staffableAsRole) {
    this.dispatchResourcesLoaderAction(true);
    this.store.dispatch(
      new resourcesActions.UpsertResourceStaffableAsRole(staffableAsRole)
    );
  }

  private deleteStaffableAsRole(staffableAsRole) {
    this.dispatchResourcesLoaderAction(true);
    this.store.dispatch(
      new resourcesActions.DeleteResourceStaffableAsRole(staffableAsRole)
    );
  }

  private dispatchResourcesLoaderAction(isShowProgressBar) {
    this.showProgressBar = isShowProgressBar;
    this.store.dispatch(
      new resourcesActions.ResourcesLoader(true)
    );
  }

  private refreshLastBillableDate(employeeCodes: string) {
    this.dispatchResourcesLoaderAction(true);

    this.store.dispatch(
      new resourcesActions.LoadLastBillableDateForResources({ employeeCodes })
    );
  }

  private getSavedFiltersForLoggedInResource() {
    this.dispatchResourcesLoaderAction(true);

    this.store.dispatch(
      new resourcesActions.LoadSavedResourceFilters({})
    );
  }

  private deleteSavedFilter(filterIdToDelete: string) {

    this.store.dispatch(
      new resourcesActions.DeleteSavedResourceFilter(filterIdToDelete)
    );
  }

  clearSearchData() {
    this.isSearchStringExist = false;
    // dispatch action to clear search data from store
    this.store.dispatch(
      new resourcesActions.ClearSearchData()
    );
  }

  private deleteResourceAssignmentFromProjectHandler(deletedObjId) {
    this.store.dispatch(
      new resourcesActions.DeleteResourceStaffing(deletedObjId)
    );
  }

  private deleteResourcesAssignmentsFromProjectHandler(deletedObjIds) {
    this.store.dispatch(
      new resourcesActions.DeleteResourcesStaffing(deletedObjIds)
    );
  }

  private deleteResourcesAllocationsCommitments(dataToDelete) {
    this.store.dispatch(
      new resourcesActions.DeleteAllocationsCommitmentsStaffing(dataToDelete)
    );
  }


}
