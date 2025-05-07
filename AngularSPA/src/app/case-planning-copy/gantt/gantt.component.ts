import { Component, ElementRef, EventEmitter, Input, OnInit, Output, SimpleChange, SimpleChanges, ViewChild } from '@angular/core';
import { DateService } from 'src/app/shared/dateService';

@Component({
  selector: 'app-gantt',
  templateUrl: './gantt.component.html',
  styleUrls: ['./gantt.component.scss']
})
export class GanttComponent implements OnInit {
  filteredGanttCasesData: any;
  filteredPlanningCards: any;
  projects: any;
  //------------------- Input Event ----------------------- //
  @Input() scrollDistance: number;  // how much percentage the scroll event should fire ( 2 means (100 - 2*10) = 80%)
  @Input() dateRange: [Date, Date];
  @Input() ganttCasesData: any;
  @Input() planningCards: any;
  @Input() planningBoard;
  @Input() planningBoardColumnMetrics;
  // ------------------------Output Events--------------------------------------------//
  @Output() loadMoreCasesEmitter = new EventEmitter();
  @Output() openPlaceholderForm = new EventEmitter();
  @Output() showQuickPeekDialog = new EventEmitter();
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

  ngOnChanges(changes: SimpleChanges) {
    if ((changes.ganttCasesData && this.ganttCasesData) || (changes.planningCards && this.planningCards)) {    
      this.filteredGanttCasesData = this.ganttCasesData;
      this.filteredPlanningCards = this.planningCards;
    }
  }

  loadMoreCases() {
    this.loadMoreCasesEmitter.emit();
  }

  //------------------ Child Output/evet handleers -------------------------//
  sortEmitterHandler(event) { 
    if(event.column.field == 'client') {       
      if (event.sort === 1) {
        event.projects.sort((a, b) => a.clientName?.localeCompare(b.clientName));
      } else if(event.sort === -1) {
        event.projects.sort((a, b) => b.clientName?.localeCompare(a.clientName));
      }
    }
    else if(event.column.field == 'case') {
      if (event.sort === 1) {
        event.projects.sort((a, b) => a.caseName?.localeCompare(b.caseName));
      } else if(event.sort === -1) {
        event.projects.sort((a, b) => b.caseName?.localeCompare(a.caseName));
      }
    }
    else if(event.column.field == 'manager') {
      if (event.sort === 1) {
        event.projects.sort((a, b) => a.manager?.localeCompare(b.manager));
      } else if(event.sort === -1) {
        event.projects.sort((a, b) => b.manager?.localeCompare(a.manager));
      }
    }
    else if(event.column.field == 'code') {
      if (event.sort === 1) {
        event.projects.sort((a, b) => a.caseCode?.localeCompare(b.caseCode));
      } else if(event.sort === -1) {
        event.projects.sort((a, b) => b.caseCode?.localeCompare(a.caseCode));
      }
    }
    else if (event.column.field == '%') {
      if (event.sort === 1) {
        event.projects.sort((a, b) => a.probabilityPercent - b.probabilityPercent);
      } else if(event.sort === -1) {
        event.projects.sort((a, b) => b.probabilityPercent - a.probabilityPercent);
      }
    }
    else if(event.column.field == 'start') {
      if (event.sort === 1) {
        event.projects.sort((a, b) => new Date(a.startDate).getTime() - new Date(b.startDate).getTime());
      } else if(event.sort === -1) {
        event.projects.sort((a, b) => new Date(b.startDate).getTime() - new Date(a.startDate).getTime());
      }
    }
    else if(event.column.field == 'end') {
      if (event.sort === 1) {
        event.projects.sort((a, b) => new Date(a.endDate).getTime() - new Date(b.endDate).getTime());
      } else if(event.sort === -1) {
        event.projects.sort((a, b) => new Date(b.endDate).getTime() - new Date(a.endDate).getTime());
      }
    }
    else if(event.column.field == 'office') {
      if (event.sort === 1) {
        event.projects.sort((a, b) => a.office.managingOfficeAbbreviation?.localeCompare(b.office.managingOfficeAbbreviation));
      } else if(event.sort === -1) {
        event.projects.sort((a, b) => b.office.managingOfficeAbbreviation?.localeCompare(a.office.managingOfficeAbbreviation));
      }
    }
    else if(event.column.field == 'include in demand') {
      if (event.sort === 1) {
        event.projects.sort((a, b) => a.includeInDemand - b.includeInDemand);
      } else if(event.sort === -1) {
        event.projects.sort((a, b) => b.includeInDemand - a.includeInDemand);
      }
    }
    this.projects = event.projects;
  }

  filterEmitterHandler(filter) {  
      if (filter.value && filter.value.length > 0) {
        this.filteredGanttCasesData = this.ganttCasesData.filter(caseItem => 
          filter.value.some(value => {
            if (filter.field === 'client') return caseItem.clientName == value;
            if (filter.field === 'case') return caseItem.caseName == value || caseItem.opportunityName == value;
            if (filter.field === 'manager') return caseItem.caseManagerFullName == value;
            if (filter.field === 'code') return caseItem.oldCaseCode == value;
            if (filter.field === '%') return String(caseItem.probabilityPercent) == value;
            if (filter.field === 'start') return DateService.convertDateInBainFormat(caseItem.startDate) == value;
            if (filter.field === 'end') return DateService.convertDateInBainFormat(caseItem.endDate) == value;
            if (filter.field === 'include in demand') return caseItem.includeInDemand == value;
            if (filter.field === 'office') return caseItem.managingOfficeCode == value;
            return true;
          })
      );
  
        this.filteredPlanningCards = this.planningCards.filter(card => 
          filter.value.some(value => {
            if (filter.field === 'client') return false;
            if (filter.field === 'case') return card.name == value;
            if (filter.field === 'manager') return false;
            if (filter.field === 'code') return false;
            if (filter.field === '%') return String(card.probabilityPercent) == value;
            if (filter.field === 'start') return DateService.convertDateInBainFormat(card.startDate) == value;
            if (filter.field === 'end') return DateService.convertDateInBainFormat(card.endDate) == value;
            if (filter.field === 'include in demand') return card.includeInDemand == value;
            if (filter.field === 'office') {
              const officeCodes = card.sharedOfficeCodes?.split(',').map(code => code.trim()) || [];
                return officeCodes.includes(value);
            }
            return true;
          })
        );
      } else {
        this.filteredGanttCasesData = this.ganttCasesData;
        this.filteredPlanningCards = this.planningCards;
      }
    }


  openPlaceholderFormhandler(event) {
    this.openPlaceholderForm.emit(event);
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

  expandCollapseSidebarEmitterHandler(isLeftSideCollapsed){
    this.isLeftSideCollapsed = isLeftSideCollapsed;
  }

}
