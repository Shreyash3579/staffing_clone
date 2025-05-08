// ----------------------- Angular Package References ----------------------------------//
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

// ----------------------- Component/Service References ----------------------------------//
import { environment } from '../../../environments/environment';
import { CoreService } from '../../core/core.service';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { UserPreferences } from '../../shared/interfaces/userPreferences.interface';
import { UserPreferenceSupplyGroup } from 'src/app/shared/interfaces/userPreferenceSupplyGroup';

// --------------------------3rd Party libraries -----------------------------------------//
import { TableauService } from '../common/tableau.service';

@Component({
  selector: "app-daily-deployment-details",
  templateUrl: "./daily-deployment-details.component.html"
})
export class DailyDeploymentDetailsComponent implements OnInit {

  coreServiceSub: any;
  userPreferences: UserPreferences;
  userPreferenceSupplyGroups: UserPreferenceSupplyGroup;
  url = `${environment.settings.tableauDailyDeploymentDetailsReportUrl}`;
  tableauServiceInstance: any;
  placeholderDiv: any;
  options: any;
  //------------------Share Variables -----------------------------------------------//
  landingUrl:any;
  envPageName:string ='';

  constructor(
    private coreService: CoreService,
    private localStorageService: LocalStorageService,
    private activeroute: ActivatedRoute
  ) { }

  ngOnInit(): void {
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
    this.coreServiceSub.unsubscribe();
  }

  // ------------------------ Helper Functions------------------------------------------//
  public onResize(event) {
    this.tableauServiceInstance.onResize(this.placeholderDiv);
  }

  private loadTableauDashboard() {
    this.placeholderDiv = document.getElementById('vizContainerDDD');
    if (!this.tableauServiceInstance) {
      this.tableauServiceInstance = new TableauService(this.url, this.localStorageService, `${environment.settings.environmentUrl}`);
    }
    const filterOptions = this.tableauServiceInstance.getSelectedFilters(this.userPreferences, this.userPreferenceSupplyGroups);
    this.setTableauFilters(filterOptions);
    this.tableauServiceInstance.loadTableauDashboard(this.placeholderDiv, this.options);
  }

  private setTableauFilters(filterOptions) {
    this.options = filterOptions.options;
    this.options['Employee Name + Ecode'] = filterOptions.selectedSupplyGroupEmployees ?? '';
    this.options['Operating Office Name'] = filterOptions.selectedOfficeNames ?? '';
    this.options['Level Grade'] = filterOptions.selectLevelGrades ?? '';
    this.options['Service Line Name'] = filterOptions.selectedStaffingTags?.split(',').filter(x => !x.toLowerCase().includes("peg")).join(',') ?? '';
    this.options['URL_Passthrough'] = filterOptions.urlPassThrough;
  }

}
