import { Component, OnInit, Input, Output, EventEmitter, OnDestroy } from "@angular/core";
import { Resource } from "../../../../shared/interfaces/resource.interface";
import { OverlayDialogService } from "src/app/overlay/dialogHelperService/overlayDialog.service";
import { BsModalService } from 'ngx-bootstrap/modal';
import { ResourceOrCasePlanningViewNote } from "src/app/shared/interfaces/resource-or-case-planning-view-note.interface";
import { NoteTypeCode } from "src/app/shared/constants/enumMaster";
import { NotesModalComponent } from "src/app/shared/notes-modal/notes-modal.component";
import { Store } from "@ngrx/store";
import * as StaffingSupplyActions from "src/app/home-copy/state/actions/staffing-supply.action";
import * as fromStaffingSupply from "src/app/home-copy/state/reducers/staffing-supply.reducer";

@Component({
  selector: "app-resourceview",
  templateUrl: "./resourceview.component.html",
  styleUrls: ["./resourceview.component.scss"]
})
export class ResourceviewComponent implements OnInit, OnDestroy {
  showAlertDetails: boolean = false;
  isNotesReadonly: boolean = false;
  upcomingCommitmentsForAlerts:any[] = [];
  
  //-----------------------Input Variables--------------------------------------------//

  @Input() resource: Resource;

  //-----------------------Output Events--------------------------------------------//

  @Output() openResourceDetailsDialog = new EventEmitter();
  @Output() resourceSelectedEmitter = new EventEmitter();

  constructor(private overlayDialogService: OverlayDialogService,
    private modalService: BsModalService,
    private supplyStore: Store<fromStaffingSupply.State>) {}

  //-----------------------Component LifeCycle Events and Functions--------------------------------------------//

  ngOnInit() {
  }

  resourceClickHandler(event) {
    if (event.ctrlKey) {
      this.resource.isSelected = !this.resource.isSelected;
      this.resourceSelectedEmitter.emit(this.resource);
    }
  }

  isAlertForStaffableAs(alert) {
    return alert.indexOf("Staffable as") > -1;
  }

  //-------------------Component Event Handlers-------------------------------------//

  openResourceDetailsDialogHandler(employeeCode) {
    this.overlayDialogService.openResourceDetailsDialogHandler(employeeCode);
  
  }

  toggleAlertDetails() {
    this.showAlertDetails = !this.showAlertDetails;
  }

  hideAlertDetails() {
    this.showAlertDetails = false;
  }

  updateLatestNoteForResource(resource, latestNote) {
    resource.latestCasePlanningBoardViewNote = latestNote;
  }

  openNotesModalhandler() {

    this.resource.resourceViewNotes = this.resource.resourceViewNotes || [];

    const cardObject = {
      name: this.resource.fullName,
      data: {
        employeeCode: this.resource.employeeCode,
        notes: this.resource.resourceViewNotes,
        noteTypeCode: NoteTypeCode.RESOURCE_ALLOCATION_NOTE
      }, 
    };
    
    const modalRef = this.modalService.show(NotesModalComponent, {
      initialState: {
        cardData: cardObject
      },
      class: "demand-notes-modal modal-dialog-centered",
      ignoreBackdropClick: false,
      backdrop: false
    });

    modalRef.content.upsertNotes.subscribe((upsertedNote: ResourceOrCasePlanningViewNote) => {
      this.supplyStore.dispatch( new StaffingSupplyActions.UpsertResourceViewNotes(upsertedNote));
    });

    modalRef.content.deleteNotes.subscribe((caseNoteIdToDelete) => {
      this.supplyStore.dispatch( new StaffingSupplyActions.DeleteResourceViewNotes(caseNoteIdToDelete));
    })  
  }

  
  ngOnDestroy() {
  }
}
