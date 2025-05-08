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
declare var tableau: any;
declare var TabScale: any;

@Component({
  selector: 'app-test-responsive-report',
  templateUrl: './test-responsive-report.component.html',
  styleUrls: ['./test-responsive-report.component.scss']
})
export class TestResponsiveReportComponent implements OnInit, OnDestroy {

  // -----------------------Local Variables--------------------------------------------//
  coreServiceSub: any;
  userPreferences: UserPreferences;
  destroy$: Subject<boolean> = new Subject<boolean>();
  url = `${environment.settings.tableauMonthlyDeploymentReportUrl}`;
 //url = 'https://tableauqa.bain.com/t/Staffing/views/MonthlyDeployment/MonthlyDeploymentSummaryAcrossTime/54474@bain.com/BossCustomViewFilters?:toolbar=top&:showShareOptions=false&:embed=y&:showVizHome=n&:apiID=host0#navType=0&navSrc=Parse&4';
  tableauServiceInstance: any;
  placeholderDiv: any;
  options: any;
  viz: any;


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
    this.loadTableauDashboard();
  }

  ngOnDestroy() {
    
  }
  // ------------------------ Helper Functions------------------------------------------//
  public onResize(event) {
    this.tableauServiceInstance.onResize(this.placeholderDiv);
  }

  private loadTableauDashboard() {
    this.placeholderDiv = document.getElementById('vizContainerMD');
    // if (!this.tableauServiceInstance) {
    //   this.tableauServiceInstance = new TableauService(this.url, this.localStorageService, `${environment.settings.environmentUrl}`);
    // }
    // const filterOptions = this.tableauServiceInstance.getAppSelectedFilters(this.userPreferences);
    // this.setTableauFilters(filterOptions);
    // this.tableauServiceInstance.loadTableauDashboard(this.placeholderDiv, this.options);

      // Dispose tableau visualization if already exists to allow load new viz on change of user settings
      var vizUrl = 'https://public.tableau.com/views/SuperstoreDashboard_14/SSDashboard?:embed=y&:display_count=yes&:origin=viz_share_link';
      var viz;
      var tabScale = new TabScale.Scale(document.getElementById('vizContainerMD'));
      // scale the viz horizontally and from the bottom right
      tabScale.scaleDirection = TabScale.Options.ScaleDirection.Horizontal;
      tabScale.scaleFrom = TabScale.Options.ScaleFrom.BottomRight;
      var that = this
      this.options = {
        hideTabs: false,
        hideToolbar: false,
        showShareOptions: false,
        width: window.innerWidth - 50,
        height: window.innerHeight - 100,
        onFirstInteractive: function () {
          tabScale.initialize();
         }
      }
      this.viz = new tableau.Viz(tabScale, vizUrl, this.options);


    
  }

  private setTableauFilters(filterOptions) {
    this.options = filterOptions.options;
    this.options['Operating Office Name'] = filterOptions.selectedOfficeNames;
    this.options['Staffing Tag'] = filterOptions.selectedStaffingTags;
    this.options['Level Grade'] = filterOptions.selectLevelGrades;
    this.options['URL_Passthrough'] = filterOptions.urlPassThrough;
  }

}
