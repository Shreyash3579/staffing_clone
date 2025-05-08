// ----------------------- Angular Package References ----------------------------------//
import { Component, OnInit,OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

// ----------------------- Component/Service References ----------------------------------//
import { environment } from '../../../environments/environment';
import { LocalStorageService } from '../../shared/local-storage.service';

// --------------------------3rd Party libraries -----------------------------------------//
import { TableauService } from '../common/tableau.service';

@Component({
  selector: 'app-case-economics',
  templateUrl: './case-economics.component.html'
})
export class CaseEconomicsComponent implements OnInit,OnDestroy {
  url = `${environment.settings.tableauCaseEconomicsReportUrl}`;
  tableauServiceInstance: any;
  placeholderDiv: any;
  options: any;
  //------------------Share Variables -----------------------------------------------//
  landingUrl:any;
  envPageName:string ='';

  constructor(private localStorageService: LocalStorageService,
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
    this.loadTableauDashboard();
  }
  ngOnDestroy() {
  }

  // ------------------------ Helper Functions------------------------------------------//
  public onResize(event) {
    this.tableauServiceInstance.onResize(this.placeholderDiv);
  }

  private loadTableauDashboard() {
    this.placeholderDiv = document.getElementById('vizContainerCE');
    if (!this.tableauServiceInstance) {
      this.tableauServiceInstance = new TableauService(this.url, this.localStorageService, `${environment.settings.environmentUrl}`);
    }

    this.options = this.tableauServiceInstance.getTableauDashboardDefaultOptions();
    this.tableauServiceInstance.loadTableauDashboard(this.placeholderDiv, this.options);
  }

}

