// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';


// --------------------------utilities -----------------------------------------//
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { SharedNotesModalComponent } from 'src/app/shared/shared-notes-modal/shared-notes-modal.component';
import { SharedNotesService } from 'src/app/core/services/shared-notes-info.service';
import { Subject } from 'rxjs';


@Injectable({
    providedIn: 'root' // This makes the service a singleton and available application-wide
  })
export class NotesAlertDialogService {
showBanner: Subject<boolean> = new Subject<boolean>();

private caseIntakeDataSubject = new Subject<any>();  // Sends raw data to HomeComponent
//private filteredDataSubject = new Subject<any>();    // Receives filtered data from HomeComponent

// Expose observables for subscriptions
caseIntakeData$ = this.caseIntakeDataSubject.asObservable();
//filteredData$ = this.filteredDataSubject.asObservable();


constructor(private modalService: BsModalService, private sharedNotesService:SharedNotesService ) { }

  // --------------------------Local Variable -----------------------------------------//

  bsModalRef: BsModalRef;
  // --------------------------Overlay -----------------------------------------//
  
  openNotesAlertDialogHandler(event) {
    // check if the popup is already open
      // class is required to center align the modal on large screens
      const config = {
        class: 'modal-dialog-centered',
        ignoreBackdropClick: false,
        initialState: {
          sharedNotesInfo: event
        }
      };
  
      this.bsModalRef = this.modalService.show(SharedNotesModalComponent, config);
  
      this.bsModalRef.content.updateSharedNotesStatus.subscribe(employeeCode => {
         this.sharedNotesService.updateSharedNotesStatus(employeeCode).subscribe();
       });

       this.bsModalRef.content.closeEvent.subscribe(() => {
           this.showBanner.next(false);
       });
  }

   // Emit raw case intake data to HomeComponent
  sendCaseIntakeData(data: any): void {
    this.caseIntakeDataSubject.next(data);
  }

  // Method for HomeComponent to send back filtered data
  // sendFilteredData(data: any): void {
  //   this.filteredDataSubject.next(data);
  // }

  openCaseIntakeAlertDialogHandler(event) {
    // check if the popup is already open
      // class is required to center align the modal on large screens
      const config = {
        class: 'modal-dialog-centered',
        ignoreBackdropClick: false,
        initialState: {
          caseIntakeInfo: event
        }
      };
  
      this.bsModalRef = this.modalService.show(SharedNotesModalComponent, config);
  
      this.bsModalRef.content.updateSharedNotesStatus.subscribe(employeeCode => {
         this.sharedNotesService.updateCaseIntakeAlertsStatus(employeeCode).subscribe();
       });

       this.bsModalRef.content.closeEvent.subscribe(() => {
           this.showBanner.next(false);
       });
  }

  //check this method
  openAlertsDialogHandler(notes, caseIntakeAlerts) {
    // check if the popup is already open
      // class is required to center align the modal on large screens
      const config = {
        class: 'modal-dialog-centered',
        ignoreBackdropClick: false,
        initialState: {
          sharedNotesInfo: notes,
          caseIntakeInfo: caseIntakeAlerts
        }
      };
  
      this.bsModalRef = this.modalService.show(SharedNotesModalComponent, config);
  
      this.bsModalRef.content.updateSharedNotesStatus.subscribe(employeeCode => {
         this.sharedNotesService.updateCaseIntakeAlertsStatus(employeeCode).subscribe();
       });

       this.bsModalRef.content.closeEvent.subscribe(() => {
           this.showBanner.next(false);
       });
  }

}
