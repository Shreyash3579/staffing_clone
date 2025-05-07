// -------------------Angular References---------------------------------------//
import { Component, OnInit, ViewChild } from '@angular/core';
import { select, Store } from '@ngrx/store';
import { Subject, Subscription } from 'rxjs';

// -------------------Interfaces---------------------------------------//
import { PlanningCard } from '../shared/interfaces/planningCard.interface';
import { OfficeHierarchy } from '../shared/interfaces/officeHierarchy.interface';
import { CaseType } from '../shared/interfaces/caseType.interface';
import { KeyValue } from '../shared/interfaces/keyValue.interface';
import { ServiceLine } from '../shared/interfaces/serviceLine.interface';
import { OpportunityStatusType } from '../shared/interfaces/opportunityStatusType';
import { DemandFilterCriteria } from '../shared/interfaces/demandFilterCriteria.interface';
import { Office } from '../shared/interfaces/office.interface';
import { Project } from '../shared/interfaces/project.interface';
import { UserPreferences } from '../shared/interfaces/userPreferences.interface';
import { PracticeArea } from '../shared/interfaces/practiceArea.interface';

// -------------------Components---------------------------------------//
import { ProjectsGanttComponent } from './projects-gantt/projects-gantt.component';

// -------------------Services---------------------------------------//
import { LocalStorageService } from '../shared/local-storage.service';
import { CoreService } from '../core/core.service';
import { DateService } from '../shared/dateService';
import { ResourcesCommitmentsDialogService } from '../overlay/dialogHelperService/resourcesCommitmentsDialog.service';

// -------------------Constants/Enums---------------------------------------//
import { ConstantsMaster } from '../shared/constants/constantsMaster';
import { CaseType as CaseTypeEnum, ServiceLine as ServiceLineEnum } from '../shared/constants/enumMaster';
import { GoogleAnalytics } from '../shared/google-analytics/googleAnalytics';

// --------------------------Redux Component -----------------------------------------//
import * as casePlanningActions from './State/case-planning.actions';
import * as fromProjects from './State/case-planning.reducer';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { CaseRollFormComponent } from '../shared/case-roll-form/case-roll-form.component';
import { CaseRollService } from '../overlay/behavioralSubjectService/caseRoll.service';
import { OverlayMessageService } from '../overlay/behavioralSubjectService/overlayMessage.service';
import { ShowQuickPeekDialogService } from '../overlay/dialogHelperService/show-quick-peek-dialog.service';
import { PlaceholderDialogService } from '../overlay/dialogHelperService/placeholderDialog.service';
import { SupplyFilterCriteria } from '../shared/interfaces/supplyFilterCriteria.interface';
import { ProjectOverlayComponent } from '../overlay/project-overlay/project-overlay.component';

// --------------------------utilities -----------------------------------------//
import { MatDialogRef, MatDialog } from '@angular/material/dialog';
import { ResourceOverlayComponent } from '../overlay/resource-overlay/resource-overlay.component';
import { AgGridNotesComponent } from '../overlay/ag-grid-notes/ag-grid-notes.component';
import { AgGridSplitAllocationPopUpComponent } from '../overlay/ag-grid-split-allocation-pop-up/ag-grid-split-allocation-pop-up.component';
import { QuickAddFormComponent } from '../shared/quick-add-form/quick-add-form.component';
import { BackfillFormComponent } from '../shared/backfill-form/backfill-form.component';
import { PlaceholderAssignmentService } from '../overlay/behavioralSubjectService/placeholderAssignment.service';
import { PlaceholderFormComponent } from '../shared/placeholder-form/placeholder-form.component';
import { ResourceAssignmentService } from '../overlay/behavioralSubjectService/resourceAssignment.service';
import { BackfillDialogService } from '../overlay/dialogHelperService/backFillDialog.service';
import { SkuCaseTermService } from '../overlay/behavioralSubjectService/skuCaseTerm.service';
import { UserPreferenceService } from '../overlay/behavioralSubjectService/userPreference.service';
import { OpportunityService } from '../overlay/behavioralSubjectService/opportunity.service';
import { AddTeamSkuComponent } from './add-team-sku/add-team-sku.component';
import { SkuTerm } from '../shared/interfaces/skuTerm.interface';
import { PDGrade } from '../shared/interfaces/pdGrade.interface';
import { OverlappedTeamsFormComponent } from '../shared/overlapped-teams-form/overlapped-teams-form.component';
import { CommitmentType } from '../shared/interfaces/commitmentType.interface';
import { PlanningBoardModalComponent } from '../case-planning-whiteboard/planning-board-modal/planning-board-modal.component';
import { ResourceOrCasePlanningViewNote } from '../shared/interfaces/resource-or-case-planning-view-note.interface';

