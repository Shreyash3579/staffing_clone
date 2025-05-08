// ----------------------- Angular Package References ----------------------------------//
import { Component, OnInit, OnDestroy, Output, EventEmitter } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { combineLatest, Subject, Subscription } from "rxjs";
import { filter, takeUntil } from "rxjs/operators";

// ----------------------- Service References ----------------------------------//
import { CoreService } from "../core/core.service";
import { LocalStorageService } from "../shared/local-storage.service";
import { ResourceService } from "../shared/helperServices/resource.service";

// --------------------------Interfaces -----------------------------------------//
import { CaseRoleType } from "../shared/interfaces/caseRoleType.interface";
import { CaseType } from "../shared/interfaces/caseType.interface";
import { CommitmentType } from "../shared/interfaces/commitmentType.interface";
import { DemandFilterCriteria } from "../shared/interfaces/demandFilterCriteria.interface";
import { InvestmentCategory } from "../shared/interfaces/investmentCateogry.interface";
import { LevelGrade } from "../shared/interfaces/levelGrade.interface";
import { Office } from "../shared/interfaces/office.interface";
import { OfficeHierarchy } from "../shared/interfaces/officeHierarchy.interface";
import { Project } from "../shared/interfaces/project.interface";
import { ServiceLine } from "../shared/interfaces/serviceLine.interface";
import { SupplyFilterCriteria } from "../shared/interfaces/supplyFilterCriteria.interface";
import { UserPreferences } from "../shared/interfaces/userPreferences.interface";

// --------------------------Utils -----------------------------------------//
import * as moment from "moment";
import { DateService } from "../shared/dateService";
import { ConstantsMaster } from "../shared/constants/constantsMaster";
import { ServiceLineHierarchy } from "../shared/interfaces/serviceLineHierarchy";
import { AzureSearchTriggeredFromEnum, ServiceLine as ServiceLineEnum } from "../shared/constants/enumMaster";
import { CaseType as CaseTypeEnum } from "../shared/constants/enumMaster";
import { PlanningCard } from "../shared/interfaces/planningCard.interface";
import { PracticeArea } from "../shared/interfaces/practiceArea.interface";
import { PositionHierarchy } from "../shared/interfaces/positionHierarchy.interface";
import { ResourceCommitment } from "../shared/interfaces/resourceCommitment";
import { AppInsightsService } from "../app-insights.service";
import { UserPreferenceSupplyGroupViewModel } from "../shared/interfaces/userPreferenceSupplyGroupViewModel";
import { CombinedUserPreferences } from "../shared/interfaces/combinedUserPreferences.interface";
import { SupplyGroupFilterCriteria } from "../shared/interfaces/supplyGroupFilterCriteria.interface";
import { NotificationService } from "../shared/notification.service";
import { PlaceholderAllocation } from "../shared/interfaces/placeholderAllocation.interface";
import { OverlayMessageService } from "../overlay/behavioralSubjectService/overlayMessage.service";
import { PegOpportunityDialogService } from "../overlay/dialogHelperService/peg-opportunity-dialog.service";
import { ShowQuickPeekDialogService } from "../overlay/dialogHelperService/show-quick-peek-dialog.service";
import { CommonService } from "../shared/commonService";
import { CaseRollDialogService } from "../overlay/dialogHelperService/caseRollDialog.service";
import { PlaceholderDialogService } from "../overlay/dialogHelperService/placeholderDialog.service";
import { UserPreferenceService } from "../overlay/behavioralSubjectService/userPreference.service";
import { Store, select } from "@ngrx/store";
import { QuickAddDialogService } from "../overlay/dialogHelperService/quickAddDialog.service";
import { ResourcesCommitmentsDialogService } from "../overlay/dialogHelperService/resourcesCommitmentsDialog.service";

// --------------------------Redux Component -----------------------------------------//
import * as StaffingDemandActions from './state/actions/staffing-demand.action';
import * as StaffingSupplyActions from './state/actions/staffing-supply.action';
import * as fromStaffingDemand from './state/reducers/staffing-demand.reducer';
import * as fromStaffingSupply from './state/reducers/staffing-supply.reducer';
import { OverlappedTeamDialogService } from "../overlay/dialogHelperService/overlapped-team-dialog.service";
import { AffiliationRole } from "../shared/interfaces/affiliationRole.interface";
import { BossSearchCriteria, BossSearchResult } from "../shared/interfaces/azureSearchCriteria.interface";
import { SignalrService } from "../shared/signalR.service";
import { PegOpportunity } from "../shared/interfaces/pegOpportunity.interface";
import { OverlayDialogService } from "../overlay/dialogHelperService/overlayDialog.service";
import { SortHelperService } from "../shared/helperServices/sortHelper.service";
import { NotesAlertDialogService } from "../overlay/dialogHelperService/notesAlertDialog.service";
import { CaseIntakeAlert } from "../shared/interfaces/caseIntakeAlert.interface";
import { SharedNotesService } from "../core/services/shared-notes-info.service";
import * as fromProjectAllocationsStore from 'src/app/state/reducers/project-allocations.reducer';
import * as fromPlanningCardOverlayStore from 'src/app/state/reducers/planning-card-overlay.reducer';
import * as projectAllocationsAction from 'src/app/state/actions/project-allocations.action';
import * as planningCardOverlayAction from 'src/app/state/actions/planning-card-overlay.action';



@Component({
  selector: "app-home",
  templateUrl: "./home.component.html",
  styleUrls: ["./home.component.scss"]
})
export class HomeComponent implements OnInit, OnDestroy {
  // ----------------------- Notifiers ------------------------------------------------//
  supplyFilterCriteria$: Subject<SupplyFilterCriteria> = new Subject();

  // -----------------------Local Variables--------------------------------------------//
  webWorker: Worker;
  destroy$: Subject<void> = new Subject<void>();

  allPlanningCards: PlanningCard[];
  projects: Project[];
  historicalProjects: Project[];
  planningCards: PlanningCard[] = [];
  historicalPlanningCards: PlanningCard[] = [];

  caseTypes: CaseType[];
  officeHierarchy: OfficeHierarchy;
  staffingTags: ServiceLine[];
  staffingTagsHierarchy: ServiceLineHierarchy[];
  positionsHierarchy: PositionHierarchy[];
  levelGrades: LevelGrade[];
  positions: string[];
  homeOffice: Office;
  projectStartIndex = 1;
  scrollDistance: number; // how much percentage the scroll event should fire ( 2 means (100 - 2*10) = 80%)
  pageScrolled = false;
  pageSize: number;
  commitmentTypes: CommitmentType[];
  investmentCategories: InvestmentCategory[];
  caseRoleTypes: CaseRoleType[];
  demandTypes: any[];
  demandFilterCriteriaObj: DemandFilterCriteria = {} as DemandFilterCriteria;
  historicalDemandFilterCriteriaObj: DemandFilterCriteria = {} as DemandFilterCriteria;
  routerSub: any;
  loadProjects = true;
  isDemandLoaded = false;
  isHistoricalDemandLoaded = false;
  subscription: Subscription = new Subscription();
  currRoute = "";
  availabilityyWeeksRange: any;
  practiceAreas: PracticeArea[] = [];
  affiliationRoles: AffiliationRole[] =[];
  highlightedResourcesInPlanningCards = [];
  highlightedResourcesInHistoricalPlanningCards = [];
  userPreferences: UserPreferences;
  storeSub: Subscription = new Subscription();
  collapseHistoricalDemand: boolean = true;
  isPdfExport: boolean = false;

  //for getting the week by week data
  dateRangeForResourcesOrProjects: { startDate: any; endDate: any; };
  historicalDateRangeForResourcesOrProjects: { startDate: any; endDate: any; };
  groupingArray: string[] = [];
  today = new Date().toISOString().split('T')[0];

  // for stage
  resourceLength = 0;
  isSupplyLoaded = false;
  supplyGroupPreferences: UserPreferenceSupplyGroupViewModel[] = [];
  supplyFilterCriteriaObj: SupplyFilterCriteria = {} as SupplyFilterCriteria;
  supplyGroupFilterCriteriaObj: SupplyGroupFilterCriteria = {} as SupplyGroupFilterCriteria;
  isPinned: boolean;
  isHistoricalDemandCollapsed: boolean;
  selectedGroupingOption:string = "Weekly";
  availableResources: any;
  allResourcesWithAvailability: any;
  searchInSupply: boolean = false;
  searchString: string = '';
  clearEmployeeSearch= false;
  resourceGroups: any;
  expandPanelComplete: boolean = false;
  private projectsLoaded$ = new Subject<void>();
  private planningCardsLoaded$ = new Subject<void>();
  private historicalProjectsLoaded$ = new Subject<void>();
  sharedNotes = [];
  caseIntakeAlerts = [];
  projectIdentifiers: string[] = [];
  historicalProjectIdentifiers: string[] = [];
  planningCardIds: string[] = [];

