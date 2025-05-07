// ----------------------- Angular Package References ----------------------------------//
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, NavigationStart } from '@angular/router';
import { forkJoin, Subject, Subscription } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

// ----------------------- Service References ----------------------------------//
import { BackfillDialogService } from '../overlay/dialogHelperService/backFillDialog.service';
import { CaseRollDialogService } from '../overlay/dialogHelperService/caseRollDialog.service';
import { CoreService } from '../core/core.service';
import { HomeService } from './home.service';
import { LocalStorageService } from '../shared/local-storage.service';
import { NotificationService } from '../shared/notification.service';
import { QuickAddDialogService } from '../overlay/dialogHelperService/quickAddDialog.service';
import { ResourceAssignmentService } from '../overlay/behavioralSubjectService/resourceAssignment.service';
import { ResourceService } from '../shared/helperServices/resource.service';
import { OverlayDialogService } from '../overlay/dialogHelperService/overlayDialog.service';
import { OverlayMessageService } from '../overlay/behavioralSubjectService/overlayMessage.service';
import { UserPreferenceService } from '../overlay/behavioralSubjectService/userPreference.service';
import { ShowQuickPeekDialogService } from '../overlay/dialogHelperService/show-quick-peek-dialog.service';

// --------------------------Interfaces -----------------------------------------//
import { CaseRoleType } from '../shared/interfaces/caseRoleType.interface';
import { CaseType } from '../shared/interfaces/caseType.interface';
import { CommitmentType } from '../shared/interfaces/commitmentType.interface';
import { DemandFilterCriteria } from '../shared/interfaces/demandFilterCriteria.interface';
import { InvestmentCategory } from '../shared/interfaces/investmentCateogry.interface';
import { LevelGrade } from '../shared/interfaces/levelGrade.interface';
import { Office } from '../shared/interfaces/office.interface';
import { OfficeHierarchy } from '../shared/interfaces/officeHierarchy.interface';
import { Project } from '../shared/interfaces/project.interface';
import { Resource } from '../shared/interfaces/resource.interface';
import { ResourceGroup } from '../shared/interfaces/resourceGroup.interface';
import { ServiceLine } from '../shared/interfaces/serviceLine.interface';
import { SupplyFilterCriteria } from '../shared/interfaces/supplyFilterCriteria.interface';
import { UserPreferences } from '../shared/interfaces/userPreferences.interface';

// --------------------------Utils -----------------------------------------//
import * as moment from 'moment';
import { DateService } from '../shared/dateService';
import { GoogleAnalytics } from '../shared/google-analytics/googleAnalytics';
import { ConstantsMaster } from '../shared/constants/constantsMaster';
import { ServiceLineHierarchy } from '../shared/interfaces/serviceLineHierarchy';
import { ServiceLine as ServiceLineEnum } from '../shared/constants/enumMaster';
import { CaseType as CaseTypeEnum } from '../shared/constants/enumMaster';
import { PlaceholderAllocation } from '../shared/interfaces/placeholderAllocation.interface';
import { PlanningCard } from '../shared/interfaces/planningCard.interface';
import { ResourcesCommitmentsDialogService } from '../overlay/dialogHelperService/resourcesCommitmentsDialog.service';
import { PracticeArea } from '../shared/interfaces/practiceArea.interface';
import { PositionHierarchy } from '../shared/interfaces/positionHierarchy.interface';
import { ResourceCommitment } from '../shared/interfaces/resourceCommitment';
import { PlaceholderDialogService } from '../overlay/dialogHelperService/placeholderDialog.service';
import { AppInsightsService } from '../app-insights.service';
import { CaseOppChanges } from '../shared/interfaces/caseOppChanges.interface';
import { CommonService } from '../shared/commonService';
import { OverlappedTeamDialogService } from '../overlay/dialogHelperService/overlapped-team-dialog.service';
import { PlanningCardService } from '../core/services/planning-card.service';
import { SharedService } from 'src/app/shared/shared.service';
import { UserPreferenceSupplyGroupViewModel } from '../shared/interfaces/userPreferenceSupplyGroupViewModel';
import { CombinedUserPreferences } from '../shared/interfaces/combinedUserPreferences.interface';
import { SupplyGroupFilterCriteria } from '../shared/interfaces/supplyGroupFilterCriteria.interface';
import { PegOpportunityDialogService } from '../overlay/dialogHelperService/peg-opportunity-dialog.service';
import { AffiliationRole } from '../shared/interfaces/affiliationRole.interface';

