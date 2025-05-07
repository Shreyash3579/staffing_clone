import { Component, OnInit, EventEmitter, Output } from '@angular/core';

// ----------------------- External Libraries References ----------------------------------//
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ResourceAllocation } from 'src/app/shared/interfaces/resourceAllocation.interface';
import { DateService } from 'src/app/shared/dateService';

@Component({
  selector: 'app-overlay-ag-grid-notes',
  templateUrl: './ag-grid-notes.component.html',
  styleUrls: ['./ag-grid-notes.component.scss']
})
export class AgGridNotesComponent implements OnInit {
  // -----------------------Local Variables--------------------------------------------//
  public projectData: any;
  public notes: string = "";

  private allocationData: ResourceAllocation;

  // --------------------------Ouput Events--------------------------------//
  @Output() updateNotesForAllocation = new EventEmitter<any>();

  constructor(public bsModalRef: BsModalRef) { }

  //--------------------------Life Cycle Event handlers---------------------------------//
  ngOnInit() {
    if (this.projectData && this.projectData.notes) {
      this.notes = this.projectData.notes
    }
  }

  closeForm() {
    this.bsModalRef.hide();
  }

  updateCommitment() {
    if (this.projectData) {
      let allocation = isNaN(this.projectData.allocation) ?
        (this.projectData.allocation.indexOf('%') !== -1) ?
          Number(this.projectData.allocation.slice(0, this.projectData.allocation.indexOf('%'))) :
          Number(this.projectData.allocation) :
        this.projectData.allocation;

      this.allocationData = {
        id: this.projectData.id,
        caseName: this.projectData.caseName,
        clientName: this.projectData.clientName,
        oldCaseCode: this.projectData.oldCaseCode,
        employeeCode: this.projectData.employeeCode,
        employeeName: this.projectData.employeeName,
        currentLevelGrade: this.projectData.currentLevelGrade,
        serviceLineCode: this.projectData.serviceLineCode,
        serviceLineName: this.projectData.serviceLineName,
        operatingOfficeCode: this.projectData.operatingOfficeCode,
        operatingOfficeAbbreviation: this.projectData.operatingOfficeAbbreviation,
        pipelineId: this.projectData.pipelineId,
        opportunityName: this.projectData.opportunityName,
        investmentCode: this.projectData.investmentCode,
        investmentName: this.projectData.investmentName,
        caseRoleCode: this.projectData.caseRoleCode,
        allocation: allocation,
        startDate: this.projectData.startDate,
        endDate: this.projectData.endDate,
        caseStartDate: this.projectData.caseStartDate,
        caseEndDate: this.projectData.caseEndDate,
        opportunityStartDate: this.projectData.opportunityStartDate,
        opportunityEndDate: this.projectData.opportunityEndDate,
        lastUpdatedBy: null,
        notes: this.notes
      };

      this.updateNotesForAllocation.emit({
        resourceAllocation: this.allocationData,
        event: 'notesPopUp',
        showMoreThanYearWarning: false
      })
      this.closeForm();
    }
  }
}