  constructor(
    private demandStore: Store<fromStaffingDemand.State>,
    private supplyStore: Store<fromStaffingSupply.State>,
    private coreService: CoreService,
    private localStorageService: LocalStorageService,
    private router: Router,
    private appInsightsService: AppInsightsService,
    private overlayMessageService: OverlayMessageService,
    private notifyService: NotificationService,
    private pegOpportunityDialogService: PegOpportunityDialogService,
    private showQuickPeekDialogService: ShowQuickPeekDialogService,
    private caseRollDialogService: CaseRollDialogService,
    private placeholderDialogService: PlaceholderDialogService,
    private userpreferencesService: UserPreferenceService,
    private quickAddDialogService: QuickAddDialogService,
    private overlappedTeamDialogService: OverlappedTeamDialogService,
    private signalrService: SignalrService,
    private overlayDialogService: OverlayDialogService,
    private activatedRoute: ActivatedRoute,
    private notesAlertDialogService: NotesAlertDialogService,
    private sharedNotesService: SharedNotesService,
    private resourcesCommitmentsDialogService: ResourcesCommitmentsDialogService,
    private projectAllocationsStore: Store<fromProjectAllocationsStore.State>,
    private planningCardOverlayStore: Store<fromPlanningCardOverlayStore.State>,
  ) {
  }

  // -------------------Component LifeCycle Events and Functions----------------------//

  ngOnInit() {
    this.currRoute = this.router.url;
    this.appInsightsService.logPageView(
      this.coreService.loggedInUser.employeeCode,
      ConstantsMaster.appScreens.page.home
    );

    this.activatedRoute.queryParams.subscribe(params => {
      this.isPdfExport = params['export'];

      if (this.isPdfExport) {
        this.getDataForExport();
      } else {
        this.pageSize = this.coreService.appSettings.projectsPageSize;
        this.scrollDistance = this.coreService.appSettings.scrollDistance;
        this.homeOffice = this.coreService.loggedInUser.office;   
        
        // Load data
        this.getDataBasedOnUserPreferences();
        this.getLookupListFromLocalStorage();
        this.subscribeEvents();
        this.setStoreSuscriptions();
        combineLatest([this.projectsLoaded$, this.planningCardsLoaded$, this.historicalProjectsLoaded$]).subscribe(() => {
          // Call your API functions after both data sets are loaded
          this.getSharedNotesInfo(this.coreService.loggedInUser.employeeCode);
          this.getCaseIntakeAlertsInfo(this.coreService.loggedInUser.employeeCode);
        });
      }
    }); 
  }

  getDataForExport() {
    this.historicalPlanningCards = this.localStorageService.get(ConstantsMaster.localStorageKeys.historicalPlanningCards);
    this.historicalProjects = this.localStorageService.get(ConstantsMaster.localStorageKeys.historicalProjects);
    this.highlightedResourcesInHistoricalPlanningCards = this.localStorageService.get(ConstantsMaster.localStorageKeys.highlightedResourcesInPlanningCards);
    this.expandPanelComplete = this.localStorageService.get(ConstantsMaster.localStorageKeys.expandPanelComplete);
    this.isHistoricalDemandLoaded = this.localStorageService.get(ConstantsMaster.localStorageKeys.hideLoading);
  }

  setStoreSuscriptions() {
    this.getResourcesFromStore();
    this.getFilteredResourcesOnSearchInSupplyFromStore();
    this.getProjectsFromStore();
    this.getHistoricalProjectsFromStore();
    this.getPlanningCardsFromStore();
    this.getProjectsIdentifierFromStore();
    this.getHistoricalProjectsIdentifierFromStore();
    this.getPlanningCardIdsFromStore();
  }


  getProjectsIdentifierFromStore() {
    this.storeSub.add(this.projectAllocationsStore
      .pipe(
        select(fromProjectAllocationsStore.getStaffingProjectsOldCaseCodes))
      .subscribe((projects: string[]) => {
        if (projects) {
          this.projectIdentifiers = projects;
          this.projectsLoaded$.next();
        }
      }))
  }

  getHistoricalProjectsIdentifierFromStore() {
    this.storeSub.add(this.projectAllocationsStore
      .pipe(
        select(fromProjectAllocationsStore.getStaffingHistoricalProjectsOldCaseCodes))
      .subscribe((projectsData: string[]) => {
        if (projectsData) {
          this.historicalProjectIdentifiers = projectsData;
          this.historicalProjectsLoaded$.next();
        }
      }))
  }

  getPlanningCardIdsFromStore() {
    this.storeSub.add(this.planningCardOverlayStore
      .pipe(
        select(fromPlanningCardOverlayStore.getStaffingPlanningCardsId))
      .subscribe((planningCardsData: string[]) => {
        if (planningCardsData) {
          this.planningCardIds = planningCardsData;
          this.planningCardsLoaded$.next();
        }
      }))
  }

private getCaseIntakeAlertsInfo(loggedInEmployeeCode){
  this.sharedNotesService.getCaseIntakeAlerts(loggedInEmployeeCode).subscribe(caseIntakeAlerts => {
    this.caseIntakeAlerts = this.filterDemandPresentInStore(caseIntakeAlerts);
  });
}



filterDemandPresentInStore(caseIntakeAlertsData: CaseIntakeAlert[]) {
    
  let filteredProjects = [];
  let filteredPlanningCards = [];

  if(this.projects || this.allPlanningCards)
  {
  caseIntakeAlertsData.forEach(data => {
    if (data.oldCaseCode) {
      const project = this.projectIdentifiers.find(x => x === data.oldCaseCode);
      if (project) {
        filteredProjects.push(data);
      }
    } else if (data.opportunityId) {
      const project = this.projectIdentifiers.find(x => x === data.opportunityId);
      if (project) {
        filteredProjects.push(data);
      }
    } else if (data.planningCardId) {
      const planningCard = this.planningCardIds.find(x => x === data.planningCardId);
      if (planningCard) {
        filteredPlanningCards.push(data);
      }
    }
  });
  }

  if(this.historicalProjectIdentifiers )
  {
  caseIntakeAlertsData.forEach(data => {
    if (data.oldCaseCode || data.opportunityId) {
      const project = this.historicalProjectIdentifiers.find(x => x === data.oldCaseCode || x === data.opportunityId);
      if (project) {
        filteredProjects.push(data);
      }
    } 
  });
  }

  return filteredProjects.concat(filteredPlanningCards);
}



private getSharedNotesInfo(loggedInEmployeeCode){
  this.sharedNotesService.getSharedNotes(loggedInEmployeeCode).subscribe(notes => {
    this.sharedNotes = notes;
    if(this.sharedNotes && this.sharedNotes.length > 0){
    }
  });
}

  getResourcesFromStore() {
    this.storeSub.add(this.supplyStore
      .pipe(
        select(fromStaffingSupply.getStaffingResources),
        filter(resourcesData => resourcesData !== null)
      )
      .subscribe((resourcesData: any ) => {
        if (resourcesData) {
          this.loadResourcesOnSupply(resourcesData);
          this.isSupplyLoaded = true;
        }
      }))
  }

  getFilteredResourcesOnSearchInSupplyFromStore(){
    this.storeSub.add(this.supplyStore
      .pipe(
        select(fromStaffingSupply.getSearchQueryWithResults),
        filter(searchQueryWithResults => searchQueryWithResults !== null)
      )
      .subscribe((searchQueryWithResults: BossSearchResult ) => {
         
        if (searchQueryWithResults.searchResultsEcodes) {
          this.availableResources = this.allResourcesWithAvailability.filter(resource => searchQueryWithResults.searchResultsEcodes.includes(resource.employeeCode.toUpperCase()));
           
            const [startDate, endDate] = DateService.getDateRangeAvalabilityFilterExpression(searchQueryWithResults.generatedLuceneSearchQuery.filter);
  
            if(startDate){
              this.availableResources = this.availableResources.filter(item => {
                const date = new Date(item.dateFirstAvailable).setHours(0,0,0,0); //remove time component and then comapre
                return date >= startDate.getTime();
              });
            }
  
            if(endDate){
              this.availableResources = this.availableResources.filter(item => {
                const date = new Date(item.dateFirstAvailable).setHours(0,0,0,0);
                return date <= endDate.getTime();
              });
            }
          
        }else{
          this.availableResources = [];
        }
        this.isSupplyLoaded = true;
      }))
  }

  getProjectsFromStore() {
    this.storeSub.add(this.demandStore
      .pipe(
        select(fromStaffingDemand.getStaffingProjects))
      .subscribe((projectsData: Project[]) => {
        if (projectsData) {
          this.projects = projectsData;
          this.isDemandLoaded = true;
          projectsData.forEach(project => {
            project.allocatedResources.forEach(allocation => {
              this.convertAllocationDatesInBainFormat(allocation);
            });
            project.placeholderAllocations.forEach(placeholderAllocation => {
              this.convertAllocationDatesInBainFormat(placeholderAllocation);
            });
          });
          this.projects = this.setProjects(projectsData, this.demandFilterCriteriaObj, this.projects);
        }
      }))
  }

  getHistoricalProjectsFromStore() {
    this.storeSub.add(this.demandStore
      .pipe(
        select(fromStaffingDemand.getStaffingHistoricalProjects))
      .subscribe((projectsData: Project[]) => {
        if (projectsData) {
          this.historicalProjects = projectsData;
          this.isHistoricalDemandLoaded = true;
          this.historicalProjects = this.setProjects(projectsData, this.historicalDemandFilterCriteriaObj, this.historicalProjects);
        }
      }))
  }

