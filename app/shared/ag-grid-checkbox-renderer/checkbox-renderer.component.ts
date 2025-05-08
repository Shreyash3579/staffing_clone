import { Component, OnDestroy } from '@angular/core';

import { ICellRendererAngularComp } from 'ag-grid-angular';
import { ICellRendererParams } from 'ag-grid-community';

@Component({
  selector: 'checkbox-renderer',
  template: `
    <input
      type="checkbox"
      (click)="checkedHandler($event)"
      [checked]="params.value"
    />
`,
})
export class CheckboxRenderer implements ICellRendererAngularComp, OnDestroy {
  public refresh(params: ICellRendererParams): boolean {
    this.params = params;
    this.gridApi.refreshCells();
    return true;
  }
  public params: any;
  public gridApi:any;

  agInit(params: any): void {
    this.params = params;
    this.gridApi= params.api;
  }

  checkedHandler(event) {
      let checked = event.target.checked;
      let colId = this.params.column.colId;
      this.params.node.setDataValue(colId, checked);
  }
  ngOnDestroy() {
    //this.params.unsubscribe();
}
}
