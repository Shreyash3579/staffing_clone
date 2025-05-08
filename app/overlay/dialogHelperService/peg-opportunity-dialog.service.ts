// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';

// ----------------------- Component References ----------------------------------//
import { PegOpportunityComponent } from 'src/app/shared/peg-opportunity/peg-opportunity.component';

// --------------------------utilities -----------------------------------------//
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';

@Injectable()
export class PegOpportunityDialogService {
  // --------------------------Local Variable -----------------------------------------//
  bsModalRef: BsModalRef;

  constructor(private modalService: BsModalService) { }

  // --------------------------Overlay -----------------------------------------//

  openPegOpportunityDetailPopUp(pegOpportunityId) {
    const config = {
      class: 'peg-opportunity-popup',
      ignoreBackdropClick: true,
      initialState: {
        pegOpportunityId: pegOpportunityId
      }
    };

    this.bsModalRef = this.modalService.show(PegOpportunityComponent, config);
  }

  closeDialog() {
    this.bsModalRef?.hide();
  }

}
