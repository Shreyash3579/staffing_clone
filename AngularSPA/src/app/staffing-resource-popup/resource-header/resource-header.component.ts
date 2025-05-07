import { Component, OnInit } from "@angular/core";

@Component({
  selector: "app-resource-header",
  templateUrl: "./resource-header.component.html",
  styleUrls: ["./resource-header.component.scss"]
})
export class ResourceHeaderComponent implements OnInit {
  // header labels
  columns = ["Employee", "Position", "Lvl", "Off"];

  constructor() {}

  ngOnInit(): void {}
}
