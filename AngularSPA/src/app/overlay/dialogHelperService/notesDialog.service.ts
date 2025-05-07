// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';

// ----------------------- Component References ----------------------------------//
import { AgGridNotesComponent } from '../ag-grid-notes/ag-grid-notes.component';
import { ProjectOverlayComponent } from '../project-overlay/project-overlay.component';
import { ResourceOverlayComponent } from '../resource-overlay/resource-overlay.component';

// ----------------------- Service References ----------------------------------//
import { ResourceAssignmentService } from '../behavioralSubjectService/resourceAssignment.service';

// --------------------------utilities -----------------------------------------//
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { MatDialogRef } from '@angular/material/dialog';


@Injectable()
export class NotesDialogService {

  constructor(private modalService: BsModalService,
    private resourceAssignmentService: ResourceAssignmentService ) { }

  // --------------------------Local Variable -----------------------------------------//

  bsModalRef: BsModalRef;
  projectDialogRef: MatDialogRef<ProjectOverlayComponent, any>;
  dialogRef: MatDialogRef<ResourceOverlayComponent, any>;
  // --------------------------Overlay -----------------------------------------//
  
  openNotesDialogHandler(event) {
    // check if the popup is already open
    if (!this.bsModalRef) {
      // class is required to center align the modal on large screens
      const config = {
        class: 'modal-dialog-centered',
        ignoreBackdropClick: true,
        initialState: {
          projectData: event.projectData,
          popupType: event.popupType
        }
      };
      this.bsModalRef = this.modalService.show(AgGridNotesComponent, config);

      // inserts & updates resource data when changes are made to notes of an allocation
      this.bsModalRef.content.updateNotesForAllocation.subscribe(updatedData => {
        this.resourceAssignmentService.upsertResourceAllocationsToProject(updatedData, this.dialogRef, this.projectDialogRef);
        this.resourceAssignmentService.upsertPlaygroundAllocationsForCasePlanningMetrics(updatedData.resourceAllocation);
      });

      //clear bsModalRef value on closing modal
      this.modalService.onHidden.subscribe(() => {
        this.bsModalRef = null;
      })
    }
  }

}
