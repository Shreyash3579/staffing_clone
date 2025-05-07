import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ResourceFiltersBasicMenu } from 'src/app/shared/interfaces/resource-filters-basic-menu.interface';

/**
 * Small size sets, font-size to 12px, height to 32px, and padding to 8px vertically and 10px horizontally
 * Medium sets, font-size to 14px, height to 38px, and both vertical and horizontal padding to 10px
 */
const SIZES = ["small", "medium"];
const POSITIONS = ["down", "up", "right"];

@Component({
  selector: 'app-resource-basic-menu',
  templateUrl: './resource-basic-menu.component.html',
  styleUrls: ['./resource-basic-menu.component.scss']
})
export class ResourceBasicMenuComponent implements OnInit {

  // Inputs
  @Input() menuOptions: ResourceFiltersBasicMenu[];
  @Input() size: any;
  @Input() menuPosition: string;
  @Input() placeholder: string = "Select";
  @Input() selectedValue: any;

  // Outputs
  @Output() optionSelectedEmitter = new EventEmitter();
  @Output() onResourceMenuToggleEmitter = new EventEmitter();

  isPlaceholderActive: boolean = true;
  selectedOption: ResourceFiltersBasicMenu = {
    label: "", value: "", selected: false, isHidden: false
  };

  constructor() { }

  ngOnInit(): void {
    // Set default size
    if (SIZES.includes(this.size)) {
      this.size = SIZES.filter((x) => x == this.size);
    } else {
      this.size = SIZES[0];
    }

    // Set default menu position
    if (this.menuOptions && POSITIONS.includes(this.menuPosition)) {
      const temp = POSITIONS.filter((x) => x == this.menuPosition);
      this.menuPosition = "drop" + temp;
    } else {
      this.menuPosition = "drop" + POSITIONS[0];
    }

    // get value
    if (this.selectedValue) {
      const temp = this.menuOptions.filter((x) => x.value == this.selectedValue);
      this.selectedOption = {
        label: temp[0].label || "",
        value: temp[0].value || "",
        selected: true
      }
    }
  }

  handleMenuOptionsClick(optionSelected: ResourceFiltersBasicMenu) {
    this.menuOptions.forEach((option) => {
      if (option.value == optionSelected.value) {
        this.selectedOption = option;
        this.optionSelectedEmitter.emit(option);
      }
    });

    this.getSelectedOption();
  }

  getSelectedOption() {
    this.menuOptions.forEach((option) => {
      if (option.value == this.selectedOption.value) {
        option.selected = true;

        this.selectedOption = option;
        this.isPlaceholderActive = false;
      } else {
        option.selected = false;
      }
    });
  }

  onResourceMenuToggle(){
    this.onResourceMenuToggleEmitter.emit();
  }

}
