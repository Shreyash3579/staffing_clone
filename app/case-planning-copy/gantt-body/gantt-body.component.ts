import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { DateService } from 'src/app/shared/dateService';

@Component({
  selector: 'app-gantt-body',
  templateUrl: './gantt-body.component.html',
  styleUrls: ['./gantt-body.component.scss']
})
export class GanttBodyComponent implements OnInit, OnChanges {
  public perDayClass = [];
  public projectStartDate = '';
  public projectEndDate = '';

  // ------------------ Input Events ---------------------- //
  @Input() casesGanttData: any;
  @Input() planningCards: any;
  @Input() dateRange: [Date, Date];
  @Input() projects: any;
  // @Input() allRowsCollapsed: boolean;
  @Input() isLeftSideCollapsed: boolean;


  // ------------------ Output Events ---------------- //
  @Output() openPlaceholderForm = new EventEmitter();
  @Output() showQuickPeekDialog = new EventEmitter();
  @Output() openAddTeamSkuForm = new EventEmitter();
  @Output() upsertCasePlanningNote = new EventEmitter();
  @Output() deleteCasePlanningNotes = new EventEmitter();

  //-------------Variables driven by other components ----------------//
  showPlaceholdersOverlay = false;
  selectedProjectId: string;

  // ------------------------Life Cycle Events---------------------------------------
  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.dateRange && this.dateRange) {
      this.perDayClass = [];
      this.getClassNameForEachDay();
    }
  }

  trackById(index: number, item: any): number {
    return item.trackById;
  }

  getClassNameForEachDay(): any {
    this.projectStartDate = DateService.getFormattedDate(this.dateRange[0]);
    this.projectEndDate = DateService.getFormattedDate(this.dateRange[1]);
    const datesBetweenRange = DateService.getDates(this.projectStartDate, this.projectEndDate);
    datesBetweenRange.forEach(date => {
      const day = date.format('ddd').toLowerCase();
      const className = (day === 'sat' || day === 'sun') ? 'weekend' : '';
      this.perDayClass.push(className);
    });
  }

  //------------------ Child Output/evet handleers -------------------------//
  openPlaceholderFormhandler(event) {
    if (this.casesGanttData) {
      this.openPlaceholderForm.emit(event);
    }
  }

  openAddTeamSkuFormHandler(projectToOpen) {
    this.openAddTeamSkuForm.emit(projectToOpen);
  }

  upsertCasePlanningNoteHandler(event) {
    this.upsertCasePlanningNote.emit(event);
  }

  deleteCasePlanningNotesHandler(event) {
    this.deleteCasePlanningNotes.emit(event);
  }

  skuTermClickForProjectHandler(clickedProjectId) {

    if (this.selectedProjectId === clickedProjectId) {
      this.togglePlaceHolderOverlayHandler();
    } else {
      this.openPlaceholderOverlayHandler();
    }

    this.selectedProjectId = clickedProjectId;
  }

  //------------------------------private helpers -----------------------------
  togglePlaceHolderOverlayHandler() {
    this.showPlaceholdersOverlay = !this.showPlaceholdersOverlay;
  }

  openPlaceholderOverlayHandler() {
    this.showPlaceholdersOverlay = true;
  }

  closePlaceholdersOverlayHandler() {
    this.showPlaceholdersOverlay = false;
  }

}