import { Store, select } from '@ngrx/store';
import * as fromResourcesGlobalStore from "../../app/state/reducers/resource-commitment.reducer";
import * as fromProjectssGlobalStore from "../../app/state/reducers/project-allocations.reducer";
import * as ResourcesGlobalStoreActions from "../../app/state/actions/resource-commitment.action";
import * as ProjectsGlobalStoreActions from "../../app/state/actions/project-allocations.action";

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {

  // ----------------------- Notifiers ------------------------------------------------//

  clearEmployeeSearch: Subject<boolean> = new Subject();
  supplyFilterCriteria$: Subject<SupplyFilterCriteria> = new Subject();

  // -----------------------Local Variables--------------------------------------------//
  webWorker: Worker;
  destroy$: Subject<void> = new Subject<void>();
  projects: Project[];
  resourceGroups: ResourceGroup[];
  searchedResourceGroups: ResourceGroup[];
  caseTypes: CaseType[];
  officeHierarchy: OfficeHierarchy;
  staffingTags: ServiceLine[];
  staffingTagsHierarchy: ServiceLineHierarchy[];
  positionsHierarchy: PositionHierarchy[];
  levelGrades: LevelGrade[];
  positions: string[];
  homeOffice: Office;
  resourceLength = 0;
  projectStartIndex = 1;
  scrollDistance: number; // how much percentage the scroll event should fire ( 2 means (100 - 2*10) = 80%)
  pageScrolled = false;
  pageSize: number;
  commitmentTypes: CommitmentType[];
  investmentCategories: InvestmentCategory[];
  caseRoleTypes: CaseRoleType[];
  demandTypes: any[];
  supplyFilterCriteriaObj: SupplyFilterCriteria = {} as SupplyFilterCriteria;
  demandFilterCriteriaObj: DemandFilterCriteria = {} as DemandFilterCriteria;
  supplyGroupFilterCriteriaObj: SupplyGroupFilterCriteria = {} as SupplyGroupFilterCriteria;
  routerSub: any;
  coreServiceSub: Subscription = new Subscription();
  loadProjects = true;
  isSupplyLoaded = false;
  isDemandLoaded = false;
  subscription: Subscription = new Subscription();
  currRoute = '';
  availabilityyWeeksRange: any;
  planningCards: PlanningCard[] = [];
  practiceAreas: PracticeArea[] = [];
  affiliationRoles: AffiliationRole[] =[];
  highlightedResourcesInPlanningCards = [];
  userPreferences: UserPreferences;
  supplyGroupPreferences: UserPreferenceSupplyGroupViewModel[] = [];
  storeSub: Subscription = new Subscription();

  constructor(
    private homeService: HomeService,
    private coreService: CoreService,
    private planningCardService: PlanningCardService,
    private localStorageService: LocalStorageService,
    private resourceAssignmentService: ResourceAssignmentService,
    private overlayMessageService: OverlayMessageService,
    private userpreferencesService: UserPreferenceService,
    private caseRollDialogService: CaseRollDialogService,
    private router: Router,
    private overlayDialogService: OverlayDialogService,
    private quickAddDialogService: QuickAddDialogService,
    private placeholderDialogService: PlaceholderDialogService,
    private backfillDialogService: BackfillDialogService,
    private overlappedTeamDialogService: OverlappedTeamDialogService,
    private notifyService: NotificationService,
    private showQuickPeekDialogService: ShowQuickPeekDialogService,
    private appInsightsService: AppInsightsService,
    private resourcesCommitmentsDialogService: ResourcesCommitmentsDialogService,
    private sharedService: SharedService,
    private pegOpportunityDialogService: PegOpportunityDialogService,
    private resourcesGlobalStore: Store<fromResourcesGlobalStore.State>,
    private projectsGlobalStore: Store<fromProjectssGlobalStore.State>) {

    this.subscribeEvents();

  }

  // -------------------Component LifeCycle Events and Functions----------------------//

  ngOnInit() {
    this.currRoute = this.router.url;
    GoogleAnalytics.staffingTrackPageView(this.coreService.loggedInUser.employeeCode, ConstantsMaster.appScreens.page.home, '');
    this.appInsightsService.logPageView(this.coreService.loggedInUser.employeeCode, ConstantsMaster.appScreens.page.home);

    this.pageSize = this.coreService.appSettings.projectsPageSize;
    this.scrollDistance = this.coreService.appSettings.scrollDistance;
    this.homeOffice = this.coreService.loggedInUser.office;

    // Load data
    this.getDataBasedOnUserPreferences();
    this.getLookupListFromLocalStorage();

    // Close all opened Mat Dialogs on naviagting away from home page
    this.routerSub = this.router.events.subscribe(event => {
      if (event instanceof NavigationStart) {
        this.closeDialogs();
      }
    });


  }

  private loadResourcesOnSupply(resourcesData: ResourceCommitment) {
    if (resourcesData.resources.length <= 0) {
      this.resourceGroups = [];
      this.resourceLength = 0;
      this.isSupplyLoaded = true;
      return;
    }

    if (typeof Worker !== 'undefined') {
      this.runWorkerToGetAvailableResources(resourcesData);
    } else {
      const availableResources = ResourceService.createResourcesDataForStaffing(resourcesData, this.supplyFilterCriteriaObj.startDate, this.supplyFilterCriteriaObj.endDate,
        this.supplyFilterCriteriaObj, this.commitmentTypes, this.coreService.getUserPreferencesValue(), false);
      this.getGroupedSortedResources(availableResources);
    }
  }

  private runWorkerToGetAvailableResources(resourcesData: ResourceCommitment) {
    this.webWorker = new Worker(new URL('../shared/web-workers/supply-resource-availability.worker', import.meta.url), { type: "module" });
    this.webWorker.onmessage = (availableResources) => {
      this.getGroupedSortedResources(availableResources.data);
    };

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

  private getGroupedSortedResources(availableResources: any) {
    if (availableResources && availableResources.length) {
      this.getResourcesSortAndGroupBySelectedValues(availableResources, this.supplyFilterCriteriaObj.groupBy, this.supplyFilterCriteriaObj.sortBy);
    }

    this.resourceLength = 0; // So that the count always starts with zero whether any office is selected or unselected.
    if (this.resourceGroups && this.resourceGroups.length) {
      this.resourceGroups.forEach(groups => {
        this.resourceLength += groups.resources.length;
        groups.groupTitle = `${groups.groupTitle}`;
      });
    }
    this.isSupplyLoaded = true;
  }

  closeDialogs() {
    this.overlayDialogService.closeDialogs();
  }

  subscribeEvents() {
    this.storeSub.add(this.resourcesGlobalStore
      .pipe(
        select(fromResourcesGlobalStore.getRefreshResources))
      .subscribe((refreshResources) => {
        if (refreshResources) {
          this.refreshResources();
          this.resourcesGlobalStore.dispatch(
            new ResourcesGlobalStoreActions.ResetRefreshResourcesSuccess(false));
        }
      }))

      this.storeSub.add(this.projectsGlobalStore
        .pipe(
          select(fromProjectssGlobalStore.getRefreshCasesAndOpoortunities))
        .subscribe((refreshCasesAndOpportunities) => {
          if (refreshCasesAndOpportunities) {
            this.refreshProjects();
            this.projectsGlobalStore.dispatch(
              new ProjectsGlobalStoreActions.ResetRefreshCasesAndOpportunitiesSuccess(false));
          }
        }))

    this.subscription.add(this.overlayMessageService.refreshCasesAndopportunties().subscribe(result => {
      if (result) {
        this.refreshProjects();
      }
    }));

    this.subscription.add(this.overlayMessageService.refreshResources().subscribe(result => {
      if (result) {
        this.refreshResources();
      }
    }));

    this.subscription.add(this.overlayMessageService.refreshHighlightedResources().subscribe(allocationId => {
      if (allocationId) {
        this.refreshHighlightedResources(allocationId);
      }
    }));

    this.subscription.add(this.overlayMessageService.refreshProjectForCaseRoll().subscribe(result => {
      if (result !== null) {
        const matchedProject: Project = this.projects.find(x => (x.oldCaseCode && x.oldCaseCode === result.project.oldCaseCode)
          || x.pipelineId && x.pipelineId === result.pipelineId);

        if (!matchedProject)
          return;

        matchedProject.allocatedResources = matchedProject.allocatedResources.map(x => {
          if (result.updatedResourceAllocation.some(r => r.id === x.id)) {
            return result.updatedResourceAllocation.find(allocation => allocation.id === x.id);
          } else {
            return x;
          }
        });
        matchedProject.caseRoll = result.caseRoll;

        if (matchedProject.oldCaseCode) {
          this.projects.map(obj => obj.oldCaseCode === matchedProject?.oldCaseCode || obj);
        } else {
          this.projects.map(obj => obj.pipelineId === matchedProject?.pipelineId || obj);
        }
      }
    }));

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

    this.subscription.add(this.overlayMessageService.getUpdatedUserPreferences().subscribe((userpreferences: UserPreferences) => {
      if (userpreferences !== null) {

        if (this.demandFilterCriteriaObj) {
          this.demandFilterCriteriaObj.caseExceptionShowList = userpreferences.caseExceptionShowList || '';
          this.demandFilterCriteriaObj.caseExceptionHideList = userpreferences.caseExceptionHideList || '';
          this.demandFilterCriteriaObj.opportunityExceptionShowList = userpreferences.opportunityExceptionShowList || '';
          this.demandFilterCriteriaObj.opportunityExceptionHideList = userpreferences.opportunityExceptionHideList || '';
          this.demandFilterCriteriaObj.caseOppSortOrder = userpreferences.caseOppSortOrder || '';
          this.demandFilterCriteriaObj.planningCardsSortOrder = userpreferences.planningCardsSortOrder || '';
        }

      }
    }));

    this.subscription.add(this.overlayMessageService.getCaseopportunityToUpdate().subscribe(event => {
      if (event !== null) {
        if (event.oldCaseCode) {
          this.projects = this.projects.filter(project => project.oldCaseCode !== event.oldCaseCode);
        } else if (event.pipelineId) {
          this.projects = this.projects.filter(project => project.pipelineId !== event.pipelineId);
        }
      }
    }));

    this.subscription.add( this.coreService.openCaseOppOverlayFromNotifications().pipe(takeUntil(this.destroy$)).subscribe(value => {
      if (value && this.currRoute.includes('home')) {
        this.openProjectDetailsDialogHandler(value);
      }
    }));
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

  getDataBasedOnUserPreferences() {
    // this is done so that whenever user changes their user settings, it reflects in the projects and resources data
    this.coreServiceSub.add(this.coreService.getCombinedUserPreferences().subscribe((combinedUserPreferences : CombinedUserPreferences) => {
      this.userPreferences = combinedUserPreferences.userPreferences;
      this.supplyGroupPreferences = combinedUserPreferences.userPreferenceSupplyGroups;

      this.clearEmployeeSearch.next(true);
      this.updateSupplyAndDemandSettings();
      this.updateDefaultSupplyGroupSettings();

      // load projects and resources data

      this.getResourcesByDefaultSettings();
      this.getProjectsFilteredBySelectedValues(this.demandFilterCriteriaObj);
      this.getPlanningCardsBySelectedValues(this.demandFilterCriteriaObj);
    }));
  }

  getResourcesByDefaultSettings(){
    if(this.supplyGroupPreferences.some(x => x.isDefault)){
      this.getResourcesFilteredBySelectedGroups(this.supplyGroupFilterCriteriaObj);
    }else{
      this.getResourcesFilteredBySelectedValues(this.supplyFilterCriteriaObj);
    }

  }

  onStageScrolled() {
    // if total projects displayed are less than the total page size, it means there are not more projects that needs to be fetched
    const totalPageSize = this.projectStartIndex + this.pageSize - 1; //subtracting 1 because index starts from 1

    if (this.projects.length < totalPageSize) {
      return;
    }

    this.getMoreProjects();
  }

  // ------------------------ Helper Functions------------------------------------------//

  getMoreProjects() {
    this.projectStartIndex = this.projectStartIndex + this.pageSize;
    this.getProjectsFilteredBySelectedValues(this.demandFilterCriteriaObj, true);
  }

  setProjects(projectsData, demandFilterCriteriaObj: DemandFilterCriteria) {

    if (!this.pageScrolled) {
      this.projects = [];
    }
    projectsData = projectsData.map(p => {
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
    const projectList = this.projects.concat(projectsData).sort((a, b) => <any>b.isProjectPinned - <any>a.isProjectPinned);

    /*NOTE: We were getting duplicate records when we pinned/unpinned cases & page scrolled was done together.
    REFER BUG NO. 51061*/
    if (this.pageScrolled) {
      this.projects = this.removeDuplicateProjects(projectList);
    } else {
      this.projects = projectList;
    }

    //sorting as per drag and drop pinned projects
    this.projects = demandFilterCriteriaObj.caseOppSortOrder.length > 0 ?
      this.sortDemandSideCardsByUserPreference(this.projects, demandFilterCriteriaObj.caseOppSortOrder) : this.projects;

    this.sortPlanningCardsAllocations();
    this.sortCaseOppsAllocations();
  }

  sortCaseOppsAllocations() {
    this.projects.map(project => {
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

  // -----------------------API Calls/Functions--------------------------------------------//
  getProjectsFilteredBySelectedValues(demandFilterCriteriaObj: DemandFilterCriteria, isAdditionalProjects = false) {
    /*getprojects is called when filters change or when page is scrolled
      *If filter changes -> set startIndex to 1 for getting projects from beginning based on new filter
      *If page is scrolled -> increment startIndex for fetching next set of records
    */
    if (demandFilterCriteriaObj.officeCodes === ''
      || demandFilterCriteriaObj.caseTypeCodes === ''
      || demandFilterCriteriaObj.demandTypes === '') {
      // As loading the cases takes the time and by the time user unchecks all the offices, then do not load the cases
      this.loadProjects = false;
      this.projects = [];
      this.isDemandLoaded = true;
      return false;
    } else {
      this.loadProjects = true;
      this.isDemandLoaded = false;
    }

    this.pageScrolled = isAdditionalProjects;

    this.projectStartIndex = this.pageScrolled ? this.projectStartIndex : 1;

    demandFilterCriteriaObj.projectStartIndex = this.projectStartIndex;
    demandFilterCriteriaObj.pageSize = this.pageSize;

    this.homeService.getProjectsFilteredBySelectedValues(demandFilterCriteriaObj)
      .pipe(takeUntil(this.destroy$)).subscribe(projectsData => {
        if (this.loadProjects) {
          this.setProjects(projectsData, demandFilterCriteriaObj);
          this.isDemandLoaded = true;
        }
      });
  }

  resetProjectData() {
    if (!this.pageScrolled) {
      this.projects = [];
    }
  }

  getResourcesFilteredBySelectedValues(supplyFilterCriteriaObj : SupplyFilterCriteria) {
    const officeCodes = supplyFilterCriteriaObj.officeCodes;
    this.searchedResourceGroups = [];
    if (!officeCodes || officeCodes === '') {
      this.resourceGroups = null;
      this.resourceLength = 0;
      this.isSupplyLoaded = true;
      return false;
    } else {
      this.isSupplyLoaded = false;

      this.homeService.getResourcesFilteredBySelectedValues(supplyFilterCriteriaObj)
      .subscribe(resourcesData => {
        this.loadResourcesOnSupply(resourcesData);
      });
    }
  }

  getResourcesFilteredBySelectedGroups(supplyGroupFilterCriteriaObj : SupplyGroupFilterCriteria) {
    const employeeCodes = supplyGroupFilterCriteriaObj.employeeCodes;
    this.searchedResourceGroups = [];
    if (!employeeCodes) {
      this.resourceGroups = null;
      this.resourceLength = 0;
      this.isSupplyLoaded = true;
      return false;
    }else {
      this.isSupplyLoaded = false;

      this.homeService.getResourcesFilteredBySelectedGroup(supplyGroupFilterCriteriaObj)
      .subscribe(resourcesData => {
        this.loadResourcesOnSupply(resourcesData);
      });
    }
  }


  getResourcesSortAndGroupBySelectedValuesHandler(event) {
    let resources: Resource[];
    if (this.resourceGroups) {
      this.resourceGroups.forEach(group => {
        resources = (resources || []).concat(group.resources);
      });

      this.getResourcesSortAndGroupBySelectedValues(resources, event.groupByList, event.sortByList);
    }
  }

  getResourcesAvailabilityBySelectedValuesHandler(event) {
    // TODO: Write logic for filtering on client side only rather than making APi call
  }

  sortAlphanumeric(previous, next) {
    const regexAlpha = /[^a-zA-Z]/g;
    const regexNumeric = /[^0-9]/g;
    const previousAlphaPart = previous.replace(regexAlpha, '');
    const nextAlphaPart = next.replace(regexAlpha, '');
    if (previousAlphaPart === nextAlphaPart) {
      const previousNumericPart = parseInt(previous.replace(regexNumeric, ''), 10);
      const nextNumericPart = parseInt(next.replace(regexNumeric, ''), 10);
      if (previousNumericPart > nextNumericPart) { return 1; }
      if (previousNumericPart < nextNumericPart) { return -1; }
    } else {
      if (previousAlphaPart > nextAlphaPart) { return 1; }
      if (previousAlphaPart < nextAlphaPart) { return -1; }
    }

  }

  getResourcesSortAndGroupBySelectedValues(resources, groupBy, sortBy) {
    if (sortBy === undefined || sortBy == null || sortBy === '') {
      sortBy = 'levelGrade';
    } else {
      this.supplyFilterCriteriaObj.sortBy = sortBy;
    }
    if (groupBy === undefined || groupBy == null || groupBy === '') {
      groupBy = 'serviceLine';
    } else {
      this.supplyFilterCriteriaObj.groupBy = groupBy;
    }
    const sortByList = (sortBy && sortBy.length) > 1 ? sortBy.split(',') : null;

    const groupByList = (groupBy && groupBy.length) > 0 ? groupBy : null;

    this.resourceGroups = this.groupResourceBySelectedKey(resources, groupByList);

    if (sortByList && this.resourceGroups && this.resourceGroups.length > 0) {
      this.resourceGroups.forEach(group => {
        group.resources.sort((previousElement, nextElement) => {
          for (let index = 0; index < sortByList.length; index++) {
            if (sortByList[index] === 'levelGrade') {
              const comparer = this.sortAlphanumeric(previousElement.levelGrade, nextElement.levelGrade);
              if (comparer === 1 || comparer === -1) { return comparer; }
            }
            if (sortByList[index] === 'office' &&
              previousElement[sortByList[index]].officeName > nextElement[sortByList[index]].officeName) {
              return 1;
            }
            if (sortByList[index] === 'office' &&
              previousElement[sortByList[index]].officeName < nextElement[sortByList[index]].officeName) {
              return -1;
            }
            if (sortByList[index] === 'dateFirstAvailable' &&
              new Date(previousElement[sortByList[index]]) > new Date(nextElement[sortByList[index]])) {
              return 1;
            }
            if (sortByList[index] === 'dateFirstAvailable' &&
              new Date(previousElement[sortByList[index]]) < new Date(nextElement[sortByList[index]])) {
              return -1;
            }
            if (previousElement[sortByList[index]] > nextElement[sortByList[index]]) {
              return 1;
            }
            if (previousElement[sortByList[index]] < nextElement[sortByList[index]]) {
              return -1;
            }

          }

        });
      });
    }
  }

  private groupBy(array, func) {
    const groups = {};
    array.forEach(obj => {
      const group = JSON.stringify(func(obj));
      groups[group] = groups[group] || [];
      groups[group].push(obj);
    });
    return groups;
  }


  groupResourceBySelectedKey(resources, groupByList) {
    let groupedResources: ResourceGroup[];
    if (!groupByList?.length) {
      groupByList = [];
      groupByList.push('serviceLine');
    }

    if (!resources?.length) {
      return groupedResources;
    }

    // sort resources before grouping
    if (groupByList.indexOf('dateFirstAvailable') > -1) {
      resources.sort((previousElement, nextElement) => {
        return <any>new Date(previousElement.dateFirstAvailable) - <any>new Date(nextElement.dateFirstAvailable);
      });
    } else if (groupByList.indexOf('office') > -1) {
      resources.sort((previousElement, nextElement) => {
        return previousElement.schedulingOffice.officeName - nextElement.schedulingOffice.officeName;
      });
    } else {
      resources.sort((previousElement, nextElement) => {
        return previousElement.levelGrade.toString().localeCompare(nextElement.levelGrade, 'en', { numeric: true });
      }).reverse();
    }

    if (groupByList.indexOf('weeks') > -1) {
      resources.sort((previousElement, nextElement) => {
        return <any>new Date(previousElement.dateFirstAvailable) - <any>new Date(nextElement.dateFirstAvailable);
      });
      this.availabilityyWeeksRange = this.getWeeksRange(resources);
    }

    const reducedArray = this.groupBy(resources, resource => {
      const groupingArray = [];
      if (groupByList.indexOf('office') > -1) {
        groupingArray.push(resource.schedulingOffice.officeName);
      }
      if (groupByList.indexOf('serviceLine') > -1) {
        groupingArray.push(resource.serviceLine.serviceLineName);
      }
      if (groupByList.indexOf('position') > -1) {
        groupingArray.push(resource.position.positionGroupName);
      }
      if (groupByList.indexOf('levelGrade') > -1) {
        groupingArray.push(resource.levelGrade);
      }
      if (groupByList.indexOf('dateFirstAvailable') > -1) {
        groupingArray.push(moment(resource.dateFirstAvailable).format('DD-MMM-YYYY'));
      }
      if (groupByList.indexOf('availability') > -1) {
        groupingArray.push(resource.availabilityStatus);
      }
      if (groupByList.indexOf('weeks') > -1) {
        const weekStartDate = this.getWeekStartDate(this.availabilityyWeeksRange, resource);
        groupingArray.push(weekStartDate);
      }
      return groupingArray;
    });

    Object.entries(reducedArray).forEach(([key, value]) => {
      const keyWithoutDoubleQuotes = key.replace(/['"]+/g, '');
      const groupingKey = keyWithoutDoubleQuotes.substring(1, keyWithoutDoubleQuotes.lastIndexOf(']')).split(',');
      const group: ResourceGroup = {
        groupTitle: groupingKey.length > 1 ? groupingKey.join(' - ') : groupingKey[0],
        resources: JSON.parse(JSON.stringify(value))
      };

      group.groupTitle += ` (${group.resources.length})`;
      (groupedResources = groupedResources || []).push(group);
    });

    return groupedResources;
  }

  getWeekStartDate(availabilityyWeeksRange, resource) {
    let effectiveDate = '';
    for (let index = 0; index < availabilityyWeeksRange.length; index++) {
      const startDate = availabilityyWeeksRange[index];
      const endDate = availabilityyWeeksRange[index + 1];
      if ((moment(resource.dateFirstAvailable).isSameOrAfter(startDate)
        && moment(resource.dateFirstAvailable).isBefore(endDate)) ||
        (endDate === undefined && (moment(resource.dateFirstAvailable).isSameOrAfter(startDate)))) {
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
    if (moment(resources[0].dateFirstAvailable).day(7 + day).isAfter(moment(resources[resources.length - 1].dateFirstAvailable))) {
      availabilityByWeeks.push(firstResourceAvailableDate.day(7 + day).clone());
    } else {
      while (firstResourceAvailableDate.day(7 + day).isSameOrBefore(moment(resources[resources.length - 1].dateFirstAvailable))) {
        availabilityByWeeks.push(firstResourceAvailableDate.clone());
      }
    }
    availabilityByWeeks = [moment(availabilityByWeeks[0]).subtract(7, 'days')].concat(availabilityByWeeks);
    availabilityByWeeks.push(moment(availabilityByWeeks[availabilityByWeeks.length - 1]).add(7, 'days'));
    return availabilityByWeeks;
  }

  getResourcesIncludingTerminatedBySearchString(searchString) {
    // Show all resources for loggedInUser office if typeahead is deleted or character is less than 3
    searchString = searchString.trim();

    if (searchString.length < 3) {
      this.searchedResourceGroups = [];
      return;
    }

    const addTransfers = true;

    this.homeService.getResourcesIncludingTerminatedBySearchString(searchString.trim(), addTransfers)
      .pipe(takeUntil(this.destroy$))
      .subscribe(searchedResources => {
        // Promise might take time to return hence, avoiding return of Searched employees when employeeSearch string is empty
        if (searchedResources.resources.length === 0) {
          return;
        }

        const availableResources = ResourceService.createResourcesDataForStaffing(searchedResources, null, null,
          this.supplyFilterCriteriaObj, this.commitmentTypes, this.coreService.getUserPreferencesValue(), true);
        const group = {
          groupTitle: `Search Results (${availableResources.length})`,
          resources: [...availableResources]
        };
        this.searchedResourceGroups = [group];

      });
  }

  // -----------------------Output Event Handlers--------------------------------------------//

  updateDemandCardsSortOrderEmitterHandler(event) {
    this.userpreferencesService.updateDemandSortOrder(event);
  }

  updateProjectChangesHandler(event: Project) {
    var projectChanges: CaseOppChanges = {
      pipelineId: event.pipelineId,
      oldCaseCode: event.oldCaseCode,
      startDate: event.startDate,
      endDate: event.endDate,
      notes: event.notes
    }
    if (event.oldCaseCode) {
      this.homeService.updateCaseChanges(projectChanges).subscribe(result => {
        this.notifyService.showSuccess('Notes added successfully');
      });
    } else {
      this.homeService.updateOppChanges(projectChanges).subscribe(result => {
        this.notifyService.showSuccess('Notes added successfully');
      });
    }
  }

  deleteNotesOnProjectHandler(event) {
    var projectChanges: CaseOppChanges = {
      pipelineId: event.pipelineId,
      oldCaseCode: event.oldCaseCode,
      startDate: event.startDate,
      endDate: event.endDate,
      notes: event.notes
    }
    if (event.oldCaseCode) {
      this.homeService.updateCaseChanges(projectChanges).subscribe(result => {
        this.notifyService.showSuccess('Notes deleted successfully');
      });
    } else {
      this.homeService.updateOppChanges(projectChanges).subscribe(result => {
        this.notifyService.showSuccess('Notes deleted successfully');
      });
    }
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

  refreshResources() {
    this.clearEmployeeSearch.next(true);
    this.getResourcesByDefaultSettings();
    this.refreshListOfHighlightedResourcesInPlanningCards();
  }

  refreshProjects() {
    this.getProjectsFilteredBySelectedValues(this.demandFilterCriteriaObj);
    this.getPlanningCardsBySelectedValues(this.demandFilterCriteriaObj);
    this.refreshListOfHighlightedResourcesInPlanningCards();
  }

  refreshHighlightedResources(allocationId) {
    this.planningCards.forEach(planningCard => {
      const allocationIndex = planningCard.allocations.findIndex(x => x.id == allocationId);
      if (allocationIndex > -1)
        planningCard.allocations.splice(allocationIndex, 1);
    });
    this.refreshListOfHighlightedResourcesInPlanningCards();
  }

  getResourcesHandler(event) {

    this.supplyFilterCriteriaObj.startDate = DateService.getFormattedDate(event.dateRange[0]);
    this.supplyFilterCriteriaObj.endDate = DateService.getFormattedDate(event.dateRange[1]);
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

  getResourcesIncludingTerminatedBySearchStringHandler(event) {
    this.getResourcesIncludingTerminatedBySearchString(event.typeahead);
  }


  getProjectsOnBasicFilterChangeHandler(event) {
    // set variables so that these can be used for getting paginated projects on scroll

    this.demandFilterCriteriaObj.startDate = DateService.getFormattedDate(event.dateRange[0]);
    this.demandFilterCriteriaObj.endDate = DateService.getFormattedDate(event.dateRange[1]);
    this.demandFilterCriteriaObj.officeCodes = event.officeCodes;
    this.demandFilterCriteriaObj.caseTypeCodes = event.caseTypeCodes;
    this.demandFilterCriteriaObj.demandTypes = event.demandTypes;

    this.updateDemandFilterCriteriaFromSupply();
    this.getPlanningCardsBySelectedValues(this.demandFilterCriteriaObj);
    this.getProjectsFilteredBySelectedValues(this.demandFilterCriteriaObj);
    sessionStorage.setItem('demandFilterCriteriaObj', JSON.stringify(this.demandFilterCriteriaObj));
  }

  getProjectsOnAdvancedFilterChangeHandler(event) {
    this.demandFilterCriteriaObj.opportunityStatusTypeCodes = event.opportunityStatusTypeCodes;
    this.demandFilterCriteriaObj.caseAttributeNames = event.caseAttributeNames;
    this.demandFilterCriteriaObj.minOpportunityProbability = event.minOpportunityProbability;
    this.demandFilterCriteriaObj.caseAllocationsSortBy = event.selectedSortByItem;
    this.demandFilterCriteriaObj.industryPracticeAreaCodes = event.selectedIndustryPracticeAreaCodes;
    this.demandFilterCriteriaObj.capabilityPracticeAreaCodes = event.selectedCapabilityPracticeAreaCodes;
    this.demandFilterCriteriaObj.isStaffedFromSupply = event.isStaffedFromSupply;

    this.getPlanningCardsBySelectedValues(this.demandFilterCriteriaObj);
    this.getProjectsFilteredBySelectedValues(this.demandFilterCriteriaObj);
    sessionStorage.setItem('demandFilterCriteriaObj', JSON.stringify(this.demandFilterCriteriaObj));
  }

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
    this.resourceAssignmentService.upsertResourceAllocationsToProject(upsertedAllocations, null, null);
    this.resourceAssignmentService.upsertPlaygroundAllocationsForCasePlanningMetrics(upsertedAllocations.resourceAllocation);
  }

  openProjectDetailsDialogHandler(projectData) {
    this.overlayDialogService.openProjectDetailsDialogHandler(projectData);
  }

  openResourceDetailsDialogHandler(employeeCode) {
    this.overlayDialogService.openResourceDetailsDialogHandler(employeeCode);
  }

  openQuickAddFormHandler(modalData) {
    this.quickAddDialogService.openQuickAddFormHandler(modalData);
  }

  openPlaceholderFormHandler(modalData) {
    this.placeholderDialogService.openPlaceholderFormHandler(modalData);
  }

  openBackFillFormHandler(event) {
    this.backfillDialogService.openBackFillFormHandler(event);
  }

  openOverlappedTeamsFormHandler(event) {
    this.overlappedTeamDialogService.openOverlappedTeamsFormHandler(event);
  }

  openProjectDetailsDialogFromTypeaheadHandler(event) {
    this.openProjectDetailsDialogHandler(event);
  }

  openCaseRollFormHandler(event) {
    this.caseRollDialogService.openCaseRollFormHandler(event);
  }

  showResourcesCommitmentsDialog(event) {
    this.resourcesCommitmentsDialogService.showResourcesCommitmentsDialogHandler(event);
  }

  showQuickPeekDialogHandler(event) {
    this.showQuickPeekDialogService.showQuickPeekDialogHandler(event);
  }

  updateDemandFilterCriteriaFromSupply() {
    if (this.demandFilterCriteriaObj.demandTypes?.includes("CasesStaffedBySupply")) {
      this.demandFilterCriteriaObj.supplyFilterCriteria = this.supplyFilterCriteriaObj;
    } else {
      this.demandFilterCriteriaObj.supplyFilterCriteria = null;
    }

  }

  updateSupplyAndDemandSettings() {
    const defaultDateRange = DateService.getFormattedDateRange(null); // Default date range will be today + 2 weeks on load
    const userPreferences = this.userPreferences;

    if (userPreferences && typeof (userPreferences) === 'object') {
      const startDate = new Date();
      let dateRangeForProjects: { startDate: any; endDate: any; };

      /*-------------- Set user preferences for Supply Side ---------------------*/
      let dateRangeForResources: { startDate: any; endDate: any; };

      if (userPreferences.supplyWeeksThreshold) {
        const today = new Date();
        const endDate = new Date(today.setDate(today.getDate() + userPreferences.supplyWeeksThreshold * 7));
        const date = { startDate: startDate, endDate: endDate };

        dateRangeForResources = DateService.getFormattedDateRange(date);
      } else {

        dateRangeForResources = defaultDateRange;
      }

      this.supplyFilterCriteriaObj.startDate = dateRangeForResources.startDate;
      this.supplyFilterCriteriaObj.endDate = dateRangeForResources.endDate;
      this.supplyFilterCriteriaObj.officeCodes = userPreferences.supplyViewOfficeCodes || this.homeOffice.officeCode.toString();
      this.supplyFilterCriteriaObj.levelGrades = userPreferences.levelGrades;
      this.supplyFilterCriteriaObj.availabilityIncludes = userPreferences.availabilityIncludes;
      this.supplyFilterCriteriaObj.staffingTags = userPreferences.supplyViewStaffingTags || ServiceLineEnum.GeneralConsulting;
      this.supplyFilterCriteriaObj.groupBy = userPreferences.groupBy;
      this.supplyFilterCriteriaObj.sortBy = userPreferences.sortBy;
      this.supplyFilterCriteriaObj.practiceAreaCodes = userPreferences.practiceAreaCodes;
      this.supplyFilterCriteriaObj.positionCodes = userPreferences.positionCodes;
      this.supplyFilterCriteriaObj.affiliationRoleCodes = userPreferences.affiliationRoleCodes;

      /*-------------- Set user preferences for Demand Side ---------------------*/

      if (userPreferences.demandWeeksThreshold) {
        const today = new Date();
        const endDate = new Date(today.setDate(today.getDate() + userPreferences.demandWeeksThreshold * 7));
        const date = { startDate: startDate, endDate: endDate };

        dateRangeForProjects = DateService.getFormattedDateRange(date);
      } else {
        // Default date range will be today + 2weeks on load
        dateRangeForProjects = defaultDateRange;
      }

      this.demandFilterCriteriaObj.startDate = dateRangeForProjects.startDate;
      this.demandFilterCriteriaObj.endDate = dateRangeForProjects.endDate;
      this.demandFilterCriteriaObj.officeCodes = userPreferences.demandViewOfficeCodes || this.homeOffice.officeCode.toString();
      this.demandFilterCriteriaObj.caseTypeCodes = userPreferences.caseTypeCodes || CaseTypeEnum.Billable;
      this.demandFilterCriteriaObj.caseAttributeNames = userPreferences.caseAttributeNames || '';
      this.demandFilterCriteriaObj.opportunityStatusTypeCodes = userPreferences.opportunityStatusTypeCodes;
      this.demandFilterCriteriaObj.demandTypes = userPreferences.demandTypes;
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

      this.supplyFilterCriteriaObj.startDate = defaultDateRange.startDate;
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

  private updateDefaultSupplyGroupSettings(){

    const defaultDateRange = DateService.getFormattedDateRange(null); // Default date range will be today + 2 weeks on load

    const defaultGroupPreferences : UserPreferenceSupplyGroupViewModel = this.supplyGroupPreferences.find(x => x.isDefault);

    if (defaultGroupPreferences && typeof defaultGroupPreferences === 'object') {
      const startDate = new Date();

      let dateRangeForResources: { startDate: any; endDate: any; };

      if (this.userPreferences.supplyWeeksThreshold) {
        const today = new Date();
        const endDate = new Date(today.setDate(today.getDate() + this.userPreferences.supplyWeeksThreshold * 7));
        const date = { startDate: startDate, endDate: endDate };

        dateRangeForResources = DateService.getFormattedDateRange(date);
      } else {

        dateRangeForResources = defaultDateRange;
      }

      this.supplyGroupFilterCriteriaObj.startDate = dateRangeForResources.startDate;
      this.supplyGroupFilterCriteriaObj.endDate = dateRangeForResources.endDate;
      this.supplyGroupFilterCriteriaObj.employeeCodes = defaultGroupPreferences.groupMembers.map(x => x.employeeCode).join(",");
      this.supplyGroupFilterCriteriaObj.availabilityIncludes = this.supplyFilterCriteriaObj.availabilityIncludes;
      this.supplyGroupFilterCriteriaObj.sortBy = this.supplyFilterCriteriaObj.sortBy;
      this.supplyGroupFilterCriteriaObj.groupBy = this.supplyFilterCriteriaObj.groupBy;
    }
  }

  upsertPlaceholderHandler(event) {
    let placeholderAllocations: PlaceholderAllocation[] = [];
    placeholderAllocations = placeholderAllocations.concat(event.allocations);
    if (placeholderAllocations.length <= 0) {
      return true;
    }
    this.homeService.upsertPlaceholderAllocations(placeholderAllocations).pipe(takeUntil(this.destroy$)).subscribe(
      (updatedData) => {
        if (event.isMergeFromPlanningCard) {
          this.onSuccessPlanningCardMerge(updatedData);
          if (event.isCopyAndMerge) {
            this.notifyService.showSuccess('Planning Card has been copied and merged');
          }
        } else if (updatedData[0].employeeCode && updatedData[0].id) {
          this.notifyService.showSuccess(`Placeholder assignment for ${updatedData[0].employeeName} is created/updated`);
          event.allocations.id = updatedData[0].id;
        } else if (!updatedData[0].employeeCode && updatedData[0].id && updatedData[0].operatingOfficeCode) {
          this.notifyService.showSuccess(`Guessed Placeholder assignment is created/updated`);
          event.allocations.id = updatedData[0].id;
        } else if (updatedData[0].employeeCode === null && event.allocations.id !== null) {
          this.notifyService.showSuccess(`Placeholder assignment is removed`);
        } else if (event.allocations.id === null) {
          this.notifyService.showSuccess('Placeholder Created');
          if (!this.isPlaceholderAllocationOnPlanningCard(event.allocations)) {
            const projectToUpdate = updatedData[0]?.oldCaseCode
              ? this.projects.find(x => x.oldCaseCode === updatedData[0].oldCaseCode)
              : this.projects.find(x => x.pipelineId === updatedData[0].pipelineId);
            projectToUpdate?.placeholderAllocations.push(updatedData[0]);
            if (updatedData[0].oldCaseCode) {
              this.projects.map(obj => obj.oldCaseCode === projectToUpdate?.oldCaseCode || obj);
            } else {
              this.projects.map(obj => obj.pipelineId === projectToUpdate?.pipelineId || obj);
            }
          } else {
            // add placeholder allocation to planning card
            const planningCardToUpdate = this.planningCards.find(x => x.id === updatedData[0].planningCardId)
            planningCardToUpdate?.placeholderAllocations.push(updatedData[0]);
            planningCardToUpdate?.allocations.push(updatedData[0]);

          }
        }
        this.refreshListOfHighlightedResourcesInPlanningCards();
        this.sortPlanningCardsAllocations();
      }
    );
  }

  private onSuccessPlanningCardMerge(updatedData) {
    const projectToUpdate = updatedData[0]?.oldCaseCode
      ? this.projects.find(x => x.oldCaseCode === updatedData[0].oldCaseCode)
      : this.projects.find(x => x.pipelineId === updatedData[0].pipelineId);
    if (projectToUpdate) {
      projectToUpdate.placeholderAllocations = projectToUpdate.placeholderAllocations.concat(updatedData);
    }
    if (updatedData[0].oldCaseCode) {
      this.projects.map(obj => obj.oldCaseCode === projectToUpdate?.oldCaseCode || obj);
    } else {
      this.projects.map(obj => obj.pipelineId === projectToUpdate?.pipelineId || obj);
    }
  }

  isPlaceholderAllocationOnPlanningCard(event) {
    return (event.planningCardId && !(event.oldCaseCode || event.pipelineId));
  }

  removePlaceHolderEmitterHandler(event) {
    this.homeService.deletePlaceholdersByIds(event.placeholderIds).pipe(takeUntil(this.destroy$)).subscribe(
      (updatedData) => {
        if (event.notifyMessage) {
          this.notifyService.showSuccess(event.notifyMessage);
        }
        this.refreshListOfHighlightedResourcesInPlanningCards();
      }
    );
  }

  addPlanningCardEmitterHandler(event) {
    const planningCard: PlanningCard = {
      createdBy: this.coreService.loggedInUser.employeeCode,
      lastUpdatedBy: this.coreService.loggedInUser.employeeCode
    };
    this.homeService.insertPlanningCard(planningCard).pipe(takeUntil(this.destroy$)).subscribe(
      (insertedData) => {
        if (insertedData.createdBy && insertedData.id) {
          this.notifyService.showSuccess(`Planning Card has been created`);
          insertedData.placeholderAllocations = [];
          insertedData.regularAllocations = [];
          this.planningCards.push(insertedData);
          this.refreshListOfHighlightedResourcesInPlanningCards();
        }
      }
    );
  }

  removePlanningCardEmitterHandler(event) {
    let { userPreferences, sortOrderUpdated } = this.getUpdatedPlanningCardsSortOrderOnPlanningCardMerge(event.id);
    var notifyMessage = `Planning Card deleted successfully!!`;

    if (sortOrderUpdated) {
      forkJoin([this.coreService.updateUserPreferences(userPreferences), this.homeService.deletePlanningCardAndItsAllocations(event.id)])
        .subscribe(
          () => {
            this.updateDemandSideOnPlanningCardDelete(event, notifyMessage);
          }
        );
    } else {
      this.homeService.deletePlanningCardAndItsAllocations(event.id).pipe(takeUntil(this.destroy$)).subscribe(
        () => {
          this.updateDemandSideOnPlanningCardDelete(event, notifyMessage);
        });
    }
  }

  private updateDemandSideOnPlanningCardDelete(event, notifyMessage) {
    this.notifyService.showSuccess(notifyMessage);
    this.planningCards.splice(
      this.planningCards.indexOf(this.planningCards.find(item => item.id === event.id)), 1);
    this.refreshListOfHighlightedResourcesInPlanningCards();
  }

  private getUpdatedPlanningCardsSortOrderOnPlanningCardMerge(planningCardId) {
    let userPreferences = this.coreService.getUserPreferencesValue();
    if (userPreferences.planningCardsSortOrder.length < 1) return { userPreferences, sortOrderUpdated: false };
    const planningCardsSortOrder = userPreferences.planningCardsSortOrder.split(',');
    planningCardsSortOrder.splice(planningCardsSortOrder.indexOf(planningCardId), 1);
    userPreferences.planningCardsSortOrder = planningCardsSortOrder.toString();
    return { userPreferences, sortOrderUpdated: true };
  }

  getPlanningCardsBySelectedValues(demandFilterCriteriaObj) {

    if (!demandFilterCriteriaObj.demandTypes?.includes('PlanningCards')) {
      this.planningCards = [];
      return;
    }

    this.homeService.getPlanningCardsBySelectedValues(demandFilterCriteriaObj).pipe(takeUntil(this.destroy$)).subscribe(
      (data) => {
        if (data) {

          data.forEach(planningCard => {
            planningCard.placeholderAllocations = planningCard.allocations.filter(x => x.isPlaceholderAllocation);
            planningCard.regularAllocations = planningCard.allocations.filter(x => !x.isPlaceholderAllocation);
          });

          // sort planning cards as per sort order
          this.planningCards = demandFilterCriteriaObj.planningCardsSortOrder.length > 0 ? this.sortDemandSideCardsByUserPreference(data, demandFilterCriteriaObj.planningCardsSortOrder) : data;
          this.refreshListOfHighlightedResourcesInPlanningCards();
        }
      }
    );
  }

  private sortDemandSideCardsByUserPreference(cards: any, sortOrder: string) {
    let sortedList = [];
    sortOrder.split(',').forEach(id => {
      let card = cards.find(x => x.oldCaseCode === id || x.pipelineId === id || x.id === id);
      if (card) {
        sortedList.push(card);
        cards.splice(cards.indexOf(card), 1);
      }
    });
    return sortedList.concat(cards);
  }

  updatePlanningCardEmitterHandler(event) {
    this.planningCardService.updatePlanningCard(event).pipe(takeUntil(this.destroy$)).subscribe(
      (updatedData) => {
        if (updatedData && updatedData.lastUpdatedBy) {
          this.notifyService.showSuccess(`Planning Card has been updated`);
          this.refreshListOfHighlightedResourcesInPlanningCards();
        }
      }
    );
}


  mergePlanningCardAndAllocationsHandler(payload){
    this.planningCardService.mergePlanningCardAndAllocations(payload.planningCard, payload.resourceAllocations, payload.placeholderAllocations)
        .pipe(takeUntil(this.destroy$))
        .subscribe(()=>{
          //TODO: trigger refresh directly from planning card service
          var notifyMessage = `Planning Card merged successfully!!`;

          if(payload.allocationDataBeforeSplitting) {
            if (payload.allocationDataBeforeSplitting.every((r)=> r.oldCaseCode))
            {
                this.sharedService.checkPegRingfenceAllocationAndInsertDownDayCommitments(payload.allocationDataBeforeSplitting).subscribe(commitments =>
                {
                    if(commitments?.length > 0)
                    {
                        this.notifyService.showSuccess(ConstantsMaster.Messages.DownDaySaved);
                    }
                });
            }
        }
          this.resourceAssignmentService.upsertPlaygroundAllocationsForCasePlanningMetrics(payload.resourceAllocations);
          this.updateDemandSideOnPlanningCardDelete(payload.planningCard, notifyMessage);
          this.refreshProjects();
        });
  }

  sharePlanningCardEmitterHandler(event) {
    const planningCard = event.planningCard;
    this.homeService.sharePlanningCard(planningCard).pipe(takeUntil(this.destroy$)).subscribe(
      (updatedData) => {
        if (updatedData && updatedData.lastUpdatedBy) {
          this.notifyService.showSuccess(`Planning Card has been shared`);
          this.updateHomeScreenForSharedPlanningCard(updatedData);
        }
      }
    );
  }

  updateHomeScreenForSharedPlanningCard(updatedData: PlanningCard) {
    const sharedOfficeCodes = updatedData.sharedOfficeCodes.split(',');
    const sharedStaffingTags = updatedData.sharedStaffingTags.split(',');
    let isSharedOfficeSelected = false;
    let isSharedStaffingTagSelected = false;

    const planningCardToUpdateOrDelete = this.planningCards.find(x => x.id === updatedData.id);
    if (!!planningCardToUpdateOrDelete) {
      planningCardToUpdateOrDelete.isShared = updatedData.isShared;
      planningCardToUpdateOrDelete.sharedOfficeCodes = updatedData.sharedOfficeCodes;
      planningCardToUpdateOrDelete.sharedStaffingTags = updatedData.sharedStaffingTags;
      planningCardToUpdateOrDelete.includeInCapacityReporting = updatedData.includeInCapacityReporting;

      sharedOfficeCodes.forEach(officeCode => {
        if (this.demandFilterCriteriaObj.officeCodes.includes(officeCode)) {
          isSharedOfficeSelected = true;
        }
      });

      sharedStaffingTags.forEach(staffingTag => {
        if ((!this.demandFilterCriteriaObj.caseAttributeNames
          && staffingTag === ConstantsMaster.ServiceLine.GeneralConsulting)
          || this.demandFilterCriteriaObj.caseAttributeNames.includes(staffingTag)) {
          isSharedStaffingTagSelected = true;
        }
      });

      if (!isSharedOfficeSelected || !isSharedStaffingTagSelected) {
        this.planningCards.splice(
          this.planningCards.indexOf(this.planningCards.find(item => item.id === updatedData.id)), 1);
      }
      this.refreshListOfHighlightedResourcesInPlanningCards();
    }
  }

  refreshListOfHighlightedResourcesInPlanningCards() {
    if (this.planningCards.length > 0) {
      let employeeCodesInPlanningCards = [];
      this.planningCards.forEach(planningCard => {
        const uniqueECodesInPlanningCard = [...new Set(planningCard.allocations.map(allocation => allocation.employeeCode))];
        employeeCodesInPlanningCards = employeeCodesInPlanningCards.concat(uniqueECodesInPlanningCard);
      });
      this.highlightedResourcesInPlanningCards = CommonService.findDuplicatesInArray(employeeCodesInPlanningCards).filter(x => x);
    }
  }

  openPegRFPopUpHandler(pegOpportunityId){
      this.pegOpportunityDialogService.openPegOpportunityDetailPopUp(pegOpportunityId)
  }

  // ---------------------------Component Unload--------------------------------------------//

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    // this.destroy$.unsubscribe();
    this.routerSub.unsubscribe();
    this.coreServiceSub.unsubscribe();
    this.subscription.unsubscribe();
    this.currRoute = '';
    this.storeSub.unsubscribe();
  }

}
