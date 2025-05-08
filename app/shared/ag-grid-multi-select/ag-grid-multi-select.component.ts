import { Component, OnInit } from '@angular/core';
import { ICellEditorAngularComp } from "ag-grid-angular";

@Component({
  selector: 'app-ag-grid-multi-select',
  templateUrl: './ag-grid-multi-select.component.html',
  styleUrls: ['./ag-grid-multi-select.component.scss']
})
export class AgGridMultiSelectComponent implements ICellEditorAngularComp {

  constructor(){
  }

  dropdownList;
  selectedValueList = [];
  appendParentCodesOnSelect=false;

  params: any;
  dropdownTitle: string = '';
  agInit(params: any): void {
    this.params = params;
    this.dropdownTitle  = params.dropdownTitle;
    this.appendParentCodesOnSelect = params.appendParentCodesOnSelect ?? false;
    if(params.dropdownTypeCode == 'H'){
      this.initializeHierarchyDropdown(params);
    }else{
      this.initializeNonHierarchyDropdown(params);
    }
    
  }
  
  initializeHierarchyDropdown(params) {
    const dropdownData = params.values;

    if (dropdownData) {
      const childrenList = dropdownData.map(item => {
        return {
          text: item.text,
          value: item.value,
          collapsed: true,
          children: item.children.map(child => {
            return {
              text: child.text,
              value: child.value,
              checked: false
            };
          }),
          checked: false
        };
      });

      this.dropdownList = {
        text: 'All',
        value: 0,
        checked: false,
        children: childrenList
      };
      this.selectedValueList = params.data[params.column.colId]? params.data[params.column.colId].split(',') : [] ;
    
    }
  }

  initializeNonHierarchyDropdown(params) {
    const dropdownData = params.values;

    if (dropdownData) {
       this.dropdownList = {
        text: 'All',
        value: 0,
        checked: false,
        children: dropdownData.map(item => {
          return {
            text: item[Object.keys(item)[1]].toString(),
            value: item[Object.keys(item)[0]].toString(),
            checked: false
          }
        })
      };
      
      this.selectedValueList = params.data[params.column.colId]? params.data[params.column.colId].split(',') : [];
    }
  }

  
  setSelectedValues(data){
    if (this.isArrayEqual(this.selectedValueList.map(String), data.split(','))) {
      return false;
    }

    this.selectedValueList = data.split(',');
  }

  isPopup(): boolean {
    return true;
  }

  private isArrayEqual(array1, array2) {
    return JSON.stringify(array1) === JSON.stringify(array2);
  }

  isCancelAfterEnd?() {
    return false;
  }


  getValue(): any {
   // this.params.data.levelGrade = this.selectedLevelGradeList 
    return this.selectedValueList;
  }

}