  getPlanningCardsFromStore() {
    this.storeSub.add(this.demandStore
      .pipe(
        select(fromStaffingDemand.getStaffingPlanningCards))
      .subscribe((planningCardsData: PlanningCard[]) => {
        if (planningCardsData) {
          planningCardsData.forEach(planningCard => {
            planningCard.placeholderAllocations = planningCard.allocations.filter(x => x.isPlaceholderAllocation);
            planningCard.regularAllocations = planningCard.allocations.filter(x => !x.isPlaceholderAllocation);
            planningCard.placeholderAllocations.forEach(allocation => {
              this.convertAllocationDatesInBainFormat(allocation);
            });
            planningCard.regularAllocations.forEach(allocation => {
              this.convertAllocationDatesInBainFormat(allocation);
            });
          });

          this.allPlanningCards = planningCardsData;

          //  filtered planning cards to get planning cards based on start and end date selected in staffing settings demand section
          this.planningCards = planningCardsData.filter(
            x => DateService.isSameOrBefore(x.startDate, this.dateRangeForResourcesOrProjects.endDate)
            && DateService.isSameOrAfter(x.startDate, this.dateRangeForResourcesOrProjects.startDate)

          );

          this.getOnGoingPlanningCards();
          this.sortPlanningCards(this.planningCards);
          this.sortPlanningCardsAllocations();
          this.highlightedResourcesInPlanningCards = this.refreshListOfHighlightedResourcesInPlanningCards(this.planningCards);
        }
      }))
  }

  getOnGoingPlanningCards() {
    let demandStartDate = new Date(this.historicalDemandFilterCriteriaObj.startDate).toISOString().split('T')[0];
    let demandEndDate = new Date(this.historicalDemandFilterCriteriaObj.endDate).toISOString().split('T')[0];

    this.historicalPlanningCards = this.allPlanningCards?.filter(x => {
      let startDate = new Date(x.startDate).toISOString().split('T')[0];
      let endDate = new Date(x.endDate).toISOString().split('T')[0];

      return (startDate < demandEndDate && endDate >= demandStartDate)
    });
    this.highlightedResourcesInHistoricalPlanningCards = this.refreshListOfHighlightedResourcesInPlanningCards(this.historicalPlanningCards);
    this.sortPlanningCards(this.historicalPlanningCards);
  }

  sortPlanningCards(planningCards) {
    planningCards = planningCards?.sort((a, b) => {
        if(!DateService.isSame(a.startDate,b.startDate)) {
          return new Date(a.startDate).getTime() - new Date(b.startDate).getTime();
        } if (!a.name || !b.name) {
          return !a.name ? -1 : (!b.name ? 1: 0);
        }
        return a.name.localeCompare(b.name);
      });
  }

  onActiveCasesScrolled() {
    // if total projects displayed are less than the page size, it means there are not more projects that needs to be fetched
    if (this.historicalProjects.length < this.pageSize) {
      return;
    }
  
    this.getMoreProjects();
  }

  getMoreProjects() {
    this.projectStartIndex = this.projectStartIndex + this.pageSize;
    this.getHistoricalProjectsFilteredBySelectedValues(this.historicalDemandFilterCriteriaObj, true);
  }

  convertAllocationDatesInBainFormat(allocation) {
    allocation.startDate = DateService.convertDateInBainFormat(allocation.startDate);
    allocation.endDate = DateService.convertDateInBainFormat(allocation.endDate);
    return allocation;
  }

  getDataBasedOnUserPreferences() {
    // this is done so that whenever user changes their user settings, it reflects in the projects and resources data
    this.coreService.getCombinedUserPreferences()
      .pipe(takeUntil(this.destroy$))
      .subscribe((combinedUserPreferences: CombinedUserPreferences) => {
        this.userPreferences = combinedUserPreferences.userPreferences;
        this.supplyGroupPreferences = combinedUserPreferences.userPreferenceSupplyGroups;

        this.updateSupplyAndDemandFilterCriteria();
        this.updateDefaultSupplyGroupSettings();
        this.updateHistoricalDemandFilterCriteriaForFilter();

        // load projects and resources data
        this.getResourcesByDefaultSettings();
        this.getProjectsFilteredBySelectedValuesAndSyncToGlobalState(this.demandFilterCriteriaObj);
        this.getPlanningCardsBySelectedValuesAndSyncToGlobalState(this.demandFilterCriteriaObj);
        if(this.selectedGroupingOption === "Weekly") {
          this.getWeeks();
        }
        else{
          this.getDaily();
        }
        this.getHistoricalProjectsFilteredBySelectedValuesAndSyncToGlobalState(this.historicalDemandFilterCriteriaObj);
        this.getHistoricalDemandsPinnedState();
      })

      //this.coreService.triggerLoadUpdatedDemandData({demandFilterCriteriaObj:this.demandFilterCriteriaObj, historicalDemandFilterCriteriaObj:this.historicalDemandFilterCriteriaObj});
  }

  subscribeEvents() {

     //Commenting out the code since we use redux to make realtime updates on 2.0 tab and we are using this anymore

    // this.subscription.add(this.overlayMessageService.refreshProjectForCaseRoll().subscribe(result => {
    //   if (result !== null) {
    //     const matchedProject: Project = this.projects.find(x => (x.oldCaseCode && x.oldCaseCode === result.project.oldCaseCode)
    //       || x.pipelineId && x.pipelineId === result.pipelineId);

    //     if (!matchedProject)
    //       return;

    //     matchedProject.allocatedResources = matchedProject.allocatedResources.map(x => {
    //       if (result.updatedResourceAllocation.some(r => r.id === x.id)) {
    //         return result.updatedResourceAllocation.find(allocation => allocation.id === x.id);
    //       } else {
    //         return x;
    //       }
    //     });
    //     matchedProject.caseRoll = result.caseRoll;

    //     if (matchedProject.oldCaseCode) {
    //       this.projects.map(obj => obj.oldCaseCode === matchedProject?.oldCaseCode || obj);
    //     } else {
    //       this.projects.map(obj => obj.pipelineId === matchedProject?.pipelineId || obj);
    //     }
    //   }
    // }));


    this.subscription.add(this.overlayMessageService.getReosurceAssignmentToCase().subscribe(addedData => {
      if (addedData !== null) {
        addedData.forEach(resource => {
          // Assign Ids to the inserted rows. Leave updated rows as it is since they already have IDs
          const project = this.projects.find(x =>
            (resource.oldCaseCode && x.oldCaseCode === resource.oldCaseCode) ||
            (resource.pipelineId && x.pipelineId === resource.pipelineId));

          const allocation = project.allocatedResources.find(y =>
            y.id == null && y.employeeCode === resource.employeeCode && moment(y.startDate).isSame(moment(resource.startDate)) &&
            moment(y.endDate).isSame(moment(resource.endDate)) && y.investmentCode === resource.investmentCode);

          if (allocation) {
            allocation.id = resource.id;
          }

        });
      }
    }));

    this.subscription.add(this.signalrService.caseIntakeBannerSubject$.subscribe((alert : any) => {  
      this.getCaseIntakeAlertsInfo(this.coreService.loggedInUser.employeeCode);
    }));

    this.subscription.add(this.overlayMessageService.getUpdatedUserPreferences().subscribe((userpreferences: UserPreferences) => {
      if (userpreferences !== null) {

        if (this.demandFilterCriteriaObj) {
          this.demandFilterCriteriaObj.caseExceptionShowList = userpreferences.caseExceptionShowList || '';
          this.demandFilterCriteriaObj.caseExceptionHideList = userpreferences.caseExceptionHideList || '';
          this.demandFilterCriteriaObj.opportunityExceptionShowList = userpreferences.opportunityExceptionShowList || '';
          this.demandFilterCriteriaObj.opportunityExceptionHideList = userpreferences.opportunityExceptionHideList || '';
          this.demandFilterCriteriaObj.caseOppSortOrder = userpreferences.caseOppSortOrder || '';
          this.demandFilterCriteriaObj.planningCardsSortOrder = userpreferences.planningCardsSortOrder || '';

          
          this.historicalDemandFilterCriteriaObj.caseExceptionShowList = this.userPreferences.caseExceptionShowList || '';
          this.historicalDemandFilterCriteriaObj.caseExceptionHideList = this.userPreferences.caseExceptionHideList || '';
          this.historicalDemandFilterCriteriaObj.opportunityExceptionShowList = this.userPreferences.opportunityExceptionShowList || '';
          this.historicalDemandFilterCriteriaObj.opportunityExceptionHideList = this.userPreferences.opportunityExceptionHideList || '';
          this.historicalDemandFilterCriteriaObj.caseOppSortOrder = this.userPreferences.caseOppSortOrder || '';
          this.historicalDemandFilterCriteriaObj.planningCardsSortOrder = this.userPreferences.planningCardsSortOrder || '';
        }

      }
    }));

    this.subscription.add(this.signalrService.onPegDataUpdated().subscribe((updatedPegPlanningCardData : PegOpportunity) => {
      this.demandStore.dispatch(
        new StaffingDemandActions.UpdatePegPlanningCardSuccess({
          updatedPegPlanningCardData : updatedPegPlanningCardData,
          demandFilterCriteria : this.demandFilterCriteriaObj
        })
      );
    }));


    this.subscription.add( this.coreService.openCaseOppOverlayFromNotifications().pipe(takeUntil(this.destroy$)).subscribe(value => {
      if (value && this.currRoute.includes('home')) {
        this.overlayDialogService.openProjectDetailsDialogHandler(value);
      }
    }));
  }

  getProjectsAndResourcesOnDateChangeHandler(event) {

    this.updateSupplyAndDemandFilterCriteriaForFilter(event.dateRange);
    this.updateSupplyGroupForFilter(event.dateRange);

        // load projects and resources data
    this.getResourcesByDefaultSettings();
    this.getProjectsFilteredBySelectedValues(this.demandFilterCriteriaObj);
    this.getPlanningCardsBySelectedValues(this.demandFilterCriteriaObj);
    if(this.selectedGroupingOption === "Weekly") {
      this.getWeeks();
    }
    else{
      this.getDaily();
    }


  }

