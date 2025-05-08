import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { Office } from 'src/app/shared/interfaces/office.interface';
import { Project } from 'src/app/shared/interfaces/project.interface';
import { LocalStorageService } from 'src/app/shared/local-storage.service';

@Component({
  selector: 'app-update-case-card',
  templateUrl: './update-case-card.component.html',
  styleUrls: ['./update-case-card.component.scss']
})
export class UpdateCaseCardComponent implements OnInit {
  selectedOffice: string;
  selectedColumn: string;
  
  officeList = [];
  offices: Office[] = [];

  validationMessage: string = '';

  //--------------------Initial State Data -----------------------------//
  projectToAdd: Project;
  planningBoardColumnsList;

  // -----------------------Output Events--------------------------------------------//
  @Output() addSelectedProjectToBoard = new EventEmitter();
  
  constructor(private localStorageService: LocalStorageService,
    private bsModalRef: BsModalRef) { }

  ngOnInit(): void {
    
    this.offices = this.localStorageService.get(ConstantsMaster.localStorageKeys.OfficeList);
    this.selectedOffice = this.projectToAdd.managingOfficeCode;
    
    this.setOfficeDropdown();
  }

  setOfficeDropdown() {
    this.officeList = this.offices.map(data => {
      return {
        "text": data.officeName,
        "value": data.officeCode
      }
    })
    this.officeList = this.officeList.sort((a, b) => a.text.localeCompare(b.text))
  }

  closeForm() {
    this.bsModalRef.hide();
  }

  setSelectedColumn(selectedOption) {
    this.selectedColumn = selectedOption.value;
  }

  setSelectedOffice(selectedOption) {
    this.selectedOffice = selectedOption.value;
  }

  addCardToBoard(){

    if(!this.selectedOffice){
      this.validationMessage = "Please select an office";
    }else if(!this.selectedColumn){
      this.validationMessage = "Please select a week column";
    }
    else if(this.selectedOffice == this.projectToAdd.managingOfficeCode)
      this.validationMessage = "Please select a different office than the current billing/managing office";
    else if(this.selectedOffice && this.selectedColumn) {
      
      const officeData = this.offices.find(x => x.officeCode === parseInt(this.selectedOffice));

      const selectedDataObj = {
        selectedProject: this.projectToAdd, 
        selectedColumn: this.selectedColumn,
        selectedOffice: officeData
      }

      this.addSelectedProjectToBoard.emit(selectedDataObj);
      this.bsModalRef.hide();
    }
  }

}
