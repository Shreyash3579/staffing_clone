// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';

// ----------------------- Component References ----------------------------------//
import { ProjectOverlayComponent } from '../project-overlay/project-overlay.component';

// ----------------------- Service References ----------------------------------//
import { BackfillDialogService } from './backFillDialog.service';
import { ResourceAssignmentService } from '../behavioralSubjectService/resourceAssignment.service';
import { ResourceCommitmentService } from '../behavioralSubjectService/resourceCommitment.service';

// --------------------------utilities -----------------------------------------//
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { MatDialogRef } from '@angular/material/dialog';
import { PlaceholderAssignmentService } from '../behavioralSubjectService/placeholderAssignment.service';
import { PlaceholderFormComponent } from 'src/app/shared/placeholder-form/placeholder-form.component';

import { Store } from '@ngrx/store';
import * as fromProjectAllocationsStore from 'src/app/state/reducers/project-allocations.reducer';
import * as ProjectAllocationsActions from 'src/app/state/actions/project-allocations.action';
import { PlanningCardOverlayComponent } from '../planning-card-overlay/planning-card-overlay.component';

@Injectable()
export class PlaceholderDialogService {

    constructor(private modalService: BsModalService,
        private backfillDialogService: BackfillDialogService,
        private resourceAssignmentService: ResourceAssignmentService,
        private placeholderAssignmentService: PlaceholderAssignmentService,
        private projectAllocationsStore: Store<fromProjectAllocationsStore.State>) { }

    // --------------------------Local Variable -----------------------------------------//

    bsModalRef: BsModalRef;
    projectDialogRef: MatDialogRef<ProjectOverlayComponent, any>;
    planningCardDialogRef: MatDialogRef<PlanningCardOverlayComponent, any>;
    // --------------------------Overlay -----------------------------------------//

    openPlaceholderFormHandler(modalData) {
        // class is required to center align the modal on large screens
        let initialState = null;

        if (modalData) {

            initialState = {
                projectData: modalData.project,
                planningCardData: modalData.planningCardData,
                placeholderAllocationData: modalData.placeholderAllocationData,
                isUpdateModal: modalData.isUpdateModal
            };

        }

        const config = {
            class: 'modal-dialog-centered',
            ignoreBackdropClick: true,
            initialState: initialState
        };

        this.bsModalRef = this.modalService.show(PlaceholderFormComponent, config);

        // inserts & updates resource data when changes are made to resource
        this.bsModalRef.content.upsertPlaceholderAllocationsToProject.subscribe(updatedCommitment => {           
            this.projectAllocationsStore.dispatch(
                new ProjectAllocationsActions.UpsertPlaceholderAllocations({
                    placeholderAllocations: updatedCommitment.placeholderAllocations,
                    planningCardDialogRef: this.planningCardDialogRef,
                    projectDialogRef: this.projectDialogRef
                })
            )
        });

        this.bsModalRef.content.deletePlaceholderAllocationByIds.subscribe(event => {
            this.projectAllocationsStore.dispatch(
                new ProjectAllocationsActions.DeletePlaceholderAllocations({
                    placeholderIds : event.placeholderIds,
                    placeholderAllocation: event.placeholderAllocation,
                    notifyMessage: event.notifyMessage, 
                    projectDialogRef: this.projectDialogRef,
                    planningCardDialogRef: this.planningCardDialogRef
                })
            )
        });
        
        this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe(event => {
            let upsertedAllocations : any = event;
            this.projectAllocationsStore.dispatch(
                new ProjectAllocationsActions.UpsertResourceAllocations({
                    resourceAllocation: upsertedAllocations.resourceAllocation,
                    splitSuccessMessage: upsertedAllocations.splitSuccessMessage,
                    showMoreThanYearWarning: upsertedAllocations.showMoreThanYearWarning,
                    allocationDataBeforeSplitting: upsertedAllocations.allocationDataBeforeSplitting,
                    projectDialogRef: this.projectDialogRef
                })
              )
        });

        this.bsModalRef.content.openBackFillPopUp.subscribe(result => {
            this.backfillDialogService.projectDialogRef = this.projectDialogRef;
            this.backfillDialogService.openBackFillFormHandler(result);
        });
    }

}