  getHistoricalDataOnDateChangeHandler(date) {
    this.updateHistoricalDemandFilterCriteriaForFilter(date);

    // load historical projects and planning card data
    this.getHistoricalProjectsFilteredBySelectedValues(this.historicalDemandFilterCriteriaObj);
    this.getOnGoingPlanningCards();
  }

  updateHistoricalDemandFilterCriteriaForFilter(date : [Date, Date] = null) {
    let updatedDateRange;
    if(!date) {
      const endDate = DateService.getStartOfWeek();
      const startDate = DateService.subtractDays(endDate, 14);
      updatedDateRange = DateService.getFormattedDateRange({
        startDate: new Date(startDate),
        endDate: new Date(DateService.subtractDays(endDate, 1))
      });
    }else{
      updatedDateRange = DateService.getFormattedDateRange({
        startDate: date[0],
        endDate: date[1]
      });
    }

    this.historicalDateRangeForResourcesOrProjects = updatedDateRange;

    this.historicalDemandFilterCriteriaObj = {...this.demandFilterCriteriaObj};
    // this.historicalDemandFilterCriteriaObj.demandTypes = 'ActiveCase';
    this.historicalDemandFilterCriteriaObj.demandTypes = 'ActiveCase,NewDemand';
    if (this.demandFilterCriteriaObj.demandTypes?.includes('CasesStaffedBySupply')) {
      this.historicalDemandFilterCriteriaObj.demandTypes += ',CasesStaffedBySupply';
    }
    
    this.historicalDemandFilterCriteriaObj.startDate = this.historicalDateRangeForResourcesOrProjects.startDate;
    this.historicalDemandFilterCriteriaObj.endDate = this.historicalDateRangeForResourcesOrProjects.endDate;

    sessionStorage.setItem('historicalDemandFilterCriteriaObj', JSON.stringify(this.historicalDemandFilterCriteriaObj));
  }

  updateSupplyAndDemandFilterCriteriaForFilter(selectedDateRange) {
    const updatedDateRange = DateService.getFormattedDateRange({
      startDate: selectedDateRange[0],
      endDate:  new Date(DateService.addDays(selectedDateRange[1], 6))
    });

    this.dateRangeForResourcesOrProjects = updatedDateRange;
    this.historicalDateRangeForResourcesOrProjects = updatedDateRange;

    this.demandFilterCriteriaObj.startDate = updatedDateRange.startDate;
    this.demandFilterCriteriaObj.endDate = updatedDateRange.endDate;
    this.supplyFilterCriteriaObj.startDate = updatedDateRange.startDate;
    this.supplyFilterCriteriaObj.endDate = updatedDateRange.endDate;
    sessionStorage.setItem('demandFilterCriteriaObj', JSON.stringify(this.demandFilterCriteriaObj));
  }

  updateSupplyGroupForFilter(selectedDateRange) {
    const updatedDateRange = DateService.getFormattedDateRange({
      startDate: selectedDateRange[0],
      endDate:  new Date(DateService.addDays(selectedDateRange[1], 6))
    });

    this.dateRangeForResourcesOrProjects = updatedDateRange;

    this.supplyGroupFilterCriteriaObj.startDate = updatedDateRange.startDate;
    this.supplyGroupFilterCriteriaObj.endDate = updatedDateRange.endDate;
  }


  updateDemandFilterCriteriaFromSupply() {
    if (this.demandFilterCriteriaObj.demandTypes?.includes("CasesStaffedBySupply")) {
      this.demandFilterCriteriaObj.supplyFilterCriteria = this.supplyFilterCriteriaObj;
    } else {
      this.demandFilterCriteriaObj.supplyFilterCriteria = null;
    }
  }

  updateSupplyAndDemandFilterCriteria() {
    const today = new Date();
    const userPreferences = this.userPreferences;
    var startOfWeek = DateService.getStartOfWeek();

    let endOfDuration = new Date(startOfWeek);
    endOfDuration.setDate(endOfDuration.getDate() + 5 * 7 + 4); //need data till end of 5 weeks

    //endDate => set it as what is derived from the staffingSettings
    // Default date range will be for 5 weeks from the start of current week
    const defaultDateRange = DateService.getFormattedDateRange({
      startDate: startOfWeek,
      endDate: endOfDuration,
    });

    if (userPreferences && typeof userPreferences === "object") {

      /*-------------- Set user preferences for Supply/Demand Side ---------------------*/
      //let dateRangeForResourcesOrProjects: { startDate: any; endDate: any; };

      if (this.userPreferences.demandWeeksThreshold) {
        const startDateOfLastWeek = DateService.addWeeks(startOfWeek, this.userPreferences.demandWeeksThreshold - 1);
        const endDate = new Date(DateService.addDays(startDateOfLastWeek, 6));
        const date = { startDate: startOfWeek, endDate: endDate };
        this.dateRangeForResourcesOrProjects = DateService.getFormattedDateRange(date);
      }
      //remove this -> derive from demandWeeksThreshold and test for 0
      else {
        this.dateRangeForResourcesOrProjects = defaultDateRange;
      }

      this.supplyFilterCriteriaObj.startDate = today.toDateString();
      this.supplyFilterCriteriaObj.endDate = this.dateRangeForResourcesOrProjects.endDate;

      this.supplyFilterCriteriaObj.officeCodes =
        userPreferences.supplyViewOfficeCodes || this.homeOffice.officeCode.toString();
      this.supplyFilterCriteriaObj.levelGrades = userPreferences.levelGrades;
      this.supplyFilterCriteriaObj.availabilityIncludes = userPreferences.availabilityIncludes;
      this.supplyFilterCriteriaObj.staffingTags =
        this.userPreferences.supplyViewStaffingTags || ServiceLineEnum.GeneralConsulting;
      this.supplyFilterCriteriaObj.groupBy = userPreferences.groupBy;
      this.supplyFilterCriteriaObj.sortBy = userPreferences.sortBy;
      this.supplyFilterCriteriaObj.practiceAreaCodes = userPreferences.practiceAreaCodes;
      this.supplyFilterCriteriaObj.affiliationRoleCodes = userPreferences.affiliationRoleCodes;
      this.supplyFilterCriteriaObj.positionCodes = userPreferences.positionCodes;
      this.supplyFilterCriteriaObj.staffableAsTypeCodes = userPreferences.staffableAsTypeCodes;
      
      /*-------------- Set user preferences for Demand Side ---------------------*/

      this.demandFilterCriteriaObj.startDate = this.dateRangeForResourcesOrProjects.startDate;
      this.demandFilterCriteriaObj.endDate = this.dateRangeForResourcesOrProjects.endDate;
      this.demandFilterCriteriaObj.officeCodes = userPreferences.demandViewOfficeCodes || this.homeOffice.officeCode.toString();
      this.demandFilterCriteriaObj.caseTypeCodes = userPreferences.caseTypeCodes || CaseTypeEnum.Billable;
      this.demandFilterCriteriaObj.caseAttributeNames = userPreferences.caseAttributeNames || '';
      this.demandFilterCriteriaObj.opportunityStatusTypeCodes = userPreferences.opportunityStatusTypeCodes;
      this.demandFilterCriteriaObj.demandTypes = this.filterDemandTypesNotIncludingCases(userPreferences.demandTypes);
      this.demandFilterCriteriaObj.minOpportunityProbability = userPreferences.minOpportunityProbability;
      this.demandFilterCriteriaObj.caseExceptionShowList = userPreferences.caseExceptionShowList || '';
      this.demandFilterCriteriaObj.caseExceptionHideList = userPreferences.caseExceptionHideList || '';
      this.demandFilterCriteriaObj.opportunityExceptionShowList = userPreferences.opportunityExceptionShowList || '';
      this.demandFilterCriteriaObj.opportunityExceptionHideList = userPreferences.opportunityExceptionHideList || '';
      this.demandFilterCriteriaObj.caseAllocationsSortBy =
        userPreferences.caseAllocationsSortBy || ConstantsMaster.CaseAllocationsSortByOptions[0].value;
      this.demandFilterCriteriaObj.planningCardsSortOrder = userPreferences.planningCardsSortOrder || '';
      this.demandFilterCriteriaObj.caseOppSortOrder = userPreferences.caseOppSortOrder || '';
      this.demandFilterCriteriaObj.industryPracticeAreaCodes = userPreferences.industryPracticeAreaCodes;
      this.demandFilterCriteriaObj.capabilityPracticeAreaCodes = userPreferences.capabilityPracticeAreaCodes;
      this.demandFilterCriteriaObj.isStaffedFromSupply = false;

      this.updateDemandFilterCriteriaFromSupply();

    } else {
      /*-------------- Set default search criteria for Supply Side ---------------------*/

      this.supplyFilterCriteriaObj.startDate = today.toDateString();
      this.supplyFilterCriteriaObj.endDate = defaultDateRange.endDate;
      this.supplyFilterCriteriaObj.officeCodes = this.homeOffice.officeCode.toString();
      this.supplyFilterCriteriaObj.staffingTags = ServiceLineEnum.GeneralConsulting;

      /*-------------- Set default search criteria for Demand Side ---------------------*/

      this.demandFilterCriteriaObj.startDate = defaultDateRange.startDate;
      this.demandFilterCriteriaObj.endDate = defaultDateRange.endDate;
      this.demandFilterCriteriaObj.officeCodes = this.homeOffice.officeCode.toString();
      this.demandFilterCriteriaObj.caseTypeCodes = CaseTypeEnum.Billable;
    }

    sessionStorage.setItem('demandFilterCriteriaObj', JSON.stringify(this.demandFilterCriteriaObj));
  }

