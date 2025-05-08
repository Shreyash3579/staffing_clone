import { Component, OnInit, Input } from "@angular/core";

@Component({
  selector: "app-commitments-body",
  templateUrl: "./commitments-body.component.html",
  styleUrls: ["./commitments-body.component.scss"]
})
export class CommitmentsBodyComponent implements OnInit {
  @Input() weeklyArray: any[];
  @Input() commitments: any[];

  dates = [];
  dateRange = [];

  constructor() {}

  ngOnInit(): void {
    this.weeklyArray.forEach((week) => {
      week.dates.forEach((day) => {
        this.dates.push(day);
      });
    });

    this.dateRange[0] = this.dates[0];
    this.dateRange[1] = this.dates[this.dates.length - 1];

    this.commitments = this.commitments.filter(
      (opp) =>
        new Date(opp.endDate).getTime() >= new Date(this.dateRange[0]).getTime() &&
        new Date(opp.startDate).getTime() <= new Date(this.dateRange[1]).getTime() &&
        opp.commitmentTypeCode !== "" &&
        opp.render !== "split"
    );
  }
}
