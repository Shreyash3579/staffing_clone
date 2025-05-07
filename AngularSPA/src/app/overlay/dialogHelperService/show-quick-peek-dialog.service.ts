// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';

// ----------------------- Component References ----------------------------------//
import { QuickPeekComponent } from 'src/app/shared/quick-peek/quick-peek.component';

// --------------------------utilities -----------------------------------------//
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';

@Injectable()
export class ShowQuickPeekDialogService {
  // --------------------------Local Variable -----------------------------------------//
  bsModalRef: BsModalRef;

  constructor(private modalService: BsModalService) { }

  // --------------------------Overlay -----------------------------------------//

  showQuickPeekDialogHandler(employees) {
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        employees: employees
      }
    };

    this.bsModalRef = this.modalService.show(QuickPeekComponent, config);
  }

  closeDialog() {
    this.bsModalRef?.hide();
  }

}
