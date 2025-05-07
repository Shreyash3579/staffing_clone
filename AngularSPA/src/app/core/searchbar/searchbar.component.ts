import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { Subject, Subscription, forkJoin } from 'rxjs';
import { debounceTime, filter } from 'rxjs/operators';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { ResourceService } from 'src/app/shared/helperServices/resource.service';
import { CommitmentType } from 'src/app/shared/interfaces/commitmentType.interface';
import { CoveoAnalyticsClickParams } from 'src/app/shared/interfaces/coveo-analytics-click-params.interface';
import { Office } from 'src/app/shared/interfaces/office.interface';
import { SupplyFilterCriteria } from 'src/app/shared/interfaces/supplyFilterCriteria.interface';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { CoreService } from '../core.service';
import { SeachbarService } from './searchbar.service';
import { UniversalSearchOptions } from 'src/app/shared/constants/enumMaster';
import { SharedService } from 'src/app/shared/shared.service';
import { ResourcesCommitmentsDialogService } from 'src/app/overlay/dialogHelperService/resourcesCommitmentsDialog.service';
import { CommonService } from 'src/app/shared/commonService';
@Component({
  selector: 'app-searchbar',
  templateUrl: './searchbar.component.html',
  styleUrls: ['./searchbar.component.scss']
})
export class SearchbarComponent implements OnInit {
  searchString = "";
  searchInResources = true;
  searchInCases = false;
  changeInSearchQuery$: Subject<void> = new Subject();
  resourceLoader = '';
  activeCaseLoader = '';
  inActiveCaseLoader = '';
  activePlanningCardLoader = '';
  inActivePlanningCardLoader = '';
  commitmentTypes: CommitmentType[];
  supplyFilterCriteriaObj: SupplyFilterCriteria = {} as SupplyFilterCriteria;
  officeFlatList: Office[];
  isMinimized:boolean = true;
  activeTab: string;
  private resourceDeselectionSubscription: Subscription;
  public resourcesList = []
  public filteredResourcesList = []
  public activeCaseList = []
  public inActiveCaseList = []
  selectedResourcesList =[];
  public activePlanningCardList = []
  public inActivePlanningCardList = []

  constructor(
    private localStorageService: LocalStorageService,
    private coreService: CoreService,
    private seachbarService: SeachbarService,
    private sharedService: SharedService,
    private router: Router,
    private route: ActivatedRoute,
    private resourcesCommitmentsDialogService: ResourcesCommitmentsDialogService) { }

  ngOnInit(): void {
    this.subscribeSearchQueryChanges();
   this.getActiveTab();
    this.deselectResourceOnDeleteFromResourceViewPopup();
  }
  ngOnDestroy() {
    this.resourceDeselectionSubscription.unsubscribe();
  }


