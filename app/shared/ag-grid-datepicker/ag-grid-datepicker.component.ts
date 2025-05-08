import { Component, ViewChild, OnInit, ElementRef } from '@angular/core';
import { ICellEditorAngularComp } from 'ag-grid-angular';
import { DateService } from '../dateService';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { ValidationService } from '../validationService';

@Component({
  selector: 'ag-grid-datepicker',
  templateUrl: './ag-grid-datepicker.component.html',
  styleUrls: ['./ag-grid-datepicker.component.scss']
})
export class AgGridDatepickerComponent implements OnInit, ICellEditorAngularComp {
  /*--------------------Local Variables *------------------------------*/

  @ViewChild('datePicker', { static: true }) datePickerElement: ElementRef;
  private params: any;
  public date: any;
  bsConfig: Partial<BsDatepickerConfig>;

  /*---------------------Constructor *-----------------------------*/

  constructor() { }

  /*---------------------Componenent Life cycle events *-----------------------------*/
  ngOnInit() {
    this.bsConfig = {
      containerClass: 'theme-red',
      customTodayClass: 'custom-today-class',
      dateInputFormat: 'DD-MMM-YYYY',
      showWeekNumbers: false,
      selectFromOtherMonth: true,
      returnFocusToInput: true
    };
  }

  /*------------------ICellEditorAngularComp methods implemented ---------------*/

  agInit(params: any): void {
    this.params = params;
    if (!this.params.value)
      this.date = null;
    else {
      this.date = DateService.convertDateInBainFormat(new Date(this.params.value));
    }
  }

  isCancelAfterEnd?() {
    let inputValue = this.datePickerElement.nativeElement.value;
    if (!ValidationService.isValidDate(inputValue)) {
      return true;
    }
    return false;
  }

  getValue() {
    const inputValue = this.datePickerElement.nativeElement.value;
    if (inputValue != null) {
      return DateService.convertDateInBainFormat(inputValue);
    } else {
      return DateService.convertDateInBainFormat(this.date);
    }
  }

  isPopup() {
    //  We MUST tell agGrid that this is a popup control, to make it display properly.
    return true;
  }

  /*------------------ngx bootstrap DatePicker methods ---------------*/
  onDateSelect(date) {
    // This is not required as setting returnFocusToInput as true in bsConfig already does this
    // setTimeout(() => { // this will focus the cell in AG grid
    //   this.datePickerElement.nativeElement.focus();
    // }, 0);
  }
}
