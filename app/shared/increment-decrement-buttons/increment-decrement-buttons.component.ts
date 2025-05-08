import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { ValidationService } from '../validationService';

@Component({
  selector: 'shared-increment-decrement-buttons',
  templateUrl: './increment-decrement-buttons.component.html',
  styleUrls: ['./increment-decrement-buttons.component.scss']
})
export class IncrementDecrementButtonsComponent implements OnInit {
  // -----------------------Local & Input Variables--------------------------------------------//
  @Input() selectedValue: number = 0;
  @Input() minValue: number = 0;
  @Input() maxValue: number = 100;
  @Input() stepValue: number = 1;

  // -----------------------Output Events--------------------------------------------//
  @Output() valueChanged = new EventEmitter();

  constructor() { }

  ngOnInit() {
  }

  //---------------------------Event Handlers -----------------------------------------//

  increamentSelectedValue() {
    if (this.selectedValue >= this.maxValue) {
      this.selectedValue = this.maxValue;
    }
    else {
      this.selectedValue = parseInt(this.selectedValue.toString()) + parseInt(this.stepValue.toString());
    }
    this.valueChanged.emit(this.selectedValue);
  }

  decrementSelectedValue() {
    if (this.selectedValue <= this.minValue) {
      this.selectedValue = this.minValue;
    }
    else {
      this.selectedValue = parseInt(this.selectedValue.toString()) - parseInt(this.stepValue.toString());
    }
    this.valueChanged.emit(this.selectedValue);
  }

  updateSelectedValue() {
    if (ValidationService.isValidNumberBetween(this.selectedValue, this.minValue, this.maxValue)) {
      this.valueChanged.emit(this.selectedValue);
    }
  }


}