  getActiveTab(): void {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        const url = this.router.url.split('/')[1]; 
        this.activeTab = url;
        this.isMinimized = this.activeTab === 'home' ? true : false;
      });
  }

  clearSelectedResources() {
    this.selectedResourcesList = [];
    this.filteredResourcesList = this.resourcesList;
  }

  deselectResourceOnDeleteFromResourceViewPopup() {
    this.resourceDeselectionSubscription = this.resourcesCommitmentsDialogService.resourceToBeDeselected.subscribe(
      (employeeCode) => {
        this.selectedResourcesList = this.selectedResourcesList.filter(x => x.employeeCode !== employeeCode);
        const selectedEmployeeCodes = this.selectedResourcesList.map(resource => resource.employeeCode).join(',');
        this.filteredResourcesList = this.resourcesList.filter(x => !selectedEmployeeCodes.includes(x.employeeCode));
        });
  }
  

  subscribeSearchQueryChanges() {
    this.changeInSearchQuery$.pipe(debounceTime(500) ).subscribe(() => {
      if(this.searchString.length < 3)
        return;
      this.commitmentTypes = this.localStorageService.get(ConstantsMaster.localStorageKeys.commitmentTypes);
      const userPreferences = this.coreService.getUserPreferencesValue();
      const addTransfers = true;

      if (this.searchInResources && this.searchInCases ) {
        this.resourceLoader = 'Loading...';
        this.activeCaseLoader = 'Loading...';
        this.inActiveCaseLoader = 'Loading...';
        const resourceDetails$ = this.sharedService.getResourcesIncludingTerminatedBySearchString(this.searchString,addTransfers);
        const projectDetails$ = this.sharedService.getProjectsBySearchString(this.searchString);
        const planningCardDetails$ = this.sharedService.getPlanningCardsBySearchString(this.searchString);
    
        forkJoin([resourceDetails$, projectDetails$,planningCardDetails$]).subscribe(([resources, projects,planningCards]) => {
        
          this.setOfficeDetailsFromLocalStore(resources);
          const availableResources = ResourceService.createResourcesDataForStaffing(resources, null, null,
          this.supplyFilterCriteriaObj, this.commitmentTypes, userPreferences, true);
          this.resourcesList = availableResources;
          const selectedEmployeeCodes = this.selectedResourcesList.map(resource => resource.employeeCode).join(',');
          this.filteredResourcesList = this.resourcesList.filter(x => !selectedEmployeeCodes.includes(x.employeeCode));
          this.resourceLoader = this.filteredResourcesList && this.filteredResourcesList.length > 0 ? '' : 'No data found';
          this.activeCaseList = projects?.filter(activeData => activeData.projectStatus == 'Active');
          this.activeCaseLoader = this.activeCaseList && this.activeCaseList.length > 0 ? '' : 'No data found';

          this.inActiveCaseList = projects?.filter(activeData => activeData.projectStatus == 'Inactive');
          this.inActiveCaseLoader = this.inActiveCaseList && this.inActiveCaseList.length > 0 ? '' : 'No data found';

          this.activePlanningCardList = planningCards.filter(activeData => activeData.status == 'Active');
          this.activePlanningCardLoader = this.activePlanningCardList && this.activePlanningCardList.length > 0 ? '' : 'No data found';

          this.inActivePlanningCardList = planningCards.filter(activeData => activeData.status == 'Inactive');
          this.inActivePlanningCardLoader = this.inActivePlanningCardList && this.inActivePlanningCardList.length > 0 ? '' : 'No data found';
        });
      }
       else if (this.searchInResources) {
        this.resourceLoader = 'Loading...';
        this.sharedService.getResourcesIncludingTerminatedBySearchString(this.searchString,addTransfers).subscribe(searchData => {
          this.setOfficeDetailsFromLocalStore(searchData);
          const availableResources = ResourceService.createResourcesDataForStaffing(searchData, null, null,
            this.supplyFilterCriteriaObj, this.commitmentTypes, userPreferences, true);
            this.resourcesList = availableResources;
            const selectedEmployeeCodes = this.selectedResourcesList.map(resource => resource.employeeCode).join(',');
            this.filteredResourcesList = this.resourcesList.filter(x => !selectedEmployeeCodes.includes(x.employeeCode));
          this.resourceLoader = this.filteredResourcesList && this.filteredResourcesList.length > 0 ? '' : 'No data found';
        })
      } else if (this.searchInCases) {
        this.activeCaseLoader = 'Loading...';
        this.inActiveCaseLoader = 'Loading...';
        this.activePlanningCardLoader ='Loading...';
        this.inActivePlanningCardLoader ='Loading...';
        forkJoin([
          this.sharedService.getProjectsBySearchString(this.searchString),
          this.sharedService.getPlanningCardsBySearchString(this.searchString)
        ]).subscribe(([projects, planningCards]) => {
          this.activeCaseList = projects.filter(activeData => activeData.projectStatus == 'Active');
          this.activeCaseLoader = this.activeCaseList && this.activeCaseList.length > 0 ? '' : 'No data found';

          this.inActiveCaseList = projects.filter(activeData => activeData.projectStatus == 'Inactive');
          this.inActiveCaseLoader = this.inActiveCaseList && this.inActiveCaseList.length > 0 ? '' : 'No data found';

          this.activePlanningCardList = planningCards.filter(activeData => activeData.status == 'Active');
          this.activePlanningCardLoader = this.activePlanningCardList && this.activePlanningCardList.length > 0 ? '' : 'No data found';

          this.inActivePlanningCardList = planningCards.filter(activeData => activeData.status == 'Inactive');
          this.inActivePlanningCardLoader = this.inActivePlanningCardList && this.inActivePlanningCardList.length > 0 ? '' : 'No data found';
        });

      }
    });
  }

  changesInQuery() {
    if (this.searchString && this.searchString.length > 2) {
      this.resourcesList = this.filteredResourcesList =this.activeCaseList = this.inActiveCaseList = this.activePlanningCardList =this.inActivePlanningCardList = [];
      this.changeInSearchQuery$.next();
    }
  }

  toggleSearch(searchInSource) {
    switch (searchInSource) {
      case 'resources':
        {
          this.searchInResources = true;
          this.searchInCases = false;
          break;
        }
      case 'cases':
        {
          this.searchInResources = false;
          this.searchInCases = true;
          break;
        }
      default: {
        this.searchInResources = true;
        this.searchInCases = true;
        break;
      }
    }
    this.changeInSearchQuery$.next();
  }

  openProjectDetailsDialogHandler(projectData) {
    // this.overlayDialogService.openProjectDetailsDialogHandler(projectData);
    const analyticsClickLog: CoveoAnalyticsClickParams = this.setParamsForCoveoAnalyticsClickEvent(projectData);
    this.seachbarService.analyticsClickLog(analyticsClickLog).subscribe(() => { });
    this.searchString = '';
    this.router.navigate(['/overlay'], { queryParams: this.getQueryParamValue(null, projectData) })
    this.closeDialog();
  }

  openResourceDetailsDialogHandler(resource) {
    //this.overlayDialogService.openResourceDetailsDialogHandler(employeeCode);
    const analyticsClickLog: CoveoAnalyticsClickParams = this.setParamsForCoveoAnalyticsClickEvent(resource);
    this.seachbarService.analyticsClickLog(analyticsClickLog).subscribe(() => { });

    this.searchString = '';
    this.router.navigate(['/overlay'], { queryParams: this.getQueryParamValue(resource) })
    this.closeDialog();
  }

  getQueryParamValue(resource = null, projectData = null) {
    if (resource) {
      return { employee: resource.employeeCode };
    }
    else if (projectData) {
      if (projectData.pipelineId) {
        return { pipelineId: projectData.pipelineId }
      }
      else if(projectData.oldCaseCode) {
        return { oldCaseCode: projectData.oldCaseCode };
      }
      else{
        return { planningCardId: projectData.id }
      }
    }
  }

  private setOfficeDetailsFromLocalStore(searchData) {
    if (!searchData) return;

    this.officeFlatList = this.localStorageService.get(ConstantsMaster.localStorageKeys.OfficeList);

    searchData.resources?.forEach(resource => {
      resource.office = this.setOfficeDetailsByOfficeCode(resource.office);
      resource.schedulingOffice = this.setOfficeDetailsByOfficeCode(resource.schedulingOffice);
    });
    searchData.terminations?.forEach(termination => {
      termination.operatingOffice = this.setOfficeDetailsByOfficeCode(termination.operatingOffice);
      return termination;
    });
    searchData.transfers?.forEach(transfer => {
      transfer.operatingOffice = this.setOfficeDetailsByOfficeCode(transfer.operatingOffice);
      transfer.operatingOfficeCurrent = this.setOfficeDetailsByOfficeCode(transfer.operatingOfficeCurrent);
      transfer.operatingOfficeProposed = this.setOfficeDetailsByOfficeCode(transfer.operatingOfficeProposed);
      return transfer;
    });
    searchData.promotions?.forEach(promotion => {
      promotion.operatingOffice = this.setOfficeDetailsByOfficeCode(promotion.operatingOffice);
      return promotion;
    });
    searchData.transitions?.forEach(transition => {
      transition.operatingOffice = this.setOfficeDetailsByOfficeCode(transition.operatingOffice);
      return transition;
    });
  }

  private setOfficeDetailsByOfficeCode(office: Office) {
    if (!office.officeCode || !office.officeAbbreviation || !office.officeName) {
      if (office.officeCode) {
        const officeData = this.officeFlatList.find(x => x.officeCode === office.officeCode)
        office.officeAbbreviation = officeData.officeAbbreviation;
        office.officeName = officeData.officeName;
      }
      else if (office.officeAbbreviation) {
        const officeData = this.officeFlatList.find(x => x.officeAbbreviation === office.officeAbbreviation)
        office.officeCode = officeData.officeCode;
        office.officeName = officeData.officeName;
      }
      else if (office.officeName) {
        const officeData = this.officeFlatList.find(x => x.officeName === office.officeName)
        office.officeCode = officeData.officeCode;
        office.officeAbbreviation = officeData.officeAbbreviation;
      }
    }
    return office;
  }

  private setParamsForCoveoAnalyticsClickEvent(obj) {
    const analyticsClickLog: CoveoAnalyticsClickParams = {
      documentUri: obj.uri,
      documentUriHash: obj.uriHash,
      searchQueryUid: obj.searchUid,
      sourceName: obj.source,
      collectionName: obj.sysCollection,
      anonymous: false,
      userName: this.coreService.loggedInUser.internetAddress,
      userDisplayName: this.coreService.loggedInUser.fullName,
      documentTitle: obj.title,
      originLevel2: this.searchInResources && !this.searchInCases
        ? UniversalSearchOptions.RESOURCE
        : !this.searchInResources && this.searchInCases
          ? UniversalSearchOptions.PROJECT
          : UniversalSearchOptions.EVERYTHING
    }

    return analyticsClickLog;
  }

  viewResourceData() {
    let employees = [];
    this.selectedResourcesList.forEach((resource) => {
        employees.push({
          employeeCode: resource.employeeCode,
          employeeName: resource.fullName,
          levelGrade: resource.levelGrade,
          dateFirstAvailable: resource.dateFirstAvailable,
          percentAvailable: resource.percentAvailable,
          internetAddress: resource.internetAddress,
          isNotAvailable:
            resource.isTerminated ||
            resource.onTransitionOrTerminationAndNotAvailable ||
            !resource.dateFirstAvailable ||
            !resource.percentAvailable
        });
    });
    this.resourcesCommitmentsDialogService.showResourcesCommitmentsDialogHandler(employees);
  }

  emailResources()
  {
    let employees = [];
    this.selectedResourcesList.forEach((resource) => {
        employees.push({
          internetAddress: resource.internetAddress
        });
    });
    CommonService.sendEmailToSelectedResources(employees);
  }

  closeDialog() {
    this.searchString = '';
  }
  
  toggleMinimize() {
    this.isMinimized = !this.isMinimized;
  }

  addResourceToSelectedListEmitterHanler(selectedResource){

    this.selectedResourcesList.push(selectedResource);
    const selectedEmployeeCodes = this.selectedResourcesList.map(resource => resource.employeeCode).join(',');
    this.filteredResourcesList = this.resourcesList.filter(x => !selectedEmployeeCodes.includes(x.employeeCode));

  }
  removeResourceFromSelectedListEmitterHandler(selectedResourceEmployeeCode){
    this.selectedResourcesList = this.selectedResourcesList.filter(x => x.employeeCode!== selectedResourceEmployeeCode);
    const selectedEmployeeCodes = this.selectedResourcesList.map(resource => resource.employeeCode).join(',');
    this.filteredResourcesList = this.resourcesList.filter(x => !selectedEmployeeCodes.includes(x.employeeCode));
  }
}
