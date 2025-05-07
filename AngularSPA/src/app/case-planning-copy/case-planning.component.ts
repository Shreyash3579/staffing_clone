// -------------------Angular References---------------------------------------//
import { Component, OnInit, ViewChild } from "@angular/core";
import { select, Store } from "@ngrx/store";
import { Subject, Subscription, combineLatest } from "rxjs";
import { filter, takeUntil } from 'rxjs/operators';

// -------------------Interfaces---------------------------------------//
import { PlanningCard } from "../shared/interfaces/planningCard.interface";
import { OfficeHierarchy } from "../shared/interfaces/officeHierarchy.interface";
import { CaseType } from "../shared/interfaces/caseType.interface";
import { KeyValue } from "../shared/interfaces/keyValue.interface";
import { ServiceLine } from "../shared/interfaces/serviceLine.interface";
import { OpportunityStatusType } from "../shared/interfaces/opportunityStatusType";
import { DemandFilterCriteria } from "../shared/interfaces/demandFilterCriteria.interface";
import { Office } from "../shared/interfaces/office.interface";
import { Project } from "../shared/interfaces/project.interface";
import { UserPreferences } from "../shared/interfaces/userPreferences.interface";
import { PracticeArea } from "../shared/interfaces/practiceArea.interface";
import { UserPreferenceSupplyGroup } from "../shared/interfaces/userPreferenceSupplyGroup";
import { UserPreferenceSupplyGroupViewModel } from "../shared/interfaces/userPreferenceSupplyGroupViewModel";
import { CasePlanningBoardBucket } from "../shared/interfaces/case-planning-board-bucket.interface";
import { CaseOppChanges } from "../shared/interfaces/caseOppChanges.interface";
import { DropdownFilterOption } from "../shared/interfaces/dropdown-filter-option";
import { CombinedUserPreferences } from "../shared/interfaces/combinedUserPreferences.interface";
import { CaseGrouping } from 'src/app/shared/constants/enumMaster';

// -------------------Components---------------------------------------//
import { GanttComponent } from "./gantt/gantt.component";
import { PlanningBoardStageComponent } from "../case-planning-whiteboard/planning-board-stage/planning-board-stage.component";
//import { CustomGroupModalComponent } from "../shared/viewing-group/custom-group-modal/custom-group-modal.component";
//import { UpdateCaseCardComponent } from "../shared/update-case-card/update-case-card.component";

// -------------------Services---------------------------------------//
import { LocalStorageService } from "../shared/local-storage.service";
import { CoreService } from "../core/core.service";
import { DateService } from "../shared/dateService";
//import { CasePlanningService } from "./case-planning.service";
import { NotificationService } from "../shared/notification.service";
import { ValidationService } from "../shared/validationService";

// -------------------Constants/Enums---------------------------------------//
import { ConstantsMaster } from "../shared/constants/constantsMaster";
import { CaseType as CaseTypeEnum, ServiceLine as ServiceLineEnum } from "../shared/constants/enumMaster";
import { GoogleAnalytics } from "../shared/google-analytics/googleAnalytics";
import { CasePlanningBoardBucketEnum } from "../shared/constants/enumMaster";
import { ResourcesSupplyFilterGroupEnum } from "../shared/constants/enumMaster";

// --------------------------Redux Component -----------------------------------------//
import * as casePlanningActions from "./State/case-planning.actions";
import * as fromProjects from "./State/case-planning.reducer";
import { BsModalService, BsModalRef } from "ngx-bootstrap/modal";
import { CaseRollFormComponent } from "../shared/case-roll-form/case-roll-form.component";
import { CaseRollService } from "../overlay/behavioralSubjectService/caseRoll.service";
import { OverlayMessageService } from "../overlay/behavioralSubjectService/overlayMessage.service";
import { ShowQuickPeekDialogService } from "../overlay/dialogHelperService/show-quick-peek-dialog.service";
import { PlaceholderDialogService } from "../overlay/dialogHelperService/placeholderDialog.service";
import { SupplyFilterCriteria } from "../shared/interfaces/supplyFilterCriteria.interface";
import { ProjectOverlayComponent } from "../overlay/project-overlay/project-overlay.component";
import * as ProjectAllocationsActions from 'src/app/state/actions/project-allocations.action';

// --------------------------utilities -----------------------------------------//
import { MatDialogRef, MatDialog } from "@angular/material/dialog";
import { ResourceOverlayComponent } from "../overlay/resource-overlay/resource-overlay.component";
import { AgGridNotesComponent } from "../overlay/ag-grid-notes/ag-grid-notes.component";
import { AgGridSplitAllocationPopUpComponent } from "../overlay/ag-grid-split-allocation-pop-up/ag-grid-split-allocation-pop-up.component";
import { QuickAddFormComponent } from "../shared/quick-add-form/quick-add-form.component";
import { BackfillFormComponent } from "../shared/backfill-form/backfill-form.component";
import { PlaceholderAssignmentService } from "../overlay/behavioralSubjectService/placeholderAssignment.service";
import { PlaceholderFormComponent } from "./placeholder-form/placeholder-form.component";
import { ResourceAssignmentService } from "../overlay/behavioralSubjectService/resourceAssignment.service";
import { BackfillDialogService } from "../overlay/dialogHelperService/backFillDialog.service";
import { SkuCaseTermService } from "../overlay/behavioralSubjectService/skuCaseTerm.service";
import { UserPreferenceService } from "../overlay/behavioralSubjectService/userPreference.service";
import { OpportunityService } from "../overlay/behavioralSubjectService/opportunity.service";
import { SkuTerm } from "../shared/interfaces/skuTerm.interface";
import { PDGrade } from "../shared/interfaces/pdGrade.interface";
import { OverlappedTeamsFormComponent } from "../shared/overlapped-teams-form/overlapped-teams-form.component";
import { CommitmentType } from "../shared/interfaces/commitmentType.interface";
import { PlanningBoardModalComponent } from "../case-planning-whiteboard/planning-board-modal/planning-board-modal.component";
import { ResourceOrCasePlanningViewNote } from "../shared/interfaces/resource-or-case-planning-view-note.interface";
import { OverlayService } from "../overlay/overlay.service";
import { OverlayDialogService } from "../overlay/dialogHelperService/overlayDialog.service";
import { CasePlanningService } from "./case-planning.service";
import { AddTeamSkuComponent } from "./add-team-sku/add-team-sku.component";

@Component({
  selector: "app-case-planning",
  templateUrl: "./case-planning.component.html",
  styleUrls: ["./case-planning.component.scss"]
})
export class CasePlanningComponent implements OnInit {
  // ------------- Input events --------------------------- //
  bsModalRef: BsModalRef;
  dateRange: [Date, Date];
  ganttCasesData: any;
  supplyFilterCriteriaObj: SupplyFilterCriteria = {} as SupplyFilterCriteria;
  demandFilterCriteriaObj: DemandFilterCriteria = {} as DemandFilterCriteria;
  homeOffice: Office;

  @ViewChild("projectsGantt", { static: false }) projectsGantt: GanttComponent;
  @ViewChild("planningStage", { static: false }) planningStage: PlanningBoardStageComponent;

  //-------------------- Local Variables ---------------------- //
  destroy$: Subject<boolean> = new Subject<boolean>();
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
  clickTypeIndicator: any;

  
  private subData: Subscription = new Subscription();
  private storeSub: Subscription = new Subscription();
  private overlayMessageServiceSub: Subscription = new Subscription();
  public projects: Project[];
  public projectsToBeDisplayed: Project[];
  public planningCards: PlanningCard[];
  public planningCardsToBeDisplayed: any;
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
  flaggedGroupingOn = false;

  // planning board & metrics
  NEW_DEMAND_COLUMN_TITLE = "New Demands";
  DEMAND_METRICS_PROJECTS = "Projects To Be Included In Demand Metrics";

  planningBoardBucketLookUp: CasePlanningBoardBucket[] = [];
  planningBoard = [];
  planningBoardColumnMetrics = [];
  updatedProjectId;

  supplyMetricsData;
  demandMetricsProjects;

  isPreviousWeekData: boolean = false;
  isSupplyMetricsLodaed: boolean = false;
  isDemandMetricsLoaded: boolean = false;
  isCountOfIndividualResourcesToggle: boolean = false;
  enableNewlyAvailableHighlighting: boolean = false;

  // viewing groups
  allSupplyDropdownOptions: DropdownFilterOption[] = [];
  supplyGroupPreferences: UserPreferenceSupplyGroupViewModel[] = [];

  // ----------------------- Notifiers ------------------------------------------------//
  clearProjectSearch: Subject<boolean> = new Subject();
  searchedProjects: Subject<Project[]> = new Subject();

  //--------------------Life- Cycle Events ------------------- //

  constructor(
    private store: Store<fromProjects.State>,
    private coreService: CoreService,
    private localStorageService: LocalStorageService,
    private modalService: BsModalService,
    private placeholderAssignmentService: PlaceholderAssignmentService,
    private resourceAssignmentService: ResourceAssignmentService,
    private backfillDialogService: BackfillDialogService,
    public dialog: MatDialog,
    public overlayDialogService: OverlayDialogService,
  ) { }

  ngOnInit(): void {
    GoogleAnalytics.staffingTrackPageView(
      this.coreService.loggedInUser.employeeCode,
      ConstantsMaster.appScreens.page.casePlanningCopy,
      ""
    );
    this.homeOffice = this.coreService.loggedInUser.office;
    this.pageSize = this.coreService.appSettings.projectsPageSize;
    this.scrollDistance = this.coreService.appSettings.scrollDistance;
    this.subscribeuserPrefences();
    this.setStoreSuscriptions();
    // this.subscribeEvents();
    this.getLookupListFromLocalStorage();
  }

