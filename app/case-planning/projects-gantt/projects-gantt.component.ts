import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';

@Component({
  selector: 'app-projects-gantt',
  templateUrl: './projects-gantt.component.html',
  styleUrls: ['./projects-gantt.component.scss']
})
export class ProjectsGanttComponent implements OnInit {
  //------------------- Input Event ----------------------- //
  @Input() scrollDistance: number;  // how much percentage the scroll event should fire ( 2 means (100 - 2*10) = 80%)
  @Input() dateRange: [Date, Date];
  @Input() ganttCasesData: any;
  @Input() planningCards: any;
  // ------------------------Output Events--------------------------------------------//
  @Output() loadMoreCasesEmitter = new EventEmitter();
  @Output() openCaseRollForm = new EventEmitter();
  @Output() openPlaceholderForm = new EventEmitter();
  @Output() showQuickPeekDialog = new EventEmitter();
  @Output() openCaseDetailsDialog = new EventEmitter();
  @Output() openAddTeamSkuForm = new EventEmitter();
  @Output() upsertCasePlanningNote = new EventEmitter();
  @Output() deleteCasePlanningNotes = new EventEmitter();

  //------------------------Referenced by Parent--------------------------------------------//
  @ViewChild('ganttContainer', { static: false }) ganttContainer: ElementRef;
  
  //---------------------------local variables--------------------------------------------//
  isLeftSideCollapsed = false;
  
  // ------------------------Life Cycle Events---------------------------------------
  constructor() { }

  ngOnInit(): void {
  }

  loadMoreCases() {
    this.loadMoreCasesEmitter.emit();
  }

  //------------------ Child Output/evet handleers -------------------------//
  openPlaceholderFormhandler(event) {
    this.openPlaceholderForm.emit(event);
  }

  openAddTeamSkuFormHandler(projectToOpen) {
    this.openAddTeamSkuForm.emit(projectToOpen);
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

  upsertCasePlanningNoteHandler(event) {
    this.upsertCasePlanningNote.emit(event);
  }

  deleteCasePlanningNotesHandler(event) {
    this.deleteCasePlanningNotes.emit(event);
  }

  expandCollapseSidebarEmitterHandler(isLeftSideCollapsed){
    this.isLeftSideCollapsed = isLeftSideCollapsed;
  }

}
