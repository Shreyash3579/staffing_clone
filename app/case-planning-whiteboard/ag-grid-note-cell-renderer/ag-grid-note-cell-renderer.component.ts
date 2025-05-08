import { Component, EventEmitter, Output } from '@angular/core';
import { ICellRendererParams } from 'ag-grid-community';
import { ICellRendererAngularComp } from 'ag-grid-angular';
import { ResourceOrCasePlanningViewNote } from 'src/app/shared/interfaces/resource-or-case-planning-view-note.interface';

@Component({
  selector: 'app-ag-grid-note-cell-renderer',
  templateUrl: './ag-grid-note-cell-renderer.component.html',
  styleUrls: ['./ag-grid-note-cell-renderer.component.scss']
})
export class AgGridNoteCellRendererComponent implements ICellRendererAngularComp {

  @Output() openNotesModal = new EventEmitter();

  params;
  project: any;
  noteIconUrl: string = "";
  latestNoteData: ResourceOrCasePlanningViewNote;
  public caseNotes: ResourceOrCasePlanningViewNote[] = [];


  constructor() { }

  agInit(params: ICellRendererParams): void {
    this.params = params;
    this.project = params.data;
    this.latestNoteData = params.data.latestCasePlanningBoardViewNote;
    this.getNotesIcon();
  }

  getNotesIcon() {
    this.noteIconUrl = this.latestNoteData ? "assets/img/red-notes-icon.svg" : "assets/img/add-note-icon.svg";
  }

  // Toggle New Note Modal
  toggleNoteModal() {
    this.openNotesModal.emit(this.project);
  }

  refresh(): boolean {
    return true;
  }
}