  // subscribeEvents() {
  //   this.overlayMessageServiceSub.add(
  //     this.overlayMessageService.refreshResources().subscribe((result) => {
  //       if (result === true) {
  //         //TODO: think of a way to prevet reload of entire page when updates occur
  //         this.reloadProjects();
  //       }
  //     })
  //   );
  //   this.overlayMessageServiceSub.add(
  //     this.overlayMessageService.refreshCasesAndopportunties().subscribe((result) => {
  //       if (result === true) {
  //         //TODO: think of a way to prevet reload of entire page when updates occur
  //         this.reloadProjects();
  //       }
  //     })
  //   );
  // }

  loadViewingFilterDropdown() {
    // let savedFilters: DropdownFilterOption[] = this.savedResourceFilters?.map((data: ResourceFilter) => {
    //   return {
    //     text: data.title,
    //     value: data.id,
    //     filterGroupId: ResourcesSupplyFilterGroupEnum.SAVED_GROUP,
    //     id: data.id,
    //     selected: false,
    //     isDefault: false,
    //     isDefaultForResourcesTab: false
    //   };
    // }).sort((a, b) => a.text.localeCompare(b.text));


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

    // this.allSupplyDropdownOptions = [...staffingSettings, ...supplygroups, ...savedFilters];
    this.allSupplyDropdownOptions = [...staffingSettings, ...supplygroups];

    //setting default based on precedence
    if (this.supplyGroupPreferences.some(x => x.isDefaultForResourcesTab)) {
      const defaultSupplyGrpPreference: UserPreferenceSupplyGroupViewModel = this.supplyGroupPreferences.find(x => x.isDefaultForResourcesTab)

      this.allSupplyDropdownOptions.find(x => x.id == defaultSupplyGrpPreference.id).selected = true;
      this.allSupplyDropdownOptions.find(x => x.id == defaultSupplyGrpPreference.id).isDefault = true;
    }
    // else if (this.savedResourceFilters.find(x => x.isDefault)) {
    //   const defaultSavedFilter: ResourceFilter = this.savedResourceFilters.find(x => x.isDefault);
    //   this.allSupplyDropdownOptions.find(x => x.id == defaultSavedFilter.id).selected = true;
    //   this.allSupplyDropdownOptions.find(x => x.id == defaultSavedFilter.id).isDefault = true;
    // }
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
      this.store.dispatch(new casePlanningActions.ClearSearchData());
    }
  }

  private initializeAndSubscribeUserPreferences() {
    const combinedUserPreferences = this.coreService.getCombinedUserPreferencesValue();
    this.userPreferences = combinedUserPreferences.userPreferences;
    this.supplyGroupPreferences = combinedUserPreferences.userPreferenceSupplyGroups;
    this.subscibeToUserPreferences();
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

  // onSupplyGroupFilterChangedHandler(selectedSupplyDropdownId) {
  //   // console.log("selectedSupplyDropdownId", selectedSupplyDropdownId);
  // }

  private getDataBasedOnDefaultSettings() {
    // if (this.isSearchStringExist) {
    //   this.clearEmployeeSearch.next(true);
    //   this.clearSearchData();
    // }

    // this.updateDefaultFilterCriteria();
    // // load resources data
    // this.getResourcesByDefaultSettings();
    this.loadViewingFilterDropdown();
  }

  // private convertToBucketRowObj(toColumn: string, bucketId = null) {
  //   if (!bucketId) bucketId = CasePlanningBoardBucketEnum.STAFF_FROM_MY_SUPPLY;

  //   let toBucket = this.planningBoard?.find(x => x.title == toColumn)?.buckets?.find(y => y.bucketId == bucketId);
  //   if (!toBucket) {
  //     toBucket = this.setDefaultBucketDataForColumn(toColumn);
  //   }
  //   return toBucket;
  // }

  // private setDefaultBucketDataForColumn(toColumn) {
  //   return {
  //     bucketId: this.planningBoardBucketLookUp[0].id,
  //     bucketName: this.planningBoardBucketLookUp[0].bucketName,
  //     date: toColumn,
  //     projects: null
  //   }
  // }

  // updateProjectChanges(updatedProject) {
  //   let updatedProjectData: CaseOppChanges = {
  //     pipelineId: updatedProject.pipelineId,
  //     oldCaseCode: updatedProject.oldCaseCode,
  //     startDate: updatedProject.startDate,
  //     endDate: updatedProject.endDate,
  //     probabilityPercent: updatedProject.probabilityPercent,
  //     notes: updatedProject.notes,
  //     caseServedByRingfence: updatedProject.caseServedByRingfence,
  //     staffingOfficeCode: updatedProject.staffingOfficeCode
  //   }

  //   this.updateProjectChangesHandler(updatedProjectData, updatedProject);
  // }

  // updateProjectChangesHandler(updatedCaseOppChanges, project) {
  //   this.opportunityService.updateProjectChangesHandler(updatedCaseOppChanges);

  //   const demandMetricsProject = this.demandMetricsProjects.find(x => (project.oldCaseCode && x.oldCaseCode == project.oldCaseCode)
  //     || (project.pipelineId && x.pipelineId == project.pipelineId)
  //     || (project.planningCardId && x.planningCardId == project.planningCardId))

  //   const dataToUpdate = {
  //     id: project.planningBoardId || undefined,
  //     bucketId: project?.date ? project?.bucketId : null,
  //     date: demandMetricsProject ? demandMetricsProject?.date : project?.date,
  //     pipelineId: project?.pipelineId,
  //     oldCaseCode: project?.oldCaseCode,
  //     planningCardId: project?.planningCardId,
  //     projectEndDate: project?.endDate
  //   }
  //   this.upsertOrDeleteCasePlanningBoardCard([].concat(project), [].concat(dataToUpdate))
  // }

  // upsertOrDeleteCasePlanningBoardCard(projects, dataToUpdate) {
  //   if (dataToUpdate[0].date) {
  //     this.casePlanningService.upsertCasePlanningBoardData(dataToUpdate).subscribe(upsertedCards => {
  //       projects.forEach(project => {
  //         project.planningBoardId = upsertedCards.find(upsertedCard => (project.oldCaseCode && upsertedCard.oldCaseCode == project.oldCaseCode)
  //           || (project.pipelineId && upsertedCard.pipelineId == project.pipelineId)
  //           || (project.planningCardId && upsertedCard.planningCardId == project.planningCardId)).id;
  //       })
  //       this.notifyService.showSuccess(ValidationService.casePlanningBoardUpdatedSuccessfullyMsg);
  //       this.overlayMessageService.triggerCaseAndOpportunityRefreshOnCasePlanning(upsertedCards);
  //       upsertedCards.forEach(upsertedData => {
  //         let upsertedDemandMetricsProject = this.demandMetricsProjects.find(x => (upsertedData.oldCaseCode && x.oldCaseCode == upsertedData.oldCaseCode)
  //           || (upsertedData.pipelineId && x.pipelineId == upsertedData.pipelineId)
  //           || (upsertedData.planningCardId && x.planningCardId == upsertedData.planningCardId));

  //         if (!upsertedDemandMetricsProject) {
  //           let upsertedColumn = this.planningBoard.find(x => new Date(x.title).getTime() === new Date(upsertedData.date).getTime());
  //           let upsertedProject
  //           if (!!upsertedColumn) {
  //             upsertedProject = upsertedColumn.buckets.find(x => x.bucketId == upsertedData.bucketId).projects.find(x => x.planningBoardId == upsertedData.id);
  //             if (!upsertedProject) {
  //               upsertedProject = projects.find(project => (upsertedData.oldCaseCode && project.oldCaseCode == upsertedData.oldCaseCode)
  //                 || (upsertedData.pipelineId && project.pipelineId == upsertedData.pipelineId)
  //                 || (upsertedData.planningCardId && project.planningCardId == upsertedData.planningCardId));
  //             }
  //             upsertedProject.bucketId = upsertedData.bucketId;
  //             if (upsertedProject.bucketId == CasePlanningBoardBucketEnum.ACTION_NEEDED && this.planningBoardBucketLookUp.find(x => x.id == CasePlanningBoardBucketEnum.ACTION_NEEDED).includeInDemand) {
  //               upsertedProject.includeInDemand = true;
  //             }
  //             upsertedProject.planningBoardId = upsertedData.id;
  //             upsertedProject.date = upsertedData.date;
  //             upsertedProject.endDate = new Date(upsertedData.projectEndDate);
  //             this.demandMetricsProjects.push(upsertedProject);
  //           }
  //         } else {
  //           upsertedDemandMetricsProject.bucketId = upsertedData.bucketId;
  //           if (upsertedDemandMetricsProject.bucketId == CasePlanningBoardBucketEnum.ACTION_NEEDED && this.planningBoardBucketLookUp.find(x => x.id == CasePlanningBoardBucketEnum.ACTION_NEEDED).includeInDemand) {
  //             upsertedDemandMetricsProject.includeInDemand = true;
  //           }
  //           upsertedDemandMetricsProject.planningBoardId = upsertedData.id;
  //           upsertedDemandMetricsProject.date = upsertedData.date;
  //           upsertedDemandMetricsProject.endDate = upsertedData.projectEndDate;
  //         }
  //       });
  //       this.createMetricsForSupplyAndDemand();
  //     });
  //   } else {
  //     this.casePlanningService.deleteCasePlanningBoardByIds(projects[0].planningBoardId).subscribe(deletedData => {
  //       this.notifyService.showSuccess(ValidationService.casePlanningBoardUpdatedSuccessfullyMsg);
  //     });
  //   }
  // }

  // addSelectedProjectToBoardHandler(selectedData) {

  //   //update staffing office withe selected office
  //   selectedData.selectedProject.staffingOfficeCode = selectedData.selectedOffice?.officeCode;
  //   selectedData.selectedProject.staffingOfficeAbbreviation = selectedData.selectedOffice?.officeAbbreviation;

  //   //show the added case on board
  //   let bucketToAdd = this.convertToBucketRowObj(selectedData.selectedColumn);
  //   bucketToAdd?.projects?.push(selectedData.selectedProject);

  //   selectedData.selectedProject.bucketId = bucketToAdd.bucketId;
  //   selectedData.selectedProject.date = selectedData.selectedColumn;

  //   this.updateProjectChanges(selectedData.selectedProject);
  // }

  // isProjectAvailableonBoard(selectedProject) {
  //   let isProjectExists = false;

  //   this.planningBoard.every(x => {
  //     x.buckets.every(y => {
  //       if (y.projects.findIndex(z => z.oldCaseCode === selectedProject.oldCaseCode && z.pipelineId === selectedProject.pipelineId) > -1) {
  //         isProjectExists = true;
  //         return false; //used to break out of the loop
  //       }
  //       return true; //used to conitnue in the loop
  //     })

  //     if (isProjectExists)
  //       return false;
  //     else
  //       return true;
  //   });

  //   return isProjectExists;
  // }

  openProjectDetailsDialogFromTypeaheadHandler(event) {
    this.openDetailsDialogHandler(event);
  }

  public openDetailsDialogHandler(event) {
    setTimeout(() => {
      // 'clickTypeIndicator' prevent single click from firing twice in case of double click
      if (this.clickTypeIndicator) {
        return true;
      } else {
        this.overlayDialogService.openProjectDetailsDialogHandler({ oldCaseCode: event.oldCaseCode, pipelineId: event.pipelineId });
       }
    }, 500);
    this.clickTypeIndicator = 0;
  }

  // openCustomGroupModalHandler(event) {
  //   const isEditMode: boolean = event.isEditMode;
  //   const groupToEdit: UserPreferenceSupplyGroup | UserPreferenceSupplyGroupViewModel = event.groupToEdit;
  //   const modalRef = this.modalService.show(CustomGroupModalComponent, {
  //     backdrop: false,
  //     class: "custom-group-modal modal-dialog-centered",
  //     initialState: {
  //       isEditMode: isEditMode,
  //       groupToEdit: groupToEdit
  //     }
  //   });

  //   // modalRef.content.upsertUserPreferenceSupplyGroups.subscribe((upsertedData: UserPreferenceSupplyGroup) => {
  //   //   this.upsertUserPreferencesSupplyGroupHandler(upsertedData);
  //   // });
  // }

  // upsertUserPreferencesSupplyGroupHandler(supplyGroupDataToUpsert: UserPreferenceSupplyGroup) {
  //   this.userpreferencesService.upsertUserPreferencesSupplyGroupsWithSharedInfo([].concat(supplyGroupDataToUpsert));
  //   //the upserted data is subscribed to in refreshUserPreferencesSupplyGroups subscription;
  // }

  // // ---------------------------------Redux dispatch/Subscribe------------------------------------------//
  private getProjectsByFilterValues(demandFilterCriteriaObj: DemandFilterCriteria, isAdditionalProjects = false) {
    this.pageScrolled = isAdditionalProjects;
    this.projectStartIndex = this.pageScrolled ? this.projectStartIndex : 1;

    demandFilterCriteriaObj.projectStartIndex = this.projectStartIndex;
    demandFilterCriteriaObj.pageSize = this.pageSize;

    if (
      this.demandFilterCriteriaObj.officeCodes === "" ||
      demandFilterCriteriaObj.caseTypeCodes === "" ||
      demandFilterCriteriaObj.demandTypes === ""
    ) {
      this.projectsToBeDisplayed = [];
      return;
    }

    this.dispatchProjectsLoaderAction(true);

    if (!this.pageScrolled) {
      this.store.dispatch(
        new casePlanningActions.LoadProjects({
          demandFilterCriteriaObj
        })
      );
    } else {
      // this.store.dispatch(
      //   new casePlanningActions.LoadCasesOnPageScroll({
      //     demandFilterCriteriaObj
      //   })
      // );
    }
  }

  private getPlanningCardsByFilterValues(demandFilterCriteriaObj: DemandFilterCriteria) {
    if (!demandFilterCriteriaObj.demandTypes?.includes("PlanningCards")) {
      this.planningCardsToBeDisplayed = [];
      return;
    }

    this.dispatchProjectsLoaderAction(true);

    this.store.dispatch(
      new casePlanningActions.LoadPlanningCards({
        demandFilterCriteriaObj
      })
    );
  }

  // openCasePlanningWhiteboardPopupHandler() {
  //   const config = {
  //     ignoreBackdropClick: true,
  //     keyboard: false, //disable escape button close for planning board
  //     class: "white-board-modal modal-lg",
  //     initialState: {}
  //   };
  //   this.bsModalRef = this.modalService.show(PlanningBoardModalComponent, config);
  // }

  private dispatchProjectsLoaderAction(isShowProgressBar) {
    this.showProgressBar = isShowProgressBar;
    this.store.dispatch(new casePlanningActions.CasePlanningLoader(true));
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

      // load planning board
      this.loadPlanningBoardData();
    });
  }

  private updateSupplyAndDemandSettings() {
    const startDate = DateService.getStartOfWeek(new Date());
    let endDate = new Date();
    endDate.setDate(endDate.getDate() + 40);
    endDate = DateService.getEndOfWeek(true, endDate);

    // Default date range will be today + 40 days on load
    const defaultDateRange = DateService.getFormattedDateRange({
      startDate: startDate,
      endDate: endDate
    });

    if (this.userPreferences && typeof this.userPreferences === "object") {
      const today = new Date();
      let dateRangeForProjects: { startDate: any; endDate: any };

      /*-------------- Set user preferences for Supply Side ---------------------*/
      let dateRangeForResources: { startDate: any; endDate: any };

      if (this.userPreferences.supplyWeeksThreshold) {
        const today = new Date();
        let endDate = new Date();
        if(this.userPreferences.supplyWeeksThreshold > 6) {
          endDate = new Date(today.setDate(today.getDate() + this.userPreferences.supplyWeeksThreshold * 7));
        } else {
          endDate = new Date(today.setDate(startDate.getDate() + 40));
        }
        const date = { startDate: startDate, endDate: endDate };

        dateRangeForResources = DateService.getFormattedDateRange(date);
      } else {
        dateRangeForResources = defaultDateRange;
      }

      this.supplyFilterCriteriaObj.startDate = dateRangeForResources.startDate;
      this.supplyFilterCriteriaObj.endDate = dateRangeForResources.endDate;
      this.supplyFilterCriteriaObj.officeCodes =
        this.userPreferences.supplyViewOfficeCodes || this.homeOffice.officeCode.toString();
      this.supplyFilterCriteriaObj.levelGrades = this.userPreferences.levelGrades;
      this.supplyFilterCriteriaObj.availabilityIncludes = this.userPreferences.availabilityIncludes;
      this.supplyFilterCriteriaObj.staffingTags =
        this.userPreferences.supplyViewStaffingTags || ServiceLineEnum.GeneralConsulting;
      this.supplyFilterCriteriaObj.groupBy = this.userPreferences.groupBy;
      this.supplyFilterCriteriaObj.sortBy = this.userPreferences.sortBy;
      this.supplyFilterCriteriaObj.practiceAreaCodes = this.userPreferences.practiceAreaCodes;
      this.supplyFilterCriteriaObj.positionCodes = this.userPreferences.positionCodes;

      /*-------------- Set user preferences for Demand Side ---------------------*/

      if (this.userPreferences.demandWeeksThreshold) {
        const userSettingsEndDate = new Date(
          today.setDate(today.getDate() + this.userPreferences.demandWeeksThreshold * 7)
        );

        const differenceInDays = DateService.getDatesDifferenceInDays(startDate, userSettingsEndDate);
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
      this.demandFilterCriteriaObj.officeCodes =
        this.userPreferences.demandViewOfficeCodes || this.homeOffice.officeCode.toString();
      this.demandFilterCriteriaObj.caseTypeCodes = this.userPreferences.caseTypeCodes || CaseTypeEnum.Billable;
      this.demandFilterCriteriaObj.demandTypes = this.userPreferences.demandTypes;
      this.demandFilterCriteriaObj.opportunityStatusTypeCodes = this.userPreferences.opportunityStatusTypeCodes;
      this.demandFilterCriteriaObj.caseAttributeNames = this.userPreferences.caseAttributeNames || "";
      this.demandFilterCriteriaObj.minOpportunityProbability = this.userPreferences.minOpportunityProbability;
      this.demandFilterCriteriaObj.industryPracticeAreaCodes = this.userPreferences.industryPracticeAreaCodes;
      this.demandFilterCriteriaObj.capabilityPracticeAreaCodes = this.userPreferences.capabilityPracticeAreaCodes;

      //Don't need these variables for now
      this.demandFilterCriteriaObj.caseExceptionShowList = "";
      this.demandFilterCriteriaObj.caseExceptionHideList = "";
      this.demandFilterCriteriaObj.opportunityExceptionShowList = "";
      this.demandFilterCriteriaObj.opportunityExceptionHideList = "";
      this.demandFilterCriteriaObj.caseAllocationsSortBy = "";
      this.demandFilterCriteriaObj.planningCardsSortOrder = "";
      this.demandFilterCriteriaObj.caseOppSortOrder = "";

      this.updateDemandFilterCriteriaFromSupply();
    } else {
      this.demandFilterCriteriaObj.startDate = defaultDateRange.startDate;
      this.demandFilterCriteriaObj.endDate = defaultDateRange.endDate;
      this.demandFilterCriteriaObj.officeCodes = this.homeOffice.officeCode.toString();
      this.demandFilterCriteriaObj.caseTypeCodes = CaseTypeEnum.Billable;
    } 
    sessionStorage.setItem('demandFilterCriteriaObj', JSON.stringify(this.demandFilterCriteriaObj));
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
    this.capabilityPracticeAreas = this.localStorageService.get(
      ConstantsMaster.localStorageKeys.capabilityPracticeAreas
    );
    this.planningBoardBucketLookUp = this.localStorageService.get(
      ConstantsMaster.localStorageKeys.casePlanningBoardBuckets
    );
  }

  private setStoreSuscriptions() {
    this.initializeAndSubscribeUserPreferences();
    this.loadViewingFilterDropdown();

    this.getProjectsFromStore();
    this.getPlanningCardsFromStore();
    this.getMetricsDataFromStore();
    this.getProjectsFromStoreOnSearch();
   // this.refreshCaseAndResourceOverlayListener();
    this.projectsLoaderListener();
  }

  private getProjectsFromStore() {
    this.storeSub.add(
      this.store.pipe(select(fromProjects.getProjects)).subscribe((projectData: Project[]) => {
        // Check for filter values as async call might take some time to return data and hence,
        // if user select All Bain Offices and Unselect the next moment, records will show up
        // as first request took some time to return.
        if (this.demandFilterCriteriaObj.officeCodes === "" || projectData === undefined) {
          this.projectsToBeDisplayed = [];
        } else {
          this.loadProjectsData(projectData);
        }
      })
    );
  }

  private getPlanningCardsFromStore() {
    this.storeSub.add(
      this.store.pipe(select(fromProjects.getPlanningCards)).subscribe((planningCardsData: PlanningCard[]) => {
        //get only those planning cards that fall within the selected date range
        this.planningCards = planningCardsData.filter(
          (x) =>
            DateService.isSameOrBefore(x.startDate, this.demandFilterCriteriaObj.endDate) &&
            DateService.isSameOrAfter(x.endDate, this.demandFilterCriteriaObj.startDate)
        );
        if(this.flaggedGroupingOn) {
          this.planningCardsToBeDisplayed = this.planningCards.filter((planningCard) => planningCard.isFlagged);
        } else {
          this.planningCardsToBeDisplayed = this.planningCards;
        }
      })
    );
  }

  private getMetricsDataFromStore() {
    this.storeSub.add(
      combineLatest([
        this.store.pipe(select(fromProjects.getAvailabilityMetrics)),
        this.store.pipe(select(fromProjects.getProjects)),
        this.store.pipe(select(fromProjects.getPlanningCards))
      ])
      .subscribe(([supplyMetricsData, projects, planningCards]: [any, any, any]) => {

        if(supplyMetricsData) {
          this.supplyMetricsData = supplyMetricsData;
          this.demandMetricsProjects = projects.concat(planningCards);

          this.isSupplyMetricsLodaed = true;
          this.isDemandMetricsLoaded = true;

          this.getsixColumnsView();
          this.createMetricsForSupplyAndDemand();
        }
      })
    );
  }

  getsixColumnsView() {
    this.planningBoard = [];
    const numberOfWeeksForCasePlanningBoard = Number(`${this.coreService.appSettings.NumberOfWeeksForCasePlanningBoard}`);
    for (let i = 0; i < numberOfWeeksForCasePlanningBoard; i++) {
      const columnStartDate = new Date();
      columnStartDate.setDate(new Date(this.demandFilterCriteriaObj.startDate).getDate() + i * 7);

      let columnEndDate = new Date(columnStartDate);
      columnEndDate.setDate(columnStartDate.getDate() + 6);

      this.planningBoard.push({
        title: columnStartDate.toDateString(),
        projects: this.getProjectsForColumn(this.demandMetricsProjects, columnStartDate, columnEndDate),
      });
    }
  }

  getProjectsForColumn(projects, columnStartDate, columnEndDate) {
    return projects.filter((project) => {
      return DateService.isSameOrBefore(project.startDate, columnEndDate) && DateService.isSameOrAfter(project.endDate, columnStartDate);
    });
  }
  
  private getProjectsFromStoreOnSearch() {
    this.storeSub.add(
      this.store.pipe(select(fromProjects.getSearchedProjects)).subscribe((projectsData: Project[]) => {
        if (this.isSearchStringExist) {
          this.searchedProjects.next(projectsData);
        }
      })
    );
  }

  // private refreshCaseAndResourceOverlayListener() {
  //   this.storeSub.add(
  //     this.store.pipe(select(fromProjects.refreshCaseAndResourceOverlay)).subscribe((refreshNeeded: boolean) => {
  //       if (!refreshNeeded) {
  //         return;
  //       }
  //       this.refreshCaseAndResourceOverlay();
  //       this.dispatchRefreshCaseAndResourceOverlayAction(false);
  //     })
  //   );
  // }

  // private dispatchRefreshCaseAndResourceOverlayAction(refreshNeeded) {
  //   this.store.dispatch(new casePlanningActions.RefreshCaseAndResourceOverlay(refreshNeeded));
  // }

  // // -------------------------------------Helper Functions----------------------------------//
  // private refreshCaseAndResourceOverlay() {
  //   if (
  //     this.projectDialogRef &&
  //     this.projectDialogRef.componentInstance &&
  //     this.projectDialogRef.componentInstance.project &&
  //     Object.keys(this.projectDialogRef.componentInstance.project.projectDetails).length > 0
  //   ) {
  //     const projectData = this.projectDialogRef.componentInstance.project.projectDetails;
  //     this.projectDialogRef.componentInstance.getProjectDetails(projectData);
  //   }

  //   if (this.dialogRef && this.dialogRef.componentInstance) {
  //     const employeeCode = this.dialogRef.componentInstance.data.employeeCode;
  //     this.dialogRef.componentInstance.getDetailsForResource(employeeCode);
  //   }
  // }

  private loadProjectsData(projectsData: Project[]) {
    this.projects = projectsData;
    if(this.flaggedGroupingOn) {
      this.projectsToBeDisplayed = this.projects.filter((project) => project.isFlagged);
    } else {     
      this.projectsToBeDisplayed = projectsData;
    }
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
    this.storeSub.add(
      this.store.pipe(select(fromProjects.casePlanningLoader)).subscribe((isLoader: boolean) => {
        if (isLoader === false) {
          this.showProgressBar = false;
        }
      })
    );
  }

  // loadMoreCases() {
  //   this.getMoreActiveCasesOnPageScroll(this.demandFilterCriteriaObj);
  // }

  // private getMoreActiveCasesOnPageScroll(demandFilterCriteriaObj: DemandFilterCriteria) {
  //   this.projectStartIndex = this.projectStartIndex + this.pageSize;
  //   this.getProjectsByFilterValues(this.demandFilterCriteriaObj, true);
  // }

  toggleFiltersSection() {
    this.showFilters = !this.showFilters;
  }

  getProjectsOnDemandTypeChangeHandler(event) {
    this.demandFilterCriteriaObj.demandTypes = event;
    this.updateDemandFilterCriteriaFromSupply();
    this.getPlanningCardsByFilterValues(this.demandFilterCriteriaObj);
    this.getProjectsByFilterValues(this.demandFilterCriteriaObj);
    this.resetScroll();
    sessionStorage.setItem('demandFilterCriteriaObj', JSON.stringify(this.demandFilterCriteriaObj));
  }

  getProjectsOnGroupChangeHandler(event) {
    if (event == CaseGrouping.FLAGGED) {
      this.flaggedGroupingOn = true;
      this.projectsToBeDisplayed = this.projects.filter((project) => project.isFlagged);
      this.planningCardsToBeDisplayed = this.planningCards.filter((planningCard) => planningCard.isFlagged);
    } else {
      this.flaggedGroupingOn = false
      this.projectsToBeDisplayed = this.projects;
      this.planningCardsToBeDisplayed = this.planningCards;
    }

    this.demandMetricsProjects = this.projectsToBeDisplayed.concat(this.planningCardsToBeDisplayed);
    this.isSupplyMetricsLodaed = true;
    this.isDemandMetricsLoaded = true;

    this.getsixColumnsView();
    this.createMetricsForSupplyAndDemand();
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
    sessionStorage.setItem('demandFilterCriteriaObj', JSON.stringify(this.demandFilterCriteriaObj));

    this.updateDemandFilterCriteriaFromSupply();
    this.getPlanningCardsByFilterValues(this.demandFilterCriteriaObj);
    this.getProjectsByFilterValues(this.demandFilterCriteriaObj);
    this.resetScroll();
  }

  // reloadProjects() {
  //   this.resetScroll();
  //   this.getProjectsByFilterValues(this.demandFilterCriteriaObj);
  // }

  // //-------------------------Child Output events/Handlers ---------------------------//
  // openPlaceholderForm(modalData) {
  //   this.placeholderDialogService.openPlaceholderFormHandler(modalData);
  // }

  upsertCasePlanningNoteHandler(caseViewNote: ResourceOrCasePlanningViewNote) {
    if (caseViewNote) {
      this.dispatchUpsertCaseViewNoteActionHandler(caseViewNote);
    }
  }

  deleteCasePlanningNotesHandler(idsToBeDeleted: string) {
    if (idsToBeDeleted.length > 0) {
      this.store.dispatch(new casePlanningActions.DeleteCasePlanningViewNotes(idsToBeDeleted));
    }
  }

  // openAddTeamSkuFormHandler(projectToOpen) {

  //   if (projectToOpen.oldCaseCode) {
  //     this.casePlanningService.getPlaceholderAllocationsByOldCaseCodes(projectToOpen.oldCaseCode).subscribe(data => {
  //       projectToOpen.placeholderAllocations = data;
  //       this.openAddTeamsForm(projectToOpen);
  //     });
  //   } else if (projectToOpen.pipelineId) {
  //     this.casePlanningService.getPlaceholderAllocationsByPipelineIds(projectToOpen.pipelineId).subscribe(data => {
  //       projectToOpen.placeholderAllocations = data;
  //       this.openAddTeamsForm(projectToOpen);
  //     });
  //   } else if (projectToOpen.planningCardId) {
  //     this.casePlanningService.getPlaceholderAllocationsByPlanningCardIds(projectToOpen.planningCardId).subscribe(data => {
  //       projectToOpen.placeholderAllocations = data;
  //       this.openAddTeamsForm(projectToOpen);
  //     });
  //   }

  // }

  openAddTeamSkuFormHandler(projectToOpen) {

    const modalRef = this.modalService.show(AddTeamSkuComponent, {
      animated: true,
      backdrop: false,
      ignoreBackdropClick: true,
      initialState: {
        selectedCase: projectToOpen,
        autoCalculate: true,
        isCopyCortexHidden: false
      },
      class: "sku-modal modal-dialog-centered"
    });

    // inserts & updates placeholder data when changes are made to placeholder
    this.subData.add(modalRef.content.upsertPlaceholderAllocationsToProject.subscribe(updatedData => {
      this.store.dispatch(
        new ProjectAllocationsActions.UpsertPlaceholderAllocations({
          placeholderAllocations: updatedData.placeholderAllocations,
        })
      );
    }));

    this.subData.add(modalRef.content.deletePlaceHoldersByIds.subscribe(event => {
      const placeholderAllocation = {
        oldCaseCode: projectToOpen.oldCaseCode,
        pipelineId: projectToOpen.pipelineId,
        planningCardId: projectToOpen.planningCardId
      };
      this.store.dispatch(
        new ProjectAllocationsActions.DeletePlaceholderAllocations({          
          placeholderIds : event.placeholderIds,
          placeholderAllocation: [].concat(placeholderAllocation),
        })
      )
    }));

    // inserts & updates if the placeholder data has been created for Cortex SKU
    this.subData.add(modalRef.content.upsertPlaceholderCreatedForCortexPlaceholders.subscribe(upsertedData => {
      this.placeholderAssignmentService.upsertPlaceholderCreatedForCortexInfo(upsertedData);
    }));

  }

  // // open update case cards
  // openUpdateCaseCardHandler(selectedProject) {
  //   if (this.isProjectAvailableonBoard(selectedProject)) {
  //     this.notifyService.showValidationMsg(ValidationService.projectExistsonBoard);
  //     return;
  //   }

  //   let planningBoardColumnsList = this.planningBoard
  //     .map((data) => {
  //       return {
  //         "text": data.title,
  //         "value": data.title
  //       };
  //     });

  //   if (this.isPreviousWeekData) {
  //     let extendedplanningBoardColumnsList = planningBoardColumnsList
  //       .map((data) => {
  //         return {
  //           "text": DateService.addWeeks(data.text, 6),
  //           "value": DateService.addWeeks(data.value, 6),
  //         };
  //       })
  //     planningBoardColumnsList = planningBoardColumnsList.concat(extendedplanningBoardColumnsList);
  //   }

  //   const config = {
  //     ignoreBackdropClick: true,
  //     class: 'modal-dialog-centered',
  //     initialState: {
  //       projectToAdd: selectedProject,
  //       planningBoardColumnsList: planningBoardColumnsList
  //     }
  //   };

  //   let bsModalRef = this.modalService.show(UpdateCaseCardComponent, config);

  //   // save
  //   // bsModalRef.content.saveSelectedProjectToBoard.subscribe((selectedData) => {
  //   //   this.updateProjectChanges(selectedData);
  //   // });

  //   // add to board
  //   // bsModalRef.content.addSelectedProjectToBoard.subscribe(selectedData => {
  //   //   this.addSelectedProjectToBoardHandler(selectedData);
  //   // });
  // }


  // public addResourceCommitmentHandler(resourceCommitment) {
  //   this.dispatchAddResourceCommitmentAction(resourceCommitment);
  // }

  // public updateResourceCommitmentHandler(updatedResourceCommitment) {
  //   this.dispatchUpdateResourceCommitmentAction(updatedResourceCommitment);
  // }

  // private deleteResourceCommitmentHandler(deletedCommitmentId) {
  //   this.dispatchDeleteResourceCommitmentAction(deletedCommitmentId);
  // }

  private dispatchUpsertCaseViewNoteActionHandler(upsertedNote) {
    this.upsertCaseViewNoteAction(upsertedNote);
  }

  // // planning board
  loadPlanningBoardData() {
    this.getAvailabilityMetricsByFilterValues();
    this.getCasePlanningBoardDataBySelectedValues();
  }

  getAvailabilityMetricsByFilterValues() {
    this.isSupplyMetricsLodaed = false;
    let supplyFilterCriteriaObj = JSON.parse(JSON.stringify(this.supplyFilterCriteriaObj));

    if (this.isPreviousWeekData) {
      supplyFilterCriteriaObj.startDate = DateService.addWeeks(this.supplyFilterCriteriaObj.startDate, -7);
      supplyFilterCriteriaObj.endDate = DateService.addWeeks(this.supplyFilterCriteriaObj.startDate, 7);
    } else {
      supplyFilterCriteriaObj.startDate = DateService.addWeeks(
        DateService.convertDateInBainFormat(DateService.getStartOfWeek()),
        -1
      );

      // let endOfDuration = new Date(supplyFilterCriteriaObj.startDate);
      // supplyFilterCriteriaObj.endDate = DateService.convertDateInBainFormat(
      //   new Date(endOfDuration.setDate(endOfDuration.getDate() + 6 * 7 + 4))
      // );

      let endDate = new Date(supplyFilterCriteriaObj.startDate);
      endDate.setDate(endDate.getDate() + 63);
      endDate = DateService.getEndOfWeek(true, endDate);

      supplyFilterCriteriaObj.endDate = DateService.convertDateInBainFormat(endDate);
    }

    this.store.dispatch(
      new casePlanningActions.LoadAvailabilityMetrics({
        supplyFilterCriteriaObj
      })
    );
  }

  getCasePlanningBoardDataBySelectedValues() {
    this.isDemandMetricsLoaded = false;
    let demandFilterCriteriaObj = JSON.parse(JSON.stringify(this.demandFilterCriteriaObj));

    if (this.isPreviousWeekData) {
      demandFilterCriteriaObj.startDate = DateService.addWeeks(this.demandFilterCriteriaObj.startDate, -6);
      demandFilterCriteriaObj.endDate = DateService.addWeeks(this.demandFilterCriteriaObj.startDate, 6);
    } else {
      demandFilterCriteriaObj.startDate = DateService.convertDateInBainFormat(DateService.getStartOfWeek());

      let endDate = new Date(demandFilterCriteriaObj.startDate);
      endDate.setDate(endDate.getDate() + 40);
      endDate = DateService.getEndOfWeek(true, endDate);

      demandFilterCriteriaObj.endDate = DateService.convertDateInBainFormat(endDate);
    }

    this.store.dispatch(
      new casePlanningActions.LoadMetricsDemandData({
        demandFilterCriteriaObj
      })
    );
  }

  createMetricsForSupplyAndDemand() {
    if (!this.isSupplyMetricsLodaed || !this.isDemandMetricsLoaded) {
      return;
    }

    this.planningBoardColumnMetrics = [];
    this.planningBoard.forEach((planningBoardColumnData) => {
      const columnDate = new Date(planningBoardColumnData.title).getTime();

      const projectsToIncludeInDemand = planningBoardColumnData.projects.filter((project) => {
          return project.includeInDemand == true;
      });

      let demandMetricsData = this.getSkuUsedForCalculatingDemand(projectsToIncludeInDemand);
      const metrics = [].concat(this.createMetricsData(planningBoardColumnData.title, demandMetricsData));

      this.planningBoardColumnMetrics.push({
        title: planningBoardColumnData.title,
        date: planningBoardColumnData.date,
        metrics: metrics
      });
    })
  }

  getSkuUsedForCalculatingDemand(staffFromMySupplyProjects) {
    var skuTerms = [];
    staffFromMySupplyProjects.forEach((project) => {
      if (project.skuTerm != null) {
        skuTerms.push(...project.skuTerm);
      }
    });
    return skuTerms;
  }

  createMetricsData(colName, demandMetricsDataForColumn) {
    let metricsData = [
      { name: "Supply", id: "supply", data: this.createSupplyMetricsData(colName) },
      { name: "Demand", id: "demand", data: this.createDemandMetricsData(demandMetricsDataForColumn) },
      { name: "Balance", id: "balance", data: this.createBalanceMetricsData() }
    ];

    return metricsData;
  }

  getBucketsData(projects, colName) {
    let bucketList = [];

    if (colName === this.NEW_DEMAND_COLUMN_TITLE) {
      let bucketrow = {
        bucketId: this.planningBoardBucketLookUp[0].id,
        bucketName: this.planningBoardBucketLookUp[0].bucketName,
        includeInDemand: this.planningBoardBucketLookUp[0].includeInDemand,
        isPartiallyChcked: this.planningBoardBucketLookUp[0].isPartiallyChecked,
        date: null,
        projects: projects
      };

      bucketList.push(bucketrow);
    } else {
      this.planningBoardBucketLookUp.forEach((element) => {
        let bucketedProjects = projects.filter((x) => x.bucketId === element.id);

        let bucketrow = {
          bucketId: element.id,
          bucketName: element.bucketName,
          includeInDemand: element.includeInDemand,
          isPartiallyChecked: element.isPartiallyChecked,
          date: colName,
          projects: bucketedProjects
        };

        bucketList.push(bucketrow);
      });
    }

    return bucketList;
  }

  // create metrics data
  createSupplyMetricsData(colName) {
    var levels = [];
    var levelGrades = [];

    let supplyReturnObject = [
      {
        name: "Team",
        id: "team",
        visible: false,
        available: [{ name: "Available", id: "avail", levels: [] }],
        prospective: [{ name: "Prospective", id: "prospective", levels: [] }]
      },
      {
        name: "SMAP",
        id: "smap",
        visible: false,
        available: [{ name: "Available", id: "avail", levels: [] }],
        prospective: [{ name: "Prospective", id: "prospective", levels: [] }]
      },
      {
        name: "Partner",
        id: "partner",
        visible: false,
        available: [{ name: "Available", id: "avail", levels: [] }],
        prospective: [{ name: "Prospective", id: "prospective", levels: [] }]
      },
      {
        name: "Additional Expertise",
        id: "additionalExpertise",
        visible: false,
        available: [{ name: "Available", id: "avail", levels: [] }],
        prospective: [{ name: "Prospective", id: "prospective", levels: [] }]
      }
    ];

    const smapAvailableLevels = supplyReturnObject.find((x) => x.id == "smap").available[0].levels;
    const smapProspectiveLevels = supplyReturnObject.find((x) => x.id == "smap").prospective[0].levels;
    const teamAvailableLevels = supplyReturnObject.find((x) => x.id == "team").available[0].levels;
    const teamProspectiveLevels = supplyReturnObject.find((x) => x.id == "team").prospective[0].levels;
    const partnerAvailableLevels = supplyReturnObject.find((x) => x.id == "partner").available[0].levels;
    const partnerProspectiveLevels = supplyReturnObject.find((x) => x.id == "partner").prospective[0].levels;
    const additionalExpertiseAvailableLevels = supplyReturnObject.find((x) => x.id == "additionalExpertise")
      .available[0].levels;
    const additionalExpertiseProspectiveLevels = supplyReturnObject.find((x) => x.id == "additionalExpertise")
      .prospective[0].levels;

    let weekData = this.supplyMetricsData?.availabilityMetrics?.filter(
      (x) => new Date(x.week_Of).getTime() == new Date(colName).getTime()
    );

    let weekDataDetails = this.supplyMetricsData?.availabilityMetrics_Nupur?.filter(
      (x) => new Date(x.week_Of).getTime() == new Date(colName).getTime()
    );

    let previousweekDataDetails = this.supplyMetricsData?.availabilityMetrics_Nupur?.filter(
      (x) => new Date(x.week_Of).getTime() == new Date(DateService.addWeeks(new Date(colName), -1)).getTime()
    );

    //create selected levels array from staffing settings
    if (this.supplyFilterCriteriaObj.levelGrades == "") {
      levels = this.localStorageService
        .get(ConstantsMaster.localStorageKeys.levelGradesHierarchy)
        .map((item) => item.value);
    } else {
      this.supplyFilterCriteriaObj.levelGrades?.split(",").forEach((level) => {
        levels.push(level.replace(/[\d.+]/g, ""));
        levelGrades.push(level);
      });
      levels = [...new Set(levels)];
    }

    levels.forEach((levelGrade) => {
      switch (levelGrade) {
        case "M":
          if (!smapAvailableLevels?.find((x) => x.name == levelGrade)) {
            smapAvailableLevels.push({
              name: levelGrade,
              levelGrades: [
                {
                  name: null,
                  supply: 0,
                  members: null
                }
              ]
            });
          }
          if (!smapProspectiveLevels?.find((x) => x.name == levelGrade)) {
            smapProspectiveLevels.push({
              name: levelGrade,
              levelGrades: [
                {
                  name: null,
                  supply: 0,
                  members: null
                }
              ]
            });
          }
          break;
        case "A":
        case "C":
          if (!teamAvailableLevels?.find((x) => x.name == levelGrade)) {
            teamAvailableLevels.push({
              name: levelGrade,
              levelGrades: [
                {
                  name: null,
                  supply: 0,
                  members: null
                }
              ]
            });
          }
          if (!teamProspectiveLevels?.find((x) => x.name == levelGrade)) {
            teamProspectiveLevels.push({
              name: levelGrade,
              levelGrades: [
                {
                  name: null,
                  supply: 0,
                  members: null
                }
              ]
            });
          }
          break;
        case "V":
          if (!partnerAvailableLevels?.find((x) => x.name == levelGrade)) {
            partnerAvailableLevels.push({
              name: levelGrade,
              levelGrades: [
                {
                  name: null,
                  supply: 0,
                  members: null
                }
              ]
            });
          }
          if (!partnerProspectiveLevels?.find((x) => x.name == levelGrade)) {
            partnerProspectiveLevels.push({
              name: levelGrade,
              levelGrades: [
                {
                  name: null,
                  supply: 0,
                  members: null
                }
              ]
            });
          }
          break;
        default:
          if (!additionalExpertiseAvailableLevels?.find((x) => x.name == levelGrade)) {
            additionalExpertiseAvailableLevels.push({
              name: levelGrade,
              levelGrades: [
                {
                  name: null,
                  supply: 0,
                  members: null
                }
              ]
            });
          }
          if (!additionalExpertiseProspectiveLevels?.find((x) => x.name == levelGrade)) {
            additionalExpertiseProspectiveLevels.push({
              name: levelGrade,
              levelGrades: [
                {
                  name: null,
                  supply: 0,
                  members: null
                }
              ]
            });
          }
          break;
      }
    });

    weekData?.forEach((element) => {
      let membersInMetrics = this.getMembers(weekDataDetails, element);
      this.highlightNewlyAvailableMembers(weekDataDetails, previousweekDataDetails);
      let supplyCount = this.getSupplyCount(membersInMetrics, element);

      switch (element.level) {
        case "M":
          {
            if (element.capacitySubCategory == "Available") {
              if (smapAvailableLevels?.find((x) => x.name == element.level)) {
                smapAvailableLevels
                  ?.find((x) => x.name == element.level)
                  .levelGrades.push({
                    name: element.currentLevelGrade,
                    supply: supplyCount,
                    members: membersInMetrics
                  });
              } else {
                smapAvailableLevels.push({
                  name: element.level,
                  levelGrades: [
                    {
                      name: element.currentLevelGrade,
                      supply: supplyCount,
                      members: membersInMetrics
                    }
                  ]
                });
              }
            } else {
              if (smapProspectiveLevels?.find((x) => x.name == element.level)) {
                smapProspectiveLevels
                  ?.find((x) => x.name == element.level)
                  .levelGrades.push({
                    name: element.currentLevelGrade,
                    supply: supplyCount,
                    members: membersInMetrics
                  });
              } else {
                smapProspectiveLevels?.push({
                  name: element.level,
                  levelGrades: [
                    {
                      name: element.currentLevelGrade,
                      supply: supplyCount,
                      members: membersInMetrics
                    }
                  ]
                });
              }
            }
          }
          break;
        case "A":
        case "C":
          {
            if (element.capacitySubCategory == "Available") {
              if (teamAvailableLevels?.find((x) => x.name == element.level)) {
                teamAvailableLevels
                  ?.find((x) => x.name == element.level)
                  .levelGrades.push({
                    name: element.currentLevelGrade,
                    supply: supplyCount,
                    members: membersInMetrics
                  });
              } else {
                teamAvailableLevels?.push({
                  name: element.level,
                  levelGrades: [
                    {
                      name: element.currentLevelGrade,
                      supply: supplyCount,
                      members: membersInMetrics
                    }
                  ]
                });
              }
            } else {
              if (teamProspectiveLevels?.find((x) => x.name == element.level)) {
                teamProspectiveLevels
                  ?.find((x) => x.name == element.level)
                  .levelGrades.push({
                    name: element.currentLevelGrade,
                    supply: supplyCount,
                    members: membersInMetrics
                  });
              } else {
                teamProspectiveLevels?.push({
                  name: element.level,
                  levelGrades: [
                    {
                      name: element.currentLevelGrade,
                      supply: supplyCount,
                      members: membersInMetrics
                    }
                  ]
                });
              }
            }
          }
          break;
        case "V":
          {
            if (element.capacitySubCategory == "Available") {
              if (partnerAvailableLevels?.find((x) => x.name == element.level)) {
                partnerAvailableLevels
                  ?.find((x) => x.name == element.level)
                  .levelGrades.push({
                    name: element.currentLevelGrade,
                    supply: supplyCount,
                    members: membersInMetrics
                  });
              } else {
                partnerAvailableLevels?.push({
                  name: element.level,
                  levelGrades: [
                    {
                      name: element.currentLevelGrade,
                      supply: supplyCount,
                      members: membersInMetrics
                    }
                  ]
                });
              }
            } else {
              if (partnerProspectiveLevels?.find((x) => x.name == element.level)) {
                partnerProspectiveLevels
                  ?.find((x) => x.name == element.level)
                  .levelGrades.push({
                    name: element.currentLevelGrade,
                    supply: supplyCount,
                    members: membersInMetrics
                  });
              } else {
                partnerProspectiveLevels?.push({
                  name: element.level,
                  levelGrades: [
                    {
                      name: element.currentLevelGrade,
                      supply: supplyCount,
                      members: membersInMetrics
                    }
                  ]
                });
              }
            }
          }
          break;
        default:
          {
            if (element.capacitySubCategory == "Available") {
              if (additionalExpertiseAvailableLevels?.find((x) => x.name == element.level)) {
                additionalExpertiseAvailableLevels
                  ?.find((x) => x.name == element.level)
                  .levelGrades.push({
                    name: element.currentLevelGrade,
                    supply: supplyCount,
                    members: membersInMetrics
                  });
              } else {
                additionalExpertiseAvailableLevels?.push({
                  name: element.level,
                  levelGrades: [
                    {
                      name: element.currentLevelGrade,
                      supply: supplyCount,
                      members: membersInMetrics
                    }
                  ]
                });
              }
            } else {
              if (additionalExpertiseProspectiveLevels?.find((x) => x.name == element.level)) {
                additionalExpertiseProspectiveLevels
                  ?.find((x) => x.name == element.level)
                  .levelGrades.push({
                    name: element.currentLevelGrade,
                    supply: supplyCount,
                    members: membersInMetrics
                  });
              } else {
                additionalExpertiseProspectiveLevels?.push({
                  name: element.level,
                  levelGrades: [
                    {
                      name: element.currentLevelGrade,
                      supply: supplyCount,
                      members: membersInMetrics
                    }
                  ]
                });
              }
            }
          }
          break;
      }
    });
    this.setVisibilityOfLevelsInMetrics(supplyReturnObject, levels);

    return supplyReturnObject;
  }

  createDemandMetricsData(demandMetricsData) {
    var levels = [];

    let demandReturnObject = [
      { name: "Team", id: "team", levels: [], visible: false },
      { name: "SMAP", id: "smap", levels: [], visible: false },
      { name: "Partner", id: "partner", levels: [], visible: false },
      { name: "Additional Expertise", id: "additionalExpertise", levels: [], visible: false }
    ];

    const teamLevels = demandReturnObject.find((x) => x.id == "team").levels;
    const smapLevels = demandReturnObject.find((x) => x.id == "smap").levels;
    const partnerLevels = demandReturnObject.find((x) => x.id == "partner").levels;
    const additionalExpertiseLevels = demandReturnObject.find((x) => x.id == "additionalExpertise").levels;

    levels = this.getLevelGradesSelectedInSupply();
    this.showDefaultValueForSelectedLevelGrades(
      levels,
      smapLevels,
      teamLevels,
      partnerLevels,
      additionalExpertiseLevels
    );

    //sum for each level
    demandMetricsData.forEach((skuData) => {
      //ADD SKU's for level grades that are selected by users. Ignore for others.

      if (levels.includes(skuData.level) && skuData.aggregateDemand) {
        switch (skuData.level) {
          case "M": {
            if (smapLevels?.find((x) => x.name == skuData.level)) {
              let level = smapLevels?.find((x) => x.name == skuData.level);
              level.sum += Number(parseFloat(skuData.aggregateDemand));
            } else {
              smapLevels.push({
                name: skuData.level,
                sum: Number(parseFloat(skuData.aggregateDemand))
              });
            }
            break;
          }
          case "A":
          case "C": {
            if (teamLevels?.find((x) => x.name == skuData.level)) {
              let level = teamLevels?.find((x) => x.name == skuData.level);
              level.sum += Number(parseFloat(skuData.aggregateDemand));
            } else {
              teamLevels.push({
                name: skuData.level,
                sum: Number(parseFloat(skuData.aggregateDemand))
              });
            }
            break;
          }
          case "V": {
            if (partnerLevels?.find((x) => x.name == skuData.level)) {
              let level = partnerLevels?.find((x) => x.name == skuData.level);
              level.sum += Number(parseFloat(skuData.aggregateDemand));
            } else {
              partnerLevels.push({
                name: skuData.level,
                sum: Number(parseFloat(skuData.aggregateDemand))
              });
            }
            break;
          }
          default: {
            if (additionalExpertiseLevels?.find((x) => x.name == skuData.level)) {
              let level = additionalExpertiseLevels?.find((x) => x.name == skuData.level);
              level.sum += Number(parseFloat(skuData.aggregateDemand));
            } else {
              additionalExpertiseLevels.push({
                name: skuData.level,
                sum: Number(parseFloat(skuData.aggregateDemand))
              });
            }
            break;
          }
        }
      }
    });
    this.setVisibilityOfLevelsInMetrics(demandReturnObject, levels);

    return demandReturnObject;
  }

  createBalanceMetricsData() {
    var levels = [];

    let balanceReturnObject = [
      { name: "Team", id: "team", levels: [], visible: false },
      { name: "SMAP", id: "smap", levels: [], visible: false },
      { name: "Partner", id: "partner", levels: [], visible: false },
      { name: "Additional Expertise", id: "additionalExpertise", levels: [], visible: false }
    ];
    const teamLevels = balanceReturnObject.find((x) => x.id == "team").levels;
    const smapLevels = balanceReturnObject.find((x) => x.id == "smap").levels;
    const partnerLevels = balanceReturnObject.find((x) => x.id == "partner").levels;
    const additionalExpertiseLevels = balanceReturnObject.find((x) => x.id == "additionalExpertise").levels;

    levels = this.getLevelGradesSelectedInSupply();
    this.showDefaultValueForSelectedLevelGrades(
      levels,
      smapLevels,
      teamLevels,
      partnerLevels,
      additionalExpertiseLevels
    );
    this.setVisibilityOfLevelsInMetrics(balanceReturnObject, levels);

    return balanceReturnObject;
  }

  getLevelGradesSelectedInSupply() {
    var levels = [];
    var levelGrades = [];

    //create selected levels array from staffing settings
    if (this.supplyFilterCriteriaObj.levelGrades == "") {
      levels = this.localStorageService
        .get(ConstantsMaster.localStorageKeys.levelGradesHierarchy)
        .map((item) => item.value);
    } else {
      this.supplyFilterCriteriaObj.levelGrades?.split(",").forEach((level) => {
        levels.push(level.replace(/[\d.+]/g, ""));
        levelGrades.push(level);
      });
      levels = [...new Set(levels)];
    }
    return levels;
  }

  showDefaultValueForSelectedLevelGrades(levels, smapLevels, teamLevels, partnerLevels, additionalExpertiseLevels) {
    levels.forEach((level) => {
      switch (level) {
        case "M":
          if (!smapLevels?.find((x) => x.name == level))
            smapLevels.push({
              name: level,
              sum: 0
            });
          break;
        case "A":
        case "C":
          if (!teamLevels?.find((x) => x.name == level))
            teamLevels.push({
              name: level,
              sum: 0
            });
          break;
        case "V":
          if (!partnerLevels?.find((x) => x.name == level))
            partnerLevels.push({
              name: level,
              sum: 0
            });
          break;
        default:
          if (!additionalExpertiseLevels?.find((x) => x.name == level))
            additionalExpertiseLevels.push({
              name: level,
              sum: 0
            });
          break;
      }
    });
  }

  setVisibilityOfLevelsInMetrics(returnObject, levels) {
    //create return object and set visibility based on selected levels in settings
    if (!this.supplyFilterCriteriaObj.levelGrades || levels.includes("A") || levels.includes("C")) {
      returnObject.find((x) => x.id == "team").visible = true;
    }
    if (!this.supplyFilterCriteriaObj.levelGrades || levels.includes("M")) {
      returnObject.find((x) => x.id == "smap").visible = true;
    }
    if (!this.supplyFilterCriteriaObj.levelGrades || levels.includes("V")) {
      returnObject.find((x) => x.id == "partner").visible = true;
    }
    if (
      !this.supplyFilterCriteriaObj.levelGrades ||
      levels.filter(
        (levels) => !levels.includes("A") && !levels.includes("C") && !levels.includes("M") && !levels.includes("V")
      ).length > 0
    ) {
      returnObject.find((x) => x.id == "additionalExpertise").visible = true;
    }
  }

  private getMembers(weekDataDetails, element) {
    if (!!weekDataDetails && !!element)
      return weekDataDetails.filter(
        (x) => x.currentLevelGrade == element.currentLevelGrade && x.capacitySubCategory == element.capacitySubCategory
      );

    return null;
  }

  private highlightNewlyAvailableMembers(weekDataDetails, previousweekDataDetails) {
    weekDataDetails.forEach((x) => {
      if (
        !previousweekDataDetails.some(
          (previousWeekEmployee) =>
            x.employeeCode == previousWeekEmployee.employeeCode &&
            x.capacitySubCategory == previousWeekEmployee.capacitySubCategory
        )
      ) {
        x.highlight = true;
      }
    });
  }

  private getSupplyCount(membersInMetrics, element) {
    if (!!membersInMetrics && !!element)
      return !this.isCountOfIndividualResourcesToggle
        ? Number(parseFloat(element.aggregateAvailability))
        : membersInMetrics.length;

    return null;
  }

  // ------------------Pop-Ups -----------------------------------------------------------
  public openQuickAddFormHandler(modalData) {
    // class is required to center align the modal on large screens
    if (modalData) {
      const config = {
        class: "modal-dialog-centered",
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
        class: "modal-dialog-centered",
        ignoreBackdropClick: true
      };

      this.bsModalRef = this.modalService.show(QuickAddFormComponent, config);
    }

    // TODO: Same functionality is available in QuickAddDialog.Service file
    // There should be one place to register all emitter events to prevent functionality split
    // from different parts of the screen like
    // open overlay from home vs resource tab
    // this.storeSub.add(
    //   this.bsModalRef.content.insertResourcesCommitments.subscribe((commitment) => {
    //     this.addResourceCommitmentHandler(commitment);
    //   })
    // );

    // this.storeSub.add(
    //   this.bsModalRef.content.updateResourceCommitment.subscribe((updatedCommitment) => {
    //     this.updateResourceCommitmentHandler(updatedCommitment.resourceAllocation);
    //   })
    // );

    // this.storeSub.add(
    //   this.bsModalRef.content.deleteResourceCommitment.subscribe((deletedObjId) => {
    //     this.deleteResourceCommitmentHandler(deletedObjId);
    //   })
    // );

    // inserts & updates resource data when changes are made to resource
    // this.storeSub.add(
    //   this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe((upsertedAllocations) => {
    //     this.upsertResourceAllocationsToProjectHandler(upsertedAllocations.resourceAllocation);
    //   })
    // );

    // this.storeSub.add(
    //   this.bsModalRef.content.deleteResourceAllocationFromCase.subscribe((deletedObj) => {
    //     this.deleteResourceAssignmentFromProjectHandler(deletedObj.allocationId);
    //     this.resourceAssignmentService.deletePlaygroundAllocationsForCasePlanningMetrics(deletedObj.resourceAllocation);
    //   })
    // );

    // this.storeSub.add(
    //   this.bsModalRef.content.openBackFillPopUp.subscribe((result) => {
    //     this.openBackFillFormHandler(result);
    //   })
    // );

    // this.storeSub.add(
    //   this.bsModalRef.content.openOverlappedTeamsPopup.subscribe((result) => {
    //     this.openOverlappedTeamsFormHandler(result);
    //   })
    // );
  }

  // private openBackFillFormHandler(event) {
  //   // class is required to center align the modal on large screens
  //   const config = {
  //     class: "modal-dialog-centered",
  //     ignoreBackdropClick: true,
  //     initialState: {
  //       project: event.project,
  //       resourceAllocation: event.resourceAllocation,
  //       showMoreThanYearWarning: event.showMoreThanYearWarning
  //     }
  //   };
  //   this.bsModalRef = this.modalService.show(BackfillFormComponent, config);

  //   this.storeSub.add(
  //     this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe((data) => {
  //       event.project.allocatedResources = event.project.allocatedResources.concat(data.resourceAllocation);
  //       this.upsertResourceAllocationsToProjectHandler(data.resourceAllocation);
  //     })
  //   );
  // }

  // private openOverlappedTeamsFormHandler(event) {
  //   // class is required to center align the modal on large screens
  //   const config = {
  //     class: "modal-dialog-centered",
  //     ignoreBackdropClick: true,
  //     initialState: {
  //       projectData: event.projectData,
  //       overlappedTeams: event.overlappedTeams,
  //       allocation: event.allocation
  //     }
  //   };
  //   this.bsModalRef = this.modalService.show(OverlappedTeamsFormComponent, config);

  //   this.storeSub.add(
  //     this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe((data) => {
  //       this.upsertResourceAllocationsToProjectHandler(data.resourceAllocation);
  //     })
  //   );
  // }

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
      class: "modal-dialog-centered",
      ignoreBackdropClick: true,
      initialState: initialState
    };

    this.bsModalRef = this.modalService.show(PlaceholderFormComponent, config);

    // inserts & updates resource data when changes are made to resource
    this.bsModalRef.content.upsertPlaceholderAllocationsToProject.subscribe((updatedCommitment) => {
      this.placeholderAssignmentService.upsertPlcaeholderAllocations(updatedCommitment, null, this.projectDialogRef);
    });

    this.bsModalRef.content.deletePlaceholderAllocationByIds.subscribe((event) => {
      this.placeholderAssignmentService.deletePlaceHoldersByIds(event, this.projectDialogRef);
    });

    this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe((event) => {
      this.resourceAssignmentService.upsertResourceAllocationsToProject(event, null, this.projectDialogRef);
      this.resourceAssignmentService.upsertPlaygroundAllocationsForCasePlanningMetrics(event.resourceAllocation);
    });

    this.bsModalRef.content.openBackFillPopUp.subscribe((result) => {
      this.backfillDialogService.projectDialogRef = this.projectDialogRef;
      this.backfillDialogService.openBackFillFormHandler(result);
    });
  }

  // private openNotesDialogHandler(event) {
  //   // class is required to center align the modal on large screens
  //   const config = {
  //     class: "modal-dialog-centered",
  //     ignoreBackdropClick: true,
  //     initialState: {
  //       projectData: event.projectData,
  //       popupType: event.popupType
  //     }
  //   };
  //   this.bsModalRef = this.modalService.show(AgGridNotesComponent, config);

  //   // inserts & updates resource data when changes are made to notes of an allocation
  //   this.storeSub.add(
  //     this.bsModalRef.content.updateNotesForAllocation.subscribe((updatedData) => {
  //       this.upsertResourceAllocationsToProjectHandler(updatedData.resourceAllocation);
  //     })
  //   );
  // }

  // opens from - resource-overlay.component, project-overlay.component
  // openSplitAllocationDialogHandler(event) {
  //   // check if the popup is already open
  //   // if (!this.bsModalRef) {
  //   // class is required to center align the modal on large screens
  //   const config = {
  //     class: "modal-dialog-centered",
  //     ignoreBackdropClick: true,
  //     initialState: {
  //       allocationData: event.allocationData,
  //       popupType: event.popupType
  //     }
  //   };
  //   this.bsModalRef = this.modalService.show(AgGridSplitAllocationPopUpComponent, config);

  //   // inserts & updates resource data when changes are made to notes of an allocation
  //   this.storeSub.add(
  //     this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe((upsertData) => {
  //       this.upsertResourceAllocationsToProjectHandler(upsertData.resourceAllocation);
  //     })
  //   );

  //   // clear bsModalRef value on closing modal
  //   this.storeSub.add(
  //     this.modalService.onHidden.subscribe(() => {
  //       this.bsModalRef = null;
  //     })
  //   );
  //   // }
  // }

  // public upsertResourceAllocationsToProjectHandler(resourceAllocation) {
  //   let addedResourceAsArray = [];
  //   const addedResource = resourceAllocation;
  //   if (Array.isArray(addedResource)) {
  //     addedResourceAsArray = addedResource;
  //   } else {
  //     addedResourceAsArray.push(addedResource);
  //   }
  //   this.dispatchUpsertResourceAction(addedResourceAsArray);
  // }

  // private dispatchUpsertResourceAction(upsertedAllocations) {
  //   this.store.dispatch(new casePlanningActions.UpsertResourceStaffing(upsertedAllocations));
  // }

  // private dispatchUpdateResourceCommitmentAction(updatedResourceCommitment) {
  //   this.store.dispatch(new casePlanningActions.UpdateResourceCommitment(updatedResourceCommitment));
  // }

  // private dispatchDeleteResourceCommitmentAction(deletedCommitmentId) {
  //   this.store.dispatch(new casePlanningActions.DeleteResourceCommitment(deletedCommitmentId));
  // }

  // private deleteResourceAssignmentFromProjectHandler(deletedObjId) {
  //   this.store.dispatch(new casePlanningActions.DeleteResourceStaffing(deletedObjId));
  // }

  // private deleteResourcesAssignmentsFromProjectHandler(deletedObjIds) {
  //   this.store.dispatch(new casePlanningActions.DeleteResourcesStaffing(deletedObjIds));
  // }

  // private deleteResourcesAllocationsCommitments(dataToDelete) {
  //   this.store.dispatch(new casePlanningActions.DeleteAllocationsCommitmentsStaffing(dataToDelete));
  // }

  // private dispatchAddResourceCommitmentAction(resourceCommitment) {
  //   this.store.dispatch(new casePlanningActions.AddResourceCommitment(resourceCommitment));
  // }

  // private upsertStaffableAsRole(staffableAsRole) {
  //   this.dispatchProjectsLoaderAction(true);
  //   this.store.dispatch(new casePlanningActions.UpsertResourceStaffableAsRole(staffableAsRole));
  // }

  // private deleteStaffableAsRole(staffableAsRole) {
  //   this.dispatchProjectsLoaderAction(true);
  //   this.store.dispatch(new casePlanningActions.DeleteResourceStaffableAsRole(staffableAsRole));
  // }

  private upsertCaseViewNoteAction(caseViewNote: ResourceOrCasePlanningViewNote) {
    this.dispatchProjectsLoaderAction(true);
    this.store.dispatch(new casePlanningActions.UpsertCasePlanningViewNote(caseViewNote));
  }

  // ---------------------------Component Unload--------------------------------------------//

  ngOnDestroy() {
    this.overlayMessageServiceSub.unsubscribe();
    this.storeSub.unsubscribe();
  }
}