import * as fromResourcesGlobalStore from "../../app/state/reducers/resource-commitment.reducer";
import * as fromProjectssGlobalStore from "../../app/state/reducers/project-allocations.reducer";
import * as ResourcesGlobalStoreActions from "../../app/state/actions/resource-commitment.action";
import * as ProjectsGlobalStoreActions from "../../app/state/actions/project-allocations.action";
import { SharedNotesService } from '../core/services/shared-notes-info.service';

@Component({
  selector: 'app-case-planning',
  templateUrl: './case-planning.component.html',
  styleUrls: ['./case-planning.component.scss']
})
export class CasePlanningComponent implements OnInit {

  // ------------- Input events --------------------------- //
  bsModalRef: BsModalRef;
  dateRange: [Date, Date];
  ganttCasesData: any;
  supplyFilterCriteriaObj: SupplyFilterCriteria = {} as SupplyFilterCriteria;
  demandFilterCriteriaObj: DemandFilterCriteria = {} as DemandFilterCriteria;
  homeOffice: Office;

  @ViewChild('projectsGantt', { static: false }) projectsGantt: ProjectsGanttComponent;

  //-------------------- Local Variables ---------------------- //
  userPreferences: UserPreferences;
  officeHierarchy: OfficeHierarchy[] = [];
  caseTypes: CaseType[] = [];
  demandTypes: KeyValue[] = [];
  opportunityStatusTypes: OpportunityStatusType[] = [];
  staffingTagsHierarchy: ServiceLine[] = [];
  skuTerms: SkuTerm[] = [];
  offices: Office[];
  serviceLines: ServiceLine[];
  pdGrades: PDGrade[];
  commitmentTypes: CommitmentType[];
  industryPracticeAreas: PracticeArea[] = [];
  capabilityPracticeAreas: PracticeArea[] = [];

  private storeSub: Subscription = new Subscription();
  private overlayMessageServiceSub: Subscription = new Subscription();
  public projects: Project[];
  public planningCards: PlanningCard[];
  public pageNumber = 1;
  pageScrolled = false;
  pageSize: number;
  scrollDistance: number;
  projectStartIndex = 1;
  showProgressBar = false;
  showFilters = false;
  dialogRef: MatDialogRef<ResourceOverlayComponent, any>;
  projectDialogRef: MatDialogRef<ProjectOverlayComponent, any>;
  private isSearchStringExist = false;
  isDefaultWidgetSet = false;
  sharedNotes = [];
  caseIntakeAlerts=[];
  // ----------------------- Notifiers ------------------------------------------------//
  clearProjectSearch: Subject<boolean> = new Subject();
  searchedProjects: Subject<Project[]> = new Subject();

  //--------------------Life- Cycle Events ------------------- //

  constructor(
    private store: Store<fromProjects.State>,
    private coreService: CoreService,
    private localStorageService: LocalStorageService,
    private modalService: BsModalService,
    private placeholderDialogService: PlaceholderDialogService,
    private caseRollService: CaseRollService,
    private overlayMessageService: OverlayMessageService,
    private showQuickPeekDialogService: ShowQuickPeekDialogService,
    private placeholderAssignmentService: PlaceholderAssignmentService,
    private resourceAssignmentService: ResourceAssignmentService,
    private backfillDialogService: BackfillDialogService,
    private skuCaseTermService: SkuCaseTermService,
    private userpreferencesService: UserPreferenceService,
    private opportunityService: OpportunityService,
    public dialog: MatDialog,
    private resourcesGlobalStore: Store<fromResourcesGlobalStore.State>,
    private projectsGlobalStore: Store<fromProjectssGlobalStore.State>,
    private sharedNotesService: SharedNotesService,
    private resourcesCommitmentsDialogService: ResourcesCommitmentsDialogService,
  ) { }

  ngOnInit(): void {
    GoogleAnalytics.staffingTrackPageView(this.coreService.loggedInUser.employeeCode, ConstantsMaster.appScreens.page.casePlanning, '');
    this.homeOffice = this.coreService.loggedInUser.office;
    this.pageSize = this.coreService.appSettings.projectsPageSize;
    this.scrollDistance = this.coreService.appSettings.scrollDistance;
    this.subscribeuserPrefences();
    this.setStoreSuscriptions();
    this.subscribeEvents();
    this.getLookupListFromLocalStorage();
    this.getSharedNotesInfo(this.coreService.loggedInUser.employeeCode);
  }

