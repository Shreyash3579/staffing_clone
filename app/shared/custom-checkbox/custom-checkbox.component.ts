import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

const SIZES = ["small", "medium", "large"];

@Component({
  selector: 'app-custom-checkbox',
  templateUrl: './custom-checkbox.component.html',
  styleUrls: ['./custom-checkbox.component.scss']
})

export class CustomCheckboxComponent implements OnInit {

  // Inputs
  @Input() isChecked: boolean;
  @Input() isPartiallySelected: boolean;
  @Input() size: string;

  // Outputs
  @Output() toggleCheckbox = new EventEmitter<any>();

  constructor() { }

  ngOnInit(): void { }

  getCheckboxSize() {
    let defaultSize;

    // Check size
    if (this.size) {
      defaultSize = SIZES.filter((s) => s == this.size);
    } else {
      defaultSize = SIZES[0];
    }

    return defaultSize;
  }

  onToggleCheckbox() {
    this.toggleCheckbox.emit();
  }

}
