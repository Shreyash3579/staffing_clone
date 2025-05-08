import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";
import { BsModalRef } from "ngx-bootstrap/modal";
import { PopupDragService } from "../services/popupDrag.service";
import { ConstantsMaster } from "../constants/constantsMaster";

@Component({
    selector: 'app-inline-editable-notes',
    templateUrl: './inline-editable-notes.component.html',
    styleUrls: ['./inline-editable-notes.component.scss']
})
export class InlineEditableNotesComponent implements OnInit {
    public inputNotes: any;
    public uniqueId: string;
    public maxLength: number;
    public event: string;
    public totalCharactersCount = 0;
    public showDeleteButton = false;
    public notesAlertText = ConstantsMaster.NotesAlert;
    // --------------------------Ouput Events--------------------------------//
    @Output() updateNotesEventEmitter = new EventEmitter<any>();
    @Output() deleteNotesEventEmitter = new EventEmitter<any>();

    constructor(public bsModalRef: BsModalRef,
        private _popupDragService: PopupDragService,) { }

    //--------------------------Life Cycle Event handlers---------------------------------//
    ngOnInit() {
        this._popupDragService.dragEvents();
        this.countCharacters();
        this.toggleDeleteButton();
    }

    toggleDeleteButton(){
        if(this.inputNotes && this.event === 'caseCard'){
            this.showDeleteButton = true;
        }
    }
    closeNotes() {
        this.bsModalRef.hide();
    }

    updateNotes() {
        this.updateNotesEventEmitter.emit({ updatedNotes: this.inputNotes?.trim(), uniqueId: this.uniqueId });
        this.closeNotes();
    }

    countCharacters() {
        this.totalCharactersCount = this.inputNotes?.trim().length;
    }

    deleteNotes() {
        this.deleteNotesEventEmitter.emit({ uniqueId: this.uniqueId });
        this.closeNotes();
    }
}