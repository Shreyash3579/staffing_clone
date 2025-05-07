// ----------------------- Angular Package References ----------------------------------//
import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { TableauService } from 'src/app/analytics/common/tableau.service';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { LocalStorageService } from 'src/app/shared/local-storage.service';

// ----------------------- Component/Service References ----------------------------------//
import { environment } from '../../../environments/environment';
// --------------------------3rd Party libraries -----------------------------------------//
declare var tableau: any;
@Component({
  selector: 'app-case-economics',
  templateUrl: './case-economics.component.html',
  styleUrls: ['./case-economics.component.scss']
})
export class CaseEconomicsComponent implements OnInit, OnChanges {
  billableReportUrl = `${environment.settings.tableauCaseEconomicsReportUrl}`;
  nonBillableReportUrl = `${environment.settings.tableauCaseEconomicsNonBillableReportUrl}`;
  placeholderDiv: any;
  viz: any;
  // Tableau Dashboard Default options
  options = {
    hideTabs: false,
    hideToolbar: true,
    width: window.innerWidth - 50,
    height: window.innerHeight - 100,
    onFirstInteractive: function () {
    }
  };
  url = '';

  @Input() project: Project;
  @Input() triggerTableauDashboardLoad: boolean;

  constructor(private localStorageService: LocalStorageService) { }

  ngOnInit(): void { }



  ngOnChanges(changes: SimpleChanges) {
    if (changes.triggerTableauDashboardLoad && !!changes.triggerTableauDashboardLoad.currentValue) {
      this.loadTableauDashboard();
    }
  }

  // ------------------------ Helper Functions------------------------------------------//
  private loadTableauDashboard() {
    this.placeholderDiv = document.getElementById('vizContainerCaseOverlayCE');
    if (this.viz) {
      this.viz.dispose();
    }
    this.setReportByCaseType();
    this.viz = new tableau.Viz(this.placeholderDiv, this.url, this.options);
  }

  setReportByCaseType() {
    //escape commas with forward slash as Tableau does not accept unescaped commas in the URL
    if (this.project.caseType.toLowerCase() == 'billable') {
      this.options['Case Code Filter'] = this.allowFilteringWithSpecialCharacters();
      this.url = this.billableReportUrl;
    } else {
      this.options['Case Code Filter Non Billable'] = this.allowFilteringWithSpecialCharacters();
      this.url = this.nonBillableReportUrl;
    }
  }

  allowFilteringWithSpecialCharacters() {
    let newString = `${this.project.oldCaseCode} - ${this.project.caseName} (${this.project.clientName})`.replace(/[,+.?@!#&]/g, (match) => {
          return '\\' + match;
    });
    return newString;
  }

}
