import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { CoreService } from 'src/app/core/core.service';
import { AllocationNotesContextMenuComponent } from 'src/app/resources/allocation-notes-context-menu/allocation-notes-context-menu.component';
import { ResourceOrCasePlanningViewNote } from '../interfaces/resource-or-case-planning-view-note.interface';
import { ConstantsMaster } from '../constants/constantsMaster';
import * as fromUserSpecificDetails from 'src/app/state/reducers/user-specific-details.reducer';
import { Store, select } from '@ngrx/store';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-shared-gantt-notes',
  templateUrl: './gantt-notes.component.html',
  styleUrls: ['./gantt-notes.component.scss']
})
export class GanttNotesComponent implements OnInit {
  // -----------------------Input Events-----------------------------------------------//
  @Input() rowIndex = "";
  @Input() notes: ResourceOrCasePlanningViewNote[] = [];
  @Input() hideAddNewNote: boolean = true;
  @Input() isNotesReadonly: boolean = false;
  @Input() isHideCloseIcon: boolean = true;

  // -----------------------Output Events-----------------------------------------------//
  @Output() upsertNote = new EventEmitter<any>();
  @Output() deleteNotes = new EventEmitter<any>();
  @Output() closeNotesWrapperEmitter = new EventEmitter<boolean>();
  public userNote = "";
  public shareOptionsCollapsed = true;
  public editNoteMode = false;
  public loggedInUser: string;
  public loggedInUserName: string;
  public noteWrapperCollapsed: boolean = false;
  public notesAlertText = ConstantsMaster.NotesAlert;
  public mostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode: any[] = [];
  public storeSub: Subscription = new Subscription();


  public editedNoteObj: ResourceOrCasePlanningViewNote = {
    note: "",
    lastUpdated: new Date(),
    createdBy: "",
    createdByName: "",
    id: "",
    isPrivate: false,
    sharedWith: "",
    sharedWithDetails: null,
    lastUpdatedBy: ""
  };

  public sharingOption = {
    label: "Only me",
    icon: "",
    checked: true,
    users: []
  };

  // Sharing Options
  public options = [
    { label: "Only me", icon: "lock", checked: true },
    { label: "All users", icon: "globe", checked: false },
    { label: "Specific users", icon: "user", checked: false, specifcUsers: [] },
    {label: 'Most Recent',icon: "user", checked: false}
  ];

  bsModalRef: BsModalRef;
  notesContextMenuDialogRef: MatDialogRef<AllocationNotesContextMenuComponent, any>;

  constructor(
    private coreService: CoreService,
    private modalService: BsModalService,
    private readonly changeDetector: ChangeDetectorRef,
    private userSpecificStore: Store<fromUserSpecificDetails.State>,
  ) { }

  ngOnInit(): void {
    this.setNotesIfNull();
    this.loggedInUser = this.coreService.loggedInUser.employeeCode;
    this.loggedInUserName = this.coreService.loggedInUser.fullName;
    this.getMostRecentSharedWithEmployeeGroupsFromStore();
    this.updateAddNotesSectionVisibility();
  }

  updateAddNotesSectionVisibility() {
    if (this.isNotesReadonly) {
        this.hideAddNewNote = true;
    }
}

  setNotesIfNull() {
    if (!this.notes) {
      this.notes = [];
    }
  }
  getMostRecentSharedWithEmployeeGroupsFromStore() {
    this.storeSub.add(this.userSpecificStore
      .pipe(
        select(fromUserSpecificDetails.getMostRecentSharedWithEmployeeGroups))
      .subscribe((mostRecentSharedWithEmployeeGroups: any[]) => {
        this.mostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode = mostRecentSharedWithEmployeeGroups;
        this.selectSharingOption();
      }))
  }

  selectSharingOption() {
    if (this.mostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode.length > 0) {
      this.sharingOption = {
        label: this.options[3].label,
        icon: this.options[3].icon,
        checked: true,
        users: [...this.mostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode[0].sharedWithEmployees]
      }
    }
    else{
      this.sharingOption = {
        label: this.options[0].label,
        icon: this.options[0].icon,
        checked: true,
        users: []
      }
    }

  }


  // Handles note context menu clicked option
  // contextMenuClick(event, note, index) {
  //   if (event.target.id === "edit") {
  //     this.editNote(note);
  //     this.changeDetector.detectChanges();
  //   } else if (event.target.id === "delete") {
  //     this.notes.forEach((item) => {
  //       if (item.id === note.id) {
  //         this.notes.splice(index, 1);
  //         this.deleteNotes.emit(note.id);
  //       }
  //     });
  //   }

  //   this.toggleNoteContextMenu(event, note, index);
  // }

