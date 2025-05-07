// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';
import { AddNewPlanningCardFormComponent } from 'src/app/shared/add-new-planning-card-form/add-new-planning-card-form.component';

// --------------------------utilities -----------------------------------------//
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';

import { Store } from '@ngrx/store';
import { Subject } from 'rxjs';

import* as fromPlanningCardOverlayStore from 'src/app/state/reducers/planning-card-overlay.reducer';
import * as PlanningCardOverlayActions from 'src/app/state/actions/planning-card-overlay.action';

@Injectable()
export class addPlanningCardDialogService {

    constructor(private modalService: BsModalService,
        private planningCardOverlayStore:Store<fromPlanningCardOverlayStore.State>) { }

    // --------------------------Local Variable -----------------------------------------//

    bsModalRef: BsModalRef;
    newlycreatedPlanningCard: Subject<any> = new Subject<any>(); // Add the newlycreatedPlanningCard subject

    // --------------------------Overlay -----------------------------------------//

    openAddNewPlanningCardFormHandler() {
        // class is required to center align the modal on large screens
        let initialState = null;

        const config = {
            class: 'modal-dialog-centered',
            ignoreBackdropClick: true,
        };

        this.bsModalRef = this.modalService.show(AddNewPlanningCardFormComponent, config);

        this.bsModalRef.content.upsertPlanningCard.subscribe(response => {
            this.newlycreatedPlanningCard.next(response)
            this.planningCardOverlayStore.dispatch(
                new PlanningCardOverlayActions.UpsertPlanningCard({ planningCard: response })
            );
            this.newlycreatedPlanningCard.next(response); // Emit the newly created planning card
        });
    }

}
