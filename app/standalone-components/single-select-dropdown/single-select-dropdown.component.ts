import { Component, OnInit, OnChanges, Input, EventEmitter, Output, SimpleChanges, ViewChild } from '@angular/core';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';

@Component({
  selector: 'shared-single-select-dropdown',
  standalone: true,
  templateUrl: './single-select-dropdown.component.html',
  styleUrls: ['./single-select-dropdown.component.scss'],
  imports: [MatFormFieldModule, MatSelectModule]
})
export class SingleSelectDropdownComponent implements OnInit {

  // -----------------------Input Events--------------------------------------------//
  @Input() dropdownList;
  @Input() selectedItem;
  @Input() title = "null";
  @Input() hasFilter = false;
  @Input() width = "100";

  // -----------------------Output Events--------------------------------------------//
  @Output() valueChange = new EventEmitter<any>();

  constructor() {
  }

  // -----------------------Component Lifecycle Hooks --------------------------------------------//
  ngOnInit() {
  }

  change(item){
    this.valueChange.emit(item);
  }
}

