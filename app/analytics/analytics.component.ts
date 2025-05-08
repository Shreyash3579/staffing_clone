// ----------------------- Angular Package References ----------------------------------//
import { Component, OnInit, OnDestroy, ViewEncapsulation } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { AppInsightsService } from '../app-insights.service';
import { CoreService } from '../core/core.service';
import { ConstantsMaster } from '../shared/constants/constantsMaster';
import { GoogleAnalytics } from '../shared/google-analytics/googleAnalytics';
import { SharedNotesService } from '../core/services/shared-notes-info.service';


@Component({
  selector: 'app-analytics',
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class AnalyticsComponent implements OnInit, OnDestroy {

  page = ConstantsMaster.appScreens.report.weeklyDeploymentSummaryView;
  routerSub;
  popover: any;
  report = ConstantsMaster.appScreens.report;
  sharedNotes = [];
  caseIntakeAlerts=[];


  // -------------------Constructor----------------------//
  constructor(private router: Router,
    private coreService: CoreService, private appInsightsService: AppInsightsService, private sharedNotesService: SharedNotesService) {
    this.routerSub = router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        this.page = event.url;
        this.popover?.hide();
      }
    });
  }

  // -------------------Component LifeCycle Events and Functions----------------------//
  ngOnInit() {
    GoogleAnalytics.staffingTrackPageView(this.coreService.loggedInUser.employeeCode, ConstantsMaster.appScreens.page.analytics, '');
    this.appInsightsService.logPageView(this.coreService.loggedInUser.employeeCode, ConstantsMaster.appScreens.page.analytics);
    this.getSharedNotesInfo(this.coreService.loggedInUser.employeeCode);
  }

  private getSharedNotesInfo(loggedInEmployeeCode){
    this.sharedNotesService.getSharedNotes(loggedInEmployeeCode).subscribe(notes => {
      this.sharedNotes = notes;
    });
  }

  ngOnDestroy() {
    this.routerSub.unsubscribe();
  }

  popoverOpen(popover) {
    this.popover = popover;
  }


  // TODO: This is temp solution and explore better solution to show popover
  getContainerClass(reportName) {
    let isBossUser = this.coreService.loggedInUserClaims.Roles.some(x => x.includes('BOSS'));
    let roles = this.coreService.loggedInUserClaims.Roles;
    if (isBossUser) {
      return reportName
    } if (roles.some(x => x === 'Finance')
      && roles.some(x => x === 'PracticeLeader' || x === 'PracticeManager')
      && reportName === 'finance') {
      return 'ADGroupReportFinancePracticeAccess'
    } if (roles.some(x => x === 'Finance')
      && roles.some(x => x === 'PracticeLeader' || x === 'PracticeManager')
      && reportName === 'staffingAllocation') {
      return 'ADGroupReportStaffingAllocationPracticeAccess'
    } if (roles.some(x => x === 'Finance')
      && roles.some(x => x === 'PracticeLeader' || x === 'PracticeManager')
      && reportName === 'monthlyDeployment') {
      return 'ADGroupReportMonthlyDeploymentPracticeAccess'
    } if (roles.some(x => x === 'Finance') && reportName === 'finance') {
      return 'ADGroupReportFinanceAccess'
    } if (roles.some(x => x === 'Finance') && reportName === 'staffingAllocation') {
      return 'ADGroupReportStaffingAllocationAccess'
    } if (roles.some(x => x === 'Finance') && reportName === 'weeklyDeploymentIndividualResourceDetails') {
      return 'ADGroupReportWeeklyDeploymentAccess'
    }
    return 'ADGroupReportAccess'

  }
}
