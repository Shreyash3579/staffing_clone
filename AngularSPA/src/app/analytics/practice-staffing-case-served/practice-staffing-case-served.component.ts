import { Component, OnInit, OnDestroy } from "@angular/core";
import { ActivatedRoute } from '@angular/router';
import { Subject } from "rxjs";
import { CoreService } from "src/app/core/core.service";
import { UserPreferences } from "src/app/shared/interfaces/userPreferences.interface";
import { LocalStorageService } from "src/app/shared/local-storage.service";
import { environment } from "src/environments/environment";
import { TableauService } from "../common/tableau.service";

@Component({
  selector: 'app-practice-staffing-case-served',
  templateUrl: './practice-staffing-case-served.component.html',
  styleUrls: ['./practice-staffing-case-served.component.scss']
})
export class PracticeStaffingCaseServedComponent implements OnInit, OnDestroy {
  // -----------------------Local Variables--------------------------------------------//
  coreServiceSub: any;
  userPreferences: UserPreferences;
  destroy$: Subject<boolean> = new Subject<boolean>();
  url = `${environment.settings.tableauPracticeStaffingCaseServedReportUrl}`;
  tableauServiceInstance: any;
  placeholderDiv: any;
  options: any;
  //------------------Share Variables -----------------------------------------------//
  landingUrl: any;
  envPageName: string = '';

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
    if (this.landingUrl) {
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
    this.placeholderDiv = document.getElementById('vizContainerPSCS');
    if (!this.tableauServiceInstance) {
      this.tableauServiceInstance = new TableauService(this.url, this.localStorageService, `${environment.settings.environmentUrl}`);
    }
    const filterOptions = this.tableauServiceInstance.getAppSelectedFilters(this.userPreferences);
    this.setTableauFilters(filterOptions);
    this.tableauServiceInstance.loadTableauDashboard(this.placeholderDiv, this.options);
  }

  private setTableauFilters(filterOptions) {
    this.options = filterOptions.options;
    this.options['Staffing Tag'] = filterOptions.selectedStaffingTags;
    this.options['URL_Passthrough'] = filterOptions.urlPassThrough;
    this.options['Case Office Name'] = filterOptions.selectedOfficeNames;

  }
}