  onEditNote(note){
    this.editNote(note);
    this.changeDetector.detectChanges();
  }

  onDeleteNote(note, index){
  this.notes.forEach((item) => {
    if (item.id === note.id) {
      this.notes.splice(index, 1);
      this.deleteNotes.emit(note.id);
    }
    });
  }

  // Handles Editing Notee
  editNote(noteObj) {
    this.cancelNoteHandler();
    this.hideAddNewNote = false;
    this.editNoteMode = !this.editNoteMode;
    this.userNote = noteObj.note;
    this.setSharingOptionObj(noteObj);

    this.editedNoteObj = {
      id: noteObj.id,
      note: this.userNote,
      lastUpdated: noteObj.lastUpdated,
      createdBy: noteObj.createdBy,
      createdByName: noteObj.createdByName,
      employeeCode: noteObj.employeeCode,
      isPrivate: noteObj.isPrivate,
      lastUpdatedBy: noteObj.lastUpdatedBy,
      sharedWith: noteObj.sharedWith,
      sharedWithDetails: noteObj.sharedWithDetails
    };

  }

  setSharingOptionObj(noteObj: ResourceOrCasePlanningViewNote) {
    if (!noteObj.isPrivate) {
      this.sharingOption = {
        label: this.options[1].label,
        icon: this.options[1].icon,
        checked: true,
        users: []
      }
    }
    else if (noteObj.isPrivate && !noteObj.sharedWith) {
      this.sharingOption = {
        label: this.options[0].label,
        icon: this.options[0].icon,
        checked: true,
        users: []
      }
    }
    else {
      this.sharingOption = {
        label: this.options[2].label,
        icon: this.options[2].icon,
        checked: true,
        users: noteObj.sharedWithDetails
      }
    }
  }

  // Toggle Add Note Input
  toggleAddNewNote() {
    this.cancelNoteHandler();
    this.hideAddNewNote = !this.hideAddNewNote;
  }

  // Add Note Handler
  addNoteHandler() {
    const note = this.setNoteObject();

    if (this.userNote.length >= 1 || this.userNote !== "") {
      this.notes.unshift(note);
      this.cancelNoteHandler();
      this.upsertNote.emit(note);
    }
  }

  setNoteObject() {
    const note: ResourceOrCasePlanningViewNote = {
      note: this.userNote,
      lastUpdated: new Date(),
      createdBy: this.loggedInUser,
      createdByName: this.loggedInUserName,
      sharedWith: this.sharingOption?.users?.map(x => x.employeeCode).join(','),
      sharedWithDetails: this.sharingOption?.users,
      isPrivate: this.isNotePrivate(),
      lastUpdatedBy: this.loggedInUser
    };
    return note;
  }

  // Saving edited notes
  saveNoteEditHandler() {
    this.editedNoteObj.note = this.userNote;
    this.editedNoteObj.isPrivate = this.isNotePrivate();
    this.editedNoteObj.sharedWith = this.sharingOption.users.map(x => x.employeeCode).join(',');
    this.editedNoteObj.sharedWithDetails = this.sharingOption.users;

    this.notes.forEach((item) => {
      if (item.id === this.editedNoteObj.id) {
        item.note = this.userNote,
          item.lastUpdated = new Date(),
          item.createdBy = this.loggedInUser,
          item.createdByName = this.loggedInUserName,
          item.sharedWith = this.sharingOption.users.map(x => x.employeeCode).join(','),
          item.sharedWithDetails = this.sharingOption.users,
          item.isPrivate = this.isNotePrivate(),
          item.lastUpdatedBy = this.loggedInUser
      }
    });
    this.upsertNote.emit(this.editedNoteObj);

    this.editNoteMode = !this.editNoteMode;
    this.cancelNoteHandler();
  }

  // Cancel Changes
  cancelNoteHandler() {
    this.shareOptionsCollapsed = true;
    this.hideAddNewNote = true;
    this.editNoteMode = false;
    this.userNote = "";
  }

  // Get Sharing Option
  handleSharingOptionClick(event) {
    let users =
      event.specificUsers && event.specificUsers.length
        ? event.specificUsers
        : [];

    this.sharingOption = {
      label: event.label,
      icon: event.icon,
      checked: event.checked,
      users: users
    };
  }

  // Delete Specific User
  deleteUser(user, index) {
    this.sharingOption.users.forEach((item) => {
      if ((item.employeeCode ?? item) === (user.employeeCode ?? user)) {
        this.sharingOption.users.splice(index, 1);
      }
    });
  }

  isNotePrivate() {
    return this.sharingOption.label !== this.options[1].label;
  }

  closeNotesWrapper(){
    this.closeNotesWrapperEmitter.emit(true);
  }

}
