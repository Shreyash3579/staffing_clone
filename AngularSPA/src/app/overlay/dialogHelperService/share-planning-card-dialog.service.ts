import { Injectable } from '@angular/core';
import { BsModalService } from 'ngx-bootstrap/modal';
import { Observable, Subject } from 'rxjs';
import { PlanningCard } from 'src/app/shared/interfaces/planningCard.interface';
import { SharePlanningCardComponent } from 'src/app/shared/share-planning-card/share-planning-card.component';
import * as PlanningCardOverlayActions from 'src/app/state/actions/planning-card-overlay.action';
import * as fromPlanningCardOverlayStore from 'src/app/state/reducers/planning-card-overlay.reducer';
import { Store } from '@ngrx/store';
import { MatDialogRef } from '@angular/material/dialog';
import { PlanningCardOverlayComponent } from '../planning-card-overlay/planning-card-overlay.component';

@Injectable()
export class SharePlanningCardDialogService {
  sharedPlanningCard = new Subject();
  planningCardDialogRef: MatDialogRef<PlanningCardOverlayComponent, any>;

  constructor(private modalService: BsModalService,
    private projectAllocationsStore: Store<fromPlanningCardOverlayStore.State>,) { }

  setSharedPlanningCardData(value) {
    this.sharedPlanningCard.next(value);
  }

  getSharedPlanningCardData(): Observable<any> {
    return this.sharedPlanningCard.asObservable();
  }

  openSharePlanningCardDialogHandler(event) {
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        modalHeaderText: 'Share Planning Card',
        planningCard: event.planningCard,
        isPegPlanningCard: event.isPegPlanningCard,
      }
    };

    const bsModalRef = this.modalService.show(SharePlanningCardComponent, config);

    bsModalRef.content.sharePlanningCardEmitter.subscribe((event) => {
      this.projectAllocationsStore.dispatch(
        new PlanningCardOverlayActions.UpsertPlanningCard({
          planningCard: event.planningCard,
          planningCardDialogRef: this.planningCardDialogRef 
      })
      )
    });
  }
}
