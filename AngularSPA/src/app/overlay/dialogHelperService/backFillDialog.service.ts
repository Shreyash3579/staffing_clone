// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';

// ----------------------- Component References ----------------------------------//
import { BackfillFormComponent } from 'src/app/shared/backfill-form/backfill-form.component';
import { ProjectOverlayComponent } from '../project-overlay/project-overlay.component';
import { ResourceOverlayComponent } from '../resource-overlay/resource-overlay.component';

// ----------------------- Service References ----------------------------------//
import { ResourceAssignmentService } from '../behavioralSubjectService/resourceAssignment.service';

// --------------------------utilities -----------------------------------------//
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { MatDialogRef } from '@angular/material/dialog';
import { PlaceholderAssignmentService } from '../behavioralSubjectService/placeholderAssignment.service';
import { Store } from '@ngrx/store';
import * as fromStaffingDemand  from 'src/app/home-copy/state/reducers/staffing-demand.reducer'
import * as  StaffingDemandActions from 'src/app/home-copy/state/actions/staffing-demand.action';
import * as fromProjectAllocationsStore from 'src/app/state/reducers/project-allocations.reducer';
import * as ProjectAllocationsActions from 'src/app/state/actions/project-allocations.action';

@Injectable()
export class BackfillDialogService {

  constructor(private modalService: BsModalService,
    private resourceAssignmentService: ResourceAssignmentService,
    private placeholderAssignmentService: PlaceholderAssignmentService,
    private demandStore: Store<fromStaffingDemand.State>,
    private projectAllocationsStore: Store<fromProjectAllocationsStore.State> ) { }

  // --------------------------Local Variable -----------------------------------------//

  bsModalRef: BsModalRef;
  projectDialogRef: MatDialogRef<ProjectOverlayComponent, any>;
  dialogRef: MatDialogRef<ResourceOverlayComponent, any>;
  // --------------------------Overlay -----------------------------------------//
  
  openBackFillFormHandler(event) {
    // class is required to center align the modal on large screens
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        project: event.project,
        resourceAllocation: event.resourceAllocation,
        showMoreThanYearWarning: event.showMoreThanYearWarning,
        isPlaceholderAllocation: event.isPlaceholderAllocation,
        allocationDataBeforeSplitting: event.allocationDataBeforeSplitting
      }
    };
    this.bsModalRef = this.modalService.show(BackfillFormComponent, config);

    this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe(data => {
      event.project.allocatedResources = event.project.allocatedResources.concat(data.resourceAllocation);
      let upsertedAllocations:any = data;

      this.projectAllocationsStore.dispatch(
        new ProjectAllocationsActions.UpsertResourceAllocations({
          resourceAllocation: upsertedAllocations.resourceAllocation,
          splitSuccessMessage: upsertedAllocations.splitSuccessMessage,
          showMoreThanYearWarning: upsertedAllocations.showMoreThanYearWarning,
          allocationDataBeforeSplitting: upsertedAllocations.allocationDataBeforeSplitting,
          projectDialogRef: this.projectDialogRef,
          resourceDialogRef: this.dialogRef
        })
        );
    });

    this.bsModalRef.content.deletePlaceHoldersByIds.subscribe(event => {
      this.projectAllocationsStore.dispatch(
        new ProjectAllocationsActions.DeletePlaceholderAllocations({
            placeholderIds : event.placeholderIds,
            placeholderAllocation: event.placeholderAllocation,
            notifyMessage: event.notifyMessage, 
            projectDialogRef: this.projectDialogRef
        })
      )
    });

  }

  closeDialog(){
    this.bsModalRef?.hide();
  }

}