  subscribeEvents() {
    // We do not need global store here as code structure is diiferent. Will implement global store when we will revamp this entire tab.

    
    // this.storeSub.add(this.resourcesGlobalStore
    //   .pipe(
    //     select(fromResourcesGlobalStore.getRefreshResources))
    //   .subscribe((refreshResources) => {
    //     if (refreshResources) {
    //       this.reloadProjects();
    //       this.resourcesGlobalStore.dispatch(
    //         new ResourcesGlobalStoreActions.ResetRefreshResourcesSuccess(false));
    //     }
    //   }))

    //   this.storeSub.add(this.projectsGlobalStore
    //     .pipe(
    //       select(fromProjectssGlobalStore.getRefreshCasesAndOpoortunities))
    //     .subscribe((refreshCasesAndOpportunities) => {
    //       if (refreshCasesAndOpportunities) {
    //         this.reloadProjects();
    //         this.projectsGlobalStore.dispatch(
    //           new ProjectsGlobalStoreActions.ResetRefreshCasesAndOpportunitiesSuccess(false));
    //       }
    //     }))
    this.overlayMessageServiceSub.add(this.overlayMessageService.refreshResources().subscribe(result => {
      if (result === true) {
        //TODO: think of a way to prevet reload of entire page when updates occur
        this.reloadProjects();
      }
    }));
    this.overlayMessageServiceSub.add(this.overlayMessageService.refreshCasesAndopportunties().subscribe(result => {
      if (result === true) {
        //TODO: think of a way to prevet reload of entire page when updates occur
        this.reloadProjects();
      }
    }));

    this.overlayMessageServiceSub.add(this.overlayMessageService.refreshPlanningCards().subscribe(result => {
      if (result === true) {
        this.reloadPlanningCards();
      }
    }));

   
  }

  private getSharedNotesInfo(loggedInEmployeeCode){
    this.sharedNotesService.getSharedNotes(loggedInEmployeeCode).subscribe(notes => {
      this.sharedNotes = notes;
    });
  }

  getProjectsBySearchStringHandler(event) {
    const searchString: string = event.typeahead.trim();
    if (searchString.length > 2) {
      this.isSearchStringExist = true;
      this.dispatchProjectsLoaderAction(true);
      this.store.dispatch(
        new casePlanningActions.LoadProjectsBySearchString({
          searchString
        })
      );
    } else {
      this.isSearchStringExist = false;
      // dispatch action to clear search data from store
      this.store.dispatch(
        new casePlanningActions.ClearSearchData()
      );
    }
  }

  openProjectDetailsDialogFromTypeaheadHandler(event) {
    this.openProjectDetailsDialogHandler(event);
  }

  // ---------------------------------Redux dispatch/Subscribe------------------------------------------//
  private getProjectsByFilterValues(demandFilterCriteriaObj: DemandFilterCriteria, isAdditionalProjects = false) {
    this.pageScrolled = isAdditionalProjects;

    this.projectStartIndex = this.pageScrolled ? this.projectStartIndex : 1;

    demandFilterCriteriaObj.projectStartIndex = this.projectStartIndex;
    demandFilterCriteriaObj.pageSize = this.pageSize;

    if (this.demandFilterCriteriaObj.officeCodes === '' || demandFilterCriteriaObj.caseTypeCodes === '' || demandFilterCriteriaObj.demandTypes === '') {
      this.projects = [];
      return;
    }

    this.dispatchProjectsLoaderAction(true);

    if (!this.pageScrolled) {
      this.store.dispatch(
        new casePlanningActions.LoadProjects({
          demandFilterCriteriaObj
        })
      );
    }
    else {
      this.store.dispatch(
        new casePlanningActions.LoadCasesOnPageScroll({
          demandFilterCriteriaObj
        })
      );
    }

  }

  private getPlanningCardsByFilterValues(demandFilterCriteriaObj: DemandFilterCriteria) {
    if (!demandFilterCriteriaObj.demandTypes?.includes('PlanningCards')) {
      this.planningCards = [];
      return;
    }

    this.dispatchProjectsLoaderAction(true);

    this.store.dispatch(
      new casePlanningActions.LoadPlanningCards({
        demandFilterCriteriaObj
      })
    );
  }

  openCasePlanningWhiteboardPopupHandler(){
    const config = {
      ignoreBackdropClick: true,
      keyboard: false, //disable escape button close for planning board
      class: 'white-board-modal modal-lg',
      initialState: { }
    };
    this.bsModalRef = this.modalService.show(PlanningBoardModalComponent, config);
  }

  private dispatchProjectsLoaderAction(isShowProgressBar) {
    this.showProgressBar = isShowProgressBar;
    this.store.dispatch(
      new casePlanningActions.CasePlanningLoader(true)
    );
  }

