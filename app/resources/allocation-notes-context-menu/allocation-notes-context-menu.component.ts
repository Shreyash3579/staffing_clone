import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ResourceOrCasePlanningViewNote } from 'src/app/shared/interfaces/resource-or-case-planning-view-note.interface';

@Component({
  selector: 'app-allocation-notes-context-menu',
  templateUrl: './allocation-notes-context-menu.component.html',
  styleUrls: ['./allocation-notes-context-menu.component.scss']
})
export class AllocationNotesContextMenuComponent implements OnInit {
  public note: ResourceOrCasePlanningViewNote;
  public rowIndex: string;
  public noteIndex: number;

  @Output() contextMenuClick = new EventEmitter<any>();

  constructor(public modalRef: BsModalRef) { }

  ngOnInit(): void {
  }

  noteContextMenuClick(event, note, noteIndex) {
    this.contextMenuClick.emit({
      event: event,
      note: note,
      noteIndex: noteIndex
    });
    this.closeForm();
  }

  closeForm() {
    this.modalRef.hide();
  }

}
