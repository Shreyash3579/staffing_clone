// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';

// ----------------------- Component References ----------------------------------//
import { ProjectOverlayComponent } from '../project-overlay/project-overlay.component';
import { QuickAddFormComponent } from 'src/app/shared/quick-add-form/quick-add-form.component';
import { ResourceOverlayComponent } from '../resource-overlay/resource-overlay.component';

// ----------------------- Service References ----------------------------------//
import { BackfillDialogService } from './backFillDialog.service';
import { ResourceAssignmentService } from '../behavioralSubjectService/resourceAssignment.service';
import { ResourceCommitmentService } from '../behavioralSubjectService/resourceCommitment.service';

// --------------------------utilities -----------------------------------------//
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { MatDialogRef } from '@angular/material/dialog';
import { PlaceholderAssignmentService } from '../behavioralSubjectService/placeholderAssignment.service';
import { OverlappedTeamDialogService } from './overlapped-team-dialog.service';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';

// --------------------------Ngrx -----------------------------------------//
import { Store } from '@ngrx/store';
import * as fromProjectAllocationsStore from 'src/app/state/reducers/project-allocations.reducer';
import * as ProjectAllocationsActions from 'src/app/state/actions/project-allocations.action';
import * as fromResourceCommitment from 'src/app/state/reducers/resource-commitment.reducer';
import * as ResourceCommitmentActions from 'src/app/state/actions/resource-commitment.action';
import { PlanningCardOverlayComponent } from '../planning-card-overlay/planning-card-overlay.component';

@Injectable()
export class QuickAddDialogService {

  constructor(private modalService: BsModalService,
    private backfillDialogService: BackfillDialogService,
    private resourceCommitmentService: ResourceCommitmentService,
    private resourceAssignmentService: ResourceAssignmentService,
    private placeholderAssignmentService: PlaceholderAssignmentService,
    private overlappedTeamDialogService: OverlappedTeamDialogService,
    private projectAllocationsStore: Store<fromProjectAllocationsStore.State>,
    private resourceCommitmentStore: Store<fromResourceCommitment.State>) { }

  // --------------------------Local Variable -----------------------------------------//

  bsModalRef: BsModalRef;
  projectDialogRef: MatDialogRef<ProjectOverlayComponent, any>;
  planningCardDialogRef: MatDialogRef<PlanningCardOverlayComponent, any>;
  dialogRef: MatDialogRef<ResourceOverlayComponent, any>;
  // --------------------------Overlay -----------------------------------------//

  openQuickAddFormHandler(modalData) {
    // class is required to center align the modal on large screens
    let initialState = null;

    if (modalData) {

      initialState = {
        commitmentTypeCode: modalData.commitmentTypeCode,
        resourceAllocationData: modalData.resourceAllocationData,
        isUpdateModal: modalData.isUpdateModal,
        isPlaceholderAllocation: modalData.isPlaceholderAllocation
      };

    }

    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: initialState
    };

    this.bsModalRef = this.modalService.show(QuickAddFormComponent, config);

    this.bsModalRef.content.insertResourcesCommitments.subscribe(commitments => {
      this.resourceCommitmentStore.dispatch(
        new ResourceCommitmentActions.InsertResourceCommitment({
          commitments: commitments,
          resourceDialogRef: this.dialogRef
        })
      )
    });

    this.bsModalRef.content.updateResourceCommitment.subscribe(updatedCommitment => {
      this.resourceCommitmentStore.dispatch(
        new ResourceCommitmentActions.InsertResourceCommitment({
          commitments: [].concat(updatedCommitment.resourceAllocation),
          resourceDialogRef: this.dialogRef,
          projectDialogRef: this.projectDialogRef,
          planningCardDialogRef: this.planningCardDialogRef
        })
      )
    });

    // inserts & updates resource data when changes are made to resource
    this.bsModalRef.content.upsertResourceAllocationsToProject.subscribe(updatedCommitment => {
      let upsertedAllocations : any = updatedCommitment;
      this.projectAllocationsStore.dispatch(
        new ProjectAllocationsActions.UpsertResourceAllocations({
          resourceAllocation: upsertedAllocations.resourceAllocation,
          splitSuccessMessage: upsertedAllocations.splitSuccessMessage,
          showMoreThanYearWarning: upsertedAllocations.showMoreThanYearWarning,
          allocationDataBeforeSplitting: upsertedAllocations.allocationDataBeforeSplitting,
          projectDialogRef: this.projectDialogRef,
          planningCardDialogRef: this.planningCardDialogRef,
          resourceDialogRef: this.dialogRef
        })
      )
      
    });

    // inserts & updates placeholder data when changes are made to resource
    this.bsModalRef.content.upsertPlaceholderAllocationsToProject.subscribe(updatedCommitment => {
        let placeholderAllocations : PlaceholderAllocation[] = updatedCommitment.placeholderAllocations;
        this.projectAllocationsStore.dispatch(
            new ProjectAllocationsActions.UpsertPlaceholderAllocations({
              placeholderAllocations: placeholderAllocations,
              projectDialogRef: this.projectDialogRef,
              planningCardDialogRef: this.planningCardDialogRef,
              resourceDialogRef: this.dialogRef
            })
          )
    });

    this.bsModalRef.content.deleteResourceCommitment.subscribe(allocation => {
      this.projectAllocationsStore.dispatch(
        new ProjectAllocationsActions.DeleteResourceAllocationCommitment({
          allocation: [].concat(allocation.resourceAllocation),
          allocationIds: "",
          commitmentIds: allocation.commitmentId,
          notifyMessage: 'Assignment Deleted',
          resourceDialogRef: this.dialogRef
        })
      )
      
    });

    this.bsModalRef.content.deleteResourceAllocationFromCase.subscribe(allocation => {
      this.projectAllocationsStore.dispatch(
        new ProjectAllocationsActions.DeleteResourceAllocationCommitment({
          allocation: [].concat(allocation.resourceAllocation),
          allocationIds: allocation.allocationId,
          commitmentIds: "",
          notifyMessage: 'Assignment Deleted',
          projectDialogRef: this.projectDialogRef,
          planningCardDialogRef: this.planningCardDialogRef,
          resourceDialogRef: this.dialogRef
        })
      )
    });

    this.bsModalRef.content.deletePlaceholderAllocationByIds.subscribe(event => {
      this.projectAllocationsStore.dispatch(
          new ProjectAllocationsActions.DeletePlaceholderAllocations({
              placeholderIds : event.placeholderIds,
              placeholderAllocation: event.placeholderAllocation,
              notifyMessage: 'Assignment Deleted',
              projectDialogRef: this.projectDialogRef,
              planningCardDialogRef: this.planningCardDialogRef
          })
      )
  });

    this.bsModalRef.content.openBackFillPopUp.subscribe(result => {
      this.backfillDialogService.dialogRef = this.dialogRef;
      this.backfillDialogService.projectDialogRef = this.projectDialogRef;
      this.backfillDialogService.openBackFillFormHandler(result);
    });

    this.bsModalRef.content.openOverlappedTeamsPopup.subscribe(result => {
      this.overlappedTeamDialogService.dialogRef = this.dialogRef;
      this.overlappedTeamDialogService.projectDialogRef = this.projectDialogRef;
      this.overlappedTeamDialogService.openOverlappedTeamsFormHandler(result);
    });

  }

}
