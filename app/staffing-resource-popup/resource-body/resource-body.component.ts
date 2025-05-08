import { Component, OnInit, Input, OnDestroy } from "@angular/core";
import { Employee } from "src/app/shared/interfaces/employee.interface";
import { DateService } from "src/app/shared/dateService";

@Component({
  selector: "app-resource-body",
  templateUrl: "./resource-body.component.html",
  styleUrls: ["./resource-body.component.scss"]
})
export class ResourceBodyComponent implements OnInit, OnDestroy {
  @Input() employee: Employee;

  today = new Date();
  lastBillableDate: string;

  constructor() {}

  ngOnInit(): void {
    // console.log(this.employee);
    this.setLastBillableDate();
  }

  setLastBillableDate() {
    // this.lastBillableDate = DateService.convertDateInBainFormat(this.employee?.lastBillable?.lastBillableDate);

    var date = new Date();
    date.setDate(date.getDate() + 1);

    if (!this.lastBillableDate) {
      this.lastBillableDate = "N/A";
    } else if (DateService.isSameOrAfter(this.lastBillableDate, date)) {
      this.lastBillableDate = "Staffed";
    }
  }

  ngOnDestroy(): void {
  }
}