  private subscribeuserPrefences() {
    // this is done so that whenever user changes their user settings, it reflects in the projects and resources data
    this.coreService.getUserPreferences().subscribe((userPreferences) => {
      this.userPreferences = userPreferences;
      this.clearProjectSearch.next(true);
      this.updateSupplyAndDemandSettings();
      this.resetScroll();
      this.getPlanningCardsByFilterValues(this.demandFilterCriteriaObj);
      // load projects
      this.getProjectsByFilterValues(this.demandFilterCriteriaObj);
    });
  }

  private updateSupplyAndDemandSettings() {
    const startDate = new Date();
    let endDate = new Date();
    endDate.setDate(endDate.getDate() + 40);

    // Default date range will be today + 40 days on load
    const defaultDateRange = DateService.getFormattedDateRange({
      startDate: startDate,
      endDate: endDate,
    });

    if (this.userPreferences && typeof this.userPreferences === 'object') {
      const today = new Date();
      let dateRangeForProjects: { startDate: any; endDate: any };

      /*-------------- Set user preferences for Supply Side ---------------------*/
      let dateRangeForResources: { startDate: any; endDate: any; };

      if (this.userPreferences.supplyWeeksThreshold) {
        const today = new Date();
        const endDate = new Date(today.setDate(today.getDate() + this.userPreferences.supplyWeeksThreshold * 7));
        const date = { startDate: startDate, endDate: endDate };

        dateRangeForResources = DateService.getFormattedDateRange(date);
      } else {

        dateRangeForResources = defaultDateRange;
      }

      this.supplyFilterCriteriaObj.startDate = dateRangeForResources.startDate;
      this.supplyFilterCriteriaObj.endDate = dateRangeForResources.endDate;
      this.supplyFilterCriteriaObj.officeCodes = this.userPreferences.supplyViewOfficeCodes || this.homeOffice.officeCode.toString();
      this.supplyFilterCriteriaObj.levelGrades = this.userPreferences.levelGrades;
      this.supplyFilterCriteriaObj.availabilityIncludes = this.userPreferences.availabilityIncludes;
      this.supplyFilterCriteriaObj.staffingTags = this.userPreferences.supplyViewStaffingTags || ServiceLineEnum.GeneralConsulting;
      this.supplyFilterCriteriaObj.groupBy = this.userPreferences.groupBy;
      this.supplyFilterCriteriaObj.sortBy = this.userPreferences.sortBy;
      this.supplyFilterCriteriaObj.practiceAreaCodes = this.userPreferences.practiceAreaCodes;
      this.supplyFilterCriteriaObj.positionCodes = this.userPreferences.positionCodes;

      /*-------------- Set user preferences for Demand Side ---------------------*/

      if (this.userPreferences.demandWeeksThreshold) {
        const userSettingsEndDate = new Date(
          today.setDate(
            today.getDate() + this.userPreferences.demandWeeksThreshold * 7
          )
        );

        const differenceInDays = DateService.getDatesDifferenceInDays(
          startDate,
          userSettingsEndDate
        );
        const minDaysForProperRenderingOfProjectsGantt = 40;
        if (differenceInDays > minDaysForProperRenderingOfProjectsGantt) {
          endDate = userSettingsEndDate;
        }
      }
      const date = { startDate: startDate, endDate: endDate };
      this.dateRange = [startDate, endDate];
      dateRangeForProjects = DateService.getFormattedDateRange(date);

      this.demandFilterCriteriaObj.startDate = dateRangeForProjects.startDate;
      this.demandFilterCriteriaObj.endDate = dateRangeForProjects.endDate;
      this.demandFilterCriteriaObj.officeCodes = this.userPreferences.demandViewOfficeCodes || this.homeOffice.officeCode.toString();
      this.demandFilterCriteriaObj.caseTypeCodes = this.userPreferences.caseTypeCodes || CaseTypeEnum.Billable;
      this.demandFilterCriteriaObj.demandTypes = this.userPreferences.demandTypes;
      this.demandFilterCriteriaObj.opportunityStatusTypeCodes = this.userPreferences.opportunityStatusTypeCodes;
      this.demandFilterCriteriaObj.caseAttributeNames = this.userPreferences.caseAttributeNames || '';
      this.demandFilterCriteriaObj.minOpportunityProbability = this.userPreferences.minOpportunityProbability;
      this.demandFilterCriteriaObj.industryPracticeAreaCodes = this.userPreferences.industryPracticeAreaCodes;
      this.demandFilterCriteriaObj.capabilityPracticeAreaCodes = this.userPreferences.capabilityPracticeAreaCodes;

      //Don't need these variables for now
      this.demandFilterCriteriaObj.caseExceptionShowList = '';
      this.demandFilterCriteriaObj.caseExceptionHideList = '';
      this.demandFilterCriteriaObj.opportunityExceptionShowList = '';
      this.demandFilterCriteriaObj.opportunityExceptionHideList = '';
      this.demandFilterCriteriaObj.caseAllocationsSortBy = '';
      this.demandFilterCriteriaObj.planningCardsSortOrder = '';
      this.demandFilterCriteriaObj.caseOppSortOrder = '';

      this.updateDemandFilterCriteriaFromSupply();

    } else {
      this.demandFilterCriteriaObj.startDate = defaultDateRange.startDate;
      this.demandFilterCriteriaObj.endDate = defaultDateRange.endDate;
      this.demandFilterCriteriaObj.officeCodes = this.homeOffice.officeCode.toString();
      this.demandFilterCriteriaObj.caseTypeCodes = CaseTypeEnum.Billable;
    }

  }

