import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-projects-gantt-task',
  templateUrl: './projects-gantt-task.component.html',
  styleUrls: ['./projects-gantt-task.component.scss']
})
export class ProjectsGanttTaskComponent implements OnInit {

  constructor() { }

  //---------- Local variables ---------------- //
  showMoreThanYearWarning = false;
  regexNumber = new RegExp('^[0-9]+$');
  showPlaceholdersOverlay: boolean = false;
  selectedProject: any;

  // ------------------ Input Events ----------------- //
  @Input() casesGanttData: any;
  @Input() planningCards: any;
  @Input() dateRange: [Date, Date];
  @Input() bounds: any;

  // ------------------ Output Events ---------------- //
  @Output() openPlaceholderForm = new EventEmitter();
  @Output() openCaseRollForm = new EventEmitter();
  @Output() showQuickPeekDialog = new EventEmitter();
  @Output() openCaseDetailsDialog = new EventEmitter();
  @Output() openAddTeamSkuForm = new EventEmitter();
  // ------------------------Life Cycle Events---------------------------------------
  ngOnInit(): void { }

  //------------------ Child Output/evet handleers -------------------------//
  
  openPlaceholderFormhandler(event) {
    if(this.casesGanttData || this.planningCards){

      this.openPlaceholderForm.emit(event);
    }
  }

  openCaseRollFormHandler(caseRollData) {
    this.openCaseRollForm.emit(caseRollData);
  }

  showQuickPeekDialogHandler(employeesForQuickPeek) {
    this.showQuickPeekDialog.emit(employeesForQuickPeek);
  }

  openCaseDetailsDialogHandler(projectToOpen) {
    this.openCaseDetailsDialog.emit(projectToOpen);
  }

  openAddTeamSkuFormHandler(projectToOpen) {
    this.openAddTeamSkuForm.emit(projectToOpen);
  }

  

  

}