  private updateDefaultSupplyGroupSettings() {
    var startOfWeek = DateService.getStartOfWeek();

    let endOfDuration = new Date(startOfWeek);
    endOfDuration.setDate(endOfDuration.getDate() + 5 * 7 + 4); //need data till end of 5 weeks

    //endDate => set it as what is derived from the staffingSettings
    // Default date range will be for 5 weeks from the start of current week
    const defaultDateRange = DateService.getFormattedDateRange({
      startDate: startOfWeek,
      endDate: endOfDuration
    });

    const defaultGroupPreferences: UserPreferenceSupplyGroupViewModel = this.supplyGroupPreferences.find(
      (x) => x.isDefault
    );

    if (defaultGroupPreferences && typeof defaultGroupPreferences === "object") {
      if (this.userPreferences.demandWeeksThreshold) {
        const startDateOfLastWeek = DateService.addWeeks(startOfWeek, this.userPreferences.demandWeeksThreshold - 1);
        const endDate = new Date(DateService.addDays(startDateOfLastWeek, 6));
        const date = { startDate: startOfWeek, endDate: endDate };
        this.dateRangeForResourcesOrProjects = DateService.getFormattedDateRange(date);
      }
      //remove this -> derive from demandWeeksThreshold and test for 0
      else {
        this.dateRangeForResourcesOrProjects = defaultDateRange;
      }

      // for available today
      // this.supplyTodayGroupFilterCriteriaObj.startDate = DateService.getFormattedDate(today);
      this.supplyGroupFilterCriteriaObj.startDate = this.dateRangeForResourcesOrProjects.startDate;
      // this.supplyTodayGroupFilterCriteriaObj.endDate = DateService.getFormattedDate(today);
      this.supplyGroupFilterCriteriaObj.endDate = this.dateRangeForResourcesOrProjects.endDate;
      this.supplyGroupFilterCriteriaObj.employeeCodes = defaultGroupPreferences.groupMembers
        .map((x) => x.employeeCode)
        .join(",");
      this.supplyGroupFilterCriteriaObj.availabilityIncludes =
        this.supplyFilterCriteriaObj.availabilityIncludes;
      this.supplyGroupFilterCriteriaObj.sortBy = this.supplyFilterCriteriaObj.sortBy;
      this.supplyGroupFilterCriteriaObj.groupBy = this.supplyFilterCriteriaObj.groupBy;
    }
  }


  //filter comma separated string with particular values only
  filterDemandTypesNotIncludingCases(demandTypes) {
    const demandTypesForNewStaffingTab = ['NewDemand', 'CasesStaffedBySupply', 'Opportunity', 'PlanningCards'];
    const demandTypesArray = demandTypes.split(',');
    demandTypesArray.filter(demandType => {
      return demandTypesForNewStaffingTab.includes(demandType);
    })
    return demandTypesArray.join(',');
  }

  getResourcesByDefaultSettings() {
    if (this.supplyGroupPreferences.some((x) => x.isDefault)) {
      this.getResourcesFilteredBySelectedGroups(this.supplyGroupFilterCriteriaObj);
    } else {
      this.getResourcesFilteredBySelectedValues(this.supplyFilterCriteriaObj);
    }
  }

  getResourcesFilteredBySelectedGroups(supplyGroupFilterCriteriaObj: SupplyGroupFilterCriteria) {
    const employeeCodes = supplyGroupFilterCriteriaObj.employeeCodes;

    if (!employeeCodes) {
      this.isSupplyLoaded = true;
    }
    this.isSupplyLoaded = false;
    this.supplyStore.dispatch(
      new StaffingSupplyActions.LoadResourcesBySelectedGroup({
        supplyGroupFilterCriteriaObj
      })
    );

    this.clearSearchInSupplyModeHandler();
  }

  getResourcesFilteredBySelectedValues(supplyFilterCriteriaObj: SupplyFilterCriteria) {
    const officeCodes = supplyFilterCriteriaObj.officeCodes;
    const staffingTags = supplyFilterCriteriaObj.staffingTags;
    
    if (!officeCodes || officeCodes === "" || !staffingTags || staffingTags === "") {
      this.supplyStore.dispatch(new StaffingSupplyActions.ClearSupplyState());
      this.isSupplyLoaded = true;
    }
    this.isSupplyLoaded = false;
    this.supplyStore.dispatch(
      new StaffingSupplyActions.LoadResourcesBySelectedFilters({
        supplyFilterCriteriaObj
      })
    );

    this.clearSearchInSupplyModeHandler();
  }

  private getDateRangeForAvailabilityCalculation() {
    //if less than today, set as today
    const today = DateService.getFormattedDate(new Date());
    let startDate = (new Date(this.supplyFilterCriteriaObj.startDate) < new Date()) ? today: this.supplyFilterCriteriaObj.startDate; //use this in a function
    let endDate = this.supplyFilterCriteriaObj.endDate;

    return [startDate, endDate];
  }

  
  clearSearchInSupplyModeHandler(){
    if(this.searchString || this.searchInSupply === true){
      this.searchString = '';
      this.searchInSupply = false;
      this.clearEmployeeSearch = true;
      this.supplyStore.dispatch(
        new StaffingSupplyActions.ClearSearchString(true)
      );
    }
  }

  

  filterResourcesBySearchInSupplyHandler(searchString: string){
    if(!this.allResourcesWithAvailability){
      this.notifyService.showInfo("Please wait for supply to load before searching !");
      return;
    }

    if(!this.allResourcesWithAvailability.length){
      this.notifyService.showInfo("There are no resources to search in supply. Please update the supply filters !");
      return;
    }

    this.isSupplyLoaded = false;
    this.searchInSupply = true;
    this.searchString = searchString;
    const searchCriteria : BossSearchCriteria = {
      searchString: searchString,
      searchTriggeredFrom: AzureSearchTriggeredFromEnum.HOME_SEARCH_SUPPLY,
      employeeCodesToSearchIn: this.allResourcesWithAvailability.map(x => x.employeeCode).join(","),
      pageSize: this.allResourcesWithAvailability.length
    } 
    
    this.supplyStore.dispatch(
      new StaffingSupplyActions.GetResourcesBySearchString(
        searchCriteria
      )
    );

  }

  private loadResourcesOnSupply(resourcesData: ResourceCommitment) {

    let startDate, endDate;
    [startDate, endDate] = this.getDateRangeForAvailabilityCalculation();


    if (typeof Worker !== "undefined") {

      this.runWorkerToGetAvailableResources(resourcesData, startDate, endDate);

    } else {
      
        const availableResources = ResourceService.createResourcesDataForStaffing(
          resourcesData,
          startDate,
          endDate,
          this.supplyFilterCriteriaObj,
          this.commitmentTypes,
          this.coreService.getUserPreferencesValue(),
          false
        );

        this.allResourcesWithAvailability = availableResources;
        // this.supplyStore.dispatch(
        //   new StaffingSupplyActions.SetAvailableResources({allResources: availableResources})
        // );

        if(this.searchInSupply){
          this.filterResourcesBySearchInSupplyHandler(this.searchString);
        }else{
          this.availableResources = this.allResourcesWithAvailability;
        }

    }
   
  }

  private runWorkerToGetAvailableResources(resourcesData: ResourceCommitment, startDate, endDate) {
    this.webWorker = new Worker(new URL("../shared/web-workers/supply-resource-availability.worker", import.meta.url), {
      type: "module"
    });

    this.webWorker.onmessage = (availableResources) => {
      // this.supplyStore.dispatch(
      //   new StaffingSupplyActions.SetAvailableResources(availableResources.data)
      // );

      this.allResourcesWithAvailability = availableResources.data;
      if(this.searchInSupply && this.searchString){
        this.filterResourcesBySearchInSupplyHandler(this.searchString);
      }else{
        this.availableResources = this.allResourcesWithAvailability;
      }
    };

    this.webWorker.postMessage(
      JSON.stringify({
        resourcesData: resourcesData,
        searchStartDate: startDate,
        searchEndDate: endDate,
        supplyFilterCriteriaObj: this.supplyFilterCriteriaObj,
        commitmentTypes: this.commitmentTypes,
        userPreferences: this.coreService.getUserPreferencesValue(),
        isTriggeredFromSearch: false
      })
    ); 

  }


  openPegRFPopUpHandler(pegOpportunityId) {
    this.pegOpportunityDialogService.openPegOpportunityDetailPopUp(pegOpportunityId)
  }