  updateDemandFilterCriteriaFromSupply() {
    if (this.demandFilterCriteriaObj.demandTypes?.includes("CasesStaffedBySupply")) {
      this.demandFilterCriteriaObj.supplyFilterCriteria = this.supplyFilterCriteriaObj;
    } else {
      this.demandFilterCriteriaObj.supplyFilterCriteria = null;
    }

  }

  getLookupListFromLocalStorage() {
    this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.officeHierarchy);
    this.caseTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.caseTypes);
    this.demandTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.demandTypes);
    this.opportunityStatusTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.opportunityStatusTypes);
    this.staffingTagsHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffingTagsHierarchy);
    this.industryPracticeAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.industryPracticeAreas);
    this.capabilityPracticeAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.capabilityPracticeAreas);
  }


  private setStoreSuscriptions() {
    this.getProjectsFromStore();
    this.getPlanningCardsFromStore();
    this.getProjectsFromStoreOnSearch();
    this.refreshCaseAndResourceOverlayListener();
    this.projectsLoaderListener();
  }

  private getProjectsFromStore() {
    this.storeSub.add(this.store
      .pipe(select(fromProjects.getProjects))
      .subscribe((projectData: Project[]) => {
        // Check for filter values as async call might take some time to return data and hence,
        // if user select All Bain Offices and Unselect the next moment, records will show up
        // as first request took some time to return.
        if (this.demandFilterCriteriaObj.officeCodes === '' || this.projects === undefined) {
          this.projects = [];
        } else {
          this.loadProjectsData(projectData);
        }
      }));
  }

  private getPlanningCardsFromStore() {
    this.storeSub.add(this.store
      .pipe(select(fromProjects.getPlanningCards))
      .subscribe((planningCardsData: PlanningCard[]) => {
        //get only those planning cards that fall within the selected date range
        this.planningCards = planningCardsData.filter(
          x => DateService.isSameOrBefore(x.startDate, this.demandFilterCriteriaObj.endDate)
            && DateService.isSameOrAfter(x.endDate, this.demandFilterCriteriaObj.startDate)
        )
      }));
  }

  private getProjectsFromStoreOnSearch() {
    this.storeSub.add(this.store
      .pipe(select(fromProjects.getSearchedProjects))
      .subscribe((projectsData: Project[]) => {
        if (this.isSearchStringExist) {
          this.searchedProjects.next(projectsData);
        }
      }));
  }

  private refreshCaseAndResourceOverlayListener() {
    this.storeSub.add(this.store
      .pipe(select(fromProjects.refreshCaseAndResourceOverlay))
      .subscribe((refreshNeeded: boolean) => {
        if (!refreshNeeded) {
          return;
        }
        this.refreshCaseAndResourceOverlay();
        this.dispatchRefreshCaseAndResourceOverlayAction(false);
      }));

  }

  private dispatchRefreshCaseAndResourceOverlayAction(refreshNeeded) {
    this.store.dispatch(
      new casePlanningActions.RefreshCaseAndResourceOverlay(refreshNeeded)
    );
  }

  // -------------------------------------Helper Functions----------------------------------//
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

  private loadProjectsData(projectsData: Project[]) {
    this.projects = projectsData;
  }

  private resetPageNumber() {
    this.pageNumber = 1;
  }

  private resetScroll() {
    if (this.projectsGantt) {
      this.resetPageNumber();
      this.scrollToTop();
    }
  }

  private scrollToTop() {
    this.projectsGantt.ganttContainer.nativeElement.scrollTo(0, 0);
  }

  private projectsLoaderListener() {
    this.storeSub.add(this.store
      .pipe(select(fromProjects.casePlanningLoader))
      .subscribe((isLoader: boolean) => {
        if (isLoader === false) {
          this.showProgressBar = false;
        }
      }));
  }

  loadMoreCases() {
    // if total projects displayed are less than the total page size, it means there are not more projects that needs to be fetched
    const totalPageSize = this.projectStartIndex + this.pageSize - 1; //subtracting 1 because index starts from 1
    if (this.projects.length < totalPageSize) {
      return;
    }

    this.getMoreActiveCasesOnPageScroll(this.demandFilterCriteriaObj);
  }

  private getMoreActiveCasesOnPageScroll(demandFilterCriteriaObj: DemandFilterCriteria) {
    this.projectStartIndex = this.projectStartIndex + this.pageSize;
    this.getProjectsByFilterValues(this.demandFilterCriteriaObj, true);
  }

  toggleFiltersSection() {
    this.showFilters = !this.showFilters;
  }

  getProjectsOnFilterChangeHandler(event) {

    this.dateRange = event.dateRange;
    this.demandFilterCriteriaObj.startDate = DateService.getFormattedDate(event.dateRange[0]);
    this.demandFilterCriteriaObj.endDate = DateService.getFormattedDate(event.dateRange[1]);
    this.demandFilterCriteriaObj.officeCodes = event.officeCodes;
    this.demandFilterCriteriaObj.caseTypeCodes = event.caseTypeCodes;
    this.demandFilterCriteriaObj.demandTypes = event.demandTypes;
    this.demandFilterCriteriaObj.opportunityStatusTypeCodes = event.opportunityStatusTypeCodes;
    this.demandFilterCriteriaObj.caseAttributeNames = event.staffingTags;
    this.demandFilterCriteriaObj.minOpportunityProbability = event.minDemandProbabilityPercent;
    this.demandFilterCriteriaObj.industryPracticeAreaCodes = event.industryPracticeAreaCodes;
    this.demandFilterCriteriaObj.capabilityPracticeAreaCodes = event.capabilityPracticeAreaCodes;
    this.demandFilterCriteriaObj.clientCodes = event.clientCodes;

    this.updateDemandFilterCriteriaFromSupply();
    this.getPlanningCardsByFilterValues(this.demandFilterCriteriaObj);
    this.getProjectsByFilterValues(this.demandFilterCriteriaObj);
    this.resetScroll();
  }

  reloadProjects() {
    this.resetScroll();
    this.getProjectsByFilterValues(this.demandFilterCriteriaObj);
  }

  reloadPlanningCards(){
    this.resetScroll();
    this.getPlanningCardsByFilterValues(this.demandFilterCriteriaObj);
  }

  //-------------------------Child Output events/Handlers ---------------------------//
  openPlaceholderForm(modalData) {
    this.placeholderDialogService.openPlaceholderFormHandler(modalData);
  }

  upsertCasePlanningNoteHandler(caseViewNote: ResourceOrCasePlanningViewNote){
    if (caseViewNote) {
      this.dispatchUpsertCaseViewNoteActionHandler(caseViewNote);
    }
  }

  deleteCasePlanningNotesHandler(idsToBeDeleted: string) {
    if (idsToBeDeleted.length > 0) {
      this.dispatchDeleteCaseViewNotesAction(idsToBeDeleted);
    }
  }

  openAddTeamSkuFormHandler(projectToOpen) {
    const modalRef = this.modalService.show(AddTeamSkuComponent, {
      animated: true,
      backdrop: false,
      ignoreBackdropClick: true,
      initialState: {
        selectedCase: projectToOpen
      },
      class: "sku-modal"
    });

    // inserts & updates placeholder data when changes are made to placeholder
    this.storeSub.add(modalRef.content.upsertPlaceholderAllocationsToProject.subscribe(updatedData => {
      this.placeholderAssignmentService.upsertPlcaeholderAllocations(updatedData, null, this.projectDialogRef);
    }));

    this.storeSub.add(modalRef.content.deletePlaceHoldersByIds.subscribe(event => {
      this.placeholderAssignmentService.deletePlaceHoldersByIds(event, this.projectDialogRef);
    }));

    this.storeSub.add(modalRef.content.insertSKUCaseTerms.subscribe(skuTab => {
      this.skuCaseTermService.insertSKUCaseTerms(skuTab, this.dialogRef, this.projectDialogRef);
    }));

    this.storeSub.add(modalRef.content.updateSKUCaseTerms.subscribe(skuTab => {
      this.skuCaseTermService.updateSKUCaseTerms(skuTab, this.dialogRef, this.projectDialogRef);
    }));
    

  }

  openCaseRollFormHandler(event) {
    // class is required to center align the modal on large screens
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        project: event.project
      }
    };

    const modalRef = this.modalService.show(CaseRollFormComponent, config);

    this.storeSub.add(modalRef.content.upsertCaseRollAndAllocations.subscribe(response => {
      this.caseRollService.upsertCaseRollAndAllocationsHandler(response.caseRoll, response.resourceAllocations, this.projectDialogRef);
    }));

    this.storeSub.add(modalRef.content.revertCaseRollAndAllocations.subscribe(response => {
      this.caseRollService.revertCaseRollAndAllocationsHandler(response.caseRoll, response.resourceAllocations, this.projectDialogRef);
    }));

    this.storeSub.add(modalRef.content.upsertCaseRollAndPlaceholderAllocations.subscribe(response => {
      this.caseRollService.upsertCaseRollAndPlaceholderAllocationsHandler(response.caseRoll, response.resourceAllocations, this.projectDialogRef);
    }));
  }

  showQuickPeekDialogHandler(event) {
    this.resourcesCommitmentsDialogService.showResourcesCommitmentsDialogHandler(event);
  }

  openCaseDetailsDialogHandler(event) {
    this.openProjectDetailsDialogHandler(event);
  }

  openProjectDetailsDialogHandler(projectData) {
    // close previous project dialog & open new dialog
    if (this.projectDialogRef) {
      this.projectDialogRef.close('no null');
    }

    // if (this.projectDialogRef == null) {
    this.projectDialogRef = this.dialog.open(ProjectOverlayComponent, {
      closeOnNavigation: true,
      hasBackdrop: false,
      enterAnimationDuration : 0,
      data: {
        projectData: projectData,
        //investmentCategories: this.investmentCategories,
        //caseRoleTypes: this.caseRoleTypes,
        showDialog: true
      }
    });
    // Listens for click on resource name for opening the resource details pop-up
    this.storeSub.add(this.projectDialogRef.componentInstance.openResourceDetailsFromProjectDialog.subscribe(employeeCode => {
      this.openResourceDetailsDialogHandler(employeeCode);
    }));

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
    this.storeSub.add(this.projectDialogRef.componentInstance.deleteResourcesFromProject.subscribe(allocation => {
      this.deleteResourcesAssignmentsFromProjectHandler(allocation.allocationIds);      
      this.resourceAssignmentService.deletePlaygroundAllocationsForCasePlanningMetrics(allocation.resourceAllocation);
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
      this.getProjectsByFilterValues(this.demandFilterCriteriaObj);
    }));

    // delete placeholders using context menu in case-gantt in case overlay
    this.storeSub.add(this.projectDialogRef.componentInstance.deletePlaceholdersFromProject.subscribe(event => {
      this.placeholderAssignmentService.deletePlaceHoldersByIds(event, this.projectDialogRef);
    }));

    this.storeSub.add(this.projectDialogRef.beforeClosed().subscribe(result => {
      if (result !== 'no null') {
        this.projectDialogRef = null;
      }
    }));
    // } else {
    //   this.notifyService.showValidationMsg('Opening multiple cases/opporutnities not allowed.');
    // }
  }

  openResourceDetailsDialogHandler(employeeCode) {
    // close previous resource dialog & open new dialog
    if (this.dialogRef) {
      this.dialogRef.close('no null');
      this.dialogRef = null;
    }

    this.dialogRef = this.dialog.open(ResourceOverlayComponent, {
      closeOnNavigation: true,
      data: {
        employeeCode: employeeCode,
        showOverlay: true
      }
    });

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

    // inserts & updates resource data when changes are made to resource
    this.storeSub.add(this.dialogRef.componentInstance.upsertResourceAllocationsToProject.subscribe(updatedData => {
      this.upsertResourceAllocationsToProjectHandler(updatedData.resourceAllocation);
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

    this.storeSub.add(this.dialogRef.beforeClosed().subscribe((result) => {
      if (result !== 'no null') {
        this.dialogRef = null;
      }
    }));

  }

  public addResourceCommitmentHandler(resourceCommitment) {
    this.dispatchAddResourceCommitmentAction(resourceCommitment);
  }

  public updateResourceCommitmentHandler(updatedResourceCommitment) {
    this.dispatchUpdateResourceCommitmentAction(updatedResourceCommitment);
  }

  private deleteResourceCommitmentHandler(deletedCommitmentId) {
    this.dispatchDeleteResourceCommitmentAction(deletedCommitmentId);
  }

  private dispatchUpsertCaseViewNoteActionHandler(upsertedNote) {
    this.upsertCaseViewNoteAction(upsertedNote);
  }

  private dispatchDeleteCaseViewNotesAction(idsToBeDeleted) {
    this.deleteCaseViewNotesAction(idsToBeDeleted);
  }

  // ------------------Pop-Ups -----------------------------------------------------------
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
          upsertedAllocations.resourceAllocation
        );
      }
    ));

    this.storeSub.add(this.bsModalRef.content.deleteResourceAllocationFromCase.subscribe(
      (deletedObj) => {
        this.deleteResourceAssignmentFromProjectHandler(deletedObj.allocationId);      
        this.resourceAssignmentService.deletePlaygroundAllocationsForCasePlanningMetrics(deletedObj.resourceAllocation);
      }
    ));

    this.storeSub.add(this.bsModalRef.content.openBackFillPopUp.subscribe((result) => {
      this.openBackFillFormHandler(result);
    }));

    this.storeSub.add(this.bsModalRef.content.openOverlappedTeamsPopup.subscribe((result) => {
      this.openOverlappedTeamsFormHandler(result);
    }));
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
      },
    };
    this.bsModalRef = this.modalService.show(BackfillFormComponent, config);

    this.storeSub.add(this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe((data) => {
      event.project.allocatedResources = event.project.allocatedResources.concat(
        data.resourceAllocation
      );
      this.upsertResourceAllocationsToProjectHandler(data.resourceAllocation);
    }));
  }

  private openOverlappedTeamsFormHandler(event) {
    // class is required to center align the modal on large screens
    const config = {
      class: 'modal-dialog-centered',
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

    // clear bsModalRef value on closing modal
    this.storeSub.add(this.modalService.onHidden.subscribe(() => {
      this.bsModalRef = null;
    }));
    // }
  }

  public upsertResourceAllocationsToProjectHandler(resourceAllocation) {
    let addedResourceAsArray = [];
    const addedResource = resourceAllocation;
    if (Array.isArray(addedResource)) {
      addedResourceAsArray = addedResource;
    } else {
      addedResourceAsArray.push(addedResource);
    }
    this.dispatchUpsertResourceAction(addedResourceAsArray);
  }

  private dispatchUpsertResourceAction(upsertedAllocations) {
    this.store.dispatch(
      new casePlanningActions.UpsertResourceStaffing(upsertedAllocations)
    );
  }

  private dispatchUpdateResourceCommitmentAction(updatedResourceCommitment) {
    this.store.dispatch(
      new casePlanningActions.UpdateResourceCommitment(updatedResourceCommitment)
    );
  }

  private dispatchDeleteResourceCommitmentAction(deletedCommitmentId) {
    this.store.dispatch(
      new casePlanningActions.DeleteResourceCommitment(deletedCommitmentId)
    );
  }

  private deleteResourceAssignmentFromProjectHandler(deletedObjId) {
    this.store.dispatch(
      new casePlanningActions.DeleteResourceStaffing(deletedObjId)
    );
  }

  private deleteResourcesAssignmentsFromProjectHandler(deletedObjIds) {
    this.store.dispatch(
      new casePlanningActions.DeleteResourcesStaffing(deletedObjIds)
    );
  }

  private deleteResourcesAllocationsCommitments(dataToDelete) {
    this.store.dispatch(
      new casePlanningActions.DeleteAllocationsCommitmentsStaffing(dataToDelete)
    );
  }

  private dispatchAddResourceCommitmentAction(resourceCommitment) {
    this.store.dispatch(
      new casePlanningActions.AddResourceCommitment(resourceCommitment)
    );
  }

  private upsertStaffableAsRole(staffableAsRole) {
    this.dispatchProjectsLoaderAction(true);
    this.store.dispatch(
      new casePlanningActions.UpsertResourceStaffableAsRole(staffableAsRole)
    );
  }

  private deleteStaffableAsRole(staffableAsRole) {
    this.dispatchProjectsLoaderAction(true);
    this.store.dispatch(
      new casePlanningActions.DeleteResourceStaffableAsRole(staffableAsRole)
    );
  }

  private upsertCaseViewNoteAction(caseViewNote: ResourceOrCasePlanningViewNote) {
    this.dispatchProjectsLoaderAction(true);
    this.store.dispatch(
      new casePlanningActions.UpsertCasePlanningViewNote(caseViewNote)
    );
  }

  private deleteCaseViewNotesAction(idsToBeDeleted) {
    this.store.dispatch(
      new casePlanningActions.DeleteCasePlanningViewNotes(idsToBeDeleted)
    );
  }

  // ---------------------------Component Unload--------------------------------------------//

  ngOnDestroy() {
    this.overlayMessageServiceSub.unsubscribe();
    this.storeSub.unsubscribe();
  }

}
