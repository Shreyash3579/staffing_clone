// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';

// ----------------------- Component/Service References ----------------------------------//
import { AgGridSplitAllocationPopUpComponent } from '../ag-grid-split-allocation-pop-up/ag-grid-split-allocation-pop-up.component';
import { ProjectOverlayComponent } from '../project-overlay/project-overlay.component';
import { ResourceOverlayComponent } from '../resource-overlay/resource-overlay.component';
import { ResourceAssignmentService } from '../behavioralSubjectService/resourceAssignment.service';

// --------------------------utilities -----------------------------------------//
import { MatDialogRef } from '@angular/material/dialog';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { Store } from '@ngrx/store';
import * as fromProjectAllocationsStore from 'src/app/state/reducers/project-allocations.reducer';
import * as ProjectAllocationsActions from 'src/app/state/actions/project-allocations.action';
import { PlanningCardOverlayComponent } from '../planning-card-overlay/planning-card-overlay.component';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';

@Injectable()
export class SplitAllocationDialogService {

  constructor(private modalService: BsModalService,
    private resourceAssignmentService: ResourceAssignmentService,
    private projectAllocationsStore: Store<fromProjectAllocationsStore.State> ) { }

  // --------------------------Local Variable -----------------------------------------//

  bsModalRef: BsModalRef;
  projectDialogRef: MatDialogRef<ProjectOverlayComponent, any>;
  dialogRef: MatDialogRef<ResourceOverlayComponent, any>;
  planningCardDialogRef: MatDialogRef<PlanningCardOverlayComponent,any>;
  // --------------------------Overlay -----------------------------------------//
  
  openSplitAllocationDialogHandler(event) {
    // check if the popup is already open
    if (!this.bsModalRef) {
      // class is required to center align the modal on large screens
      const config = {
        class: 'modal-dialog-centered',
        ignoreBackdropClick: true,
        initialState: {
          allocationData: event.allocationData,
          popupType: event.popupType
        }
      };
      this.bsModalRef = this.modalService.show(AgGridSplitAllocationPopUpComponent, config);

      // inserts & updates resource data when changes are made to notes of an allocation
      this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe(upsertData => {
        let upsertedAllocations : any = upsertData;
        this.projectAllocationsStore.dispatch(
        new ProjectAllocationsActions.UpsertResourceAllocations({
          resourceAllocation: upsertedAllocations.resourceAllocation,
          splitSuccessMessage: upsertedAllocations.splitSuccessMessage,
          showMoreThanYearWarning: upsertedAllocations.showMoreThanYearWarning,
          allocationDataBeforeSplitting: upsertedAllocations.allocationDataBeforeSplitting,
          projectDialogRef: this.projectDialogRef,
          resourceDialogRef: this.dialogRef
        })
      )
      });

     // inserts & updates placeholder data when changes are made to placeholder
     this.bsModalRef.content.upsertPlaceholderAllocationsToProject.subscribe(updatedData => {
    let placeholderAllocations : PlaceholderAllocation[] = updatedData.placeholderAllocations;
      this.projectAllocationsStore.dispatch(
          new ProjectAllocationsActions.UpsertPlaceholderAllocations({
            placeholderAllocations: placeholderAllocations,
            planningCardDialogRef: this.planningCardDialogRef
          })
        )
   });

      //clear bsModalRef value on closing modal
      this.modalService.onHidden.subscribe(() => {
        this.bsModalRef = null;
      });
    }
  }
  

}
