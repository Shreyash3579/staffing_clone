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
import * as moment from 'moment';
import { TableauService } from '../common/tableau.service';

@Component({
  selector: 'app-staffing-report',
  templateUrl: './staffing-report.component.html',
  styleUrls: ['./staffing-report.component.scss']
})
export class StaffingReportComponent implements OnInit, OnDestroy {

  // -----------------------Local Variables--------------------------------------------//
  coreServiceSub: any;
  userPreferences: UserPreferences;
  userPreferenceSupplyGroups: UserPreferenceSupplyGroup;
  destroy$: Subject<boolean> = new Subject<boolean>();
  url = `${environment.settings.tableauStaffingAllocationReportUrl}`;
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
    this.placeholderDiv = document.getElementById('vizContainerSR');
    if (!this.tableauServiceInstance) {
      this.tableauServiceInstance = new TableauService(this.url, this.localStorageService, `${environment.settings.environmentUrl}`);
    }
    const filterOptions = this.tableauServiceInstance.getSelectedFilters(this.userPreferences, this.userPreferenceSupplyGroups);
    this.setTableauFilters(filterOptions);
    this.tableauServiceInstance.loadTableauDashboard(this.placeholderDiv, this.options);
  }

  private setTableauFilters(filterOptions) {
    this.options = filterOptions.options;
    this.options['Operating Office'] = filterOptions.selectedOfficeAbbreviations ?? ''; //need abbrevaition in this report and not name
    // this.options['Operating Office Cluster'] = filterOptions.selectedOfficeClusters;
    // this.options['Operating Office Region'] = filterOptions.selectedOfficeRegions;
    this.options['Staffing Tag'] = filterOptions.selectedStaffingTags ?? '';
    this.options['Level Grade'] = filterOptions.selectLevelGrade ?? '';;
    this.options['Range Start Date'] = moment(new Date()).startOf('day').format('YYYY-MM-DD') ?? ''; 
    this.options['Employee Name + Ecode'] = filterOptions.selectedSupplyGroupEmployees ?? '';
    this.options['Range End Date'] = moment(new Date()).startOf('day').add(filterOptions.selectedDateRangeWeeks * 7, 'day')
      .format('YYYY-MM-DD') ?? '';
  }
}
