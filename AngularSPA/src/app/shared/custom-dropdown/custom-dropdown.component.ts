import { Component, Input, OnInit, Output, EventEmitter } from "@angular/core";

@Component({
    selector: "app-custom-dropdown",
    templateUrl: "./custom-dropdown.component.html",
    styleUrls: ["./custom-dropdown.component.scss"]
})
export class CustomDropdownComponent implements OnInit {
    // @Inputs
    @Input() menuOptions: any[];
    @Input() dropdownLabel: string;

    // @Outputs
    @Output() toggleDropdownEmitter = new EventEmitter<any>();

    public isMenuopen: boolean = false;

    constructor() {}

    ngOnInit(): void {}

    // Handles dropdown toggle
    toggleDropdown() {
        this.isMenuopen = !this.isMenuopen;
    }

    // Handles option selection
    selectOption(selectedOption) {
        this.toggleDropdownEmitter.emit(selectedOption);
        this.isMenuopen = false;
    }
}
