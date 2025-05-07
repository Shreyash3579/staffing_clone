import { Component, Input, OnInit } from "@angular/core";

@Component({
  selector: "shared-validation-message",
  templateUrl: "./validation-message.component.html",
  styleUrls: ["./validation-message.component.scss"]
})
export class ValidationMessageComponent implements OnInit {

  @Input() validationMessages: string[];
  constructor() { }

  ngOnInit(): void {
  }

}
