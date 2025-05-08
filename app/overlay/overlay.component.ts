// ----------------------- Angular Package References ----------------------------------//
import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subject } from 'rxjs';
import { ActivatedRoute } from '@angular/router';
import { takeUntil } from 'rxjs/operators';

// --------------------------Service References -----------------------------------------//
import { BackfillDialogService } from './dialogHelperService/backFillDialog.service';
import { CaseRollDialogService } from './dialogHelperService/caseRollDialog.service';
import { NotesDialogService } from './dialogHelperService/notesDialog.service';
import { OverlayDialogService } from './dialogHelperService/overlayDialog.service';
import { QuickAddDialogService } from './dialogHelperService/quickAddDialog.service';
import { GoogleAnalytics } from '../shared/google-analytics/googleAnalytics';
import { CoreService } from '../core/core.service';
import { AppInsightsService } from '../app-insights.service';
import { ConstantsMaster } from '../shared/constants/constantsMaster';

@Component({
  selector: 'app-overlay',
  template: ''
})
export class OverlayComponent implements OnInit, OnDestroy {

  // -----------------------Local Variables--------------------------------------------//
  private destroy$: Subject<boolean> = new Subject<boolean>();

  // -----------------------Constructor--------------------------------------------//

  constructor(
    private route: ActivatedRoute,
    private overlayDialogService: OverlayDialogService,
    private notesDialogService: NotesDialogService,
    private quickAddDialogService: QuickAddDialogService,
    private caseRollDialogService: CaseRollDialogService,
    private backfillDialogService: BackfillDialogService,
    private appInsightsService: AppInsightsService,
    private coreService: CoreService
  ) {

    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe(params => {
      const employeeCode = params['employee'];
      const oldCaseCode = params['oldCaseCode'];
      const pipelineId = params['pipelineId'];
      const planningCardId = params['planningCardId']

      if (employeeCode != null) {
        this.openResourceDetailsDialogHandler(employeeCode);
      } else if (pipelineId || oldCaseCode) {
        this.openProjectDetailsDialogHandler({ oldCaseCode: oldCaseCode, pipelineId: pipelineId });
      }
      else{
        this.openPlanningCardDetailsDialogHandler(planningCardId);
      }
    });
  }

  // -------------------Component LifeCycle Events and Functions----------------------//

  ngOnInit() {
    GoogleAnalytics.staffingTrackPageView(this.coreService.loggedInUser.employeeCode, ConstantsMaster.appScreens.page.overlay, '');
    this.appInsightsService.logPageView(this.coreService.loggedInUser.employeeCode, ConstantsMaster.appScreens.page.overlay);
  }

  ngOnDestroy() {
    this.destroy$.next(true);
    this.destroy$.unsubscribe();
  }

  // -----------------------Helper Functions--------------------------------------------//

  private openProjectDetailsDialogHandler(projectData) {
    this.overlayDialogService.openProjectDetailsDialogHandler(projectData);
  }

  private openPlanningCardDetailsDialogHandler(projectData){
    this.overlayDialogService.openPlanningCardDetailsDialogHandler(projectData);
  }

  private openResourceDetailsDialogHandler(employeeCode) {
    this.overlayDialogService.openResourceDetailsDialogHandler(employeeCode);
  }

  openQuickAddFormHandler(modalData) {
    this.quickAddDialogService.openQuickAddFormHandler(modalData);
  }

  // private openCaseRollFormHandler(event) {
  //   this.caseRollDialogService.openCaseRollFormHandler(event);
  // }

  // private openBackFillFormHandler(event) {
  //   this.backfillDialogService.openBackFillFormHandler(event);
  // }

  // private openNotesDialogHandler(event) {
  //   this.notesDialogService.openNotesDialogHandler(event);
  // }

}
