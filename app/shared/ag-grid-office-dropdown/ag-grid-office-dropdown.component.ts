import { Component } from '@angular/core';
import { ICellEditorAngularComp } from "ag-grid-angular";
import { OfficeHierarchy } from '../interfaces/officeHierarchy.interface';

@Component({
  selector: 'app-ag-grid-office-dropdown',
  templateUrl: './ag-grid-office-dropdown.component.html',
  styleUrls: ['./ag-grid-office-dropdown.component.scss']
})
export class AgGridOfficeDropdownComponent implements ICellEditorAngularComp {
  params: any;
  officeHierarchy:OfficeHierarchy;
  selectedOfficeList = [];
  appendParentCodesOnSelect: false;
  constructor() { }

  agInit(params: any): void {
    this.params = params;
    this.appendParentCodesOnSelect = this.params.appendParentCodesOnSelect ?? false;
    this.initializeOfficesDropdown(params);
  }

  initializeOfficesDropdown(params){
    this.selectedOfficeList = params.data[params.column.colId]? params.data[params.column.colId].split(',') : [];

    // Re-assign object In order to reflect changes in Demand side office filter when changes are done in User Settings
    if (!this.officeHierarchy) {
      this.officeHierarchy = JSON.parse(JSON.stringify(params.values));
    }
  }

  setOfficesBySelectedValues(officeCodes){
    this.selectedOfficeList = officeCodes?.split(',');
  }

  isPopup(): boolean {
    return true;
  }

  isCancelAfterEnd?() {
    return false;
  }

  getValue(): any {
     return this.selectedOfficeList;
   }
 
}

