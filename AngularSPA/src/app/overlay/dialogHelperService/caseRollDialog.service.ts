// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';

// ----------------------- Component References ----------------------------------//
import { CaseRollFormComponent } from 'src/app/shared/case-roll-form/case-roll-form.component';
import { ProjectOverlayComponent } from '../project-overlay/project-overlay.component';

// ----------------------- Service References ----------------------------------//
import { CaseRollService } from '../behavioralSubjectService/caseRoll.service';

// --------------------------utilities -----------------------------------------//
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { MatDialogRef } from '@angular/material/dialog';
import { Store } from '@ngrx/store';
import * as fromProjectAllocationsStore from 'src/app/state/reducers/project-allocations.reducer';
import * as ProjectOverlayActions from 'src/app/state/actions/project-overlay.action';

@Injectable()
export class CaseRollDialogService {

  constructor(private modalService: BsModalService,
    private caseRollService: CaseRollService,
    private projectAllocationsStore: Store<fromProjectAllocationsStore.State>) { }

  // --------------------------Local Variable -----------------------------------------//

  bsModalRef: BsModalRef;
  projectDialogRef: MatDialogRef<ProjectOverlayComponent, any>;
  // --------------------------Overlay -----------------------------------------//

  openCaseRollFormHandler(event) {
    // class is required to center align the modal on large screens
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        project: event.project
      }
    };
    this.bsModalRef = this.modalService.show(CaseRollFormComponent, config);

    this.bsModalRef.content.upsertCaseRollAndAllocations.subscribe(response => {
      this.projectAllocationsStore.dispatch(
        new ProjectOverlayActions.UpsertCaseRollAndAllocations({ 
          caseRollArray: [].concat(response.caseRoll),
          resourceAllocations: response.resourceAllocations,
          project: response.project,
          allocationDataBeforeSplitting: response.allocationDataBeforeSplitting,
          projectDialogRef: this.projectDialogRef
        })
      )
    });

    this.bsModalRef.content.upsertCaseRollAndPlaceholderAllocations.subscribe(response => {
      this.projectAllocationsStore.dispatch(
        new ProjectOverlayActions.UpsertCaseRollAndPlaceholderAllocations({ 
          caseRollArray: [].concat(response.caseRoll),
          resourceAllocations: response.resourceAllocations,
          project: response.project,
          allocationDataBeforeSplitting: response.allocationDataBeforeSplitting,
          projectDialogRef: this.projectDialogRef
        })
      )
    });

    this.bsModalRef.content.revertCaseRollAndAllocations.subscribe(response => {      
      this.projectAllocationsStore.dispatch(
        new ProjectOverlayActions.RevertCaseRollAndAllocations({ 
          caseRoll: response.caseRoll,
          resourceAllocations: response.resourceAllocations,
          project: response.project,
          projectDialogRef: this.projectDialogRef
        })
      )
    });
  }

}
