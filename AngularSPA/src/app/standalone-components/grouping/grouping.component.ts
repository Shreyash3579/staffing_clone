import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";

@Component({
  selector: "app-grouping",
  standalone: true,
  imports: [],
  templateUrl: "./grouping.component.html",
  styleUrls: ["./grouping.component.scss"]
})
export class GroupingComponent implements OnInit {
  @Input() selectedValue = "";
  @Input() option1 = {text: "", value: ""};
  @Input() option2 = {text: "", value: ""};

  @Output() onToggleClicked = new EventEmitter<string>();

  constructor() {}

  ngOnInit(): void {}

  onModeChange(event) {
    if (event.target.checked === true) {
      this.selectedValue = this.option2.value;
    } else {
      this.selectedValue = this.option1.value;
    }

    this.onToggleClicked.emit(this.selectedValue);
  }
}
