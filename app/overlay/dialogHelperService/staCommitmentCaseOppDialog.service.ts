// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';

import { ProjectOverlayComponent } from '../project-overlay/project-overlay.component';
import { STACommitmentFormComponent } from 'src/app/shared/sta-commitment-form/sta-commitment.form.component';

// --------------------------utilities -----------------------------------------//
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { MatDialogRef } from '@angular/material/dialog';
import { Store } from '@ngrx/store';
import * as fromResourceCommitmentStore from 'src/app/state/reducers/resource-commitment.reducer';
import * as resourceCommitmentActions from 'src/app/state/actions/resource-commitment.action';
import { PlanningCardOverlayComponent } from '../planning-card-overlay/planning-card-overlay.component';
@Injectable()
export class staCommitmentDialogService {

  constructor(private modalService: BsModalService,
    private resourceCommitmentStore: Store<fromResourceCommitmentStore.State>) { }

  // --------------------------Local Variable -----------------------------------------//

  bsModalRef: BsModalRef;
  projectDialogRef: MatDialogRef<ProjectOverlayComponent, any>;
  planningCardDialogRef: MatDialogRef<PlanningCardOverlayComponent, any>;
  // --------------------------Overlay -----------------------------------------//

  openSTACommitmentFormHandler(event) {
    // class is required to center align the modal on large screens
    const config = {
      class: 'custom-modal-large modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        project: event.project
      }
    };
    this.bsModalRef = this.modalService.show(STACommitmentFormComponent, config);
    //this.bsModalRef = this.modalService.show(CaseRollFormComponent, config);

    this.bsModalRef.content.insertCaseOppCommitments.subscribe(response => {
      this.resourceCommitmentStore.dispatch(
        new resourceCommitmentActions.InsertCaseOppCommitments({ 
          commitments : response.commitments,
          projectDialogRef: this.projectDialogRef,
          planningCardDialogRef: this.planningCardDialogRef
        })
      )
    });

    this.bsModalRef.content.deleteCaseOppCommitments.subscribe(response => {
      this.resourceCommitmentStore.dispatch(
        new resourceCommitmentActions.DeleteCaseOppCommitments({ 
          commitmentIds: response.commitmentIds,
          oldCaseCode: response.oldCaseCode,
          opportunityId: response.opportunityId,
          planningCardId: response.planningCardId,
          projectDialogRef: this.projectDialogRef,
          planningCardDialogRef: this.planningCardDialogRef
        })
      )
    });

  }
}
