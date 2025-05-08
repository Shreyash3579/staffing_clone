import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { SystemconfirmationFormComponent } from 'src/app/shared/systemconfirmation-form/systemconfirmation-form.component';

@Component({
  selector: 'app-planning-board-modal',
  templateUrl: './planning-board-modal.component.html',
  styleUrls: ['./planning-board-modal.component.scss']
})
export class PlanningBoardModalComponent implements OnInit {
  showProgressBar: boolean = true;
  showModalCloseButton = true;
  public bsModalRef: BsModalRef;

  constructor(
    private modalRef: BsModalRef,
    private modalService: BsModalService,
    private localStorageService: LocalStorageService
  ) { }

  ngOnInit(): void {

  }

  showProgressBarHandler(showLoader) {
    this.showProgressBar = showLoader;
  }

  showModalCloseButtonHandler(showModalCloseButton) {
    this.showModalCloseButton = showModalCloseButton;
  }

  closeForm() {
    this.modalRef.hide();
  }

  isPlaygroundIdExist() {
    const userPlaygroundSessionInfo = this.localStorageService.get(ConstantsMaster.localStorageKeys.userPlaygroundSession);
    return !!userPlaygroundSessionInfo?.playgroundId
  }

  checkPlayGroundIdAndCloseForm() {
    if (this.isPlaygroundIdExist()) {

      const confirmationPopUpBodyMessage = `You are about to leave the planning board.
      Any changes made will be reflected when you return to this page.
      If you make changes to resources outside your supply settings, those resources will be reflected here as well.`

      this.openSystemConfirmationPlanningCardHandler({ confirmationPopUpBodyMessage: confirmationPopUpBodyMessage });
    } else {
      this.closeForm();
    }
  }

  openSystemConfirmationPlanningCardHandler(event) {
    const config = {
      class: 'modal-dialog-centered',
      ignoreBackdropClick: true,
      initialState: {
        confirmationPopUpBodyMessage: event.confirmationPopUpBodyMessage
      }
    };

    this.bsModalRef = this.modalService.show(SystemconfirmationFormComponent, config);

    this.bsModalRef.content.onActionResult.subscribe((actionResult) => {
      if (!!actionResult) {
        this.closeForm();
      }
    });
  }


}
