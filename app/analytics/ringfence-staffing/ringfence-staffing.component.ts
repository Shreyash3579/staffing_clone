import { Component, OnInit, OnDestroy } from '@angular/core';
import { UserPreferences } from 'src/app/shared/interfaces/userPreferences.interface';
import { Subject } from 'rxjs';
import { ActivatedRoute } from '@angular/router';
import { ServiceLine } from 'src/app/shared/constants/enumMaster';
import { environment } from 'src/environments/environment';
import { CoreService } from 'src/app/core/core.service';
import { LocalStorageService } from 'src/app/shared/local-storage.service';

// --------------------------3rd Party libraries -----------------------------------------//
import { TableauService } from '../common/tableau.service';

@Component({
  selector: 'app-ringfence-staffing',
  templateUrl: './ringfence-staffing.component.html',
  styleUrls: ['./ringfence-staffing.component.scss']
})
export class RingfenceStaffingComponent implements OnInit, OnDestroy {

  // -----------------------Local Variables--------------------------------------------//
  coreServiceSub: any;
  userPreferences: UserPreferences;
  destroy$: Subject<boolean> = new Subject<boolean>();
  serviceLineEnum: typeof ServiceLine = ServiceLine;
  url = `${environment.settings.tableauRingfenceStaffingReportUrl}`;
  tableauServiceInstance: any;
  placeholderDiv: any;
  options: any;
  //------------------Share Variables -----------------------------------------------//
  landingUrl:any;
  envPageName:string ='';

  // -------------------Constructor----------------------//
  constructor(
    private coreService: CoreService,
    private localStorageService: LocalStorageService,
    private activeroute: ActivatedRoute,
  ) { }

  // -------------------Component LifeCycle Events and Functions----------------------//
  ngOnInit() {
    //External Landing Page Container based on Paremterised URL
    this.envPageName = window.location.href.split("?")[0];
    this.landingUrl = this.activeroute.snapshot.queryParamMap.get('tabUrl');
    if(this.landingUrl){
      this.landingUrl = this.landingUrl + `${environment.settings.externalNavParameters}`
      this.url = this.landingUrl;
    }
    this.coreServiceSub = this.coreService.getUserPreferences().subscribe(userPreferences => {
      this.userPreferences = userPreferences;
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
    this.placeholderDiv = document.getElementById('vizContainerRS');
    if (!this.tableauServiceInstance) {
      this.tableauServiceInstance = new TableauService(this.url, this.localStorageService, `${environment.settings.environmentUrl}`);
    }
    const filterOptions = this.tableauServiceInstance.getAppSelectedFilters(this.userPreferences);
    this.setTableauFilters(filterOptions);
    this.tableauServiceInstance.loadTableauDashboard(this.placeholderDiv, this.options);
  }

  private setTableauFilters(filterOptions) {
    this.options = filterOptions.options;
    this.options['Operating Office'] = filterOptions.selectedOfficeNames;
    // this.options['Operating Office Cluster'] = filterOptions.selectedOfficeClusters;
    // this.options['Operating Office Region'] = filterOptions.selectedOfficeRegions;
    this.options['Level Grade'] = filterOptions.selectLevelGrades;
    this.options['Staffing Tag'] = filterOptions.selectedStaffingTags;
    this.options['URL_Passthrough'] = filterOptions.urlPassThrough;
  }
}
