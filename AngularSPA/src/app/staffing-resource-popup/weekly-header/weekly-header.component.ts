import { Component, OnInit, Input } from "@angular/core";

@Component({
  selector: "app-weekly-header",
  templateUrl: "./weekly-header.component.html",
  styleUrls: ["./weekly-header.component.scss"]
})
export class WeeklyHeaderComponent implements OnInit {
  // week dates
  @Input() weeklyArray;

  constructor() {}

  ngOnInit(): void {}
}
