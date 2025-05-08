import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { ValidationService } from '../validationService';

@Component({
  selector: 'shared-slide-bar',
  templateUrl: './slide-bar.component.html',
  styleUrls: ['./slide-bar.component.scss']
})
export class SlideBarComponent implements OnInit {
  // -----------------------Local & Input Variables--------------------------------------------//
  @Input() selectedValue: number = 0;
  @Input() minValue: number = 0;
  @Input() maxValue: number = 100;
  @Input() stepValue: number = 5;

  // -----------------------Output Events--------------------------------------------//
  @Output() valueChanged = new EventEmitter<any>();

  constructor() { }

  ngOnInit() {
  }

  //---------------------------Event Handlers -----------------------------------------//

  onSelectedValueChange() {
    if (ValidationService.isValidNumberBetween(this.selectedValue, this.minValue, this.maxValue)) {
      this.valueChanged.emit(this.selectedValue);
    }
  }

  setSelectedValue(value) {
    if (ValidationService.isValidNumberBetween(value, this.minValue, this.maxValue)) {
      this.selectedValue = value;
      this.valueChanged.emit(this.selectedValue);
    }
  }

}