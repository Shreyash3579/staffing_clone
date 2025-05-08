import { Component, OnInit, Input, EventEmitter, Output, OnChanges, SimpleChanges } from '@angular/core';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormControl,ReactiveFormsModule } from '@angular/forms';


@Component({
  selector: 'shared-multi-select-dropdown',
  standalone: true,
  templateUrl: './multi-select-dropdown.component.html',
  styleUrls: ['./multi-select-dropdown.component.scss'],
  imports: [MatFormFieldModule, MatSelectModule,ReactiveFormsModule]
})
export class MultiSelectDropdownComponent implements OnInit, OnChanges {
  // -----------------------Input Events--------------------------------------------//
  @Input() dropdownList;
  @Input() selectedItems = [];
  @Input() title = "";
  @Input() hasFilter = false;
  @Input() width:string = "100%";
  @Input() maxSelectionLimit = null;
  @Input() placeholder = null;
  selectedValuesControl = new FormControl();
  
  // -----------------------Output Events--------------------------------------------//
  @Output() valueChange = new EventEmitter<any>();

  constructor() {
  }

  // -----------------------Component Lifecycle Hooks --------------------------------------------//
  ngOnInit() {
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.selectedItems && changes.selectedItems.currentValue && !changes.selectedItems.currentValue.isArrayEqual(this.selectedValuesControl.value)) {
      this.selectedValuesControl.setValue(this.selectedItems);
    }
  }

  change(item){
    this.valueChange.emit(item.value);
  }
  
  isOptionDisabled(opt: any): boolean {
    if (this.maxSelectionLimit) {
      return this.selectedValuesControl.value.length >= this.maxSelectionLimit && !this.selectedValuesControl.value.find(el => el == opt)
    }
    return false;
  }
}

