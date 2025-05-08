// ----------------------- Angular Package References ----------------------------------//
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs/internal/Subject';
import { ActivatedRoute } from '@angular/router';

// ----------------------- Component/Service References ----------------------------------//
import { environment } from '../../../environments/environment';
import { CoreService } from '../../core/core.service';
import { LocalStorageService } from '../../shared/local-storage.service';

// --------------------------Interfaces -----------------------------------------//
import { UserPreferences } from '../../shared/interfaces/userPreferences.interface';
import { UserPreferenceSupplyGroup } from 'src/app/shared/interfaces/userPreferenceSupplyGroup';

// --------------------------3rd Party libraries -----------------------------------------//
import { TableauService } from '../common/tableau.service';

@Component({
  selector: 'app-smap-allocations',
  templateUrl: './smap-allocations.component.html'
})
export class SmapAllocationsComponent implements OnInit, OnDestroy {


  // -----------------------Local Variables--------------------------------------------//
  coreServiceSub: any;
  userPreferences: UserPreferences;
  userPreferenceSupplyGroups: UserPreferenceSupplyGroup;
  destroy$: Subject<boolean> = new Subject<boolean>();
  url = `${environment.settings.tableauWhoSMAPAllocationsReportUrl}`;
  tableauServiceInstance: any;
  placeholderDiv: any;
  options: any;

  tableauUrl:any;
  envPageName:string ='';


  // -------------------Constructor----------------------//
  constructor(
    private coreService: CoreService,
    private localStorageService: LocalStorageService,
    private activeroute: ActivatedRoute
  ) { }

  // -------------------Component LifeCycle Events and Functions----------------------//
  ngOnInit() {
   this.envPageName = window.location.href.split("?")[0];
   this.tableauUrl = this.activeroute.snapshot.queryParamMap.get('tabUrl');
   if(this.tableauUrl){
     this.tableauUrl = this.tableauUrl + `${environment.settings.externalNavParameters}`
     this.url = this.tableauUrl;
   }
   this.coreServiceSub = this.coreService.getCombinedUserPreferences().subscribe(combinedUserPreference => {
    this.userPreferences = combinedUserPreference.userPreferences;
    this.userPreferenceSupplyGroups = combinedUserPreference.userPreferenceSupplyGroups;
    this.loadTableauDashboard();
  });
  }

  ngOnDestroy() {
    this.destroy$.next(true);
    this.destroy$.unsubscribe();
    this.coreServiceSub.unsubscribe();
  }
  // ------------------------ Helper Functions------------------------------------------//
  public onResize(event) {
    this.tableauServiceInstance.onResize(this.placeholderDiv);
  }

  private loadTableauDashboard() {
    this.placeholderDiv = document.getElementById('vizContainerSMAP');
    if (!this.tableauServiceInstance) {
      this.tableauServiceInstance = new TableauService(this.url, this.localStorageService, `${environment.settings.environmentUrl}`);
    }
    const filterOptions = this.tableauServiceInstance.getSelectedFilters(this.userPreferences, this.userPreferenceSupplyGroups);
    this.setTableauFilters(filterOptions);
    this.tableauServiceInstance.loadTableauDashboard(this.placeholderDiv, this.options);
  }

  private setTableauFilters(filterOptions) {
    this.options = filterOptions.options;
    this.options['Operating Office Name'] = filterOptions.selectedOfficeNames ?? '';
    this.options['Service Line'] = filterOptions.selectedStaffingTags ?? '';
    this.options['Level Grade'] = filterOptions.selectLevelGrades ?? '';
    this.options['Employee Name + Ecode'] = filterOptions.selectedSupplyGroupEmployees ?? '';
    this.options['URL_Passthrough'] = filterOptions.urlPassThrough;
  }

}