  getLookupListFromLocalStorage() {
    this.caseTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.caseTypes);
    this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.officeHierarchy);
    this.staffingTags = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffingTags);
    this.staffingTagsHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffingTagsHierarchy);
    this.positionsHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.positionsHierarchy);
    this.levelGrades = this.localStorageService.get(ConstantsMaster.localStorageKeys.levelGradesHierarchy);
    this.positions = this.localStorageService.get(ConstantsMaster.localStorageKeys.positions);
    this.commitmentTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.commitmentTypes);
    this.investmentCategories = this.localStorageService.get(ConstantsMaster.localStorageKeys.investmentCategories);
    this.caseRoleTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.caseRoleTypes);
    this.practiceAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.practiceAreas);
    this.affiliationRoles = this.localStorageService.get(ConstantsMaster.localStorageKeys.affiliationRoles);
    this.demandTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.demandTypes);
  }

  // -----------------------API Calls/Functions--------------------------------------------//
  getProjectsFilteredBySelectedValues(demandFilterCriteriaObj: DemandFilterCriteria, isAdditionalProjects = false) {
    /*getprojects is called when filters change or when page is scrolled
     *If filter changes -> set startIndex to 1 for getting projects from beginning based on new filter
     *If page is scrolled -> increment startIndex for fetching next set of records
     */
    if (
      demandFilterCriteriaObj.officeCodes === "" ||
      demandFilterCriteriaObj.caseTypeCodes === "" ||
      demandFilterCriteriaObj.demandTypes === ""
    ) {
      // As loading the cases takes the time and by the time user unchecks all the offices, then do not load the cases
      this.loadProjects = false;
      this.projects = [];
      this.isDemandLoaded = true;
    } else {
      this.loadProjects = true;
      this.isDemandLoaded = false;
    }

    this.demandStore.dispatch(
      new StaffingDemandActions.LoadProjectsBySelectedFilters({
        demandFilterCriteriaObj
      })
    );
  }


  getProjectsFilteredBySelectedValuesAndSyncToGlobalState(demandFilterCriteriaObj: DemandFilterCriteria) {
    /**If filter changes -> set startIndex to 1 for getting projects from beginning based on new filter
     *If page is scrolled -> increment startIndex for fetching next set of records
     */
    if (
      demandFilterCriteriaObj.officeCodes === "" ||
      demandFilterCriteriaObj.caseTypeCodes === "" ||
      demandFilterCriteriaObj.demandTypes === ""
    ) {
      // As loading the cases takes the time and by the time user unchecks all the offices, then do not load the cases
      this.loadProjects = false;
      this.projects = [];
      this.isDemandLoaded = true;
    } else {
      this.loadProjects = true;
      this.isDemandLoaded = false;
    }

    this.LoadProjectsDataBasedOnSelectedVaues(demandFilterCriteriaObj);
  }
  
  LoadProjectsDataBasedOnSelectedVaues(demandFilterCriteriaObj:DemandFilterCriteria) {

      this.projectAllocationsStore.dispatch(new projectAllocationsAction.LoadProjectsBySelectedFilters({demandFilterCriteriaObj}));

  }

  LoadHistoricalProjectsDataBasedOnSelectedVaues(demandFilterCriteriaObj:DemandFilterCriteria) {

    this.projectAllocationsStore.dispatch(new projectAllocationsAction.LoadHistoricalProjectsBySelectedFilters({demandFilterCriteriaObj}));

  }

  LoadPlanningCardsDataBasedOnSelectedVaues(demandFilterCriteriaObj:DemandFilterCriteria) {

    this.planningCardOverlayStore.dispatch(new planningCardOverlayAction.LoadPlanningCardsBySelectedFilters({demandFilterCriteriaObj}));

}

  

  getHistoricalProjectsFilteredBySelectedValues(demandFilterCriteriaObj: DemandFilterCriteria, isAdditionalProjects = false) {
    /*getprojects is called when filters change or when page is scrolled
     *If filter changes -> set startIndex to 1 for getting projects from beginning based on new filter
     *If page is scrolled -> increment startIndex for fetching next set of records
     */
    if (
      demandFilterCriteriaObj.officeCodes === "" ||
      demandFilterCriteriaObj.caseTypeCodes === "" ||
      demandFilterCriteriaObj.demandTypes === ""
    ) {
      // As loading the cases takes the time and by the time user unchecks all the offices, then do not load the cases
      this.loadProjects = false;
      this.historicalProjects = [];
      this.isHistoricalDemandLoaded = true;
      return false;
    } else {
      this.loadProjects = true;
      this.isHistoricalDemandLoaded = false;
    }

    this.demandStore.dispatch(
      new StaffingDemandActions.LoadHistoricalProjectsBySelectedFilters({
        demandFilterCriteriaObj
      })
    );
    
  }


  getHistoricalProjectsFilteredBySelectedValuesAndSyncToGlobalState(demandFilterCriteriaObj: DemandFilterCriteria) {

    if (
      demandFilterCriteriaObj.officeCodes === "" ||
      demandFilterCriteriaObj.caseTypeCodes === "" ||
      demandFilterCriteriaObj.demandTypes === ""
    ) {
      // As loading the cases takes the time and by the time user unchecks all the offices, then do not load the cases
      this.loadProjects = false;
      this.historicalProjects = [];
      this.isHistoricalDemandLoaded = true;
      return false;
    } else {
      this.loadProjects = true;
      this.isHistoricalDemandLoaded = false;
    }
    this.LoadHistoricalProjectsDataBasedOnSelectedVaues(demandFilterCriteriaObj);
  }

  //get mondays of each week between start date and end date in demand filter criteria
  getWeeks() {
    this.groupingArray = [];
    let monday = DateService.getStartOfWeek(this.dateRangeForResourcesOrProjects.startDate);

    let numberOfWeeksToBucket = DateService.getDatesDifferenceInDays(DateService.parseDateInLocalTimeZone(this.dateRangeForResourcesOrProjects.startDate),
    DateService.parseDateInLocalTimeZone(this.dateRangeForResourcesOrProjects.endDate)) / 7;
    numberOfWeeksToBucket = numberOfWeeksToBucket < 5 ? 5 : numberOfWeeksToBucket;
    for (var i = 0; i < numberOfWeeksToBucket; i++) {
      this.groupingArray.push(DateService.parseDateInLocalTimeZone(monday).toDateString());
      monday.setDate(monday.getDate() + 7);
    }
  }
  getDaily() {
    this.groupingArray = [];
    const startDate = DateService.parseDateInLocalTimeZone(this.dateRangeForResourcesOrProjects.startDate);
    let numberOfDaysToBucket = DateService.getDatesDifferenceInDays(DateService.parseDateInLocalTimeZone(this.dateRangeForResourcesOrProjects.startDate),
      DateService.parseDateInLocalTimeZone(this.dateRangeForResourcesOrProjects.endDate));
      numberOfDaysToBucket = numberOfDaysToBucket < 5 ? 5 : numberOfDaysToBucket;
    for (var i = 0; i <= numberOfDaysToBucket; i++) {
      this.groupingArray.push(DateService.parseDateInLocalTimeZone(startDate).toDateString());
      startDate.setDate(startDate.getDate() + 1);
    }
}



  isPlaceholderAllocationOnPlanningCard(event) {
    return (event.planningCardId && !(event.oldCaseCode || event.pipelineId));
  }

  setProjects(projectsData, demandFilterCriteriaObj: DemandFilterCriteria, projects) {

    if (!this.pageScrolled) {
      projects = [];
    }
    const endDateCondition = new Date(demandFilterCriteriaObj.endDate);

    projectsData = projectsData
      .filter(p => {
        // Check if the project's end date is less than or equal to demandFilterCriteriaObj.endDate
        const projectStartDate = new Date(p.startDate);
        return projectStartDate <= endDateCondition;
      })
      .map(p => {
        return {
          ...p,
          isProjectPinned:
            demandFilterCriteriaObj.caseExceptionShowList
              ?.split(',')
              .includes(p.oldCaseCode) ||
            demandFilterCriteriaObj.opportunityExceptionShowList
              ?.split(',')
              .includes(p.pipelineId)
        };
      });


    const allProjects = projects.concat(projectsData);
    const pinnedProjects = allProjects.filter(p => p.isProjectPinned);
    const unpinnedProjects = allProjects.filter(p => !p.isProjectPinned);

    this.customSortOrderForProjects(pinnedProjects);
    this.customSortOrderForProjects(unpinnedProjects);

    const projectList = [...pinnedProjects, ...unpinnedProjects];



    /*NOTE: We were getting duplicate records when we pinned/unpinned cases & page scrolled was done together.
    REFER BUG NO. 51061*/
    if (this.pageScrolled) {
      projects = this.removeDuplicateProjects(projectList);
    } else {
      projects = projectList;
    }

    
    //ToDo : Remove this as we are not using this sorting and using custom sort in Staffing 2.0
    //commented this functionailty as custom sort on pinned cases is not applicable for staffing 2.0.. It was their on staffing 1.0
    //uncomment this if the functionality comes back in 2.0
    //sorting as per drag and drop pinned projects
    // projects = demandFilterCriteriaObj.caseOppSortOrder.length > 0 ?
    // this.sortDemandSideCardsByUserPreference(projects, demandFilterCriteriaObj.caseOppSortOrder) : projects;

    this.sortPlanningCardsAllocations();
    this.sortCaseOppsAllocations();

    return projects;
  }

  private customSortOrderForProjects(projects) {
    const caseTypeOrder = [1, 5, 2, 4];
    const sortingArray = [
        'pipelineId',
        'caseTypeCode',
        'startDate',
        'clientPriority',
        'probability'
    ];

    return this.customSortingOnProjects(projects, sortingArray, caseTypeOrder);
  }

  private customSortingOnProjects(projects, sortingArray, caseTypeOrder) {
    return projects.sort((a, b) => {
        for (let sortMethod of sortingArray) {
            let result = 0;
            switch (sortMethod) {
                case 'pipelineId':
                    result = SortHelperService.sortByPipelineId(a, b);
                    break;
                case 'caseTypeCode':
                    result = SortHelperService.sortByCaseTypeCode(a, b, caseTypeOrder);
                    break;
                case 'startDate':
                    result = SortHelperService.sortDates(a.startDate, b.startDate, 'asc');
                    break;
                case 'clientPriority':
                    result = SortHelperService.sortByAlphaNumericandNulls(a.clientPriority, b.clientPriority);
                    break;
                case 'probability':
                    result = SortHelperService.sortNumeric(a.probabilityPercent, b.probabilityPercent,'desc');
                    break;
            }
            if (result !== 0) return result;
        }
        return 0;
    });
  }

  
  //ToDo : Remove this as we are not using this sorting and using custom sort in Staffing 2.0
  // private sortDemandSideCardsByUserPreference(cards: any, sortOrder: string) {
  //   let sortedList = [];
  //   sortOrder.split(',').forEach(id => {
  //     let card = cards.find(x => x.oldCaseCode === id || x.pipelineId === id || x.id === id);
  //     if (card) {
  //       sortedList.push(card);
  //       cards.splice(cards.indexOf(card), 1);
  //     }
  //   });
  //   return sortedList.concat(cards);
  // }

  getAllocationsSortedBySelectedValueHandler(event) {
    this.demandFilterCriteriaObj.caseAllocationsSortBy = event;
    this.sortPlanningCardsAllocations();
    this.sortCaseOppsAllocations();
  }

  removeDuplicateProjects(projectList: Project[]) {
    const seen = new Set();
    return projectList.filter(el => {
      if (el.type === 'Opportunity') {
        const duplicate = seen.has(el.pipelineId);
        seen.add(el.pipelineId);
        return !duplicate;
      } else {
        const duplicate = seen.has(el.oldCaseCode);
        seen.add(el.oldCaseCode);
        return !duplicate;
      }
    });
  }

  sortCaseOppsAllocations() {
    this.projects?.map(project => {
      project.allocatedResources = this.sortAllocations(project.allocatedResources);
      return project;
    });
  }

  sortPlanningCardsAllocations() {
    this.planningCards.map(planningCard => {
      planningCard.regularAllocations = this.sortAllocations(planningCard.regularAllocations);
      return planningCard;
    });
  }

  sortAllocations(allocations) {
    if (allocations.length > 1) {
      switch (this.demandFilterCriteriaObj.caseAllocationsSortBy) {
        case ConstantsMaster.NameAZ:
          allocations.sort((a, b) => {
            if (!!a.employeeName && !!b.employeeName) {
              return a.employeeName.localeCompare(b.employeeName);
            } else {
              return;
            }
          });
          break;
        case ConstantsMaster.NameZA:
          allocations.sort((a, b) => {
            if (!!a.employeeName && !!b.employeeName) {
              return b.employeeName.localeCompare(a.employeeName);
            } else {
              return;
            }
          });
          break;
        case ConstantsMaster.EndDateAsc:
          allocations.sort((a, b) => <any>new Date(a.endDate) - <any>new Date(b.endDate));
          break;
        case ConstantsMaster.EndDateDesc:
          allocations.sort((a, b) => <any>new Date(b.endDate) - <any>new Date(a.endDate));
          break;
        case ConstantsMaster.LevelGradeAZ:
          allocations.sort((a, b) => {
            const comparer = this.sortAlphanumeric(a.currentLevelGrade, b.currentLevelGrade);
            if (comparer === 1 || comparer === -1) { return comparer; }
          });
          break;
        case ConstantsMaster.LevelGradeZA:
          allocations.sort((a, b) => {
            const comparer = this.sortAlphanumeric(b.currentLevelGrade, a.currentLevelGrade);
            if (comparer === 1 || comparer === -1) { return comparer; }
          });
          break;
        default:
          allocations.sort((a, b) => a.employeeName.localeCompare(b.employeeName));
          break;
      }
    }
    return allocations;
  }

  getPlanningCardsBySelectedValues(demandFilterCriteriaObj) {

    if (!demandFilterCriteriaObj.demandTypes?.includes('PlanningCards')) {
      this.planningCards = [];
    }

    this.demandStore.dispatch(
      new StaffingDemandActions.LoadPlanningCardsBySelectedFilters({
        demandFilterCriteriaObj
      })
    );
  }

  getPlanningCardsBySelectedValuesAndSyncToGlobalState(demandFilterCriteriaObj) {

    if (!demandFilterCriteriaObj.demandTypes?.includes('PlanningCards')) {
      this.planningCards = [];
    }

    this.LoadPlanningCardsDataBasedOnSelectedVaues(demandFilterCriteriaObj);

  }

  getResourcesAvailabilityBySelectedValuesHandler() {
    // TODO: Write logic for filtering on client side only rather than making APi call
  }

  sortAlphanumeric(previous, next) {
    const regexAlpha = /[^a-zA-Z]/g;
    const regexNumeric = /[^0-9]/g;
    const previousAlphaPart = previous.replace(regexAlpha, "");
    const nextAlphaPart = next.replace(regexAlpha, "");
    if (previousAlphaPart === nextAlphaPart) {
      const previousNumericPart = parseInt(previous.replace(regexNumeric, ""), 10);
      const nextNumericPart = parseInt(next.replace(regexNumeric, ""), 10);
      if (previousNumericPart > nextNumericPart) {
        return 1;
      }
      if (previousNumericPart < nextNumericPart) {
        return -1;
      }
    } else {
      if (previousAlphaPart > nextAlphaPart) {
        return 1;
      }
      if (previousAlphaPart < nextAlphaPart) {
        return -1;
      }
    }
  }




  getWeekStartDate(availabilityyWeeksRange, resource) {
    let effectiveDate = "";
    for (let index = 0; index < availabilityyWeeksRange.length; index++) {
      const startDate = availabilityyWeeksRange[index];
      const endDate = availabilityyWeeksRange[index + 1];
      if (
        (moment(resource.dateFirstAvailable).isSameOrAfter(startDate) &&
          moment(resource.dateFirstAvailable).isBefore(endDate)) ||
        (endDate === undefined && moment(resource.dateFirstAvailable).isSameOrAfter(startDate))
      ) {
        effectiveDate = DateService.convertDateInBainFormat(startDate);
      }
    }
    return effectiveDate;
  }

  getWeeksRange(resources: any) {
    const day = 1; // monday
    let availabilityByWeeks = [];
    const firstResourceAvailableDate = moment(resources[0].dateFirstAvailable).clone();

    // get all mondays in the given date range
    if (
      moment(resources[0].dateFirstAvailable)
        .day(7 + day)
        .isAfter(moment(resources[resources.length - 1].dateFirstAvailable))
    ) {
      availabilityByWeeks.push(firstResourceAvailableDate.day(7 + day).clone());
    } else {
      while (
        firstResourceAvailableDate
          .day(7 + day)
          .isSameOrBefore(moment(resources[resources.length - 1].dateFirstAvailable))
      ) {
        availabilityByWeeks.push(firstResourceAvailableDate.clone());
      }
    }
    availabilityByWeeks = [moment(availabilityByWeeks[0]).subtract(7, "days")].concat(availabilityByWeeks);
    availabilityByWeeks.push(moment(availabilityByWeeks[availabilityByWeeks.length - 1]).add(7, "days"));
    return availabilityByWeeks;
  }
  

  getHistoricalDemandsPinnedState(){
    this.isPinned = this.userPreferences?.isHistoricalDemandPinned;
    this.isHistoricalDemandCollapsed = (!this.userPreferences?.isHistoricalDemandPinned);
    this.expandPanelComplete = false;
  }


  // -----------------------Output Event Handlers--------------------------------------------//
  upsertResourceAllocationsToProjectHandler(upsertedAllocations) {
    // to sort new case allocations according to the selected SortBy option for case cards
    upsertedAllocations.resourceAllocation.map(allocation => {
      this.projects?.every(project => {
        if ((!!project.oldCaseCode && project.oldCaseCode === allocation.oldCaseCode) ||
          (!!project.pipelineId && project.pipelineId === allocation.pipelineId)) {
          project.allocatedResources = this.sortAllocations(project.allocatedResources);
          return false;
        }
        return true;
      });
    });

    this.demandStore.dispatch(
      new StaffingDemandActions.UpsertResourceAllocations({
        resourceAllocation: upsertedAllocations.resourceAllocation,
        splitSuccessMessage: upsertedAllocations.splitSuccessMessage,
        showMoreThanYearWarning: upsertedAllocations.showMoreThanYearWarning,
        allocationDataBeforeSplitting: upsertedAllocations.allocationDataBeforeSplitting,
      })
    );
  }

  upsertPlaceholderHandler(event) {
    let placeholderAllocations: PlaceholderAllocation[] = [];
    placeholderAllocations = placeholderAllocations.concat(event.allocations);
    if (placeholderAllocations.length <= 0) {
      return true;
    }
     this.demandStore.dispatch(
       new StaffingDemandActions.UpsertPlaceholderAllocations({
         event: event,
         placeholderAllocations: placeholderAllocations
         })
     );
    }

  openCaseRollFormHandler(event) {
    this.caseRollDialogService.openCaseRollFormHandler(event);
  }



  openPlaceholderFormHandler(modalData) {
    this.placeholderDialogService.openPlaceholderFormHandler(modalData);
  }

  mergePlanningCardAndAllocationsHandler(payload) {
    this.demandStore.dispatch(
      new StaffingDemandActions.MergePlanningCards({
        planningCard : payload.planningCard,
        resourceAllocations: payload.resourceAllocations,
        placeholderAllocations : payload.placeholderAllocations,
      })
    );
    this.highlightedResourcesInPlanningCards = this.refreshListOfHighlightedResourcesInPlanningCards(this.planningCards);
    this.highlightedResourcesInHistoricalPlanningCards = this.refreshListOfHighlightedResourcesInPlanningCards(this.historicalPlanningCards);
  }

  addProjectToUserExceptionHideListHandler(event) {
    this.userpreferencesService.addCaseOpportunityToUserExceptionHideList(event);
  }

  addProjectToUserExceptionShowListHandler(event) {
    this.userpreferencesService.addCaseOpportunityToUserExceptionShowList(event);
  }

  removeProjectFromUserExceptionShowListHandler(event) {
    this.userpreferencesService.removeCaseOpportunityFromUserExceptionShowList(event);
  }


  openQuickAddFormHandler(modalData) {
    this.quickAddDialogService.openQuickAddFormHandler(modalData);
  }

  openOverlappedTeamsFormHandler(event) {
    this.overlappedTeamDialogService.openOverlappedTeamsFormHandler(event);
  }

  togglePin() {
    this.isPinned = !this.isPinned;
    if(!this.isPinned)
    {
      this.collapseHistoricalDemand = true;
    }
    this.userpreferencesService.updatePinForHistoricalDemands(this.isPinned);
    this.isHistoricalDemandCollapsed = (!this.userPreferences?.isHistoricalDemandPinned);
  }

  toggleHistoricalDemand() {
    this.collapseHistoricalDemand = !this.collapseHistoricalDemand;
    this.isHistoricalDemandCollapsed = this.collapseHistoricalDemand && (!this.userPreferences?.isHistoricalDemandPinned);
  }

  getProjectsOnAdvancedFilterChangeHandler(event) {
    this.demandFilterCriteriaObj.officeCodes = event.officeCodes;
    this.demandFilterCriteriaObj.opportunityStatusTypeCodes = event.opportunityStatusTypeCodes;
    this.demandFilterCriteriaObj.caseAttributeNames = event.caseAttributeNames;
    this.demandFilterCriteriaObj.minOpportunityProbability = event.minDemandProbabilityPercent;
    this.demandFilterCriteriaObj.caseAllocationsSortBy = event.selectedSortByItem;
    this.demandFilterCriteriaObj.industryPracticeAreaCodes = event.selectedIndustryPracticeAreaCodes;
    this.demandFilterCriteriaObj.capabilityPracticeAreaCodes = event.selectedCapabilityPracticeAreaCodes;
    this.demandFilterCriteriaObj.isStaffedFromSupply = event.isStaffedFromSupply;
    this.demandFilterCriteriaObj.caseTypeCodes = event.caseTypeCodes;
    this.demandFilterCriteriaObj.demandTypes = event.demandTypes;
    sessionStorage.setItem('demandFilterCriteriaObj', JSON.stringify(this.demandFilterCriteriaObj));
    this.updateDemandFilterCriteriaFromSupply();

    this.getPlanningCardsBySelectedValues(this.demandFilterCriteriaObj);
    this.getProjectsFilteredBySelectedValues(this.demandFilterCriteriaObj);
    this.historicalDemandFilterCriteriaObj = {...this.demandFilterCriteriaObj};
    // this.historicalDemandFilterCriteriaObj.demandTypes = "ActiveCase";
    this.historicalDemandFilterCriteriaObj.demandTypes = 'ActiveCase,NewDemand'; //passing null to fix issue where cases starting between date ranges were being cosnisdered as new demand in ongoing tabs which was not correct...Issue# STAF-5502
    if (this.demandFilterCriteriaObj.demandTypes?.includes('CasesStaffedBySupply')) {
      this.historicalDemandFilterCriteriaObj.demandTypes += ',CasesStaffedBySupply';
    }
    this.historicalDemandFilterCriteriaObj.startDate = this.historicalDateRangeForResourcesOrProjects.startDate;
    this.historicalDemandFilterCriteriaObj.endDate = this.historicalDateRangeForResourcesOrProjects.endDate;
    this.getHistoricalProjectsFilteredBySelectedValues(this.historicalDemandFilterCriteriaObj);
  }

  getResourcesOnAdvancedFilterChangeHandler(event) {
    this.supplyFilterCriteriaObj.officeCodes = event.officeCodes;
    this.supplyFilterCriteriaObj.levelGrades = event.levelGrades;
    this.supplyFilterCriteriaObj.staffingTags = event.staffingTags;
    this.supplyFilterCriteriaObj.groupBy = event.groupBy;
    this.supplyFilterCriteriaObj.sortBy = event.sortBy;
    this.supplyFilterCriteriaObj.availabilityIncludes = event.availabilityIncludes;
    this.supplyFilterCriteriaObj.practiceAreaCodes = event.selectedPracticeAreaCodes;
    this.supplyFilterCriteriaObj.affiliationRoleCodes = event.selectedAffiliationRoles;
    this.supplyFilterCriteriaObj.positionCodes = event.positionCodes;

    this.getResourcesFilteredBySelectedValues(this.supplyFilterCriteriaObj);

    if (this.demandFilterCriteriaObj.demandTypes?.includes('CasesStaffedBySupply')) {
      this.updateDemandFilterCriteriaFromSupply();
      this.getProjectsFilteredBySelectedValues(this.demandFilterCriteriaObj);
    }    
  }

  refreshProjects() {
    this.getProjectsFilteredBySelectedValues(this.demandFilterCriteriaObj);
    this.getPlanningCardsBySelectedValues(this.demandFilterCriteriaObj);
    this.highlightedResourcesInPlanningCards = this.refreshListOfHighlightedResourcesInPlanningCards(this.planningCards);
    this.highlightedResourcesInHistoricalPlanningCards = this.refreshListOfHighlightedResourcesInPlanningCards(this.historicalPlanningCards);
  }


  refreshListOfHighlightedResourcesInPlanningCards(planningCards) {
    if (planningCards?.length > 0) {
      let employeeCodesInPlanningCards = [];
      planningCards.forEach(planningCard => {
        const uniqueECodesInPlanningCard = [...new Set(planningCard.allocations.map(allocation => allocation.employeeCode))];
        employeeCodesInPlanningCards = employeeCodesInPlanningCards.concat(uniqueECodesInPlanningCard);
      });
      return CommonService.findDuplicatesInArray(employeeCodesInPlanningCards).filter(x => x);
    }
  }

  showQuickPeekDialogHandler(event) {
    this.resourcesCommitmentsDialogService.showResourcesCommitmentsDialogHandler(event);
  }

  removePlaceHolderHandler(event) {
    this.demandStore.dispatch(
      new StaffingDemandActions.DeletePlaceholderAllocations(event)
    );
  }


  addPlanningCardEmitterHandler(event) {
    const planningCard: PlanningCard = {
      createdBy: this.coreService.loggedInUser.employeeCode,
      lastUpdatedBy: this.coreService.loggedInUser.employeeCode,
      startDate: event.startDate,
      endDate: event.endDate,
      probabilityPercent: event.probabilityPercent
    };

    this.demandStore.dispatch(
      new StaffingDemandActions.UpsertPlanningCard({
        planningCard 
      })
    );
  }

  updatePlanningCardEmitterHandler(event) {
    this.demandStore.dispatch(
      new StaffingDemandActions.UpsertPlanningCard({
        planningCard : event
      })
    );
    this.highlightedResourcesInPlanningCards = this.refreshListOfHighlightedResourcesInPlanningCards(this.planningCards);
    this.highlightedResourcesInHistoricalPlanningCards = this.refreshListOfHighlightedResourcesInPlanningCards(this.historicalPlanningCards);
  }

  removePlanningCardEmitterHandler(event) {
    this.demandStore.dispatch(
      new StaffingDemandActions.DeletePlanningCard({
        planningCardId : event.id
      })
    );

    this.highlightedResourcesInPlanningCards = this.refreshListOfHighlightedResourcesInPlanningCards(this.planningCards);
    this.highlightedResourcesInHistoricalPlanningCards = this.refreshListOfHighlightedResourcesInPlanningCards(this.historicalPlanningCards);
  }

  sharePlanningCardEmitterHandler(event) {
    const planningCard = event.planningCard;
    this.demandStore.dispatch(
      new StaffingDemandActions.UpsertPlanningCard({
        planningCard : planningCard,
      })
    );
    this.highlightedResourcesInPlanningCards = this.refreshListOfHighlightedResourcesInPlanningCards(this.planningCards);
    this.highlightedResourcesInHistoricalPlanningCards = this.refreshListOfHighlightedResourcesInPlanningCards(this.historicalPlanningCards);
  }

  toggleWeeklyDailyViewHandler(selectedGroupingOption: string) {
    this.selectedGroupingOption = selectedGroupingOption;
    if(this.selectedGroupingOption === "Weekly") {
      this.getWeeks();
    }
    else{
      this.getDaily();
    }
  }

  expandPanel($event: boolean) {
    this.expandPanelComplete = $event
  }

  // ---------------------------Component Unload--------------------------------------------//

  ngOnDestroy() {
    this.demandStore.dispatch(new StaffingDemandActions.ClearDemandState());
    this.supplyStore.dispatch(new StaffingSupplyActions.ClearSupplyState());
    this.destroy$.next();
    this.destroy$.complete();
    this.subscription.unsubscribe();
    this.storeSub.unsubscribe();
    this.currRoute = "";
  }
}
