// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from "@angular/core";

// ----------------------- Service References ----------------------------------//
import { ResourceAssignmentService } from "../behavioralSubjectService/resourceAssignment.service";

// ----------------------- Component References ----------------------------------//
import { ProjectOverlayComponent } from "../project-overlay/project-overlay.component";
import { ResourceOverlayComponent } from "../resource-overlay/resource-overlay.component";
import { OverlappedTeamsFormComponent } from "src/app/shared/overlapped-teams-form/overlapped-teams-form.component";

// --------------------------utilities -----------------------------------------//
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { MatDialogRef } from "@angular/material/dialog";
import { Store } from '@ngrx/store';
import * as fromProjectAllocationsStore from 'src/app/state/reducers/project-allocations.reducer';
import * as ProjectAllocationsActions from 'src/app/state/actions/project-allocations.action';

@Injectable()
export class OverlappedTeamDialogService {

    constructor(private modalService: BsModalService,
        private resourceAssignmentService: ResourceAssignmentService,
      private projectAllocationsStore: Store<fromProjectAllocationsStore.State>) { }

    // --------------------------Local Variable -----------------------------------------//

    bsModalRef: BsModalRef;
    projectDialogRef: MatDialogRef<ProjectOverlayComponent, any>;
    dialogRef: MatDialogRef<ResourceOverlayComponent, any>;

    // --------------------------Overlay -----------------------------------------//

    openOverlappedTeamsFormHandler(modalData) {
        // class is required to center align the modal on large screens
        let initialState = null;

        if (modalData) {

            initialState = {
                projectData: modalData.projectData,
                overlappedTeams: modalData.overlappedTeams,
                allocation: modalData.allocation
            };

        }

        const config = {
            class: 'custom-modal-large modal-dialog-centered',
            ignoreBackdropClick: true,
            initialState: initialState
        };

        this.bsModalRef = this.modalService.show(OverlappedTeamsFormComponent, config);

        // inserts & updates resource data when changes are made to resource
        this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe(updatedCommitment => {
            let upsertedAllocations:any = updatedCommitment;

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

    }
}
