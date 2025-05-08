// ----------------------- Angular Package References ----------------------------------//
import { Component, OnInit, OnDestroy} from '@angular/core';
import { Subject } from 'rxjs/internal/Subject';
import { ActivatedRoute } from '@angular/router';

// ----------------------- Component/Service References ----------------------------------//
import { environment } from '../../../environments/environment';
import { CoreService } from '../../core/core.service';
import { LocalStorageService } from '../../shared/local-storage.service';

// --------------------------Interfaces -----------------------------------------//
import { UserPreferences } from '../../shared/interfaces/userPreferences.interface';

// --------------------------3rd Party libraries -----------------------------------------//
import { TableauService } from '../common/tableau.service';

@Component({
  selector: 'app-monthly-deployment',
  templateUrl: './monthly-deployment.component.html'
})
export class MonthlyDeploymentComponent implements OnInit, OnDestroy {

  // -----------------------Local Variables--------------------------------------------//
  coreServiceSub: any;
  userPreferences: UserPreferences;
  destroy$: Subject<boolean> = new Subject<boolean>();
  url = `${environment.settings.tableauMonthlyDeploymentReportUrl}`;
 //url = 'https://tableauqa.bain.com/t/Staffing/views/MonthlyDeployment/MonthlyDeploymentSummaryAcrossTime/54474@bain.com/BossCustomViewFilters?:toolbar=top&:showShareOptions=false&:embed=y&:showVizHome=n&:apiID=host0#navType=0&navSrc=Parse&4';
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
    private activeroute: ActivatedRoute
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
    this.placeholderDiv = document.getElementById('vizContainerMD');
    if (!this.tableauServiceInstance) {
      this.tableauServiceInstance = new TableauService(this.url, this.localStorageService, `${environment.settings.environmentUrl}`);
    }
    const filterOptions = this.tableauServiceInstance.getAppSelectedFilters(this.userPreferences);
    this.setTableauFilters(filterOptions);
    this.tableauServiceInstance.loadTableauDashboard(this.placeholderDiv, this.options);
  }

  private setTableauFilters(filterOptions) {
    this.options = filterOptions.options;
    this.options['Operating Office Name'] = filterOptions.selectedOfficeNames;
    this.options['Staffing Tag'] = filterOptions.selectedStaffingTags;
    this.options['Level Grade'] = filterOptions.selectLevelGrades;
    this.options['URL_Passthrough'] = filterOptions.urlPassThrough;
  }

}
