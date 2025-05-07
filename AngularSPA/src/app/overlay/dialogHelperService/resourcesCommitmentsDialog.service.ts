// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';

// ----------------------- Component References ----------------------------------//
import { ResourcesCommitmentsComponent } from 'src/app/shared/resources-commitments/resources-commitments.component';

// --------------------------utilities -----------------------------------------//
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { Subject } from 'rxjs';

@Injectable()
export class ResourcesCommitmentsDialogService {
  // --------------------------Local Variable -----------------------------------------//
  bsModalRef: BsModalRef;
  public resourceToBeDeselected = new Subject<string>();

  constructor(private modalService: BsModalService) { }

  // --------------------------Overlay -----------------------------------------//

  showResourcesCommitmentsDialogHandler(employees) {
    const config = {
      class: 'custom-modal-xlarge modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        employees: employees
      }
    };

    this.bsModalRef = this.modalService.show(ResourcesCommitmentsComponent, config);
  }

  closeDialog() {
    this.bsModalRef?.hide();
  }

  resourceToBeDeselectedEmitter(employeeCode){
    this.resourceToBeDeselected.next(employeeCode);
  }


}
