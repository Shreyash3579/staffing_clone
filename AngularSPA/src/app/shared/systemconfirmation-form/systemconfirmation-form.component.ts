import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { PopupDragService } from '../services/popupDrag.service';

@Component({
  selector: 'app-systemconfirmation-form',
  templateUrl: './systemconfirmation-form.component.html',
  styleUrls: ['./systemconfirmation-form.component.scss']
})
export class SystemconfirmationFormComponent implements OnInit {

  constructor(public bsModalRef: BsModalRef,
    private _popupDragService: PopupDragService) { }

  // --------------------------Local Variables---------------------------------//
  public confirmationPopUpBodyMessage: string;
  public allocationId: string;
  public allocationIds = [];
  public commitmentIds = [];
  public placeholderIds = [];
  public resourceAllocation;
  public placeholderAllocation;
  public isBulkDelete = false;
  public splitAllocationForClosure = false;
  public templateConfirmationPopUpBodyMessage: string = null;  

  // -----------------------Output Events--------------------------------------------//
  @Output() deleteResourcesFromProject = new EventEmitter();
  @Output() deletePlaceholdersFromProject = new EventEmitter();
  @Output() deleteResourceNote = new EventEmitter();
  @Output() splitAllocationDuringOfficeClosure = new EventEmitter();
  @Output() onActionResult = new EventEmitter<boolean>();

  // --------------------------Life Cycle Event handlers---------------------------------//
  ngOnInit() {
    this._popupDragService.dragEvents();
  }


  // --------------------------Event handlers---------------------------------//
  closeForm() {
    this.bsModalRef.hide();  
  }

  cancelAction(){
    this.onActionResult.emit(false);
    
    this.closeForm();
  }

  continueAction() {
      if (this.isBulkDelete) {
        this.deleteResourcesFromProject.emit({ allocationIds: this.allocationIds, commitmentIds: this.commitmentIds, resourceAllocation: this.resourceAllocation });
      } 
      else if(this.allocationIds.length > 0 || this.commitmentIds.length > 0 || this.placeholderIds.length > 0) {
        if (this.allocationIds.length > 0 || this.commitmentIds.length > 0) {
          this.deleteResourcesFromProject.emit({ allocationIds: this.allocationIds, commitmentIds: this.commitmentIds, resourceAllocation: this.resourceAllocation });
        } if (this.placeholderIds.length > 0) {
          this.deletePlaceholdersFromProject.emit({ placeholderIds: this.placeholderIds, placeholderAllocation: this.placeholderAllocation });
        } 
      }
      else if(this.splitAllocationForClosure){
        this.splitAllocationDuringOfficeClosure.emit(true);
      }
      else {
        this.deleteResourceNote.emit();
        this.onActionResult.emit(true);
      }
    
    this.closeForm();
  }

  goToPreviousPopup(){
    this.bsModalRef.hide();
    if (this.splitAllocationForClosure) {
      this.splitAllocationDuringOfficeClosure.emit(false);
    }
  }

}
