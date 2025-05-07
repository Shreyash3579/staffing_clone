import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, SimpleChange, SimpleChanges } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ResourceNotesAlert } from '../interfaces/resource-notes-alert';
import { SharedNotesService } from 'src/app/core/services/shared-notes-info.service';
import { CoreService } from 'src/app/core/core.service';
import { Case } from '../interfaces/case.interface';
import { CaseIntakeAlert } from '../interfaces/caseIntakeAlert.interface';
import { OverlayDialogService } from 'src/app/overlay/dialogHelperService/overlayDialog.service';

@Component({
  selector: 'app-shared-notes-modal',
  templateUrl: './shared-notes-modal.component.html',
  styleUrls: ['./shared-notes-modal.component.scss']
})
export class SharedNotesModalComponent implements OnInit {

  constructor(public bsModalRef: BsModalRef, private _sharedNoteService :SharedNotesService, private coreService: CoreService) {}

  notes = []
  loggedInEmployeeCode:string;
  baseUrl : string;

  @Input() sharedNotesInfo: ResourceNotesAlert[];
  @Input() caseIntakeInfo: CaseIntakeAlert[];
  @Output() closeEvent = new EventEmitter<void>();
  @Output() updateSharedNotesStatus = new EventEmitter<string>();


  ngOnInit(): void {
   this.loggedInEmployeeCode = this.coreService.loggedInUser.employeeCode;
   this.baseUrl = `${this.coreService.appSettings.environmentUrl}/overlay?`;
  }

  ngOnChanges(changes: SimpleChanges) {

    if (changes.sharedNotes && changes.sharedNotes?.currentValue && changes.sharedNotes?.currentValue.length > 0) {
        this.sharedNotesInfo = this.sharedNotesInfo;
    }

    if(changes.caseIntakeAlerts && changes.caseIntakeAlerts?.currentValue && changes.caseIntakeAlerts?.currentValue.length > 0) {
        this.caseIntakeInfo = changes.caseIntakeAlerts?.currentValue;
    }
  }


  close(): void {
    this.bsModalRef.hide();
    this.updateSharedNotesStatus.emit(this.loggedInEmployeeCode);   
    this.closeEvent.emit(); 
  }

}