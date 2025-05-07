import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-gantt-task',
  templateUrl: './gantt-task.component.html',
  styleUrls: ['./gantt-task.component.scss']
})
export class GanttTaskComponent implements OnInit {

  constructor() { }

  //---------- Local variables ---------------- //
  showMoreThanYearWarning = false;
  regexNumber = new RegExp('^[0-9]+$');
  showPlaceholdersOverlay: boolean = false;
  selectedProject: any;

  // ------------------ Input Events ----------------- //
  @Input() casesGanttData: any;
  @Input() planningCards: any;
  @Input() project: any;
  @Input() dateRange: [Date, Date];
  @Input() bounds: any;

  // ------------------ Output Events ---------------- //
  @Output() openPlaceholderForm = new EventEmitter();
  @Output() openAddTeamSkuForm = new EventEmitter();
  // ------------------------Life Cycle Events---------------------------------------
  ngOnInit(): void { }

  //------------------ Child Output/evet handleers -------------------------//

  openPlaceholderFormhandler(event) {
    if(this.casesGanttData || this.planningCards){
      this.openPlaceholderForm.emit(event);
    }
  }

  openAddTeamSkuFormHandler(projectToOpen) {
    this.openAddTeamSkuForm.emit(projectToOpen);
  }

}