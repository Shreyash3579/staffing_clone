// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';
// --------------------------utilities -----------------------------------------//
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { AddTeamSkuComponent } from 'src/app/case-planning/add-team-sku/add-team-sku.component';

@Injectable()
export class AddTeamSkuDialogService {

  constructor(private modalService: BsModalService) { }

  // --------------------------Local Variable -----------------------------------------//

  modalRef: BsModalRef;
  // --------------------------Overlay -----------------------------------------//
  
  openAddTeamsSkuHandler(selectedCase) {
    if (!this.modalRef) {
      this.modalRef = this.modalService.show(AddTeamSkuComponent, {
        animated: true,
        backdrop: false,
        ignoreBackdropClick: true,
        initialState: selectedCase
      });
    }
  }

  closeDialog(){
    this.modalRef?.hide;
  }

}
